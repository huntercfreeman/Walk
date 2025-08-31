using System.Text;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.Characters.Models;
using Walk.TextEditor.RazorLib.Cursors.Models;
using Walk.TextEditor.RazorLib.Exceptions;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;

namespace Walk.TextEditor.RazorLib;

public partial class TextEditorService
{
    private Task ViewModel_cursorShouldBlinkTask = Task.CompletedTask;
    private CancellationTokenSource ViewModel_cursorShouldBlinkCancellationTokenSource = new();
    private TimeSpan ViewModel_blinkingCursorTaskDelay = TimeSpan.FromMilliseconds(1000);

    public bool ViewModel_CursorShouldBlink { get; private set; } = true;

    private bool ViewModel_intentStopCursorBlinking = false;
    private int ViewModel_stopCursorBlinkingId = 0;

    /// <summary>
    /// Thread Safety: most invocations of this are from the TextEditorEditContext,...
    /// ...so a decision needs to made whether this is restricted to the TextEditorEditContext
    /// and therefore thread safe, or this isn't restricted and perhaps should have a thread safe
    /// pattern involved.
    ///
    /// Precise debounce timing: I think this implementation has an imprecise debounce delay,
    /// but that is very low importance from a triage perspective. There are more important things to work on.
    /// </summary>
    public void ViewModel_StopCursorBlinking()
    {
        if (ViewModel_CursorShouldBlink)
        {
            ViewModel_CursorShouldBlink = false;
            SecondaryChanged?.Invoke(SecondaryChangedKind.ViewModel_CursorShouldBlinkChanged);
        }

        var localId = ++ViewModel_stopCursorBlinkingId;

        if (!ViewModel_intentStopCursorBlinking)
        {
            ViewModel_intentStopCursorBlinking = true;

            ViewModel_cursorShouldBlinkTask = Task.Run(async () =>
            {
                while (true)
                {
                    var id = ViewModel_stopCursorBlinkingId;

                    await Task
                        .Delay(ViewModel_blinkingCursorTaskDelay)
                        .ConfigureAwait(false);

                    if (id == ViewModel_stopCursorBlinkingId)
                    {
                        ViewModel_CursorShouldBlink = true;
                        SecondaryChanged?.Invoke(SecondaryChangedKind.ViewModel_CursorShouldBlinkChanged);
                        break;
                    }
                }

                ViewModel_intentStopCursorBlinking = false;
            });
        }
    }

    #region CREATE_METHODS
    public void ViewModel_Register(
        TextEditorEditContext editContext,
        Key<TextEditorViewModel> viewModelKey,
        ResourceUri resourceUri,
        Category category)
    {
        var viewModel = new TextEditorViewModel(
            viewModelKey,
            resourceUri,
            this,
            TextEditorVirtualizationResult.ConstructEmpty(),
            new TextEditorDimensions(0, 0, 0, 0),
            scrollLeft: 0,
            scrollTop: 0,
            scrollWidth: 0,
            scrollHeight: 0,
            marginScrollHeight: 0,
            category);

        RegisterViewModel(editContext, viewModel);
    }

    public void ViewModel_Register(TextEditorEditContext editContext, TextEditorViewModel viewModel)
    {
        RegisterViewModel(editContext, viewModel);
    }
    #endregion

    #region READ_METHODS
    public TextEditorViewModel? ViewModel_GetOrDefault(Key<TextEditorViewModel> viewModelKey)
    {
        return TextEditorState.ViewModelGetOrDefault(viewModelKey);
    }

    public Dictionary<Key<TextEditorViewModel>, TextEditorViewModel> ViewModel_GetViewModels()
    {
        return TextEditorState.ViewModelGetViewModels();
    }

    public TextEditorModel? ViewModel_GetModelOrDefault(Key<TextEditorViewModel> viewModelKey)
    {
        var viewModel = TextEditorState.ViewModelGetOrDefault(viewModelKey);
        if (viewModel is null)
            return null;

        return Model_GetOrDefault(viewModel.PersistentState.ResourceUri);
    }

    public string? ViewModel_GetAllText(Key<TextEditorViewModel> viewModelKey)
    {
        var textEditorModel = ViewModel_GetModelOrDefault(viewModelKey);

        return textEditorModel is null
            ? null
            : Model_GetAllText(textEditorModel.PersistentState.ResourceUri);
    }

    public async ValueTask<TextEditorDimensions> ViewModel_GetTextEditorMeasurementsAsync(string elementId)
    {
        return await JsRuntimeTextEditorApi
            .GetTextEditorMeasurementsInPixelsById(elementId)
            .ConfigureAwait(false);
    }
    #endregion

    #region UPDATE_METHODS
    public void ViewModel_SetScrollPositionBoth(
        TextEditorEditContext editContext,
        TextEditorViewModel viewModel,
        double scrollLeftInPixels,
        double scrollTopInPixels)
    {
        viewModel.ScrollWasModified = true;
        viewModel.SetScrollLeft((int)Math.Floor(scrollLeftInPixels));
        viewModel.SetScrollTop((int)Math.Floor(scrollTopInPixels));
    }

    public void ViewModel_SetScrollPositionLeft(
        TextEditorEditContext editContext,
        TextEditorViewModel viewModel,
        double scrollLeftInPixels)
    {
        viewModel.ScrollWasModified = true;
        viewModel.SetScrollLeft((int)Math.Floor(scrollLeftInPixels));
    }

    public void ViewModel_SetScrollPositionTop(
        TextEditorEditContext editContext,
        TextEditorViewModel viewModel,
        double scrollTopInPixels)
    {
        viewModel.ScrollWasModified = true;
        viewModel.SetScrollTop((int)Math.Floor(scrollTopInPixels));
    }

    public void ViewModel_MutateScrollVerticalPosition(
        TextEditorEditContext editContext,
        TextEditorViewModel viewModel,
        double pixels)
    {
        viewModel.ScrollWasModified = true;
        viewModel.MutateScrollTop((int)Math.Ceiling(pixels));
    }

    public void ViewModel_MutateScrollHorizontalPosition(
        TextEditorEditContext editContext,
        TextEditorViewModel viewModel,
        double pixels)
    {
        viewModel.ScrollWasModified = true;
        viewModel.MutateScrollLeft((int)Math.Ceiling(pixels));
    }

    /// <summary>
    /// If a given scroll direction is already within view of the text span, do not scroll on that direction.
    ///
    /// Measurements are in pixels.
    /// </summary>
    public void ViewModel_ScrollIntoView(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel,
        TextEditorTextSpan textSpan)
    {
        var lineInformation = modelModifier.GetLineInformationFromPositionIndex(textSpan.StartInclusiveIndex);
        var lineIndex = lineInformation.Index;
        // Scroll start
        var targetScrollTop = (lineIndex) * viewModel.PersistentState.CharAndLineMeasurements.LineHeight;
        bool lowerBoundInRange = viewModel.PersistentState.ScrollTop <= targetScrollTop;
        bool upperBoundInRange = targetScrollTop < (viewModel.PersistentState.TextEditorDimensions.Height + viewModel.PersistentState.ScrollTop);
        if (lowerBoundInRange && upperBoundInRange)
        {
            targetScrollTop = viewModel.PersistentState.ScrollTop;
        }
        else
        {
            var startDistanceToTarget = Math.Abs(targetScrollTop - viewModel.PersistentState.ScrollTop);
            var endDistanceToTarget = Math.Abs(targetScrollTop - (viewModel.PersistentState.TextEditorDimensions.Height + viewModel.PersistentState.ScrollTop));

            // Scroll end
            if (endDistanceToTarget < startDistanceToTarget)
            {
                var margin = 3 * viewModel.PersistentState.CharAndLineMeasurements.LineHeight;
                var maxMargin = viewModel.PersistentState.TextEditorDimensions.Height * .3;
                if (margin > maxMargin)
                    margin = (int)maxMargin;

                targetScrollTop -= (viewModel.PersistentState.TextEditorDimensions.Height - margin);
            }
        }

        var columnIndex = textSpan.StartInclusiveIndex - lineInformation.Position_StartInclusiveIndex;
        // Scroll start
        var targetScrollLeft = columnIndex * viewModel.PersistentState.CharAndLineMeasurements.CharacterWidth;
        lowerBoundInRange = viewModel.PersistentState.ScrollLeft <= targetScrollLeft;
        upperBoundInRange = targetScrollLeft < (viewModel.PersistentState.TextEditorDimensions.Width + viewModel.PersistentState.ScrollLeft);
        if (lowerBoundInRange && upperBoundInRange)
        {
            targetScrollLeft = viewModel.PersistentState.ScrollLeft;
        }
        else
        {
            var startDistanceToTarget = targetScrollLeft - viewModel.PersistentState.ScrollLeft;
            var endDistanceToTarget = targetScrollLeft - (viewModel.PersistentState.TextEditorDimensions.Width + viewModel.PersistentState.ScrollLeft);

            // Scroll end
            if (endDistanceToTarget < startDistanceToTarget)
            {
                var margin = 9 * viewModel.PersistentState.CharAndLineMeasurements.CharacterWidth;
                var maxMargin = viewModel.PersistentState.TextEditorDimensions.Width * .3;
                if (margin > maxMargin)
                    margin = maxMargin;

                targetScrollLeft -= (viewModel.PersistentState.TextEditorDimensions.Width - margin);
            }
        }

        if (targetScrollTop == -1 && targetScrollLeft == -1)
            return;

        viewModel.PersistentState.Changed_Cursor_AnyState = true;

        if (targetScrollTop != -1 && targetScrollLeft != -1)
            ViewModel_SetScrollPositionBoth(editContext, viewModel, targetScrollLeft, targetScrollTop);
        else if (targetScrollTop != -1)
            ViewModel_SetScrollPositionTop(editContext, viewModel, targetScrollTop);
        else
            ViewModel_SetScrollPositionLeft(editContext, viewModel, targetScrollLeft);
    }

    public ValueTask ViewModel_FocusPrimaryCursorAsync(string primaryCursorContentId)
    {
        return CommonService.JsRuntimeCommonApi
            .FocusHtmlElementById(primaryCursorContentId, preventScroll: true);
    }

    public void ViewModel_MoveCursor(
        string? key,
        string? code,
        bool ctrlKey,
        bool shiftKey,
        bool altKey,
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel)
    {
        ViewModel_MoveCursorUnsafe(
            key,
            code,
            ctrlKey,
            shiftKey,
            altKey,
            editContext,
            modelModifier,
            viewModel);

        viewModel.PersistentState.ShouldRevealCursor = true;
    }

    public void ViewModel_MoveCursorUnsafe(
        string? key,
        string? code,
        bool ctrlKey,
        bool shiftKey,
        bool altKey,
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel)
    {
        var shouldClearSelection = false;

        if (shiftKey)
        {
            if (viewModel.SelectionAnchorPositionIndex == -1 ||
                viewModel.SelectionEndingPositionIndex == viewModel.SelectionAnchorPositionIndex)
            {
                var positionIndex = modelModifier.GetPositionIndex(
                    viewModel.LineIndex,
                    viewModel.ColumnIndex);

                viewModel.SelectionAnchorPositionIndex = positionIndex;
            }
        }
        else
        {
            shouldClearSelection = true;
        }

        int lengthOfLine = 0; // This variable is used in multiple switch cases.

        switch (key)
        {
            case CommonFacts.ARROW_LEFT_KEY:
                if (TextEditorSelectionHelper.HasSelectedText(viewModel) &&
                    !shiftKey)
                {
                    var selectionBounds = TextEditorSelectionHelper.GetSelectionBounds(viewModel);

                    var lowerLineInformation = modelModifier.GetLineInformationFromPositionIndex(
                        selectionBounds.Position_LowerInclusiveIndex);

                    viewModel.LineIndex = lowerLineInformation.Index;

                    viewModel.ColumnIndex = selectionBounds.Position_LowerInclusiveIndex -
                        lowerLineInformation.Position_StartInclusiveIndex;
                }
                else
                {
                    if (viewModel.ColumnIndex <= 0)
                    {
                        if (viewModel.LineIndex != 0)
                        {
                            viewModel.LineIndex--;

                            lengthOfLine = modelModifier.GetLineLength(viewModel.LineIndex);

                            viewModel.SetColumnIndexAndPreferred(lengthOfLine);
                        }
                        else
                        {
                            viewModel.SetColumnIndexAndPreferred(0);
                        }
                    }
                    else
                    {
                        if (ctrlKey)
                        {
                            var columnIndexOfCharacterWithDifferingKind = modelModifier.GetColumnIndexOfCharacterWithDifferingKind(
                                viewModel.LineIndex,
                                viewModel.ColumnIndex,
                                true);

                            if (columnIndexOfCharacterWithDifferingKind == -1) // Move to start of line
                            {
                                viewModel.SetColumnIndexAndPreferred(0);
                            }
                            else
                            {
                                if (!altKey) // Move by character kind
                                {
                                    viewModel.SetColumnIndexAndPreferred(columnIndexOfCharacterWithDifferingKind);
                                }
                                else // Move by camel case
                                {
                                    var positionIndex = modelModifier.GetPositionIndex(viewModel);
                                    var rememberStartPositionIndex = positionIndex;

                                    var minPositionIndex = columnIndexOfCharacterWithDifferingKind;
                                    var infiniteLoopPrediction = false;

                                    if (minPositionIndex > positionIndex)
                                        infiniteLoopPrediction = true;

                                    bool useCamelCaseResult = false;

                                    if (!infiniteLoopPrediction)
                                    {
                                        while (--positionIndex > minPositionIndex)
                                        {
                                            var currentRichCharacter = modelModifier.RichCharacterList[positionIndex];

                                            if (Char.IsUpper(currentRichCharacter.Value) || currentRichCharacter.Value == '_')
                                            {
                                                useCamelCaseResult = true;
                                                break;
                                            }
                                        }
                                    }

                                    if (useCamelCaseResult)
                                    {
                                        var columnDisplacement = positionIndex - rememberStartPositionIndex;
                                        viewModel.SetColumnIndexAndPreferred(viewModel.ColumnIndex + columnDisplacement);
                                    }
                                    else
                                    {
                                        viewModel.SetColumnIndexAndPreferred(columnIndexOfCharacterWithDifferingKind);
                                    }
                                }
                            }
                        }
                        else
                        {
                            viewModel.SetColumnIndexAndPreferred(viewModel.ColumnIndex - 1);
                        }
                    }
                }

                break;
            case CommonFacts.ARROW_DOWN_KEY:
                if (viewModel.LineIndex < modelModifier.LineCount - 1)
                {
                    viewModel.LineIndex++;

                    lengthOfLine = modelModifier.GetLineLength(viewModel.LineIndex);

                    viewModel.ColumnIndex = lengthOfLine < viewModel.PreferredColumnIndex
                        ? lengthOfLine
                        : viewModel.PreferredColumnIndex;
                }

                break;
            case CommonFacts.ARROW_UP_KEY:
                if (viewModel.LineIndex > 0)
                {
                    viewModel.LineIndex--;

                    lengthOfLine = modelModifier.GetLineLength(viewModel.LineIndex);

                    viewModel.ColumnIndex = lengthOfLine < viewModel.PreferredColumnIndex
                        ? lengthOfLine
                        : viewModel.PreferredColumnIndex;
                }

                break;
            case CommonFacts.ARROW_RIGHT_KEY:
                if (TextEditorSelectionHelper.HasSelectedText(viewModel) && !shiftKey)
                {
                    var selectionBounds = TextEditorSelectionHelper.GetSelectionBounds(viewModel);

                    var upperLineMetaData = modelModifier.GetLineInformationFromPositionIndex(
                        selectionBounds.Position_UpperExclusiveIndex);

                    viewModel.LineIndex = upperLineMetaData.Index;

                    if (viewModel.LineIndex >= modelModifier.LineCount)
                    {
                        viewModel.LineIndex = modelModifier.LineCount - 1;

                        var upperLineLength = modelModifier.GetLineLength(viewModel.LineIndex);

                        viewModel.ColumnIndex = upperLineLength;
                    }
                    else
                    {
                        viewModel.ColumnIndex =
                            selectionBounds.Position_UpperExclusiveIndex - upperLineMetaData.Position_StartInclusiveIndex;
                    }
                }
                else
                {
                    lengthOfLine = modelModifier.GetLineLength(viewModel.LineIndex);

                    if (viewModel.ColumnIndex >= lengthOfLine &&
                        viewModel.LineIndex < modelModifier.LineCount - 1)
                    {
                        viewModel.SetColumnIndexAndPreferred(0);
                        viewModel.LineIndex++;
                    }
                    else if (viewModel.ColumnIndex != lengthOfLine)
                    {
                        if (ctrlKey)
                        {
                            var columnIndexOfCharacterWithDifferingKind = modelModifier.GetColumnIndexOfCharacterWithDifferingKind(
                                viewModel.LineIndex,
                                viewModel.ColumnIndex,
                                false);

                            if (columnIndexOfCharacterWithDifferingKind == -1) // Move to end of line
                            {
                                viewModel.SetColumnIndexAndPreferred(lengthOfLine);
                            }
                            else
                            {
                                if (!altKey) // Move by character kind
                                {
                                    viewModel.SetColumnIndexAndPreferred(
                                        columnIndexOfCharacterWithDifferingKind);
                                }
                                else // Move by camel case
                                {
                                    var positionIndex = modelModifier.GetPositionIndex(viewModel);
                                    var rememberStartPositionIndex = positionIndex;

                                    var maxPositionIndex = columnIndexOfCharacterWithDifferingKind;

                                    var infiniteLoopPrediction = false;

                                    if (maxPositionIndex < positionIndex)
                                        infiniteLoopPrediction = true;

                                    bool useCamelCaseResult = false;

                                    if (!infiniteLoopPrediction)
                                    {
                                        while (++positionIndex < maxPositionIndex)
                                        {
                                            var currentRichCharacter = modelModifier.RichCharacterList[positionIndex];

                                            if (Char.IsUpper(currentRichCharacter.Value) || currentRichCharacter.Value == '_')
                                            {
                                                useCamelCaseResult = true;
                                                break;
                                            }
                                        }
                                    }

                                    if (useCamelCaseResult)
                                    {
                                        var columnDisplacement = positionIndex - rememberStartPositionIndex;
                                        viewModel.SetColumnIndexAndPreferred(viewModel.ColumnIndex + columnDisplacement);
                                    }
                                    else
                                    {
                                        viewModel.SetColumnIndexAndPreferred(
                                            columnIndexOfCharacterWithDifferingKind);
                                    }
                                }
                            }
                        }
                        else
                        {
                            viewModel.SetColumnIndexAndPreferred(viewModel.ColumnIndex + 1);
                        }
                    }
                }

                break;
            case CommonFacts.HOME_KEY:
                if (ctrlKey)
                {
                    viewModel.LineIndex = 0;
                    viewModel.SetColumnIndexAndPreferred(0);
                }
                else
                {
                    var originalPositionIndex = modelModifier.GetPositionIndex(viewModel);

                    var lineInformation = modelModifier.GetLineInformation(viewModel.LineIndex);
                    var lastValidPositionIndex = lineInformation.Position_StartInclusiveIndex + lineInformation.LastValidColumnIndex;

                    viewModel.ColumnIndex = 0; // This column index = 0 is needed for the while loop below.
                    var indentationPositionIndexExclusiveEnd = modelModifier.GetPositionIndex(viewModel);

                    var cursorWithinIndentation = false;

                    while (indentationPositionIndexExclusiveEnd < lastValidPositionIndex)
                    {
                        var possibleIndentationChar = modelModifier.RichCharacterList[indentationPositionIndexExclusiveEnd].Value;

                        if (possibleIndentationChar == '\t' || possibleIndentationChar == ' ')
                        {
                            if (indentationPositionIndexExclusiveEnd == originalPositionIndex)
                                cursorWithinIndentation = true;
                        }
                        else
                        {
                            break;
                        }

                        indentationPositionIndexExclusiveEnd++;
                    }

                    if (originalPositionIndex == indentationPositionIndexExclusiveEnd)
                        viewModel.SetColumnIndexAndPreferred(0);
                    else
                        viewModel.SetColumnIndexAndPreferred(
                            indentationPositionIndexExclusiveEnd - lineInformation.Position_StartInclusiveIndex);
                }

                break;
            case CommonFacts.END_KEY:
                if (ctrlKey)
                    viewModel.LineIndex = modelModifier.LineCount - 1;

                lengthOfLine = modelModifier.GetLineLength(viewModel.LineIndex);

                viewModel.SetColumnIndexAndPreferred(lengthOfLine);

                break;
        }

        if (shiftKey)
        {
            viewModel.SelectionEndingPositionIndex = modelModifier.GetPositionIndex(
                viewModel.LineIndex,
                viewModel.ColumnIndex);
        }
        else if (!shiftKey && shouldClearSelection)
        {
            // The active selection is needed, and cannot be touched until the end.
            viewModel.SelectionAnchorPositionIndex = -1;
            viewModel.SelectionEndingPositionIndex = 0;
        }
    }

    public void ViewModel_CursorMovePageTop(
        TextEditorEditContext editContext,
        TextEditorViewModel viewModel)
    {
        ViewModel_CursorMovePageTopUnsafe(
            editContext,
            viewModel);
    }

    public void ViewModel_CursorMovePageTopUnsafe(
        TextEditorEditContext editContext,
        TextEditorViewModel viewModel)
    {
        if (viewModel.Virtualization.Count > 0)
        {
            var firstEntry = viewModel.Virtualization.EntryList[0];
            viewModel.LineIndex = firstEntry.LineIndex;
            viewModel.ColumnIndex = 0;
        }
    }

    public void ViewModel_CursorMovePageBottom(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel)
    {
        ViewModel_CursorMovePageBottomUnsafe(
            editContext,
            modelModifier,
            viewModel);
    }

    public void ViewModel_CursorMovePageBottomUnsafe(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel)
    {
        if (viewModel.Virtualization.Count > 0)
        {
            var lastEntry = viewModel.Virtualization.EntryList[viewModel.Virtualization.Count - 1];
            var lastEntriesLineLength = modelModifier.GetLineLength(lastEntry.LineIndex);

            viewModel.LineIndex = lastEntry.LineIndex;
            viewModel.ColumnIndex = lastEntriesLineLength;
        }
    }

    public void ViewModel_RevealCursor(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel)
    {
        try
        {
            if (!viewModel.PersistentState.ShouldRevealCursor)
                return;

            viewModel.PersistentState.ShouldRevealCursor = false;

            var cursorIsVisible = false;

            if (cursorIsVisible)
                return;

            // Console.WriteLine(nameof(RevealCursor));

            var cursorPositionIndex = modelModifier.GetPositionIndex(viewModel);

            var cursorTextSpan = new TextEditorTextSpan(
                cursorPositionIndex,
                cursorPositionIndex + 1,
                0);

            ViewModel_ScrollIntoView(
                editContext,
                modelModifier,
                viewModel,
                cursorTextSpan);
        }
        catch (WalkTextEditorException exception)
        {
            Console.WriteLine(exception);
        }
    }

    public void ViewModel_CalculateVirtualizationResult(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel,
        TextEditorComponentData componentData)
    {
#if DEBUG
        var startTime = Stopwatch.GetTimestamp();
#endif

        var tabWidth = editContext.TextEditorService.Options_GetOptions().TabWidth;
        viewModel.Virtualization.ShouldCalculateVirtualizationResult = false;

        var verticalStartingIndex = viewModel.PersistentState.ScrollTop /
            viewModel.PersistentState.CharAndLineMeasurements.LineHeight;

        var verticalTake = viewModel.PersistentState.TextEditorDimensions.Height /
            viewModel.PersistentState.CharAndLineMeasurements.LineHeight;

        // Vertical Padding (render some offscreen data)
        verticalTake += 1;

        var horizontalStartingIndex = (int)Math.Floor(
            viewModel.PersistentState.ScrollLeft /
            viewModel.PersistentState.CharAndLineMeasurements.CharacterWidth);

        var horizontalTake = (int)Math.Ceiling(
            viewModel.PersistentState.TextEditorDimensions.Width /
            viewModel.PersistentState.CharAndLineMeasurements.CharacterWidth);

        verticalStartingIndex = Math.Max(0, verticalStartingIndex);

        if (verticalStartingIndex + verticalTake > modelModifier.LineEndList.Count)
            verticalTake = modelModifier.LineEndList.Count - verticalStartingIndex;

        verticalTake = Math.Max(0, verticalTake);

        var lineCountAvailable = modelModifier.LineEndList.Count - verticalStartingIndex;

        var lineCountToReturn = verticalTake < lineCountAvailable
            ? verticalTake
            : lineCountAvailable;

        var endingLineIndexExclusive = verticalStartingIndex + lineCountToReturn;

        var totalWidth = (int)Math.Ceiling(modelModifier.MostCharactersOnASingleLineTuple.lineLength *
            viewModel.PersistentState.CharAndLineMeasurements.CharacterWidth);

        // Account for any tab characters on the 'MostCharactersOnASingleLineTuple'
        //
        // TODO: This code is not fully correct...
        //       ...if the longest line is 50 non-tab characters,
        //       and the second longest line is 49 tab characters,
        //       this code will erroneously take the '50' non-tab characters
        //       to be the longest line.
        var lineIndex = modelModifier.MostCharactersOnASingleLineTuple.lineIndex;
        var longestLineInformation = modelModifier.GetLineInformation(lineIndex);
        var tabCountOnLongestLine = modelModifier.GetTabCountOnSameLineBeforeCursor(
            longestLineInformation.Index,
            longestLineInformation.LastValidColumnIndex);
        // 1 of the character width is already accounted for
        var extraWidthPerTabKey = tabWidth - 1;
        totalWidth += (int)Math.Ceiling(extraWidthPerTabKey *
            tabCountOnLongestLine *
            viewModel.PersistentState.CharAndLineMeasurements.CharacterWidth);

        var totalHeight = (modelModifier.LineEndList.Count) *
            viewModel.PersistentState.CharAndLineMeasurements.LineHeight;

        // Add vertical margin so the user can scroll beyond the final line of content
        var percentOfMarginScrollHeightByPageUnit = 0.4;
        int marginScrollHeight = (int)Math.Ceiling(viewModel.PersistentState.TextEditorDimensions.Height * percentOfMarginScrollHeightByPageUnit);
        totalHeight += marginScrollHeight;

        var virtualizedLineList = new TextEditorVirtualizationLine[lineCountToReturn];

        viewModel.Virtualization = new TextEditorVirtualizationResult(
            virtualizedLineList,
            new List<TextEditorVirtualizationSpan>(),
            resultWidth: (int)Math.Ceiling(horizontalTake * viewModel.PersistentState.CharAndLineMeasurements.CharacterWidth),
            resultHeight: verticalTake * viewModel.PersistentState.CharAndLineMeasurements.LineHeight,
            left: (int)(horizontalStartingIndex * viewModel.PersistentState.CharAndLineMeasurements.CharacterWidth),
            top: verticalStartingIndex * viewModel.PersistentState.CharAndLineMeasurements.LineHeight,
            componentData,
            modelModifier,
            viewModel,
            viewModel.Virtualization);

        viewModel.PersistentState.ScrollWidth = totalWidth;
        viewModel.PersistentState.ScrollHeight = totalHeight;
        viewModel.PersistentState.MarginScrollHeight = marginScrollHeight;

        viewModel.PersistentState.GutterWidth = ViewModel_GetGutterWidthInPixels(modelModifier, viewModel, componentData);

        var absDiffScrollLeft = Math.Abs(componentData.LineIndexCache.ScrollLeftMarker - viewModel.PersistentState.ScrollLeft);
        var useCache = absDiffScrollLeft < 0.01 && componentData.LineIndexCache.ViewModelKeyMarker == viewModel.PersistentState.ViewModelKey;

        if (!useCache)
            componentData.LineIndexCache.IsInvalid = true;

        if (componentData.LineIndexCache.IsInvalid)
            componentData.LineIndexCache.Clear();
        else
            componentData.LineIndexCache.UsedKeyHashSet.Clear();

        if (SeenTabWidth != Options_GetTextEditorOptionsState().Options.TabWidth)
        {
            SeenTabWidth = Options_GetTextEditorOptionsState().Options.TabWidth;
            TabKeyOutput_ShowWhitespaceTrue = new string('-', SeenTabWidth - 1) + '>';

            var stringBuilder = new StringBuilder();

            for (int i = 0; i < SeenTabWidth; i++)
            {
                stringBuilder.Append("&nbsp;");
            }
            TabKeyOutput_ShowWhitespaceFalse = stringBuilder.ToString();
        }

        string tabKeyOutput;
        string spaceKeyOutput;

        if (Options_GetTextEditorOptionsState().Options.ShowWhitespace)
        {
            tabKeyOutput = TabKeyOutput_ShowWhitespaceTrue;
            spaceKeyOutput = "·";
        }
        else
        {
            tabKeyOutput = TabKeyOutput_ShowWhitespaceFalse;
            spaceKeyOutput = "&nbsp;";
        }

        __StringBuilder.Clear();

        var minLineWidthToTriggerVirtualizationExclusive = 2 * viewModel.PersistentState.TextEditorDimensions.Width;

        int lineOffset = -1;

        var entireSpan = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(componentData.LineIndexCache.VirtualizationSpanList);

        int position_StartInclusiveIndex;
        int position_EndExclusiveIndex;
        string leftCssValue;
        string topCssValue;
        int virtualizationSpan_StartInclusiveIndex;
        int virtualizationSpan_EndExclusiveIndex;

        while (true)
        {
            lineOffset++;

            if (viewModel.Virtualization.Count >= lineCountToReturn)
                break;
            // TODO: Is this '>' or '>='?
            if (verticalStartingIndex + lineOffset >= modelModifier.LineEndList.Count)
                break;

            lineIndex = verticalStartingIndex + lineOffset;

            useCache = componentData.LineIndexCache.Map.ContainsKey(lineIndex) &&
                       !componentData.LineIndexCache.ModifiedLineIndexList.Contains(lineIndex);

            if (useCache)
            {
                componentData.LineIndexCache.UsedKeyHashSet.Add(lineIndex);
                var cacheEntryCopy = componentData.LineIndexCache.Map[lineIndex];

                cacheEntryCopy.VirtualizationSpan_StartInclusiveIndex = viewModel.Virtualization.VirtualizationSpanList.Count;
                var smallSpan = entireSpan.Slice(
                    componentData.LineIndexCache.Map[lineIndex].VirtualizationSpan_StartInclusiveIndex,
                    componentData.LineIndexCache.Map[lineIndex].VirtualizationSpan_EndExclusiveIndex - componentData.LineIndexCache.Map[lineIndex].VirtualizationSpan_StartInclusiveIndex);
                foreach (var virtualizedSpan in smallSpan)
                {
                    viewModel.Virtualization.VirtualizationSpanList.Add(virtualizedSpan);
                }
                cacheEntryCopy.VirtualizationSpan_EndExclusiveIndex = viewModel.Virtualization.VirtualizationSpanList.Count;

                componentData.LineIndexCache.Map[cacheEntryCopy.LineIndex] = cacheEntryCopy;
                virtualizedLineList[viewModel.Virtualization.Count++] = new TextEditorVirtualizationLine(
                    cacheEntryCopy.LineIndex,
                    cacheEntryCopy.Position_StartInclusiveIndex,
                    cacheEntryCopy.Position_EndExclusiveIndex,
                    cacheEntryCopy.VirtualizationSpan_StartInclusiveIndex,
                    cacheEntryCopy.VirtualizationSpan_EndExclusiveIndex,
                    cacheEntryCopy.LeftCssValue,
                    cacheEntryCopy.TopCssValue,
                    cacheEntryCopy.GutterCssStyle,
                    cacheEntryCopy.LineCssStyle,
                    cacheEntryCopy.LineNumberString);
                continue;
            }

            // "inline" of 'TextEditorModel.GetLineInformation(...)'. This isn't a 1 to 1 inline, it avoids some redundant bounds checking.
            var lineEndLower = lineIndex == 0
                ? new(0, 0, Walk.TextEditor.RazorLib.Lines.Models.LineEndKind.StartOfFile)
                : modelModifier.LineEndList[lineIndex - 1];
            var lineEndUpper = modelModifier.LineEndList[lineIndex];
            var lineInformation = new Walk.TextEditor.RazorLib.Lines.Models.LineInformation(
                lineIndex,
                lineEndLower.Position_EndExclusiveIndex,
                lineEndUpper.Position_EndExclusiveIndex,
                lineEndLower,
                lineEndUpper);

            // TODO: Was this code using length including line ending or excluding? (2024-12-29)
            var lineLength = lineInformation.Position_EndExclusiveIndex - lineInformation.Position_StartInclusiveIndex;

            // Don't bother with the extra width due to tabs until the very end.
            // It is thought to be too costly on average to get the tab count for the line in order to take less text overall
            // than to just take the estimated amount of characters.

            var widthInPixels = (int)Math.Ceiling(lineLength * viewModel.PersistentState.CharAndLineMeasurements.CharacterWidth);

            if (widthInPixels > minLineWidthToTriggerVirtualizationExclusive)
            {
                var localHorizontalStartingIndex = horizontalStartingIndex;
                var localHorizontalTake = horizontalTake;

                // Tab key adjustments
                var firstInlineUiOnLineIndex = -1;
                var foundLine = false;
                var tabCharPositionIndexListCount = modelModifier.TabCharPositionIndexList.Count;

                // Move the horizontal starting index based on the extra character width from 'tab' characters.
                for (int i = 0; i < tabCharPositionIndexListCount; i++)
                {
                    var tabCharPositionIndex = modelModifier.TabCharPositionIndexList[i];
                    var tabKeyColumnIndex = tabCharPositionIndex - lineInformation.Position_StartInclusiveIndex;

                    if (!foundLine)
                    {
                        if (tabCharPositionIndex >= lineInformation.Position_StartInclusiveIndex)
                        {
                            firstInlineUiOnLineIndex = i;
                            foundLine = true;
                        }
                    }

                    if (foundLine)
                    {
                        if (tabKeyColumnIndex >= localHorizontalStartingIndex + localHorizontalTake)
                            break;

                        localHorizontalStartingIndex -= extraWidthPerTabKey;
                    }
                }

                if (localHorizontalStartingIndex + localHorizontalTake > lineLength)
                    localHorizontalTake = lineLength - localHorizontalStartingIndex;

                localHorizontalStartingIndex = Math.Max(0, localHorizontalStartingIndex);
                localHorizontalTake = Math.Max(0, localHorizontalTake);

                var foundSplit = false;
                var unrenderedTabCount = 0;
                var resultTabCount = 0;

                // Count the 'tab' characters that preceed the text to display so that the 'left' can be modified by the extra width.
                // Count the 'tab' characters that are among the text to display so that the 'width' can be modified by the extra width.
                if (firstInlineUiOnLineIndex != -1)
                {
                    for (int i = firstInlineUiOnLineIndex; i < tabCharPositionIndexListCount; i++)
                    {
                        var tabCharPositionIndex = modelModifier.TabCharPositionIndexList[i];
                        var tabKeyColumnIndex = tabCharPositionIndex - lineInformation.Position_StartInclusiveIndex;

                        if (tabKeyColumnIndex >= localHorizontalStartingIndex + localHorizontalTake)
                            break;

                        if (!foundSplit)
                        {
                            if (tabKeyColumnIndex < localHorizontalStartingIndex)
                                unrenderedTabCount++;
                            else
                                foundSplit = true;
                        }

                        if (foundSplit)
                            resultTabCount++;
                    }
                }

                widthInPixels = (int)Math.Ceiling(((localHorizontalTake - localHorizontalStartingIndex) + (extraWidthPerTabKey * resultTabCount)) *
                    viewModel.PersistentState.CharAndLineMeasurements.CharacterWidth);

                double leftInPixels = localHorizontalStartingIndex *
                    viewModel.PersistentState.CharAndLineMeasurements.CharacterWidth;

                // Adjust the unrendered for tab key width
                leftInPixels += (extraWidthPerTabKey *
                    unrenderedTabCount *
                    viewModel.PersistentState.CharAndLineMeasurements.CharacterWidth);

                leftInPixels = Math.Max(0, leftInPixels);

                var topInPixels = lineIndex * viewModel.PersistentState.CharAndLineMeasurements.LineHeight;
                position_StartInclusiveIndex = lineInformation.Position_StartInclusiveIndex + localHorizontalStartingIndex;

                position_EndExclusiveIndex = position_StartInclusiveIndex + localHorizontalTake;
                if (position_EndExclusiveIndex > lineInformation.UpperLineEnd.Position_StartInclusiveIndex)
                    position_EndExclusiveIndex = lineInformation.UpperLineEnd.Position_StartInclusiveIndex;

                if (leftInPixels == 0)
                {
                    leftCssValue = viewModel.PersistentState.GetGutterWidthCssValue();
                }
                else
                {
                    // Although 'GutterWidth' is an int, the 'leftInPixels' is a double,
                    // so 'System.Globalization.CultureInfo.InvariantCulture' is necessary
                    // to avoid invalid CSS due to periods becoming commas for decimal points.
                    leftCssValue = (viewModel.PersistentState.GutterWidth + leftInPixels).ToString(System.Globalization.CultureInfo.InvariantCulture);
                }

                topCssValue = (topInPixels).ToString();
            }
            else
            {
                var foundLine = false;
                var resultTabCount = 0;

                // Count the tabs that are among the rendered content.
                foreach (var tabCharPositionIndex in modelModifier.TabCharPositionIndexList)
                {
                    if (!foundLine)
                    {
                        if (tabCharPositionIndex >= lineInformation.Position_StartInclusiveIndex)
                            foundLine = true;
                    }

                    if (foundLine)
                    {
                        if (tabCharPositionIndex >= lineInformation.LastValidColumnIndex)
                            break;

                        resultTabCount++;
                    }
                }

                widthInPixels += (int)Math.Ceiling((extraWidthPerTabKey * resultTabCount) *
                    viewModel.PersistentState.CharAndLineMeasurements.CharacterWidth);

                position_StartInclusiveIndex = lineInformation.Position_StartInclusiveIndex;
                position_EndExclusiveIndex = lineInformation.UpperLineEnd.Position_StartInclusiveIndex;

                leftCssValue = viewModel.PersistentState.GetGutterWidthCssValue();
                topCssValue = ((lineIndex * viewModel.PersistentState.CharAndLineMeasurements.LineHeight)).ToString();
            }

            componentData.LineIndexCache.UsedKeyHashSet.Add(lineIndex);

            if (position_EndExclusiveIndex - position_StartInclusiveIndex <= 0)
            {
                virtualizationSpan_StartInclusiveIndex = viewModel.Virtualization.VirtualizationSpanList.Count;
                virtualizationSpan_EndExclusiveIndex = viewModel.Virtualization.VirtualizationSpanList.Count;
            }
            else
            {
                virtualizationSpan_StartInclusiveIndex = viewModel.Virtualization.VirtualizationSpanList.Count;

                var richCharacterSpan = new Span<RichCharacter>(
                    modelModifier.RichCharacterList,
                    position_StartInclusiveIndex,
                    position_EndExclusiveIndex - position_StartInclusiveIndex);

                var currentDecorationByte = richCharacterSpan[0].DecorationByte;

                foreach (var richCharacter in richCharacterSpan)
                {
                    if (currentDecorationByte == richCharacter.DecorationByte)
                    {
                        // AppendTextEscaped(textEditorService.__StringBuilder, richCharacter, tabKeyOutput, spaceKeyOutput);
                        switch (richCharacter.Value)
                        {
                            case '\t':
                                __StringBuilder.Append(tabKeyOutput);
                                break;
                            case ' ':
                                __StringBuilder.Append(spaceKeyOutput);
                                break;
                            case '\r':
                                break;
                            case '\n':
                                break;
                            case '<':
                                __StringBuilder.Append("&lt;");
                                break;
                            case '>':
                                __StringBuilder.Append("&gt;");
                                break;
                            case '"':
                                __StringBuilder.Append("&quot;");
                                break;
                            case '\'':
                                __StringBuilder.Append("&#39;");
                                break;
                            case '&':
                                __StringBuilder.Append("&amp;");
                                break;
                            default:
                                __StringBuilder.Append(richCharacter.Value);
                                break;
                        }
                        // END OF INLINING AppendTextEscaped
                    }
                    else
                    {
                        viewModel.Virtualization.VirtualizationSpanList.Add(new TextEditorVirtualizationSpan(
                            cssClass: modelModifier.PersistentState.DecorationMapper.Map(currentDecorationByte),
                            text: __StringBuilder.ToString()));
                        __StringBuilder.Clear();

                        // AppendTextEscaped(textEditorService.__StringBuilder, richCharacter, tabKeyOutput, spaceKeyOutput);
                        switch (richCharacter.Value)
                        {
                            case '\t':
                                __StringBuilder.Append(tabKeyOutput);
                                break;
                            case ' ':
                                __StringBuilder.Append(spaceKeyOutput);
                                break;
                            case '\r':
                                break;
                            case '\n':
                                break;
                            case '<':
                                __StringBuilder.Append("&lt;");
                                break;
                            case '>':
                                __StringBuilder.Append("&gt;");
                                break;
                            case '"':
                                __StringBuilder.Append("&quot;");
                                break;
                            case '\'':
                                __StringBuilder.Append("&#39;");
                                break;
                            case '&':
                                __StringBuilder.Append("&amp;");
                                break;
                            default:
                                __StringBuilder.Append(richCharacter.Value);
                                break;
                        }
                        // END OF INLINING AppendTextEscaped

                        currentDecorationByte = richCharacter.DecorationByte;
                    }
                }

                /* Final grouping of contiguous characters */
                viewModel.Virtualization.VirtualizationSpanList.Add(new TextEditorVirtualizationSpan(
                    cssClass: modelModifier.PersistentState.DecorationMapper.Map(currentDecorationByte),
                    text: __StringBuilder.ToString()));
                __StringBuilder.Clear();

                virtualizationSpan_EndExclusiveIndex = viewModel.Virtualization.VirtualizationSpanList.Count;
            }

            virtualizedLineList[viewModel.Virtualization.Count++] = new TextEditorVirtualizationLine(
                lineIndex,
                position_StartInclusiveIndex,
                position_EndExclusiveIndex,
                virtualizationSpan_StartInclusiveIndex,
                virtualizationSpan_EndExclusiveIndex,
                leftCssValue,
                topCssValue,
                viewModel.Virtualization.GetGutterStyleCss(topCssValue),
                viewModel.Virtualization.RowSection_GetRowStyleCss(topCssValue, leftCssValue),
                (lineIndex + 1).ToString());

            if (componentData.LineIndexCache.Map.ContainsKey(lineIndex))
            {
                componentData.LineIndexCache.Map[lineIndex] = new TextEditorLineIndexCacheEntry(
                    topCssValue,
                    leftCssValue,
                    lineNumberString: virtualizedLineList[viewModel.Virtualization.Count - 1].LineNumberString,
                    lineIndex,
                    position_StartInclusiveIndex,
                    position_EndExclusiveIndex,
                    virtualizationSpan_StartInclusiveIndex,
                    virtualizationSpan_EndExclusiveIndex,
                    virtualizedLineList[viewModel.Virtualization.Count - 1].GutterCssStyle,
                    virtualizedLineList[viewModel.Virtualization.Count - 1].LineCssStyle);
            }
            else
            {
                componentData.LineIndexCache.ExistsKeyList.Add(lineIndex);
                componentData.LineIndexCache.Map.Add(lineIndex, new TextEditorLineIndexCacheEntry(
                    topCssValue,
                    leftCssValue,
                    lineNumberString: virtualizedLineList[viewModel.Virtualization.Count - 1].LineNumberString,
                    lineIndex,
                    position_StartInclusiveIndex,
                    position_EndExclusiveIndex,
                    virtualizationSpan_StartInclusiveIndex,
                    virtualizationSpan_EndExclusiveIndex,
                    virtualizedLineList[viewModel.Virtualization.Count - 1].GutterCssStyle,
                    virtualizedLineList[viewModel.Virtualization.Count - 1].LineCssStyle));
            }

            __StringBuilder.Clear();
        }

        viewModel.Virtualization.CreateUi();

        componentData.LineIndexCache.ModifiedLineIndexList.Clear();

        componentData.LineIndexCache.ViewModelKeyMarker = viewModel.PersistentState.ViewModelKey;
        componentData.LineIndexCache.ScrollLeftMarker = viewModel.PersistentState.ScrollLeft;
        componentData.LineIndexCache.VirtualizationSpanList = viewModel.Virtualization.VirtualizationSpanList;

        for (var i = componentData.LineIndexCache.ExistsKeyList.Count - 1; i >= 0; i--)
        {
            if (!componentData.LineIndexCache.UsedKeyHashSet.Contains(componentData.LineIndexCache.ExistsKeyList[i]))
            {
                componentData.LineIndexCache.Map.Remove(componentData.LineIndexCache.ExistsKeyList[i]);
                componentData.LineIndexCache.ExistsKeyList.RemoveAt(i);
            }
        }

        componentData.Virtualization = viewModel.Virtualization;
    }

    private static int ViewModel_CountDigits(int argumentNumber)
    {
        var digitCount = 1;
        var runningNumber = argumentNumber;

        while ((runningNumber /= 10) > 0)
        {
            digitCount++;
        }

        return digitCount;
    }

    private int ViewModel_GetGutterWidthInPixels(TextEditorModel model, TextEditorViewModel viewModel, TextEditorComponentData componentData)
    {
        if (!componentData.ViewModelDisplayOptions.IncludeGutterComponent)
            return 0;

        var mostDigitsInARowLineNumber = ViewModel_CountDigits(model!.LineCount);

        var gutterWidthInPixels = mostDigitsInARowLineNumber *
            viewModel!.PersistentState.CharAndLineMeasurements.CharacterWidth;

        gutterWidthInPixels += TextEditorModel.GUTTER_PADDING_LEFT_IN_PIXELS + TextEditorModel.GUTTER_PADDING_RIGHT_IN_PIXELS;

        return (int)Math.Ceiling(gutterWidthInPixels);
    }

    /// <summary>
    /// Inlining this instead of invoking the function definition just to see what happens.
    /// </summary>
    /*private void ViewModel_AppendTextEscaped(
        StringBuilder spanBuilder,
        RichCharacter richCharacter,
        string tabKeyOutput,
        string spaceKeyOutput)
    {
        switch (richCharacter.Value)
        {
            case '\t':
                spanBuilder.Append(tabKeyOutput);
                break;
            case ' ':
                spanBuilder.Append(spaceKeyOutput);
                break;
            case '\r':
                break;
            case '\n':
                break;
            case '<':
                spanBuilder.Append("&lt;");
                break;
            case '>':
                spanBuilder.Append("&gt;");
                break;
            case '"':
                spanBuilder.Append("&quot;");
                break;
            case '\'':
                spanBuilder.Append("&#39;");
                break;
            case '&':
                spanBuilder.Append("&amp;");
                break;
            default:
                spanBuilder.Append(richCharacter.Value);
                break;
        }
    }*/

    public async ValueTask ViewModel_RemeasureAsync(
        TextEditorEditContext editContext,
        TextEditorViewModel viewModel)
    {
        var options = Options_GetOptions();

        var componentData = viewModel.PersistentState.ComponentData;
        if (componentData is null)
            return;

        var textEditorMeasurements = await ViewModel_GetTextEditorMeasurementsAsync(componentData.RowSectionElementId)
            .ConfigureAwait(false);

        viewModel.PersistentState.CharAndLineMeasurements = options.CharAndLineMeasurements;
        viewModel.PersistentState.TextEditorDimensions = textEditorMeasurements;
    }

    public void ViewModel_ForceRender(
        TextEditorEditContext editContext,
        TextEditorViewModel viewModel)
    {
        // Getting the ViewModel from the 'editContext' triggers a re-render
        //
        // A lot code is being changed and one result is this method now reads like non-sense,
        // (or more non-sense than it previously did)
        // Because we get a viewModel passed in to this method as an argument.
        // So this seems quite silly.
        _ = editContext.GetViewModelModifier(viewModel.PersistentState.ViewModelKey);
    }
    #endregion

    #region DELETE_METHODS
    public void ViewModel_Dispose(TextEditorEditContext editContext, Key<TextEditorViewModel> viewModelKey)
    {
        DisposeViewModel(editContext, viewModelKey);
    }
    #endregion
}
