using System.Diagnostics;
using System.Text;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.Keyboards.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib.Characters.Models;
using Walk.TextEditor.RazorLib.Cursors.Models;
using Walk.TextEditor.RazorLib.Exceptions;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;

namespace Walk.TextEditor.RazorLib.TextEditors.Models;

public sealed class TextEditorViewModelApi
{
    private readonly TextEditorService _textEditorService;
    private readonly CommonUtilityService _commonUtilityService;
    
    public TextEditorViewModelApi(
        TextEditorService textEditorService,
        CommonUtilityService commonUtilityService)
    {
        _textEditorService = textEditorService;
        _commonUtilityService = commonUtilityService;
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
			_commonUtilityService,
			TextEditorVirtualizationResult.ConstructEmpty(),
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

		viewModel.SetScrollLeft((int)Math.Floor(scrollLeftInPixels), viewModel.PersistentState.TextEditorDimensions);

		viewModel.SetScrollTop((int)Math.Floor(scrollTopInPixels), viewModel.PersistentState.TextEditorDimensions);
    }
        
    public void SetScrollPositionLeft(
        TextEditorEditContext editContext,
        TextEditorViewModel viewModel,
        double scrollLeftInPixels)
    {
    	viewModel.ScrollWasModified = true;

		viewModel.SetScrollLeft((int)Math.Floor(scrollLeftInPixels), viewModel.PersistentState.TextEditorDimensions);
    }
    
    public void SetScrollPositionTop(
        TextEditorEditContext editContext,
        TextEditorViewModel viewModel,
        double scrollTopInPixels)
    {
    	viewModel.ScrollWasModified = true;

		viewModel.SetScrollTop((int)Math.Floor(scrollTopInPixels), viewModel.PersistentState.TextEditorDimensions);
    }

    public void MutateScrollVerticalPosition(
        TextEditorEditContext editContext,
        TextEditorViewModel viewModel,
        double pixels)
    {
        viewModel.ScrollWasModified = true;

        viewModel.MutateScrollTop((int)Math.Ceiling(pixels), viewModel.PersistentState.TextEditorDimensions);
    }

    public void MutateScrollHorizontalPosition(
        TextEditorEditContext editContext,
        TextEditorViewModel viewModel,
        double pixels)
    {
        viewModel.ScrollWasModified = true;

		viewModel.MutateScrollLeft((int)Math.Ceiling(pixels), viewModel.PersistentState.TextEditorDimensions);
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
        var targetScrollTop = (lineIndex - hiddenLineCount) * viewModel.PersistentState.CharAndLineMeasurements.LineHeight;
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
        	SetScrollPositionBoth(editContext, viewModel, targetScrollLeft, targetScrollTop);
        else if (targetScrollTop != -1)
        	SetScrollPositionTop(editContext, viewModel, targetScrollTop);
    	else
        	SetScrollPositionLeft(editContext, viewModel, targetScrollLeft);
    }

    public ValueTask FocusPrimaryCursorAsync(string primaryCursorContentId)
    {
        return _commonUtilityService.JsRuntimeCommonApi
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
        if (viewModel.Virtualization.Count > 0)
        {
            var firstEntry = viewModel.Virtualization.EntryList[0];
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
        if (viewModel.Virtualization.Count > 0)
        {
            var lastEntry = viewModel.Virtualization.EntryList[viewModel.Virtualization.Count - 1];
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

		var totalHeight = (modelModifier.LineEndList.Count - viewModel.PersistentState.HiddenLineIndexHashSet.Count) *
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
		
		viewModel.PersistentState.GutterWidth = GetGutterWidthInPixels(modelModifier, viewModel, componentData);
		
		var absDiffScrollLeft = Math.Abs(componentData.LineIndexCache.ScrollLeftMarker - viewModel.PersistentState.ScrollLeft);
		var useCache = absDiffScrollLeft < 0.01 && componentData.LineIndexCache.ViewModelKeyMarker == viewModel.PersistentState.ViewModelKey;
		
		if (!useCache)
		    componentData.LineIndexCache.IsInvalid = true;
		
		if (componentData.LineIndexCache.IsInvalid)
			componentData.LineIndexCache.Clear();
		else
			componentData.LineIndexCache.UsedKeyHashSet.Clear();
		
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
		
		_textEditorService.__StringBuilder.Clear();
		
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

			if (viewModel.PersistentState.HiddenLineIndexHashSet.Contains(lineIndex))
			{
				hiddenCount++;
				continue;
			}
			
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
				
				topCssValue = (topInPixels - (viewModel.PersistentState.CharAndLineMeasurements.LineHeight * hiddenCount)).ToString();
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
				topCssValue = ((lineIndex * viewModel.PersistentState.CharAndLineMeasurements.LineHeight) - (viewModel.PersistentState.CharAndLineMeasurements.LineHeight * hiddenCount)).ToString();
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
        		                _textEditorService.__StringBuilder.Append(tabKeyOutput);
        		                break;
        		            case ' ':
        		                _textEditorService.__StringBuilder.Append(spaceKeyOutput);
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
        		    	viewModel.Virtualization.VirtualizationSpanList.Add(new TextEditorVirtualizationSpan(
        		    		cssClass: modelModifier.PersistentState.DecorationMapper.Map(currentDecorationByte),
        		    		text: _textEditorService.__StringBuilder.ToString()));
        		        _textEditorService.__StringBuilder.Clear();
        		        
        		        // AppendTextEscaped(textEditorService.__StringBuilder, richCharacter, tabKeyOutput, spaceKeyOutput);
        		        switch (richCharacter.Value)
        		        {
        		            case '\t':
        		                _textEditorService.__StringBuilder.Append(tabKeyOutput);
        		                break;
        		            case ' ':
        		                _textEditorService.__StringBuilder.Append(spaceKeyOutput);
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
        		viewModel.Virtualization.VirtualizationSpanList.Add(new TextEditorVirtualizationSpan(
            		cssClass: modelModifier.PersistentState.DecorationMapper.Map(currentDecorationByte),
            		text: _textEditorService.__StringBuilder.ToString()));
        		_textEditorService.__StringBuilder.Clear();

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
    		
    	    _textEditorService.__StringBuilder.Clear();
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
            viewModel!.PersistentState.CharAndLineMeasurements.CharacterWidth;

        gutterWidthInPixels += TextEditorModel.GUTTER_PADDING_LEFT_IN_PIXELS + TextEditorModel.GUTTER_PADDING_RIGHT_IN_PIXELS;

        return (int)Math.Ceiling(gutterWidthInPixels);
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

		viewModel.PersistentState.CharAndLineMeasurements = options.CharAndLineMeasurements;
		viewModel.PersistentState.TextEditorDimensions = textEditorMeasurements;
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