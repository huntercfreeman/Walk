using System.Diagnostics;
using System.Text;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Keyboards.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.TextEditor.RazorLib.Keymaps.Models;
using Walk.TextEditor.RazorLib.Characters.Models;
using Walk.TextEditor.RazorLib.Cursors.Models;
using Walk.TextEditor.RazorLib.Exceptions;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;
using Walk.TextEditor.RazorLib.Decorations.Models;

namespace Walk.TextEditor.RazorLib.TextEditors.Models;

public sealed class TextEditorViewModelApi
{
    private readonly TextEditorService _textEditorService;
    private readonly BackgroundTaskService _backgroundTaskService;
    private readonly IDialogService _dialogService;
    private readonly IPanelService _panelService;

    private readonly CommonBackgroundTaskApi _commonBackgroundTaskApi;
    
    private readonly CreateCacheEachSharedParameters _createCacheEachSharedParameters = new();

    public TextEditorViewModelApi(
        TextEditorService textEditorService,
        BackgroundTaskService backgroundTaskService,
        CommonBackgroundTaskApi commonBackgroundTaskApi,
        IDialogService dialogService,
        IPanelService panelService)
    {
        _textEditorService = textEditorService;
        _backgroundTaskService = backgroundTaskService;
        _commonBackgroundTaskApi = commonBackgroundTaskApi;
        _dialogService = dialogService;
        _panelService = panelService;
    }
    
    private Task _cursorShouldBlinkTask = Task.CompletedTask;
    private CancellationTokenSource _cursorShouldBlinkCancellationTokenSource = new();
    private TimeSpan _blinkingCursorTaskDelay = TimeSpan.FromMilliseconds(1000);
    
    public bool CursorShouldBlink { get; private set; } = true;
    public event Action? CursorShouldBlinkChanged;
    
    private bool _intentStopCursorBlinking = false;
    private int _stopCursorBlinkingId = 0;
    
    /// <summary>
    /// Thread Safety: most invocations of this are from the TextEditorEditContext,...
    /// ...so a decision needs to made whether this is restricted to the TextEditorEditContext
    /// and therefore thread safe, or this isn't restricted and perhaps should have a thread safe
    /// pattern involved.
    ///
    /// Precise debounce timing: I think this implementation has an imprecise debounce delay,
    /// but that is very low importance from a triage perspective. There are more important things to work on.
    /// </summary>
    public void StopCursorBlinking()
    {
        if (CursorShouldBlink)
        {
            CursorShouldBlink = false;
            CursorShouldBlinkChanged?.Invoke();
        }
        
        var localId = ++_stopCursorBlinkingId;
        
        if (!_intentStopCursorBlinking)
        {
        	_intentStopCursorBlinking = true;
        	
            _cursorShouldBlinkTask = Task.Run(async () =>
            {
                while (true)
                {
                	var id = _stopCursorBlinkingId;
                
                    await Task
                        .Delay(_blinkingCursorTaskDelay)
                        .ConfigureAwait(false);
                        
                    if (id == _stopCursorBlinkingId)
                    {
	                    CursorShouldBlink = true;
	                    CursorShouldBlinkChanged?.Invoke();
                    	break;
                    }
                }
                
                _intentStopCursorBlinking = false;
            });
        }
    }

    #region CREATE_METHODS
    public void Register(
    	TextEditorEditContext editContext,
        Key<TextEditorViewModel> viewModelKey,
        ResourceUri resourceUri,
        Category category)
    {
	    var viewModel = new TextEditorViewModel(
			viewModelKey,
			resourceUri,
			_textEditorService,
			_panelService,
			_dialogService,
			_commonBackgroundTaskApi,
			TextEditorVirtualizationResult.Empty,
			new TextEditorDimensions(0, 0, 0, 0),
			scrollLeft: 0,
	    	scrollTop: 0,
		    scrollWidth: 0,
		    scrollHeight: 0,
		    marginScrollHeight: 0,
			category);
			
		_textEditorService.RegisterViewModel(editContext, viewModel);
    }
    
    public void Register(TextEditorEditContext editContext, TextEditorViewModel viewModel)
    {
        _textEditorService.RegisterViewModel(editContext, viewModel);
    }
    #endregion

    #region READ_METHODS
    public TextEditorViewModel? GetOrDefault(Key<TextEditorViewModel> viewModelKey)
    {
        return _textEditorService.TextEditorState.ViewModelGetOrDefault(
            viewModelKey);
    }

    public Dictionary<Key<TextEditorViewModel>, TextEditorViewModel> GetViewModels()
    {
        return _textEditorService.TextEditorState.ViewModelGetViewModels();
    }

    public TextEditorModel? GetModelOrDefault(Key<TextEditorViewModel> viewModelKey)
    {
        var viewModel = _textEditorService.TextEditorState.ViewModelGetOrDefault(
            viewModelKey);

        if (viewModel is null)
            return null;

        return _textEditorService.ModelApi.GetOrDefault(viewModel.PersistentState.ResourceUri);
    }

    public string? GetAllText(Key<TextEditorViewModel> viewModelKey)
    {
        var textEditorModel = GetModelOrDefault(viewModelKey);

        return textEditorModel is null
            ? null
            : _textEditorService.ModelApi.GetAllText(textEditorModel.PersistentState.ResourceUri);
    }

    public async ValueTask<TextEditorDimensions> GetTextEditorMeasurementsAsync(string elementId)
    {
        return await _textEditorService.JsRuntimeTextEditorApi
            .GetTextEditorMeasurementsInPixelsById(elementId)
            .ConfigureAwait(false);
    }
    #endregion

    #region UPDATE_METHODS
    public void SetScrollPositionBoth(
        TextEditorEditContext editContext,
        TextEditorViewModel viewModel,
        double scrollLeftInPixels,
        double scrollTopInPixels)
    {
    	viewModel.ScrollWasModified = true;

		viewModel.SetScrollLeft((int)Math.Floor(scrollLeftInPixels), viewModel.TextEditorDimensions);

		viewModel.SetScrollTop((int)Math.Floor(scrollTopInPixels), viewModel.TextEditorDimensions);
    }
        
    public void SetScrollPositionLeft(
        TextEditorEditContext editContext,
        TextEditorViewModel viewModel,
        double scrollLeftInPixels)
    {
    	viewModel.ScrollWasModified = true;

		viewModel.SetScrollLeft((int)Math.Floor(scrollLeftInPixels), viewModel.TextEditorDimensions);
    }
    
    public void SetScrollPositionTop(
        TextEditorEditContext editContext,
        TextEditorViewModel viewModel,
        double scrollTopInPixels)
    {
    	viewModel.ScrollWasModified = true;

		viewModel.SetScrollTop((int)Math.Floor(scrollTopInPixels), viewModel.TextEditorDimensions);
    }

    public void MutateScrollVerticalPosition(
        TextEditorEditContext editContext,
        TextEditorViewModel viewModel,
        double pixels)
    {
        viewModel.ScrollWasModified = true;

        viewModel.MutateScrollTop((int)Math.Ceiling(pixels), viewModel.TextEditorDimensions);
    }

    public void MutateScrollHorizontalPosition(
        TextEditorEditContext editContext,
        TextEditorViewModel viewModel,
        double pixels)
    {
        viewModel.ScrollWasModified = true;

		viewModel.MutateScrollLeft((int)Math.Ceiling(pixels), viewModel.TextEditorDimensions);
    }

	/// <summary>
	/// If a given scroll direction is already within view of the text span, do not scroll on that direction.
	///
	/// Measurements are in pixels.
	/// </summary>
    public void ScrollIntoView(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel,
        TextEditorTextSpan textSpan)
    {
        var lineInformation = modelModifier.GetLineInformationFromPositionIndex(textSpan.StartInclusiveIndex);
        var lineIndex = lineInformation.Index;
        var hiddenLineCount = 0;
        foreach (var index in viewModel.PersistentState.HiddenLineIndexHashSet)
        {
        	if (index < lineIndex)
        		hiddenLineCount++;
        }
        // Scroll start
        var targetScrollTop = (lineIndex - hiddenLineCount) * viewModel.CharAndLineMeasurements.LineHeight;
        bool lowerBoundInRange = viewModel.ScrollTop <= targetScrollTop;
        bool upperBoundInRange = targetScrollTop < (viewModel.TextEditorDimensions.Height + viewModel.ScrollTop);
        if (lowerBoundInRange && upperBoundInRange)
        {
            targetScrollTop = viewModel.ScrollTop;
        }
        else
        {
        	var startDistanceToTarget = Math.Abs(targetScrollTop - viewModel.ScrollTop);
        	var endDistanceToTarget = Math.Abs(targetScrollTop - (viewModel.TextEditorDimensions.Height + viewModel.ScrollTop));
        	
    		// Scroll end
        	if (endDistanceToTarget < startDistanceToTarget)
        	{
        		var margin = 3 * viewModel.CharAndLineMeasurements.LineHeight;
        		var maxMargin = viewModel.TextEditorDimensions.Height * .3;
        		if (margin > maxMargin)
        			margin = (int)maxMargin;
        	
        		targetScrollTop -= (viewModel.TextEditorDimensions.Height - margin);
        	}
        }
        
		var columnIndex = textSpan.StartInclusiveIndex - lineInformation.Position_StartInclusiveIndex;
		// Scroll start
        var targetScrollLeft = columnIndex * viewModel.CharAndLineMeasurements.CharacterWidth;
        lowerBoundInRange = viewModel.ScrollLeft <= targetScrollLeft;
        upperBoundInRange = targetScrollLeft < (viewModel.TextEditorDimensions.Width + viewModel.ScrollLeft);
        if (lowerBoundInRange && upperBoundInRange)
        {
        	targetScrollLeft = viewModel.ScrollLeft;
        }
        else
        {
        	var startDistanceToTarget = targetScrollLeft - viewModel.ScrollLeft;
        	var endDistanceToTarget = targetScrollLeft - (viewModel.TextEditorDimensions.Width + viewModel.ScrollLeft);
        	
        	// Scroll end
        	if (endDistanceToTarget < startDistanceToTarget)
        	{
        		var margin = 9 * viewModel.CharAndLineMeasurements.CharacterWidth;
        		var maxMargin = viewModel.TextEditorDimensions.Width * .3;
        		if (margin > maxMargin)
        			margin = maxMargin;
        	
        		targetScrollLeft -= (viewModel.TextEditorDimensions.Width - margin);
        	}
        }
            
        if (targetScrollTop == -1 && targetScrollLeft == -1)
        	return;
        else if (targetScrollTop != -1 && targetScrollLeft != -1)
        	SetScrollPositionBoth(editContext, viewModel, targetScrollLeft, targetScrollTop);
        else if (targetScrollTop != -1)
        	SetScrollPositionTop(editContext, viewModel, targetScrollTop);
    	else
        	SetScrollPositionLeft(editContext, viewModel, targetScrollLeft);
    }

    public ValueTask FocusPrimaryCursorAsync(string primaryCursorContentId)
    {
        return _commonBackgroundTaskApi.JsRuntimeCommonApi
            .FocusHtmlElementById(primaryCursorContentId, preventScroll: true);
    }

    public void MoveCursor(
        string? key,
        string? code,
        bool ctrlKey,
        bool shiftKey,
        bool altKey,
		TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel)
    {
        MoveCursorUnsafe(
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

    public void MoveCursorUnsafe(
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
            case KeyboardKeyFacts.MovementKeys.ARROW_LEFT:
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
            case KeyboardKeyFacts.MovementKeys.ARROW_DOWN:
                if (viewModel.LineIndex < modelModifier.LineCount - 1)
                {
                    viewModel.LineIndex++;

                    lengthOfLine = modelModifier.GetLineLength(viewModel.LineIndex);

                    viewModel.ColumnIndex = lengthOfLine < viewModel.PreferredColumnIndex
                        ? lengthOfLine
                        : viewModel.PreferredColumnIndex;
                }

                break;
            case KeyboardKeyFacts.MovementKeys.ARROW_UP:
                if (viewModel.LineIndex > 0)
                {
                    viewModel.LineIndex--;

                    lengthOfLine = modelModifier.GetLineLength(viewModel.LineIndex);

                    viewModel.ColumnIndex = lengthOfLine < viewModel.PreferredColumnIndex
                        ? lengthOfLine
                        : viewModel.PreferredColumnIndex;
                }

                break;
            case KeyboardKeyFacts.MovementKeys.ARROW_RIGHT:
            	if (viewModel.PersistentState.VirtualAssociativityKind == VirtualAssociativityKind.Left)
            	{
            		viewModel.PersistentState.VirtualAssociativityKind = VirtualAssociativityKind.Right;
            	}
                else if (TextEditorSelectionHelper.HasSelectedText(viewModel) && !shiftKey)
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
            case KeyboardKeyFacts.MovementKeys.HOME:
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
            case KeyboardKeyFacts.MovementKeys.END:
                if (ctrlKey)
                    viewModel.LineIndex = modelModifier.LineCount - 1;

                lengthOfLine = modelModifier.GetLineLength(viewModel.LineIndex);

                viewModel.SetColumnIndexAndPreferred(lengthOfLine);

                break;
        }
        
        if (viewModel.PersistentState.HiddenLineIndexHashSet.Contains(viewModel.LineIndex))
        {
        	switch (key)
        	{
        		case KeyboardKeyFacts.MovementKeys.ARROW_LEFT:
        		{
        			CollapsePoint encompassingCollapsePoint = new CollapsePoint(-1, false, string.Empty, -1);;

        			foreach (var collapsePoint in viewModel.PersistentState.AllCollapsePointList)
        			{
        				var firstToHideLineIndex = collapsePoint.AppendToLineIndex + 1;
						for (var lineOffset = 0; lineOffset < collapsePoint.EndExclusiveLineIndex - collapsePoint.AppendToLineIndex - 1; lineOffset++)
						{
							if (viewModel.LineIndex == firstToHideLineIndex + lineOffset)
								encompassingCollapsePoint = collapsePoint;
						}
        			}
        			
        			if (encompassingCollapsePoint.AppendToLineIndex != -1)
        			{
        				var lineIndex = encompassingCollapsePoint.EndExclusiveLineIndex - 1;
        				var lineInformation = modelModifier.GetLineInformation(lineIndex);
        				
        				if (viewModel.ColumnIndex != lineInformation.LastValidColumnIndex)
        				{
        					for (int i = viewModel.LineIndex; i >= 0; i--)
		        			{
		        				if (!viewModel.PersistentState.HiddenLineIndexHashSet.Contains(i))
		        				{
		        					viewModel.LineIndex = i;
		        					lineInformation = modelModifier.GetLineInformation(i);
		        					viewModel.ColumnIndex = lineInformation.LastValidColumnIndex;
		        					break;
		        				}
		        			}
        				}
        			}
        		
        			break;
        		}
        		case KeyboardKeyFacts.MovementKeys.ARROW_DOWN:
        		{
        			var success = false;
        		
        			for (int i = viewModel.LineIndex + 1; i < modelModifier.LineCount; i++)
        			{
        				if (!viewModel.PersistentState.HiddenLineIndexHashSet.Contains(i))
        				{
        					success = true;
        					viewModel.LineIndex = i;
        					
        					var lineInformation = modelModifier.GetLineInformation(i);
        					
        					if (viewModel.ColumnIndex > lineInformation.LastValidColumnIndex)
        						viewModel.ColumnIndex = lineInformation.LastValidColumnIndex;
        					
        					break;
        				}
        			}
        			
        			if (!success)
        			{
        				for (int i = viewModel.LineIndex; i >= 0; i--)
	        			{
	        				if (!viewModel.PersistentState.HiddenLineIndexHashSet.Contains(i))
	        				{
	        					viewModel.LineIndex = i;
	        					
	        					var lineInformation = modelModifier.GetLineInformation(i);
	        					
	        					if (viewModel.ColumnIndex > lineInformation.LastValidColumnIndex)
	        						viewModel.ColumnIndex = lineInformation.LastValidColumnIndex;
	        					
	        					break;
	        				}
	        			}
        			}
        			
        			break;
        		}
        		case KeyboardKeyFacts.MovementKeys.ARROW_UP:
        		{
        			var success = false;
        			
        			for (int i = viewModel.LineIndex; i >= 0; i--)
        			{
        				if (!viewModel.PersistentState.HiddenLineIndexHashSet.Contains(i))
        				{
        					success = true;
        					viewModel.LineIndex = i;
        					
        					var lineInformation = modelModifier.GetLineInformation(i);
        					
        					if (viewModel.PreferredColumnIndex <= lineInformation.LastValidColumnIndex)
        						viewModel.ColumnIndex = viewModel.PreferredColumnIndex;
        					else if (viewModel.ColumnIndex > lineInformation.LastValidColumnIndex)
        						viewModel.ColumnIndex = lineInformation.LastValidColumnIndex;
        					
        					break;
        				}
        			}
        		
        			if (!success)
        			{
        				for (int i = viewModel.LineIndex + 1; i < modelModifier.LineCount; i++)
	        			{
	        				if (!viewModel.PersistentState.HiddenLineIndexHashSet.Contains(i))
	        				{
	        					viewModel.LineIndex = i;
	        					
	        					var lineInformation = modelModifier.GetLineInformation(i);
	        					
	        					if (viewModel.ColumnIndex > lineInformation.LastValidColumnIndex)
	        						viewModel.ColumnIndex = lineInformation.LastValidColumnIndex;
	        					
	        					break;
	        				}
	        			}
        			}
        			
        			break;
        		}
        		case KeyboardKeyFacts.MovementKeys.ARROW_RIGHT:
    			{
        			CollapsePoint encompassingCollapsePoint = new CollapsePoint(-1, false, string.Empty, -1);;

        			foreach (var collapsePoint in viewModel.PersistentState.AllCollapsePointList)
        			{
        				var firstToHideLineIndex = collapsePoint.AppendToLineIndex + 1;
						for (var lineOffset = 0; lineOffset < collapsePoint.EndExclusiveLineIndex - collapsePoint.AppendToLineIndex - 1; lineOffset++)
						{
							if (viewModel.LineIndex == firstToHideLineIndex + lineOffset)
								encompassingCollapsePoint = collapsePoint;
						}
        			}
        			
        			if (encompassingCollapsePoint.AppendToLineIndex != -1)
        			{
        				var lineIndex = encompassingCollapsePoint.EndExclusiveLineIndex - 1;
        			
        				var lineInformation = modelModifier.GetLineInformation(lineIndex);
						viewModel.LineIndex = lineIndex;
						viewModel.SetColumnIndexAndPreferred(lineInformation.LastValidColumnIndex);
        			}
	        			
        			break;
        		}
        		case KeyboardKeyFacts.MovementKeys.HOME:
        		case KeyboardKeyFacts.MovementKeys.END:
        		{
        			break;
        		}
        	}
        }
        
        (int lineIndex, int columnIndex) lineAndColumnIndices = (0, 0);
		var inlineUi = new InlineUi(0, InlineUiKind.None);
		
		foreach (var inlineUiTuple in viewModel.PersistentState.InlineUiList)
		{
			lineAndColumnIndices = modelModifier.GetLineAndColumnIndicesFromPositionIndex(inlineUiTuple.InlineUi.PositionIndex);
			
			if (lineAndColumnIndices.lineIndex == viewModel.LineIndex &&
				lineAndColumnIndices.columnIndex == viewModel.ColumnIndex)
			{
				inlineUi = inlineUiTuple.InlineUi;
			}
		}
		
		if (viewModel.PersistentState.VirtualAssociativityKind == VirtualAssociativityKind.None &&
			inlineUi.InlineUiKind != InlineUiKind.None)
		{
			viewModel.PersistentState.VirtualAssociativityKind = VirtualAssociativityKind.Left;
		}
		
		if (inlineUi.InlineUiKind == InlineUiKind.None)
			viewModel.PersistentState.VirtualAssociativityKind = VirtualAssociativityKind.None;

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

    public void CursorMovePageTop(
        TextEditorEditContext editContext,
        TextEditorViewModel viewModel)
    {
        CursorMovePageTopUnsafe(
	        editContext,
        	viewModel);
    }

    public void CursorMovePageTopUnsafe(
        TextEditorEditContext editContext,
        TextEditorViewModel viewModel)
    {
        if (viewModel.VirtualizationResult.Count > 0)
        {
            var firstEntry = viewModel.VirtualizationResult.EntryList[0];
            viewModel.LineIndex = firstEntry.LineIndex;
            viewModel.ColumnIndex = 0;
        }
    }

    public void CursorMovePageBottom(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel)
    {
        CursorMovePageBottomUnsafe(
        	editContext,
        	modelModifier,
        	viewModel);
    }

    public void CursorMovePageBottomUnsafe(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel)
    {
        if (viewModel.VirtualizationResult.Count > 0)
        {
            var lastEntry = viewModel.VirtualizationResult.EntryList[viewModel.VirtualizationResult.Count - 1];
            var lastEntriesLineLength = modelModifier.GetLineLength(lastEntry.LineIndex);

            viewModel.LineIndex = lastEntry.LineIndex;
            viewModel.ColumnIndex = lastEntriesLineLength;
        }
    }
    
    public void RevealCursor(
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
                0,
                modelModifier.PersistentState.ResourceUri,
                sourceText: string.Empty,
                getTextPrecalculatedResult: string.Empty);

            ScrollIntoView(
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

    public void CalculateVirtualizationResult(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
		TextEditorViewModel viewModel,
		TextEditorComponentData componentData)
    {
    	#if DEBUG
    	var startTime = Stopwatch.GetTimestamp();
    	#endif
    	
		var tabWidth = editContext.TextEditorService.OptionsApi.GetOptions().TabWidth;
		viewModel.ShouldCalculateVirtualizationResult = false;
	
		var verticalStartingIndex = viewModel.ScrollTop /
			viewModel.CharAndLineMeasurements.LineHeight;

		var verticalTake = viewModel.TextEditorDimensions.Height /
			viewModel.CharAndLineMeasurements.LineHeight;

		// Vertical Padding (render some offscreen data)
		verticalTake += 1;

		var horizontalStartingIndex = (int)Math.Floor(
			viewModel.ScrollLeft /
			viewModel.CharAndLineMeasurements.CharacterWidth);

		var horizontalTake = (int)Math.Ceiling(
			viewModel.TextEditorDimensions.Width /
			viewModel.CharAndLineMeasurements.CharacterWidth);
		
		var hiddenCount = 0;
		
		for (int i = 0; i < verticalStartingIndex; i++)
		{
			if (viewModel.PersistentState.HiddenLineIndexHashSet.Contains(i))
			{
				hiddenCount++;
				verticalStartingIndex++;
			}
		}
		
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
			viewModel.CharAndLineMeasurements.CharacterWidth);

		// Account for any tab characters on the 'MostCharactersOnASingleLineTuple'
		//
		// TODO: This code is not fully correct...
		//       ...if the longest line is 50 non-tab characters,
		//       and the second longest line is 49 tab characters,
		//       this code will erroneously take the '50' non-tab characters
		//       to be the longest line.
		{
			var lineIndex = modelModifier.MostCharactersOnASingleLineTuple.lineIndex;
			var longestLineInformation = modelModifier.GetLineInformation(lineIndex);

			var tabCountOnLongestLine = modelModifier.GetTabCountOnSameLineBeforeCursor(
				longestLineInformation.Index,
				longestLineInformation.LastValidColumnIndex);

			// 1 of the character width is already accounted for
			var extraWidthPerTabKey = tabWidth - 1;

			totalWidth += (int)Math.Ceiling(extraWidthPerTabKey *
				tabCountOnLongestLine *
				viewModel.CharAndLineMeasurements.CharacterWidth);
		}

		var totalHeight = (modelModifier.LineEndList.Count - viewModel.PersistentState.HiddenLineIndexHashSet.Count) *
			viewModel.CharAndLineMeasurements.LineHeight;

		// Add vertical margin so the user can scroll beyond the final line of content
		var percentOfMarginScrollHeightByPageUnit = 0.4;
		int marginScrollHeight = (int)Math.Ceiling(viewModel.TextEditorDimensions.Height * percentOfMarginScrollHeightByPageUnit);
		totalHeight += marginScrollHeight;
		
		var virtualizedLineList = new TextEditorVirtualizationLine[lineCountToReturn];
					
		viewModel.VirtualizationResult = new TextEditorVirtualizationResult(
			virtualizedLineList,
    		new List<TextEditorVirtualizationSpan>(),
			totalWidth: totalWidth,
	        totalHeight: totalHeight,
	        resultWidth: (int)Math.Ceiling(horizontalTake * viewModel.CharAndLineMeasurements.CharacterWidth),
	        resultHeight: verticalTake * viewModel.CharAndLineMeasurements.LineHeight,
	        left: (int)(horizontalStartingIndex * viewModel.CharAndLineMeasurements.CharacterWidth),
	        top: verticalStartingIndex * viewModel.CharAndLineMeasurements.LineHeight,
	        componentData,
	        modelModifier,
	        viewModel,
	        componentData.RenderBatchPersistentState,
	        count: 0);
		
		viewModel.ScrollWidth = totalWidth;
		viewModel.ScrollHeight = totalHeight;
		viewModel.MarginScrollHeight = marginScrollHeight;
		
		viewModel.GutterWidthInPixels = GetGutterWidthInPixels(modelModifier, viewModel, componentData);
		
		if (componentData.LineIndexCache.IsInvalid)
			componentData.LineIndexCache.Clear();
		else
			componentData.LineIndexCache.UsedKeyHashSet.Clear();
		
		var absDiffScrollLeft = Math.Abs(componentData.LineIndexCache.ScrollLeftMarker - viewModel.ScrollLeft);
		var useAll = absDiffScrollLeft < 0.01 && componentData.LineIndexCache.ViewModelKeyMarker == viewModel.PersistentState.ViewModelKey;
		
		if (_textEditorService.SeenTabWidth != _textEditorService.OptionsApi.GetTextEditorOptionsState().Options.TabWidth)
		{
			_textEditorService.SeenTabWidth = _textEditorService.OptionsApi.GetTextEditorOptionsState().Options.TabWidth;
			_textEditorService.TabKeyOutput_ShowWhitespaceTrue = new string('-', _textEditorService.SeenTabWidth - 1) + '>';
			
			var stringBuilder = new StringBuilder();
			
			for (int i = 0; i < _textEditorService.SeenTabWidth; i++)
			{
				stringBuilder.Append("&nbsp;");
			}
			_textEditorService.TabKeyOutput_ShowWhitespaceFalse = stringBuilder.ToString();
		}
		
		string tabKeyOutput;
		string spaceKeyOutput;
		
		if (_textEditorService.OptionsApi.GetTextEditorOptionsState().Options.ShowWhitespace)
		{
			tabKeyOutput = _textEditorService.TabKeyOutput_ShowWhitespaceTrue;
			spaceKeyOutput = "·";
		}
		else
		{
			tabKeyOutput = _textEditorService.TabKeyOutput_ShowWhitespaceFalse;
			spaceKeyOutput = "&nbsp;";
		}
		
		_createCacheEachSharedParameters.Model = modelModifier;
    	_createCacheEachSharedParameters.ViewModel = viewModel;
    	_createCacheEachSharedParameters.ComponentData = componentData;
    	_createCacheEachSharedParameters.TabKeyOutput = tabKeyOutput;
    	_createCacheEachSharedParameters.SpaceKeyOutput = spaceKeyOutput;
		
		_textEditorService.__StringBuilder.Clear();
		
		int linesTaken = 0;
		
		{
			// 1 of the character width is already accounted for
			var extraWidthPerTabKey = tabWidth - 1;
			
			var minLineWidthToTriggerVirtualizationExclusive = 2 * viewModel.TextEditorDimensions.Width;
				
			int lineOffset = -1;
			
			var entireSpan = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(componentData.LineIndexCache.VirtualizationSpanList);
			
			while (true)
			{
				lineOffset++;
			
				if (linesTaken >= lineCountToReturn)
					break;
				// TODO: Is this '>' or '>='?
				if (verticalStartingIndex + lineOffset >= modelModifier.LineEndList.Count)
					break;
			
				var lineIndex = verticalStartingIndex + lineOffset;

				if (viewModel.PersistentState.HiddenLineIndexHashSet.Contains(lineIndex))
				{
					hiddenCount++;
					continue;
				}
				
				var useCache = _createCacheEachSharedParameters.ComponentData.LineIndexCache.Map.ContainsKey(lineIndex) &&
					          !_createCacheEachSharedParameters.ComponentData.LineIndexCache.ModifiedLineIndexList.Contains(lineIndex);
				
				if (useCache)
				{
        			var previous = _createCacheEachSharedParameters.ComponentData.LineIndexCache.Map[lineIndex];
        			
        			var virtualizationEntry = _createCacheEachSharedParameters.ComponentData.LineIndexCache.Map[lineIndex];
        			virtualizationEntry.VirtualizationSpan_StartInclusiveIndex = _createCacheEachSharedParameters.ViewModel.VirtualizationResult.VirtualizationSpanList.Count;
		
		            _createCacheEachSharedParameters.ComponentData.LineIndexCache.UsedKeyHashSet.Add(virtualizationEntry.LineIndex);
        			
        			var smallSpan = entireSpan.Slice(
        			    previous.VirtualizationSpan_StartInclusiveIndex,
        			    previous.VirtualizationSpan_EndExclusiveIndex - previous.VirtualizationSpan_StartInclusiveIndex);
        			
        			foreach (var virtualizedSpan in smallSpan)
        			{
        				_createCacheEachSharedParameters.ViewModel.VirtualizationResult.VirtualizationSpanList.Add(virtualizedSpan);
        			}
        			
        			// WARNING CODE DUPLICATION
        			virtualizationEntry.VirtualizationSpan_EndExclusiveIndex = _createCacheEachSharedParameters.ViewModel.VirtualizationResult.VirtualizationSpanList.Count;
        			virtualizedLineList[linesTaken++] = virtualizationEntry;
        			
        			_createCacheEachSharedParameters.ComponentData.LineIndexCache.Map[virtualizationEntry.LineIndex] = virtualizationEntry;
				    continue;
				}
				
				var lineInformation = modelModifier.GetLineInformation(lineIndex);
							    
				var line_PositionStartInclusiveIndex = lineInformation.Position_StartInclusiveIndex;
				var lineEnd = modelModifier.LineEndList[lineIndex];
				
				// TODO: Was this code using length including line ending or excluding? (2024-12-29)
				var lineLength = lineInformation.Position_EndExclusiveIndex - lineInformation.Position_StartInclusiveIndex;
				
				// Don't bother with the extra width due to tabs until the very end.
				// It is thought to be too costly on average to get the tab count for the line in order to take less text overall
				// than to just take the estimated amount of characters.
				
				var widthInPixels = (int)Math.Ceiling(lineLength * viewModel.CharAndLineMeasurements.CharacterWidth);

				if (widthInPixels > minLineWidthToTriggerVirtualizationExclusive)
				{
					var localHorizontalStartingIndex = horizontalStartingIndex;
					var localHorizontalTake = horizontalTake;
					
					// Tab key adjustments
					var line = modelModifier.GetLineInformation(lineIndex);
					var firstInlineUiOnLineIndex = -1;
					var foundLine = false;
					var tabCharPositionIndexListCount = modelModifier.TabCharPositionIndexList.Count;
			
					// Move the horizontal starting index based on the extra character width from 'tab' characters.
					for (int i = 0; i < tabCharPositionIndexListCount; i++)
					{
						var tabCharPositionIndex = modelModifier.TabCharPositionIndexList[i];
						var tabKeyColumnIndex = tabCharPositionIndex - line.Position_StartInclusiveIndex;
					
						if (!foundLine)
						{
							if (tabCharPositionIndex >= line.Position_StartInclusiveIndex)
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
							var tabKeyColumnIndex = tabCharPositionIndex - line.Position_StartInclusiveIndex;
							
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
						viewModel.CharAndLineMeasurements.CharacterWidth);

					double leftInPixels = localHorizontalStartingIndex *
						viewModel.CharAndLineMeasurements.CharacterWidth;

					// Adjust the unrendered for tab key width
					leftInPixels += (extraWidthPerTabKey *
						unrenderedTabCount *
						viewModel.CharAndLineMeasurements.CharacterWidth);

					leftInPixels = Math.Max(0, leftInPixels);

					var topInPixels = lineIndex * viewModel.CharAndLineMeasurements.LineHeight;
					var positionStartInclusiveIndex = line_PositionStartInclusiveIndex + localHorizontalStartingIndex;
					
					var positionEndExclusiveIndex = positionStartInclusiveIndex + localHorizontalTake;
					if (positionEndExclusiveIndex > lineInformation.UpperLineEnd.Position_StartInclusiveIndex)
						positionEndExclusiveIndex = lineInformation.UpperLineEnd.Position_StartInclusiveIndex;
					
					virtualizedLineList[linesTaken++] = new TextEditorVirtualizationLine(
						lineIndex,
						position_StartInclusiveIndex: positionStartInclusiveIndex,
						position_EndExclusiveIndex: positionEndExclusiveIndex,
						virtualizationSpan_StartInclusiveIndex: 0,
						virtualizationSpan_EndExclusiveIndex: 0,
						widthInPixels,
						viewModel.CharAndLineMeasurements.LineHeight,
						viewModel.GutterWidthInPixels + leftInPixels,
						topInPixels - (viewModel.CharAndLineMeasurements.LineHeight * hiddenCount));
					
					CreateCacheEach(
						linesTaken - 1,
						ref entireSpan);
				}
				else
				{
					var line = modelModifier.GetLineInformation(lineIndex);
			
					var foundLine = false;
					var resultTabCount = 0;
			
					// Count the tabs that are among the rendered content.
					foreach (var tabCharPositionIndex in modelModifier.TabCharPositionIndexList)
					{
						if (!foundLine)
						{
							if (tabCharPositionIndex >= line.Position_StartInclusiveIndex)
								foundLine = true;
						}
						
						if (foundLine)
						{
							if (tabCharPositionIndex >= line.LastValidColumnIndex)
								break;
						
							resultTabCount++;
						}
					}
					
					widthInPixels += (int)Math.Ceiling((extraWidthPerTabKey * resultTabCount) *
						viewModel.CharAndLineMeasurements.CharacterWidth);
				
					virtualizedLineList[linesTaken++]= new TextEditorVirtualizationLine(
						lineIndex,
						position_StartInclusiveIndex: lineInformation.Position_StartInclusiveIndex,
						position_EndExclusiveIndex: lineInformation.UpperLineEnd.Position_StartInclusiveIndex,
						virtualizationSpan_StartInclusiveIndex: 0,
						virtualizationSpan_EndExclusiveIndex: 0,
						widthInPixels,
						viewModel.CharAndLineMeasurements.LineHeight,
						leftInPixels: viewModel.GutterWidthInPixels,
						topInPixels: (lineIndex * viewModel.CharAndLineMeasurements.LineHeight) - (viewModel.CharAndLineMeasurements.LineHeight * hiddenCount));
					
					CreateCacheEach(
						linesTaken - 1,
						ref entireSpan);
				}
			}
		}
		
		viewModel.VirtualizationResult.Count = linesTaken;

		componentData.LineIndexCache.ModifiedLineIndexList.Clear();
	
		componentData.LineIndexCache.ViewModelKeyMarker = viewModel.PersistentState.ViewModelKey;
		componentData.LineIndexCache.VirtualizationSpanList = viewModel.VirtualizationResult.VirtualizationSpanList;
		componentData.LineIndexCache.ScrollLeftMarker = viewModel.ScrollLeft;
		
		for (var i = componentData.LineIndexCache.ExistsKeyList.Count - 1; i >= 0; i--)
		{
			if (!componentData.LineIndexCache.UsedKeyHashSet.Contains(componentData.LineIndexCache.ExistsKeyList[i]))
			{
				componentData.LineIndexCache.Map.Remove(componentData.LineIndexCache.ExistsKeyList[i]);
				componentData.LineIndexCache.ExistsKeyList.RemoveAt(i);
			}
		}
		
		#if DEBUG
		WalkDebugSomething.SetTextEditorViewModelApi(Stopwatch.GetElapsedTime(startTime));
		#endif
    }
    
    private static int CountDigits(int argumentNumber)
    {
    	var digitCount = 1;
    	var runningNumber = argumentNumber;
    	
    	while ((runningNumber /= 10) > 0)
    	{
    		digitCount++;
    	}
    	
    	return digitCount;
    }

    private int GetGutterWidthInPixels(TextEditorModel model, TextEditorViewModel viewModel, TextEditorComponentData componentData)
    {
        if (!componentData.ViewModelDisplayOptions.IncludeGutterComponent)
            return 0;

        var mostDigitsInARowLineNumber = CountDigits(model!.LineCount);

        var gutterWidthInPixels = mostDigitsInARowLineNumber *
            viewModel!.CharAndLineMeasurements.CharacterWidth;

        gutterWidthInPixels += TextEditorModel.GUTTER_PADDING_LEFT_IN_PIXELS + TextEditorModel.GUTTER_PADDING_RIGHT_IN_PIXELS;

        return (int)Math.Ceiling(gutterWidthInPixels);
    }
    
    private void LineIndexCache_Create(TextEditorVirtualizationResult virtualizationResult)
    {
    	if (virtualizationResult.GutterWidth != virtualizationResult.ViewModel.GutterWidthInPixels)
    	{
    		virtualizationResult.GutterWidth = virtualizationResult.ViewModel.GutterWidthInPixels;
    		virtualizationResult.ComponentData.LineIndexCache.Clear();
    		
    		var widthInPixelsInvariantCulture = virtualizationResult.GutterWidth.ToString();
    		
    		_textEditorService.__StringBuilder.Clear();
    		_textEditorService.__StringBuilder.Append("width: ");
    		_textEditorService.__StringBuilder.Append(widthInPixelsInvariantCulture);
    		_textEditorService.__StringBuilder.Append("px;");
    		virtualizationResult.Gutter_WidthCssStyle = _textEditorService.__StringBuilder.ToString();
    		
    		_textEditorService.__StringBuilder.Clear();
    		_textEditorService.__StringBuilder.Append(virtualizationResult.LineHeightStyleCssString);
	        _textEditorService.__StringBuilder.Append(virtualizationResult.Gutter_WidthCssStyle);
	        _textEditorService.__StringBuilder.Append(virtualizationResult.ComponentData.Gutter_PaddingCssStyle);
    		virtualizationResult.Gutter_HeightWidthPaddingCssStyle = _textEditorService.__StringBuilder.ToString();
    		
    		_textEditorService.__StringBuilder.Clear();
    		_textEditorService.__StringBuilder.Append("width: calc(100% - ");
	        _textEditorService.__StringBuilder.Append(widthInPixelsInvariantCulture);
	        _textEditorService.__StringBuilder.Append("px); left: ");
	        _textEditorService.__StringBuilder.Append(widthInPixelsInvariantCulture);
	        _textEditorService.__StringBuilder.Append("px;");
    		virtualizationResult.BodyStyle = _textEditorService.__StringBuilder.ToString();

    		virtualizationResult.ViewModel.PersistentState.PostScrollAndRemeasure();
    		
    		HORIZONTAL_GetScrollbarHorizontalStyleCss(virtualizationResult);
    		HORIZONTAL_GetSliderHorizontalStyleCss(virtualizationResult);
    		
    		_textEditorService.__StringBuilder.Clear();
    		_textEditorService.__StringBuilder.Append("left: ");
    		_textEditorService.__StringBuilder.Append(widthInPixelsInvariantCulture);
    		_textEditorService.__StringBuilder.Append("px;");
    		virtualizationResult.ScrollbarSection_LeftCssStyle = _textEditorService.__StringBuilder.ToString();
    	}
    	else if (virtualizationResult.ScrollLeft != virtualizationResult.ViewModel.ScrollLeft)
    	{
    		virtualizationResult.ScrollLeft = virtualizationResult.ViewModel.ScrollLeft;
    		virtualizationResult.ComponentData.LineIndexCache.Clear();
    	}
    
    	var hiddenLineCount = 0;
    	var checkHiddenLineIndex = 0;
    	var handledCursor = false;
    	var isHandlingCursor = false;
    	
    	for (int i = 0; i < virtualizationResult.ViewModel.VirtualizationResult.Count; i++)
    	{
    		int lineIndex = virtualizationResult.ViewModel.VirtualizationResult.EntryList[i].LineIndex;
    		
    		if (lineIndex >= virtualizationResult.ViewModel.LineIndex && !handledCursor)
    		{
    		 	isHandlingCursor = true;
    		 	lineIndex = virtualizationResult.ViewModel.LineIndex;
			}
    		
    		for (; checkHiddenLineIndex < lineIndex; checkHiddenLineIndex++)
            {
            	if (virtualizationResult.ViewModel.PersistentState.HiddenLineIndexHashSet.Contains(checkHiddenLineIndex))
            		hiddenLineCount++;
            }
            
            virtualizationResult.ComponentData.LineIndexCache.UsedKeyHashSet.Add(lineIndex);
            
            if (virtualizationResult.ComponentData.LineIndexCache.Map.ContainsKey(lineIndex))
	    	{
	    		var cacheEntry = virtualizationResult.ComponentData.LineIndexCache.Map[lineIndex];
	    		
	    		if (hiddenLineCount != cacheEntry.HiddenLineCount)
	            {
	            	cacheEntry.TopCssValue = ((lineIndex - hiddenLineCount) * virtualizationResult.ViewModel.CharAndLineMeasurements.LineHeight)
	            		.ToString();
	            		
	            	cacheEntry.HiddenLineCount = hiddenLineCount;
	            	
	            	virtualizationResult.ComponentData.LineIndexCache.Map[lineIndex] = cacheEntry;
	            }
	    	}
	    	else
	    	{
	    		virtualizationResult.ComponentData.LineIndexCache.ExistsKeyList.Add(lineIndex);
	    		
	    		virtualizationResult.ComponentData.LineIndexCache.Map.Add(lineIndex, new TextEditorLineIndexCacheEntry(
	    			topCssValue: ((lineIndex - hiddenLineCount) * virtualizationResult.ViewModel.CharAndLineMeasurements.LineHeight).ToString(),
	    			leftCssValue: virtualizationResult.ViewModel.VirtualizationResult.EntryList[i].LeftInPixels.ToString(System.Globalization.CultureInfo.InvariantCulture),
					lineNumberString: (lineIndex + 1).ToString(),
					hiddenLineCount: hiddenLineCount));
	    	}
	    	
	    	if (isHandlingCursor)
	    	{
	    		isHandlingCursor = false;
	    		handledCursor = true;
	    		i--;
	    		
	    		if (virtualizationResult.ViewModel.PersistentState.HiddenLineIndexHashSet.Contains(virtualizationResult.ViewModel.LineIndex))
	    			virtualizationResult.CursorIsOnHiddenLine = true;
	    	}
    	}
    	
    	if (!handledCursor)
    	{
    		virtualizationResult.ComponentData.LineIndexCache.UsedKeyHashSet.Add(virtualizationResult.ViewModel.LineIndex);
    		
    		if (virtualizationResult.ComponentData.LineIndexCache.Map.ContainsKey(virtualizationResult.ViewModel.LineIndex))
	    	{
	    		var cacheEntry = virtualizationResult.ComponentData.LineIndexCache.Map[virtualizationResult.ViewModel.LineIndex];
	    		
	    		if (hiddenLineCount != cacheEntry.HiddenLineCount)
	            {
	            	cacheEntry.TopCssValue = (virtualizationResult.ViewModel.LineIndex * virtualizationResult.ViewModel.CharAndLineMeasurements.LineHeight)
	            		.ToString();
	            		
	            	cacheEntry.HiddenLineCount = 0;
	            	
	            	virtualizationResult.ComponentData.LineIndexCache.Map[virtualizationResult.ViewModel.LineIndex] = cacheEntry;
	            }
	    	}
	    	else
	    	{
	    		virtualizationResult.ComponentData.LineIndexCache.ExistsKeyList.Add(virtualizationResult.ViewModel.LineIndex);
	    		
	    		virtualizationResult.ComponentData.LineIndexCache.Map.Add(virtualizationResult.ViewModel.LineIndex, new TextEditorLineIndexCacheEntry(
	    			topCssValue: (virtualizationResult.ViewModel.LineIndex * virtualizationResult.ViewModel.CharAndLineMeasurements.LineHeight).ToString(),
					lineNumberString: (virtualizationResult.ViewModel.LineIndex + 1).ToString(),
					// TODO: This will cause a bug, this declares a lines left but in reality its trying to just describe the cursor and this value is placeholder.
					// But, since this placeholder is cached, if this line comes up in a future render it may or may not be positioned correctly.
					leftCssValue: virtualizationResult.ViewModel.GutterWidthInPixels.ToString(),
					hiddenLineCount: 0));
	    	}
	    		
    		if (virtualizationResult.ViewModel.PersistentState.HiddenLineIndexHashSet.Contains(virtualizationResult.ViewModel.LineIndex))
	    		virtualizationResult.CursorIsOnHiddenLine = true;
    	}
    }
    
    public void CreateUi(TextEditorVirtualizationResult virtualizationResult)
    {
        TextEditorViewModel? viewModel;
        TextEditorModel? model;
        
        if (virtualizationResult.ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.TextEditorState._viewModelMap.TryGetValue(
                virtualizationResult.ComponentData.TextEditorViewModelSlimDisplay.TextEditorViewModelKey,
                out viewModel))
        {
            _ = virtualizationResult.ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.TextEditorState._modelMap.TryGetValue(
                    viewModel.PersistentState.ResourceUri,
                    out model);
        }
        else
        {
            model = null;
        }
        
        if (!virtualizationResult.IsValid)
        {
        	DiagnoseIssues(virtualizationResult, model, viewModel);
        	return;
        }
    		
    	virtualizationResult.CursorIsOnHiddenLine = false;
    		
    	virtualizationResult.ComponentData.LineIndexCache.UsedKeyHashSet.Clear();
    	
    	LineIndexCache_Create(virtualizationResult);
    
    	// Somewhat hacky second try-catch so the presentations
    	// don't clobber the text editor's default behavior when they throw an exception.
    	try
    	{
	        GetCursorAndCaretRowStyleCss(virtualizationResult);
	        GetSelection(virtualizationResult);
	        
	        GetPresentationLayer(
	            virtualizationResult,
	        	virtualizationResult.ViewModel.PersistentState.FirstPresentationLayerKeysList,
	        	virtualizationResult.FirstPresentationLayerGroupList,
	        	virtualizationResult.FirstPresentationLayerTextSpanList);
	        	
	        GetPresentationLayer(
	            virtualizationResult,
	        	virtualizationResult.ViewModel.PersistentState.LastPresentationLayerKeysList,
	        	virtualizationResult.LastPresentationLayerGroupList,
	        	virtualizationResult.LastPresentationLayerTextSpanList);
	        
	        if (virtualizationResult.VirtualizedCollapsePointListVersion != virtualizationResult.ViewModel.PersistentState.VirtualizedCollapsePointListVersion ||
	        	virtualizationResult._seenViewModelKey != virtualizationResult.ViewModel.PersistentState.ViewModelKey)
	        {
	        	virtualizationResult.VirtualizedCollapsePointList.Clear();
	        
	        	for (int i = 0; i < virtualizationResult.ViewModel.PersistentState.VirtualizedCollapsePointList.Count; i++)
	        	{
	        		virtualizationResult.VirtualizedCollapsePointList.Add(virtualizationResult.ViewModel.PersistentState.VirtualizedCollapsePointList[i]);
	        	}
	        	
	        	GetInlineUiStyleList(virtualizationResult);
	        	
	        	virtualizationResult._seenViewModelKey = virtualizationResult.ViewModel.PersistentState.ViewModelKey;
	        	virtualizationResult.VirtualizedCollapsePointListVersion = virtualizationResult.ViewModel.PersistentState.VirtualizedCollapsePointListVersion;
	        }
        }
        catch (Exception e)
        {
        	Console.WriteLine("inner " + e);
        }
        
    	if (virtualizationResult.LineHeight != virtualizationResult.ViewModel.CharAndLineMeasurements.LineHeight)
    	{
    		virtualizationResult.LineHeight = virtualizationResult.ViewModel.CharAndLineMeasurements.LineHeight;
			
			_textEditorService.__StringBuilder.Clear();
    		_textEditorService.__StringBuilder.Append("height: ");
	        _textEditorService.__StringBuilder.Append(virtualizationResult.ViewModel.CharAndLineMeasurements.LineHeight.ToString());
	        _textEditorService.__StringBuilder.Append("px;");
	        virtualizationResult.LineHeightStyleCssString = _textEditorService.__StringBuilder.ToString();
	        
	        _textEditorService.__StringBuilder.Clear();
    		_textEditorService.__StringBuilder.Append(virtualizationResult.LineHeightStyleCssString);
	        _textEditorService.__StringBuilder.Append(virtualizationResult.Gutter_WidthCssStyle);
	        _textEditorService.__StringBuilder.Append(virtualizationResult.ComponentData.Gutter_PaddingCssStyle);
    		virtualizationResult.Gutter_HeightWidthPaddingCssStyle = _textEditorService.__StringBuilder.ToString();
		}
		
		bool shouldCalculateVerticalSlider = false;
		bool shouldCalculateHorizontalSlider = false;
		bool shouldCalculateHorizontalScrollbar = false;
		
    	if (virtualizationResult.TextEditor_Height != virtualizationResult.ViewModel.TextEditorDimensions.Height)
    	{
    		virtualizationResult.TextEditor_Height = virtualizationResult.ViewModel.TextEditorDimensions.Height;
    		shouldCalculateVerticalSlider = true;
	    }
		
    	if (virtualizationResult.Scroll_Height != virtualizationResult.ViewModel.ScrollHeight)
    	{
    		virtualizationResult.Scroll_Height = virtualizationResult.ViewModel.ScrollHeight;
    		shouldCalculateVerticalSlider = true;
	    }
		
    	if (virtualizationResult.Scroll_Top != virtualizationResult.ViewModel.ScrollTop)
    	{
    		virtualizationResult.Scroll_Top = virtualizationResult.ViewModel.ScrollTop;
    		shouldCalculateVerticalSlider = true;
	    }
		
    	if (virtualizationResult.TextEditor_Width != virtualizationResult.ViewModel.TextEditorDimensions.Width)
    	{
    		virtualizationResult.TextEditor_Width = virtualizationResult.ViewModel.TextEditorDimensions.Width;
    		shouldCalculateHorizontalSlider = true;
    		shouldCalculateHorizontalScrollbar = true;
	    }
		
    	if (virtualizationResult.Scroll_Width != virtualizationResult.ViewModel.ScrollWidth)
    	{
    		virtualizationResult.Scroll_Width = virtualizationResult.ViewModel.ScrollWidth;
    		shouldCalculateHorizontalSlider = true;
	    }
		
    	if (virtualizationResult.Scroll_Left != virtualizationResult.ViewModel.ScrollLeft)
    	{
    		virtualizationResult.Scroll_Left = virtualizationResult.ViewModel.ScrollLeft;
    		shouldCalculateHorizontalSlider = true;
	    }

		if (shouldCalculateVerticalSlider)
			VERTICAL_GetSliderVerticalStyleCss(virtualizationResult);
		
		if (shouldCalculateHorizontalSlider)
			HORIZONTAL_GetSliderHorizontalStyleCss(virtualizationResult);
		
		if (shouldCalculateHorizontalScrollbar)
			HORIZONTAL_GetScrollbarHorizontalStyleCss(virtualizationResult);
    
    	ConstructVirtualizationStyleCssStrings(virtualizationResult);
		
		for (int i = virtualizationResult.ComponentData.LineIndexCache.ExistsKeyList.Count - 1; i >= 0; i--)
		{
			if (!virtualizationResult.ComponentData.LineIndexCache.UsedKeyHashSet.Contains(virtualizationResult.ComponentData.LineIndexCache.ExistsKeyList[i]))
			{
				virtualizationResult.ComponentData.LineIndexCache.Map.Remove(virtualizationResult.ComponentData.LineIndexCache.ExistsKeyList[i]);
				virtualizationResult.ComponentData.LineIndexCache.ExistsKeyList.RemoveAt(i);
			}
		}
    }
    
    /// <summary>TODO: Determine if total width changed?</summary>
    public void ConstructVirtualizationStyleCssStrings(TextEditorVirtualizationResult virtualizationResult)
    {
		_textEditorService.__StringBuilder.Clear();
		
    	_textEditorService.__StringBuilder.Append("width: ");
    	_textEditorService.__StringBuilder.Append(virtualizationResult.ViewModel.VirtualizationResult.TotalWidth.ToString());
    	_textEditorService.__StringBuilder.Append("px; ");
    	
    	_textEditorService.__StringBuilder.Append("height: ");
    	_textEditorService.__StringBuilder.Append(virtualizationResult.ViewModel.VirtualizationResult.TotalHeight.ToString());
    	_textEditorService.__StringBuilder.Append("px;");
    	
    	virtualizationResult.ComponentData.BothVirtualizationBoundaryStyleCssString = _textEditorService.__StringBuilder.ToString();
    }
    
    public void VERTICAL_GetSliderVerticalStyleCss(TextEditorVirtualizationResult virtualizationResult)
    {
    	// Divide by zero exception
    	if (virtualizationResult.ViewModel.ScrollHeight == 0)
    		return;
    
        var scrollbarHeightInPixels = virtualizationResult.ViewModel.TextEditorDimensions.Height - ScrollbarFacts.SCROLLBAR_SIZE_IN_PIXELS;

        // Proportional Top
        var sliderProportionalTopInPixels = virtualizationResult.ViewModel.ScrollTop *
            scrollbarHeightInPixels /
            virtualizationResult.ViewModel.ScrollHeight;

		_textEditorService.__StringBuilder.Clear();
		
		_textEditorService.__StringBuilder.Append("left: 0; width: ");
		_textEditorService.__StringBuilder.Append(virtualizationResult.ComponentData.ScrollbarSizeCssValue);
		_textEditorService.__StringBuilder.Append("px; ");
		
		_textEditorService.__StringBuilder.Append("top: ");
		_textEditorService.__StringBuilder.Append(sliderProportionalTopInPixels.ToString());
		_textEditorService.__StringBuilder.Append("px;");

        // Proportional Height
        var pageHeight = virtualizationResult.ViewModel.TextEditorDimensions.Height;

        var sliderProportionalHeightInPixels = pageHeight *
            scrollbarHeightInPixels /
            virtualizationResult.ViewModel.ScrollHeight;

        var sliderProportionalHeightInPixelsInvariantCulture = sliderProportionalHeightInPixels.ToString();

		_textEditorService.__StringBuilder.Append("height: ");
		_textEditorService.__StringBuilder.Append(sliderProportionalHeightInPixelsInvariantCulture);
		_textEditorService.__StringBuilder.Append("px;");

        virtualizationResult.VERTICAL_SliderCssStyle = _textEditorService.__StringBuilder.ToString();
    }
    
    private void GetInlineUiStyleList(TextEditorVirtualizationResult virtualizationResult)
    {
    	if (virtualizationResult.InlineUiWidthStyleCssString is null || virtualizationResult.ComponentData.InlineUiWidthStyleCssStringIsOutdated)
    	{
	    	var widthPixels = virtualizationResult.ViewModel.CharAndLineMeasurements.CharacterWidth * 3;
			var widthCssValue = widthPixels.ToString(System.Globalization.CultureInfo.InvariantCulture);
			virtualizationResult.InlineUiWidthStyleCssString = $"width: {widthCssValue}px;";
			// width: @(widthCssValue)px;
		}
    
    	virtualizationResult.InlineUiStyleList.Clear();
        var tabWidth = _textEditorService.OptionsApi.GetOptions().TabWidth;
    	
    	for (int inlineUiIndex = 0; inlineUiIndex < virtualizationResult.ViewModel.PersistentState.InlineUiList.Count; inlineUiIndex++)
    	{
    		var entry = virtualizationResult.ViewModel.PersistentState.InlineUiList[inlineUiIndex];
    		
    		var lineAndColumnIndices = virtualizationResult.Model.GetLineAndColumnIndicesFromPositionIndex(entry.InlineUi.PositionIndex);
    		
    		if (!virtualizationResult.ComponentData.LineIndexCache.Map.ContainsKey(lineAndColumnIndices.lineIndex))
    			continue;
    		
    		var leftInPixels = virtualizationResult.ViewModel.GutterWidthInPixels + lineAndColumnIndices.columnIndex * virtualizationResult.ViewModel.CharAndLineMeasurements.CharacterWidth;
    		
    		// Tab key column offset
    		{
	    		var tabsOnSameLineBeforeCursor = virtualizationResult.Model.GetTabCountOnSameLineBeforeCursor(
				    lineAndColumnIndices.lineIndex,
				    lineAndColumnIndices.columnIndex);
				
				// 1 of the character width is already accounted for
				var extraWidthPerTabKey = tabWidth - 1;
				
				leftInPixels += extraWidthPerTabKey *
				    tabsOnSameLineBeforeCursor *
				    virtualizationResult.ViewModel.CharAndLineMeasurements.CharacterWidth;
			}
    		
    		var topCssValue = virtualizationResult.ComponentData.LineIndexCache.Map[lineAndColumnIndices.lineIndex].TopCssValue;

    		_textEditorService.__StringBuilder.Clear();
    		
    		_textEditorService.__StringBuilder.Append("position: absolute;");
    		
    		_textEditorService.__StringBuilder.Append("left: ");
    		_textEditorService.__StringBuilder.Append(leftInPixels.ToString(System.Globalization.CultureInfo.InvariantCulture));
    		_textEditorService.__StringBuilder.Append("px;");
    		
    		_textEditorService.__StringBuilder.Append("top: ");
    		_textEditorService.__StringBuilder.Append(topCssValue);
    		_textEditorService.__StringBuilder.Append("px;");
    		
    		_textEditorService.__StringBuilder.Append(virtualizationResult.InlineUiWidthStyleCssString);
    		
    		_textEditorService.__StringBuilder.Append(virtualizationResult.LineHeightStyleCssString);
    		
    		virtualizationResult.InlineUiStyleList.Add(_textEditorService.__StringBuilder.ToString());
    	}
    }
    
    public void GetCursorAndCaretRowStyleCss(TextEditorVirtualizationResult virtualizationResult)
    {
    	var shouldAppearAfterCollapsePoint = virtualizationResult.CursorIsOnHiddenLine;
    	var tabWidth = _textEditorService.OptionsApi.GetOptions().TabWidth;
    	
    	double leftInPixels = virtualizationResult.ViewModel.GutterWidthInPixels;
    	var topInPixelsInvariantCulture = string.Empty;
	
		if (virtualizationResult.CursorIsOnHiddenLine)
		{
			for (int collapsePointIndex = 0; collapsePointIndex < virtualizationResult.ViewModel.PersistentState.AllCollapsePointList.Count; collapsePointIndex++)
			{
				var collapsePoint = virtualizationResult.ViewModel.PersistentState.AllCollapsePointList[collapsePointIndex];
				
				if (!collapsePoint.IsCollapsed)
					continue;
			
				var lastLineIndex = collapsePoint.EndExclusiveLineIndex - 1;
				
				if (lastLineIndex == virtualizationResult.ViewModel.LineIndex)
				{
					var lastLineInformation = virtualizationResult.Model.GetLineInformation(lastLineIndex);
					
					var appendToLineInformation = virtualizationResult.Model.GetLineInformation(collapsePoint.AppendToLineIndex);
					
					// Tab key column offset
			        {
			            var tabsOnSameLineBeforeCursor = virtualizationResult.Model.GetTabCountOnSameLineBeforeCursor(
			                collapsePoint.AppendToLineIndex,
			                appendToLineInformation.LastValidColumnIndex);
			
			            // 1 of the character width is already accounted for
			
			            var extraWidthPerTabKey = tabWidth - 1;
			
			            leftInPixels += extraWidthPerTabKey *
			                tabsOnSameLineBeforeCursor *
			                virtualizationResult.ViewModel.CharAndLineMeasurements.CharacterWidth;
			        }
			        
			        // +3 for the 3 dots: '[...]'
			        leftInPixels += virtualizationResult.ViewModel.CharAndLineMeasurements.CharacterWidth * (appendToLineInformation.LastValidColumnIndex + 3);
			        
			        if (virtualizationResult.ComponentData.LineIndexCache.Map.ContainsKey(collapsePoint.AppendToLineIndex))
			        {
			        	topInPixelsInvariantCulture = virtualizationResult.ComponentData.LineIndexCache.Map[collapsePoint.AppendToLineIndex].TopCssValue;
			        }
			        else
			        {
			        	if (virtualizationResult.ViewModel.VirtualizationResult.Count > 0)
			        	{
			        		var firstEntry = virtualizationResult.ViewModel.VirtualizationResult.EntryList[0];
			        		topInPixelsInvariantCulture = virtualizationResult.ComponentData.LineIndexCache.Map[firstEntry.LineIndex].TopCssValue;
			        	}
			        	else
			        	{
			        		topInPixelsInvariantCulture = 0.ToString();
			        	}
			        }
			        
			        break;
				}
			}
		}

		if (!shouldAppearAfterCollapsePoint)
		{
	        // Tab key column offset
	        {
	            var tabsOnSameLineBeforeCursor = virtualizationResult.Model.GetTabCountOnSameLineBeforeCursor(
	                virtualizationResult.ViewModel.LineIndex,
	                virtualizationResult.ViewModel.ColumnIndex);
	
	            // 1 of the character width is already accounted for
	
	            var extraWidthPerTabKey = tabWidth - 1;
	
	            leftInPixels += extraWidthPerTabKey *
	                tabsOnSameLineBeforeCursor *
	                virtualizationResult.ViewModel.CharAndLineMeasurements.CharacterWidth;
	        }
	        
	        leftInPixels += virtualizationResult.ViewModel.CharAndLineMeasurements.CharacterWidth * virtualizationResult.ViewModel.ColumnIndex;
	        
	        for (int inlineUiTupleIndex = 0; inlineUiTupleIndex < virtualizationResult.ViewModel.PersistentState.InlineUiList.Count; inlineUiTupleIndex++)
			{
				var inlineUiTuple = virtualizationResult.ViewModel.PersistentState.InlineUiList[inlineUiTupleIndex];
				
				var lineAndColumnIndices = virtualizationResult.Model.GetLineAndColumnIndicesFromPositionIndex(inlineUiTuple.InlineUi.PositionIndex);
				
				if (lineAndColumnIndices.lineIndex == virtualizationResult.ViewModel.LineIndex)
				{
					if (lineAndColumnIndices.columnIndex == virtualizationResult.ViewModel.ColumnIndex)
					{
						if (virtualizationResult.ViewModel.PersistentState.VirtualAssociativityKind == VirtualAssociativityKind.Right)
						{
							leftInPixels += virtualizationResult.ViewModel.CharAndLineMeasurements.CharacterWidth * 3;
						}
					}
					else if (lineAndColumnIndices.columnIndex <= virtualizationResult.ViewModel.ColumnIndex)
					{
						leftInPixels += virtualizationResult.ViewModel.CharAndLineMeasurements.CharacterWidth * 3;
					}
				}
			}
	    }
        
        _textEditorService.__StringBuilder.Clear();

        var leftInPixelsInvariantCulture = leftInPixels.ToString(System.Globalization.CultureInfo.InvariantCulture);
        _textEditorService.__StringBuilder.Append("left: ");
        _textEditorService.__StringBuilder.Append(leftInPixelsInvariantCulture);
        _textEditorService.__StringBuilder.Append("px;");

		if (!shouldAppearAfterCollapsePoint)
			topInPixelsInvariantCulture = virtualizationResult.ComponentData.LineIndexCache.Map[virtualizationResult.ViewModel.LineIndex].TopCssValue;

		_textEditorService.__StringBuilder.Append("top: ");
		_textEditorService.__StringBuilder.Append(topInPixelsInvariantCulture);
		_textEditorService.__StringBuilder.Append("px;");

        _textEditorService.__StringBuilder.Append(virtualizationResult.LineHeightStyleCssString);

        var widthInPixelsInvariantCulture = virtualizationResult.TextEditorRenderBatchPersistentState.TextEditorOptions.CursorWidthInPixels.ToString(System.Globalization.CultureInfo.InvariantCulture);
        _textEditorService.__StringBuilder.Append("width: ");
        _textEditorService.__StringBuilder.Append(widthInPixelsInvariantCulture);
        _textEditorService.__StringBuilder.Append("px;");

        _textEditorService.__StringBuilder.Append(((ITextEditorKeymap)virtualizationResult.TextEditorRenderBatchPersistentState.TextEditorOptions.Keymap).GetCursorCssStyleString(
            virtualizationResult.Model,
            virtualizationResult.ViewModel,
            virtualizationResult.TextEditorRenderBatchPersistentState.TextEditorOptions));
        
        // This feels a bit hacky, exceptions are happening because the UI isn't accessing
        // the text editor in a thread safe way.
        //
        // When an exception does occur though, the cursor should receive a 'text editor changed'
        // event and re-render anyhow however.
        // 
        // So store the result of this method incase an exception occurs in future invocations,
        // to keep the cursor on screen while the state works itself out.
        virtualizationResult.CursorCssStyle = _textEditorService.__StringBuilder.ToString();
    
    	/////////////////////
    	/////////////////////
    	
    	// CaretRow starts here
    	
    	/////////////////////
    	/////////////////////
		
		_textEditorService.__StringBuilder.Clear();
		
		_textEditorService.__StringBuilder.Append("top: ");
		_textEditorService.__StringBuilder.Append(topInPixelsInvariantCulture);
		_textEditorService.__StringBuilder.Append("px;");

        _textEditorService.__StringBuilder.Append(virtualizationResult.LineHeightStyleCssString);

        var widthOfBodyInPixelsInvariantCulture =
            (virtualizationResult.Model.MostCharactersOnASingleLineTuple.lineLength * virtualizationResult.ViewModel.CharAndLineMeasurements.CharacterWidth)
            .ToString(System.Globalization.CultureInfo.InvariantCulture);

		_textEditorService.__StringBuilder.Append("width: ");
		_textEditorService.__StringBuilder.Append(widthOfBodyInPixelsInvariantCulture);
		_textEditorService.__StringBuilder.Append("px;");

        // This feels a bit hacky, exceptions are happening because the UI isn't accessing
        // the text editor in a thread safe way.
        //
        // When an exception does occur though, the cursor should receive a 'text editor changed'
        // event and re-render anyhow however.
        // 
        // So store the result of this method incase an exception occurs in future invocations,
        // to keep the cursor on screen while the state works itself out.
        virtualizationResult.CaretRowCssStyle = _textEditorService.__StringBuilder.ToString();
    }
    
    public void GetSelection(TextEditorVirtualizationResult virtualizationResult)
    {
    	virtualizationResult.SelectionStyleList.Clear();
    
    	if (TextEditorSelectionHelper.HasSelectedText(virtualizationResult.ViewModel) &&
	         virtualizationResult.ViewModel.VirtualizationResult.Count > 0)
	    {
	        virtualizationResult.SelectionBoundsInPositionIndexUnits = TextEditorSelectionHelper.GetSelectionBounds(
	            virtualizationResult.ViewModel);
	
	        var selectionBoundsInLineIndexUnits = TextEditorSelectionHelper.ConvertSelectionOfPositionIndexUnitsToLineIndexUnits(
                virtualizationResult.Model,
                virtualizationResult.SelectionBoundsInPositionIndexUnits);
	
	        var virtualLowerBoundInclusiveLineIndex = virtualizationResult.ViewModel.VirtualizationResult.EntryList[0].LineIndex;
	        var virtualUpperBoundExclusiveLineIndex = 1 + virtualizationResult.ViewModel.VirtualizationResult.EntryList[virtualizationResult.ViewModel.VirtualizationResult.Count - 1].LineIndex;
	
	        virtualizationResult.UseLowerBoundInclusiveLineIndex = virtualLowerBoundInclusiveLineIndex >= selectionBoundsInLineIndexUnits.Line_LowerInclusiveIndex
	            ? virtualLowerBoundInclusiveLineIndex
	            : selectionBoundsInLineIndexUnits.Line_LowerInclusiveIndex;
	
	        virtualizationResult.UseUpperBoundExclusiveLineIndex = virtualUpperBoundExclusiveLineIndex <= selectionBoundsInLineIndexUnits.Line_UpperExclusiveIndex
	            ? virtualUpperBoundExclusiveLineIndex
            	: selectionBoundsInLineIndexUnits.Line_UpperExclusiveIndex;
            
            var hiddenLineCount = 0;
			var checkHiddenLineIndex = 0;
            
            for (; checkHiddenLineIndex < virtualizationResult.UseLowerBoundInclusiveLineIndex; checkHiddenLineIndex++)
            {
            	if (virtualizationResult.ViewModel.PersistentState.HiddenLineIndexHashSet.Contains(checkHiddenLineIndex))
            		hiddenLineCount++;
            }
            
            for (var i = virtualizationResult.UseLowerBoundInclusiveLineIndex; i < virtualizationResult.UseUpperBoundExclusiveLineIndex; i++)
	        {
	        	checkHiddenLineIndex++;
	        
	        	if (virtualizationResult.ViewModel.PersistentState.HiddenLineIndexHashSet.Contains(i))
	        	{
	        		hiddenLineCount++;
	        		continue;
	        	}
	        	
	        	virtualizationResult.SelectionStyleList.Add(GetTextSelectionStyleCss(
	        	    virtualizationResult,
		     	   virtualizationResult.SelectionBoundsInPositionIndexUnits.Position_LowerInclusiveIndex,
		     	   virtualizationResult.SelectionBoundsInPositionIndexUnits.Position_UpperExclusiveIndex,
		     	   lineIndex: i));
	        }
	    }
    }
    
    public string GetTextSelectionStyleCss(
        TextEditorVirtualizationResult virtualizationResult,
        int position_LowerInclusiveIndex,
        int position_UpperExclusiveIndex,
        int lineIndex)
    {
        if (lineIndex >= virtualizationResult.Model.LineEndList.Count)
            return string.Empty;

        var line = virtualizationResult.Model.GetLineInformation(lineIndex);
        var tabWidth = _textEditorService.OptionsApi.GetOptions().TabWidth;

        var selectionStartingColumnIndex = 0;
        var selectionEndingColumnIndex = line.Position_EndExclusiveIndex - 1;

        var fullWidthOfLineIsSelected = true;

        if (position_LowerInclusiveIndex > line.Position_StartInclusiveIndex)
        {
            selectionStartingColumnIndex = position_LowerInclusiveIndex - line.Position_StartInclusiveIndex;
            fullWidthOfLineIsSelected = false;
        }

        if (position_UpperExclusiveIndex < line.Position_EndExclusiveIndex)
        {
            selectionEndingColumnIndex = position_UpperExclusiveIndex - line.Position_StartInclusiveIndex;
            fullWidthOfLineIsSelected = false;
        }

        var charMeasurements = virtualizationResult.ViewModel.CharAndLineMeasurements;

        _textEditorService.__StringBuilder.Clear();
        
        var topInPixelsInvariantCulture = virtualizationResult.ComponentData.LineIndexCache.Map[lineIndex].TopCssValue;
        _textEditorService.__StringBuilder.Append("top: ");
        _textEditorService.__StringBuilder.Append(topInPixelsInvariantCulture);
        _textEditorService.__StringBuilder.Append("px;");

        _textEditorService.__StringBuilder.Append(virtualizationResult.LineHeightStyleCssString);

        var selectionStartInPixels = virtualizationResult.ViewModel.GutterWidthInPixels + selectionStartingColumnIndex * charMeasurements.CharacterWidth;

        // selectionStartInPixels offset from Tab keys a width of many characters
        {
            var tabsOnSameLineBeforeCursor = virtualizationResult.Model.GetTabCountOnSameLineBeforeCursor(
                lineIndex,
                selectionStartingColumnIndex);

            // 1 of the character width is already accounted for
            var extraWidthPerTabKey = tabWidth - 1;

            selectionStartInPixels += 
                extraWidthPerTabKey * tabsOnSameLineBeforeCursor * charMeasurements.CharacterWidth;
        }

        var selectionStartInPixelsInvariantCulture = selectionStartInPixels.ToString(System.Globalization.CultureInfo.InvariantCulture);
        _textEditorService.__StringBuilder.Append("left: ");
        _textEditorService.__StringBuilder.Append(selectionStartInPixelsInvariantCulture);
        _textEditorService.__StringBuilder.Append("px;");

        var selectionWidthInPixels = 
            selectionEndingColumnIndex * charMeasurements.CharacterWidth - selectionStartInPixels + virtualizationResult.ViewModel.GutterWidthInPixels;

        // Tab keys a width of many characters
        {
            var lineInformation = virtualizationResult.Model.GetLineInformation(lineIndex);

            selectionEndingColumnIndex = Math.Min(
                selectionEndingColumnIndex,
                lineInformation.LastValidColumnIndex);

            var tabsOnSameLineBeforeCursor = virtualizationResult.Model.GetTabCountOnSameLineBeforeCursor(
                lineIndex,
                selectionEndingColumnIndex);

            // 1 of the character width is already accounted for
            var extraWidthPerTabKey = tabWidth - 1;

            selectionWidthInPixels += extraWidthPerTabKey * tabsOnSameLineBeforeCursor * charMeasurements.CharacterWidth;
        }

        _textEditorService.__StringBuilder.Append("width: ");
        var fullWidthValue = virtualizationResult.ViewModel.ScrollWidth;

        if (virtualizationResult.ViewModel.TextEditorDimensions.Width >
            virtualizationResult.ViewModel.ScrollWidth)
        {
            // If content does not fill the viewable width of the Text Editor User Interface
            fullWidthValue = virtualizationResult.ViewModel.TextEditorDimensions.Width;
        }

        var fullWidthValueInPixelsInvariantCulture = fullWidthValue.ToString();

        if (fullWidthOfLineIsSelected)
        {
        	_textEditorService.__StringBuilder.Append(fullWidthValueInPixelsInvariantCulture);
        	_textEditorService.__StringBuilder.Append("px;");
        }
        else if (selectionStartingColumnIndex != 0 &&
                 position_UpperExclusiveIndex > line.Position_EndExclusiveIndex - 1)
        {
        	_textEditorService.__StringBuilder.Append("calc(");
        	_textEditorService.__StringBuilder.Append(fullWidthValueInPixelsInvariantCulture);
        	_textEditorService.__StringBuilder.Append("px - ");
        	_textEditorService.__StringBuilder.Append(selectionStartInPixelsInvariantCulture);
        	_textEditorService.__StringBuilder.Append("px);");
        }
        else
        {
        	_textEditorService.__StringBuilder.Append(selectionWidthInPixels.ToString(System.Globalization.CultureInfo.InvariantCulture));
        	_textEditorService.__StringBuilder.Append("px;");
        }

        return _textEditorService.__StringBuilder.ToString();
    }
    
    private void GetPresentationLayer(
        TextEditorVirtualizationResult virtualizationResult,
    	List<Key<TextEditorPresentationModel>> presentationLayerKeysList,
    	List<(string CssClassString, int StartInclusiveIndex, int EndExclusiveIndex)> presentationLayerGroupList,
    	List<(string PresentationCssClass, string PresentationCssStyle)> presentationLayerTextSpanList)
    {
    	presentationLayerGroupList.Clear();
    	presentationLayerTextSpanList.Clear();
    
    	for (int presentationKeyIndex = 0; presentationKeyIndex < presentationLayerKeysList.Count; presentationKeyIndex++)
	    {
	    	var presentationKey = presentationLayerKeysList[presentationKeyIndex];
	    	
	    	var presentationLayer = virtualizationResult.Model.PresentationModelList.FirstOrDefault(
	    		x => x.TextEditorPresentationKey == presentationKey);
	        if (presentationLayer is null)
	        	continue;
	    	
	        var completedCalculation = presentationLayer.CompletedCalculation;
	        if (completedCalculation is null)
	        	continue;
	
			IReadOnlyList<TextEditorTextSpan> textSpansList = completedCalculation.TextSpanList
	            ?? Array.Empty<TextEditorTextSpan>();
	
	        IReadOnlyList<TextEditorTextModification> textModificationList = ((IReadOnlyList<TextEditorTextModification>?)completedCalculation.TextModificationsSinceRequestList)
	            ?? Array.Empty<TextEditorTextModification>();
	
        	// Should be using 'textSpansList' not 'completedCalculation.TextSpanList'?
            textSpansList = PresentationVirtualizeAndShiftTextSpans(virtualizationResult, textModificationList, completedCalculation.TextSpanList);

			var indexInclusiveStart = presentationLayerTextSpanList.Count;
			
			var hiddenLineCount = 0;
			var checkHiddenLineIndex = 0;

            for (int textSpanIndex = 0; textSpanIndex < textSpansList.Count; textSpanIndex++)
            {
            	var textSpan = textSpansList[textSpanIndex];
            	
                var boundsInPositionIndexUnits = (textSpan.StartInclusiveIndex, textSpan.EndExclusiveIndex);

                var boundsInLineIndexUnits = PresentationGetBoundsInLineIndexUnits(virtualizationResult, boundsInPositionIndexUnits);
                
                for (; checkHiddenLineIndex < boundsInLineIndexUnits.FirstLineToSelectDataInclusive; checkHiddenLineIndex++)
                {
                	if (virtualizationResult.ViewModel.PersistentState.HiddenLineIndexHashSet.Contains(checkHiddenLineIndex))
                		hiddenLineCount++;
                }

                for (var i = boundsInLineIndexUnits.FirstLineToSelectDataInclusive;
                     i < boundsInLineIndexUnits.LastLineToSelectDataExclusive;
                     i++)
                {
                	checkHiddenLineIndex++;
                	
                	if (virtualizationResult.ViewModel.PersistentState.HiddenLineIndexHashSet.Contains(i))
                	{
                		hiddenLineCount++;
                		continue;
                	}
                		
                	presentationLayerTextSpanList.Add((
                		PresentationGetCssClass(presentationLayer, textSpan.DecorationByte),
                		PresentationGetCssStyleString(
                		    virtualizationResult,
                            boundsInPositionIndexUnits.StartInclusiveIndex,
                            boundsInPositionIndexUnits.EndExclusiveIndex,
                            lineIndex: i)));
                }
            }
            
            presentationLayerGroupList.Add(
            	(
            		presentationLayer.CssClassString,
            	    indexInclusiveStart,
            	    indexExclusiveEnd: presentationLayerTextSpanList.Count)
            	);
	    }
    }
    
    public IReadOnlyList<TextEditorTextSpan> PresentationVirtualizeAndShiftTextSpans(
        TextEditorVirtualizationResult virtualizationResult,
        IReadOnlyList<TextEditorTextModification> textModifications,
        IReadOnlyList<TextEditorTextSpan> inTextSpanList)
    {
    	// TODO: Why virtualize then shift? Isn't it shift then virtualize? (2025-05-01)
    	
    	virtualizationResult.VirtualizedTextSpanList.Clear();
    	virtualizationResult.OutTextSpansList.Clear();
    
        // Virtualize the text spans
        if (virtualizationResult.ViewModel.VirtualizationResult.Count > 0)
        {
            var lowerLineIndexInclusive = virtualizationResult.ViewModel.VirtualizationResult.EntryList[0].LineIndex;
            var upperLineIndexInclusive = virtualizationResult.ViewModel.VirtualizationResult.EntryList[virtualizationResult.ViewModel.VirtualizationResult.Count - 1].LineIndex;

            var lowerLine = virtualizationResult.Model.GetLineInformation(lowerLineIndexInclusive);
            var upperLine = virtualizationResult.Model.GetLineInformation(upperLineIndexInclusive);

			// Awkward enumeration was modified 'for loop' (2025-01-22)
			// Also, this shouldn't be done here, it should be done during the editContext.
			var count = inTextSpanList.Count;
            for (int i = 0; i < count; i++)
            {
            	var textSpan = inTextSpanList[i];
            	
                if (lowerLine.Position_StartInclusiveIndex <= textSpan.StartInclusiveIndex &&
                    upperLine.Position_EndExclusiveIndex >= textSpan.StartInclusiveIndex)
                {
                	virtualizationResult.VirtualizedTextSpanList.Add(textSpan);
                }
            }
        }
        else
        {
            // No 'VirtualizationResult', so don't render any text spans.
            return Array.Empty<TextEditorTextSpan>();
        }

        // Shift the text spans
        {
            for (int textSpanIndex = 0; textSpanIndex < virtualizationResult.VirtualizedTextSpanList.Count; textSpanIndex++)
            {
            	var textSpan = virtualizationResult.VirtualizedTextSpanList[textSpanIndex];
            	
                var startingIndexInclusive = textSpan.StartInclusiveIndex;
                var endingIndexExclusive = textSpan.EndExclusiveIndex;

				// Awkward enumeration was modified 'for loop' (2025-01-22)
				// Also, this shouldn't be done here, it should be done during the editContext.
				var count = textModifications.Count;
                for (int i = 0; i < count; i++)
                {
                	var textModification = textModifications[i];
                
                    if (textModification.WasInsertion)
                    {
                        if (startingIndexInclusive >= textModification.TextEditorTextSpan.StartInclusiveIndex)
                        {
                            startingIndexInclusive += textModification.TextEditorTextSpan.Length;
                            endingIndexExclusive += textModification.TextEditorTextSpan.Length;
                        }
                    }
                    else // was deletion
                    {
                        if (startingIndexInclusive >= textModification.TextEditorTextSpan.StartInclusiveIndex)
                        {
                            startingIndexInclusive -= textModification.TextEditorTextSpan.Length;
                            endingIndexExclusive -= textModification.TextEditorTextSpan.Length;
                        }
                    }
                }

                virtualizationResult.OutTextSpansList.Add(textSpan with
                {
                    StartInclusiveIndex = startingIndexInclusive,
                    EndExclusiveIndex = endingIndexExclusive
                });
            }
        }

        return virtualizationResult.OutTextSpansList;
    }
    
    public (int FirstLineToSelectDataInclusive, int LastLineToSelectDataExclusive) PresentationGetBoundsInLineIndexUnits(
        TextEditorVirtualizationResult virtualizationResult,
    	(int StartInclusiveIndex, int EndExclusiveIndex) boundsInPositionIndexUnits)
    {
        var firstLineToSelectDataInclusive = virtualizationResult.Model
            .GetLineInformationFromPositionIndex(boundsInPositionIndexUnits.StartInclusiveIndex)
            .Index;

        var lastLineToSelectDataExclusive = virtualizationResult.Model
            .GetLineInformationFromPositionIndex(boundsInPositionIndexUnits.EndExclusiveIndex)
            .Index +
            1;

        return (firstLineToSelectDataInclusive, lastLineToSelectDataExclusive);
    }
    
    public string PresentationGetCssClass(TextEditorPresentationModel presentationModel, byte decorationByte)
    {
        return presentationModel.DecorationMapper.Map(decorationByte);
    }
    
    public string PresentationGetCssStyleString(
        TextEditorVirtualizationResult virtualizationResult,
        int position_LowerInclusiveIndex,
        int position_UpperExclusiveIndex,
        int lineIndex)
    {
        if (lineIndex >= virtualizationResult.Model.LineEndList.Count)
            return string.Empty;

        var line = virtualizationResult.Model.GetLineInformation(lineIndex);
        var tabWidth = _textEditorService.OptionsApi.GetOptions().TabWidth;

        var startingColumnIndex = 0;
        var endingColumnIndex = line.Position_EndExclusiveIndex - 1;

        var fullWidthOfLineIsSelected = true;

        if (position_LowerInclusiveIndex > line.Position_StartInclusiveIndex)
        {
            startingColumnIndex = position_LowerInclusiveIndex - line.Position_StartInclusiveIndex;
            fullWidthOfLineIsSelected = false;
        }

        if (position_UpperExclusiveIndex < line.Position_EndExclusiveIndex)
        {
            endingColumnIndex = position_UpperExclusiveIndex - line.Position_StartInclusiveIndex;
            fullWidthOfLineIsSelected = false;
        }

        var topInPixelsInvariantCulture = virtualizationResult.ComponentData.LineIndexCache.Map[lineIndex].TopCssValue;
        
        _textEditorService.__StringBuilder.Clear();
        _textEditorService.__StringBuilder.Append("position: absolute; ");

		_textEditorService.__StringBuilder.Append("top: ");
		_textEditorService.__StringBuilder.Append(topInPixelsInvariantCulture);
		_textEditorService.__StringBuilder.Append("px;");

        _textEditorService.__StringBuilder.Append("height: ");
        _textEditorService.__StringBuilder.Append(virtualizationResult.ViewModel.CharAndLineMeasurements.LineHeight.ToString());
        _textEditorService.__StringBuilder.Append("px;");
        
        // This only happens when the 'EOF' position index is "inclusive"
        // as something to be drawn for the presentation.
        if (startingColumnIndex > line.LastValidColumnIndex)
        	startingColumnIndex = line.LastValidColumnIndex;

        var startInPixels = virtualizationResult.ViewModel.GutterWidthInPixels + startingColumnIndex * virtualizationResult.ViewModel.CharAndLineMeasurements.CharacterWidth;

        // startInPixels offset from Tab keys a width of many characters
        {
            var tabsOnSameLineBeforeCursor = virtualizationResult.Model.GetTabCountOnSameLineBeforeCursor(
                lineIndex,
                startingColumnIndex);

            // 1 of the character width is already accounted for
            var extraWidthPerTabKey = tabWidth - 1;

            startInPixels += extraWidthPerTabKey * tabsOnSameLineBeforeCursor * virtualizationResult.ViewModel.CharAndLineMeasurements.CharacterWidth;
        }

        var startInPixelsInvariantCulture = startInPixels.ToString(System.Globalization.CultureInfo.InvariantCulture);
        _textEditorService.__StringBuilder.Append("left: ");
        _textEditorService.__StringBuilder.Append(startInPixelsInvariantCulture);
        _textEditorService.__StringBuilder.Append("px;");

        var widthInPixels = endingColumnIndex * virtualizationResult.ViewModel.CharAndLineMeasurements.CharacterWidth - startInPixels + virtualizationResult.ViewModel.GutterWidthInPixels;

        // Tab keys a width of many characters
        {
            var tabsOnSameLineBeforeCursor = virtualizationResult.Model.GetTabCountOnSameLineBeforeCursor(
                lineIndex,
                line.LastValidColumnIndex);

            // 1 of the character width is already accounted for
            var extraWidthPerTabKey = tabWidth - 1;

            widthInPixels += extraWidthPerTabKey * tabsOnSameLineBeforeCursor * virtualizationResult.ViewModel.CharAndLineMeasurements.CharacterWidth;
        }

        _textEditorService.__StringBuilder.Append("width: ");

        var fullWidthValue = virtualizationResult.ViewModel.ScrollWidth;

        if (virtualizationResult.ViewModel.TextEditorDimensions.Width > virtualizationResult.ViewModel.ScrollWidth)
            fullWidthValue = virtualizationResult.ViewModel.TextEditorDimensions.Width; // If content does not fill the viewable width of the Text Editor User Interface

        var fullWidthValueInPixelsInvariantCulture = fullWidthValue.ToString();

        var widthInPixelsInvariantCulture = widthInPixels.ToString(System.Globalization.CultureInfo.InvariantCulture);

        if (fullWidthOfLineIsSelected)
        {
            _textEditorService.__StringBuilder.Append(fullWidthValueInPixelsInvariantCulture);
            _textEditorService.__StringBuilder.Append("px;");
        }
        else if (startingColumnIndex != 0 && position_UpperExclusiveIndex > line.Position_EndExclusiveIndex - 1)
        {
        	_textEditorService.__StringBuilder.Append("calc(");
        	_textEditorService.__StringBuilder.Append(fullWidthValueInPixelsInvariantCulture);
        	_textEditorService.__StringBuilder.Append("px - ");
        	_textEditorService.__StringBuilder.Append(startInPixelsInvariantCulture);
        	_textEditorService.__StringBuilder.Append("px);");
        }
        else
        {
        	_textEditorService.__StringBuilder.Append(widthInPixelsInvariantCulture);
        	_textEditorService.__StringBuilder.Append("px;");
        }

        return _textEditorService.__StringBuilder.ToString();
    }
    
    public void CreateCacheEach(
    	int entryIndex,
		ref Span<TextEditorVirtualizationSpan> entireSpan)
    {
    	var virtualizationEntry = _createCacheEachSharedParameters.ViewModel.VirtualizationResult.EntryList[entryIndex];
		if (virtualizationEntry.Position_EndExclusiveIndex - virtualizationEntry.Position_StartInclusiveIndex <= 0)
			return;
		
		virtualizationEntry.VirtualizationSpan_StartInclusiveIndex = _createCacheEachSharedParameters.ViewModel.VirtualizationResult.VirtualizationSpanList.Count;
		
		_createCacheEachSharedParameters.ComponentData.LineIndexCache.UsedKeyHashSet.Add(virtualizationEntry.LineIndex);
		
	    var richCharacterSpan = new Span<RichCharacter>(
	        _createCacheEachSharedParameters.Model.RichCharacterList,
	        virtualizationEntry.Position_StartInclusiveIndex,
	        virtualizationEntry.Position_EndExclusiveIndex - virtualizationEntry.Position_StartInclusiveIndex);
	
		var currentDecorationByte = richCharacterSpan[0].DecorationByte;
	    
	    foreach (var richCharacter in richCharacterSpan)
	    {
			if (currentDecorationByte == richCharacter.DecorationByte)
		    {
		        // AppendTextEscaped(textEditorService.__StringBuilder, richCharacter, tabKeyOutput, spaceKeyOutput);
		        switch (richCharacter.Value)
		        {
		            case '\t':
		                _textEditorService.__StringBuilder.Append(_createCacheEachSharedParameters.TabKeyOutput);
		                break;
		            case ' ':
		                _textEditorService.__StringBuilder.Append(_createCacheEachSharedParameters.SpaceKeyOutput);
		                break;
		            case '\r':
		                break;
		            case '\n':
		                break;
		            case '<':
		                _textEditorService.__StringBuilder.Append("&lt;");
		                break;
		            case '>':
		                _textEditorService.__StringBuilder.Append("&gt;");
		                break;
		            case '"':
		                _textEditorService.__StringBuilder.Append("&quot;");
		                break;
		            case '\'':
		                _textEditorService.__StringBuilder.Append("&#39;");
		                break;
		            case '&':
		                _textEditorService.__StringBuilder.Append("&amp;");
		                break;
		            default:
		                _textEditorService.__StringBuilder.Append(richCharacter.Value);
		                break;
		        }
		        // END OF INLINING AppendTextEscaped
		    }
		    else
		    {
		    	_createCacheEachSharedParameters.ViewModel.VirtualizationResult.VirtualizationSpanList.Add(new TextEditorVirtualizationSpan(
		    		cssClass: _createCacheEachSharedParameters.Model.PersistentState.DecorationMapper.Map(currentDecorationByte),
		    		text: _textEditorService.__StringBuilder.ToString()));
		        _textEditorService.__StringBuilder.Clear();
		        
		        // AppendTextEscaped(textEditorService.__StringBuilder, richCharacter, tabKeyOutput, spaceKeyOutput);
		        switch (richCharacter.Value)
		        {
		            case '\t':
		                _textEditorService.__StringBuilder.Append(_createCacheEachSharedParameters.TabKeyOutput);
		                break;
		            case ' ':
		                _textEditorService.__StringBuilder.Append(_createCacheEachSharedParameters.SpaceKeyOutput);
		                break;
		            case '\r':
		                break;
		            case '\n':
		                break;
		            case '<':
		                _textEditorService.__StringBuilder.Append("&lt;");
		                break;
		            case '>':
		                _textEditorService.__StringBuilder.Append("&gt;");
		                break;
		            case '"':
		                _textEditorService.__StringBuilder.Append("&quot;");
		                break;
		            case '\'':
		                _textEditorService.__StringBuilder.Append("&#39;");
		                break;
		            case '&':
		                _textEditorService.__StringBuilder.Append("&amp;");
		                break;
		            default:
		                _textEditorService.__StringBuilder.Append(richCharacter.Value);
		                break;
		        }
		        // END OF INLINING AppendTextEscaped
		        
				currentDecorationByte = richCharacter.DecorationByte;
		    }
	    }
	    
	    /* Final grouping of contiguous characters */
		_createCacheEachSharedParameters.ViewModel.VirtualizationResult.VirtualizationSpanList.Add(new TextEditorVirtualizationSpan(
    		cssClass: _createCacheEachSharedParameters.Model.PersistentState.DecorationMapper.Map(currentDecorationByte),
    		text: _textEditorService.__StringBuilder.ToString()));
		_textEditorService.__StringBuilder.Clear();
		
		// WARNING CODE DUPLICATION (this also exists when copying a virtualizationEntry from cache).
		virtualizationEntry.VirtualizationSpan_EndExclusiveIndex = _createCacheEachSharedParameters.ViewModel.VirtualizationResult.VirtualizationSpanList.Count;
		_createCacheEachSharedParameters.ViewModel.VirtualizationResult.EntryList[entryIndex] = virtualizationEntry;
		
		if (_createCacheEachSharedParameters.ComponentData.LineIndexCache.Map.ContainsKey(virtualizationEntry.LineIndex))
		{
			_createCacheEachSharedParameters.ComponentData.LineIndexCache.Map[virtualizationEntry.LineIndex] = virtualizationEntry;
		}
		else
		{
			_createCacheEachSharedParameters.ComponentData.LineIndexCache.ExistsKeyList.Add(virtualizationEntry.LineIndex);
			_createCacheEachSharedParameters.ComponentData.LineIndexCache.Map.Add(virtualizationEntry.LineIndex, virtualizationEntry);
		}
    }
    
    public void HORIZONTAL_GetScrollbarHorizontalStyleCss(TextEditorVirtualizationResult virtualizationResult)
    {
    	var scrollbarWidthInPixels = virtualizationResult.ViewModel.TextEditorDimensions.Width -
                                     ScrollbarFacts.SCROLLBAR_SIZE_IN_PIXELS -
                                     virtualizationResult.ViewModel.GutterWidthInPixels;
        
        _textEditorService.__StringBuilder.Clear();
        _textEditorService.__StringBuilder.Append("width: ");
        _textEditorService.__StringBuilder.Append(scrollbarWidthInPixels.ToString());
        _textEditorService.__StringBuilder.Append("px;");

        virtualizationResult.HORIZONTAL_ScrollbarCssStyle = _textEditorService.__StringBuilder.ToString();
    }
    
    public void HORIZONTAL_GetSliderHorizontalStyleCss(TextEditorVirtualizationResult virtualizationResult)
    {
    	// Divide by 0 exception
    	if (virtualizationResult.ViewModel.ScrollWidth == 0)
    		return;
    
    	var scrollbarWidthInPixels = virtualizationResult.ViewModel.TextEditorDimensions.Width -
						             ScrollbarFacts.SCROLLBAR_SIZE_IN_PIXELS -
						             virtualizationResult.ViewModel.GutterWidthInPixels;
        
        // Proportional Left
    	var sliderProportionalLeftInPixels = virtualizationResult.ViewModel.ScrollLeft *
            scrollbarWidthInPixels /
            virtualizationResult.ViewModel.ScrollWidth;

		_textEditorService.__StringBuilder.Clear();
		
        _textEditorService.__StringBuilder.Append("bottom: 0; height: ");
        _textEditorService.__StringBuilder.Append(virtualizationResult.ComponentData.ScrollbarSizeCssValue);
        _textEditorService.__StringBuilder.Append("px; ");
        
        _textEditorService.__StringBuilder.Append(" left: ");
        _textEditorService.__StringBuilder.Append(sliderProportionalLeftInPixels.ToString());
        _textEditorService.__StringBuilder.Append("px;");
        
        // Proportional Width
    	var pageWidth = virtualizationResult.ViewModel.TextEditorDimensions.Width;

        var sliderProportionalWidthInPixels = pageWidth *
            scrollbarWidthInPixels /
            virtualizationResult.ViewModel.ScrollWidth;
        
        _textEditorService.__StringBuilder.Append("width: ");
        _textEditorService.__StringBuilder.Append(sliderProportionalWidthInPixels.ToString());
        _textEditorService.__StringBuilder.Append("px;");
        
        virtualizationResult.HORIZONTAL_SliderCssStyle = _textEditorService.__StringBuilder.ToString();
    }
    
    private void DiagnoseIssues(TextEditorVirtualizationResult virtualizationResult, TextEditorModel model, TextEditorViewModel viewModel)
    {
    	if (virtualizationResult.ViewModel is null)
    		return;
    }
    
    /// <summary>
    /// Inlining this instead of invoking the function definition just to see what happens.
    /// </summary>
    /*private void AppendTextEscaped(
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

    public async ValueTask RemeasureAsync(
        TextEditorEditContext editContext,
        TextEditorViewModel viewModel)
    {
        var options = _textEditorService.OptionsApi.GetOptions();
        
        var componentData = viewModel.PersistentState.ComponentData;
        if (componentData is null)
        	return;
		
		var textEditorMeasurements = await _textEditorService.ViewModelApi
			.GetTextEditorMeasurementsAsync(componentData.RowSectionElementId)
			.ConfigureAwait(false);

		viewModel.CharAndLineMeasurements = options.CharAndLineMeasurements;
		viewModel.TextEditorDimensions = textEditorMeasurements;
    }

    public void ForceRender(
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
    public void Dispose(TextEditorEditContext editContext, Key<TextEditorViewModel> viewModelKey)
    {
        _textEditorService.DisposeViewModel(editContext, viewModelKey);
    }
    #endregion
}