using System.Text;
using Walk.Common.RazorLib.Keymaps.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.Lines.Models;
using Walk.TextEditor.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;
using Walk.TextEditor.RazorLib.Cursors.Models;
using Walk.TextEditor.RazorLib.Commands.Models.Defaults;
using Walk.TextEditor.RazorLib.TextEditors.Displays.Internals;

namespace Walk.TextEditor.RazorLib.Keymaps.Models.Defaults;

public class TextEditorKeymapDefault : ITextEditorKeymap
{
	private readonly StringBuilder _indentationBuilder = new();

    public string DisplayName { get; } = nameof(TextEditorKeymapDefault);
    
    public Func<TextEditorEditContext, string?, int?, ValueTask> AltF12Func { get; set; } = (_, _, _) => ValueTask.CompletedTask;
    public Func<TextEditorEditContext, TextEditorModel, TextEditorViewModel, ValueTask> ShiftF12Func { get; set; } = (_, _, _) => ValueTask.CompletedTask;

    public Key<KeymapLayer> GetLayer(bool hasSelection)
    {
        return hasSelection
            ? TextEditorKeymapDefaultFacts.HasSelectionLayer.Key
            : TextEditorKeymapDefaultFacts.DefaultLayer.Key;
    }

    public string GetCursorCssClassString()
    {
        return TextCursorKindFacts.BeamCssClassString;
    }

    public string GetCursorCssStyleString(
        TextEditorModel textEditorModel,
        TextEditorViewModel textEditorViewModel,
        TextEditorOptions textEditorOptions)
    {
        return string.Empty;
    }

    public async ValueTask HandleEvent(
    	TextEditorComponentData componentData,
	    Key<TextEditorViewModel> viewModelKey,
	    string key,
        string code,
        bool ctrlKey,
        bool shiftKey,
        bool altKey,
        bool metaKey)
    {
    	var editContext = new TextEditorEditContext(componentData.TextEditorViewModelSlimDisplay.TextEditorService);

		// An NRE will be caught by the IBackgroundTaskService so don't bother checking 'viewModel is null'.
        var viewModel = editContext.GetViewModelModifier(viewModelKey);

        if (viewModel is null)
            return;

		var menuKind = MenuKind.None;
		var shouldClearTooltip = false;
		var shouldRevealCursor = false;
		var shouldApplySyntaxHighlighting = false;
		
		TextEditorModel? modelModifier;

		if (metaKey)
		{
			switch (code)
			{
				default:
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri, isReadOnly: true);
			    	break;
	    	}
		}
		else if (ctrlKey && altKey)
		{
			switch (code)
			{
				default:
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri, isReadOnly: true);
			    	break;
			}
		}
		else if (ctrlKey)
		{
		    switch (code)
		    {
		    	case "KeyR":
		    		modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
		    		componentData.ThrottleApplySyntaxHighlighting(modelModifier);
		            TextEditorCommandDefaultFunctions.TriggerRemeasure(
		                editContext,
		                viewModel);
		            break;
		    	case "KeyS":
		    		modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
		            TextEditorCommandDefaultFunctions.TriggerSave(
		                editContext,
		                modelModifier,
		                viewModel,
		                componentData.TextEditorViewModelSlimDisplay.CommonService.CommonComponentRenderers,
		                componentData.TextEditorViewModelSlimDisplay.CommonService);
		            
		            componentData.ThrottleApplySyntaxHighlighting(modelModifier);
		            
		            break;
		        case "KeyC":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
		            await TextEditorCommandDefaultFunctions.CopyAsync(
		                editContext,
		                modelModifier,
		                viewModel);
		            break;
		        case "KeyV":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
		            await TextEditorCommandDefaultFunctions.PasteAsync(
		                editContext,
		                modelModifier,
		                viewModel);
		            shouldRevealCursor = true;
		            shouldApplySyntaxHighlighting = true;
		            break;
		        case "KeyX":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
		            await TextEditorCommandDefaultFunctions.CutAsync(
		                editContext,
		                modelModifier,
		                viewModel);
		            shouldRevealCursor = true;
		            shouldApplySyntaxHighlighting = true;
		            break;
		        case "KeyA":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
		            TextEditorCommandDefaultFunctions.SelectAll(
		                editContext,
		                modelModifier,
		                viewModel);
		            break;
		        case "KeyZ":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
		            TextEditorCommandDefaultFunctions.Undo(
		                editContext,
		                modelModifier,
		                viewModel);
		            shouldRevealCursor = true;
		            shouldApplySyntaxHighlighting = true;
		            break;
		        case "KeyY":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
		            TextEditorCommandDefaultFunctions.Redo(
		                editContext,
		                modelModifier,
		                viewModel);
		            shouldRevealCursor = true;
		            shouldApplySyntaxHighlighting = true;
		            break;
		        case "KeyD":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
		            TextEditorCommandDefaultFunctions.Duplicate(
		                editContext,
		                modelModifier,
		                viewModel);
		            shouldRevealCursor = true;
		            shouldApplySyntaxHighlighting = true;
		            break;
		        case "ArrowDown":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri, isReadOnly: true);
		            TextEditorCommandDefaultFunctions.ScrollLineDown(
		                editContext,
		                modelModifier,
		                viewModel);
		            break;
		        case "ArrowUp":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri, isReadOnly: true);
		            TextEditorCommandDefaultFunctions.ScrollLineUp(
		                editContext,
		                modelModifier,
		                viewModel);
		            break;
		        case "PageDown":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri, isReadOnly: true);
					TextEditorCommandDefaultFunctions.CursorMovePageBottom(
		                editContext,
		                modelModifier,
		                viewModel);
		            break;
		        case "PageUp":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri, isReadOnly: true);
					TextEditorCommandDefaultFunctions.CursorMovePageTop(
		                editContext,
		                modelModifier,
		                viewModel);
		            break;
		        case "Slash":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
					await TextEditorCommandDefaultFunctions.ShowTooltipByCursorPositionAsync(
		                editContext,
		                modelModifier,
		                viewModel,
		                componentData.TextEditorViewModelSlimDisplay.TextEditorService,
		                componentData);
		            shouldRevealCursor = true;
		            break;
	            case "KeyM":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
            		CollapsePoint encompassingCollapsePoint = new CollapsePoint(-1, false, string.Empty, -1);;

					foreach (var collapsePoint in viewModel.PersistentState.AllCollapsePointList)
					{
						for (var lineOffset = 0; lineOffset < collapsePoint.EndExclusiveLineIndex - collapsePoint.AppendToLineIndex; lineOffset++)
						{
							if (viewModel.LineIndex == collapsePoint.AppendToLineIndex + lineOffset)
								encompassingCollapsePoint = collapsePoint;
						}
					}
					
	            	if (encompassingCollapsePoint.AppendToLineIndex != -1)
	            	{
	            		_ = TextEditorCommandDefaultFunctions.ToggleCollapsePoint(
		            		encompassingCollapsePoint.AppendToLineIndex,
	            			modelModifier,
	            			viewModel);
	            	}
					
		            shouldRevealCursor = true;
		            break;
		        case "KeyF":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
		        	if (shiftKey)
		        	{
		        		TextEditorCommandDefaultFunctions.PopulateSearchFindAll(
			                editContext,
			                modelModifier,
			                viewModel);
		        	}
		        	else
		        	{
						await TextEditorCommandDefaultFunctions.ShowFindOverlay(
			                editContext,
			                modelModifier,
			                viewModel,
			                componentData.TextEditorViewModelSlimDisplay.TextEditorService.JsRuntimeCommonApi);
			        }
		            
		            break;
	            case "KeyH":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
		        	if (shiftKey)
		        	{
		        		/*TextEditorCommandDefaultFunctions.PopulateSearchFindAll(
			                editContext,
			                modelModifier,
			                viewModel,
			                componentData.FindAllService);*/
		        	}
		        	else
		        	{
		        		viewModel.PersistentState.ShowReplaceButtonInFindOverlay = true;
		        	
						await TextEditorCommandDefaultFunctions.ShowFindOverlay(
			                editContext,
			                modelModifier,
			                viewModel,
			                componentData.TextEditorViewModelSlimDisplay.TextEditorService.JsRuntimeCommonApi);
			        }
		            
		            break;
		        case "ArrowLeft":
	            case "ArrowRight":
	            case "Home":
	            case "End":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri, isReadOnly: true);
					editContext.TextEditorService.ViewModel_MoveCursor(
                		key,
                        code,
                        ctrlKey,
                        shiftKey,
                        altKey,
				        editContext,
				        modelModifier,
				        viewModel);
				        
				    if (viewModel.PersistentState.MenuKind != MenuKind.None)
				    {
				    	TextEditorCommandDefaultFunctions.RemoveDropdown(
					        editContext,
					        viewModel,
					        componentData.TextEditorViewModelSlimDisplay.CommonService);
				    }

	                break;
	            case "Backspace":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
					modelModifier.Delete(
	                    viewModel,
	                    1,
	                    ctrlKey,
	                    TextEditorModel.DeleteKind.Backspace);
	                shouldRevealCursor = true;
					break;
				case "Delete":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
					modelModifier.Delete(
	                    viewModel,
	                    1,
	                    ctrlKey,
	                    TextEditorModel.DeleteKind.Delete);
	                shouldRevealCursor = true;
	                break;
	            case "Enter":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
					var valueToInsert = modelModifier.LineEndKindPreference.AsCharacters();
			
					// Match indentation on newline keystroke
					var line = modelModifier.GetLineInformation(viewModel.LineIndex);
		
					var cursorPositionIndex = line.Position_StartInclusiveIndex + viewModel.ColumnIndex;
					var indentationPositionIndex = line.Position_StartInclusiveIndex;
		
					_indentationBuilder.Clear();
					
					while (indentationPositionIndex < cursorPositionIndex)
					{
						var possibleIndentationChar = modelModifier.RichCharacterList[indentationPositionIndex++].Value;
		
						if (possibleIndentationChar == '\t' || possibleIndentationChar == ' ')
							_indentationBuilder.Append(possibleIndentationChar);
						else
							break;
					}
		
					var indentationLength = _indentationBuilder.Length;
					valueToInsert = _indentationBuilder.ToString() + valueToInsert;
					
					viewModel.SelectionAnchorPositionIndex = -1;
					viewModel.LineIndex = viewModel.LineIndex;
        			viewModel.ColumnIndex = 0;
					
					modelModifier.Insert(
			            valueToInsert,
			            viewModel);
			            
			        if (viewModel.LineIndex > 1)
			        {
			            viewModel.LineIndex--;
			            viewModel.ColumnIndex = indentationLength;
			        }
			            
	                shouldRevealCursor = true;
		            shouldApplySyntaxHighlighting = true;
	                break;
	            case "BracketRight":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
	            	TextEditorCommandDefaultFunctions.GoToMatchingCharacter(
		                editContext,
		                modelModifier,
		                viewModel,
		                shouldSelectText: shiftKey);
	            	shouldRevealCursor = true;
	            	break;
	            case "Space":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
	            	if (shiftKey)
	            	{
	            		await modelModifier.PersistentState.CompilerService.ShowCallingSignature(
							editContext,
							modelModifier,
							viewModel,
							modelModifier.GetPositionIndex(viewModel),
							componentData,
					        modelModifier.PersistentState.ResourceUri);
	            	}
	            	else
	            	{
		            	shouldRevealCursor = true;
		            	shouldClearTooltip = true;
		            	menuKind = MenuKind.AutoCompleteMenu;
			            
			            // TODO: Fix 'shouldApplySyntaxHighlighting = true' for "Space"...
			            // ...It is causing the autocomplete menu to lose focus.
			            // shouldApplySyntaxHighlighting = true;
		            }
	            	break;
	            case "Period":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
	            	await TextEditorCommandDefaultFunctions.QuickActionsSlashRefactor(
				        editContext,
				        modelModifier,
				        viewModel,
				        componentData.TextEditorViewModelSlimDisplay.TextEditorService.JsRuntimeCommonApi,
				        componentData.TextEditorViewModelSlimDisplay.TextEditorService,
				        componentData.TextEditorViewModelSlimDisplay.CommonService);
	            	break;
	            default:
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
		    		break;
		    }
		}
		else if (altKey)
		{
			switch (code)
			{
				case "F12":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
		        	TextEditorCommandDefaultFunctions.GoToDefinition(
		        		editContext,
				        modelModifier,
				        viewModel,
        				new Category("CodeSearchService"),
        				modelModifier.GetPositionIndex(viewModel));
			        break;
				default:
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
			    	break;
	    	}
		}
		else
		{
			switch (code)
			{
				case "PageDown":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri, isReadOnly: true);
					TextEditorCommandDefaultFunctions.ScrollPageDown(
		                editContext,
		                modelModifier,
		                viewModel);
		            break;
		        case "PageUp":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri, isReadOnly: true);
					TextEditorCommandDefaultFunctions.ScrollPageUp(
		                editContext,
		                modelModifier,
		                viewModel);
		            break;
				case "ArrowLeft":
	            case "ArrowDown":
	            case "ArrowUp":
	            case "ArrowRight":
	            case "Home":
	            case "End":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri, isReadOnly: true);
	            	if (("ArrowDown" == code || "ArrowUp" == code) &&
	                    viewModel.PersistentState.MenuKind == MenuKind.AutoCompleteMenu)
	                {
	                	// TODO: Focusing the menu from here isn't working?
	                	await editContext.TextEditorService.JsRuntimeCommonApi.FocusHtmlElementById(
	                		AutocompleteMenu.HTML_ELEMENT_ID,
	                		preventScroll: true);
	                		
	                	componentData.MenuShouldTakeFocus = true;
	                	menuKind = MenuKind.AutoCompleteMenu;
	                	
	                	break;
	                }
	                else
	                {
						editContext.TextEditorService.ViewModel_MoveCursor(
	                		key,
                            code,
                            ctrlKey,
                            shiftKey,
                            altKey,
					        editContext,
					        modelModifier,
					        viewModel);
					        
					    if (viewModel.PersistentState.MenuKind != MenuKind.None)
					    {
					    	TextEditorCommandDefaultFunctions.RemoveDropdown(
						        editContext,
						        viewModel,
						        componentData.TextEditorViewModelSlimDisplay.CommonService);
					    }
					    
					    break;
		            }
		        case "F12":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
		        	if (shiftKey)
		        	{
		        		await ShiftF12Func.Invoke(
		        			editContext,
		        			modelModifier,
		        			viewModel);
		        	}
		        	else
		        	{
			        	TextEditorCommandDefaultFunctions.GoToDefinition(
			        		editContext,
					        modelModifier,
					        viewModel,
	        				new Category("main"),
        				    modelModifier.GetPositionIndex(viewModel));
			        }
			        break;
		        case "F10":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
		        	if (shiftKey)
		        	{
		        		menuKind = MenuKind.ContextMenu;
	                	shouldRevealCursor = true;
                		shouldClearTooltip = true;
					    break;
		        	}
		        	break;
		        case "F7":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
		        	await TextEditorCommandDefaultFunctions.RelatedFilesQuickPick(
				        editContext,
				        modelModifier,
				        viewModel,
				        componentData.TextEditorViewModelSlimDisplay.TextEditorService.JsRuntimeCommonApi,
				        componentData.TextEditorViewModelSlimDisplay.CommonService.EnvironmentProvider,
				        componentData.TextEditorViewModelSlimDisplay.CommonService.FileSystemProvider,
				        componentData.TextEditorViewModelSlimDisplay.TextEditorService,
				        componentData.TextEditorViewModelSlimDisplay.CommonService);
				    break;
		        case "ContextMenu":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
		        	menuKind = MenuKind.ContextMenu;
	                shouldRevealCursor = true;
	                shouldClearTooltip = true;
				    break;
				case "CapsLock":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
					/*
					On Linux the 'CapsLock' to 'Escape' setting is returning:
						event.code == CapsLock
						event.key == Escape
					*/
					if (key == "Escape")
					{
						menuKind = MenuKind.None;
						shouldClearTooltip = true;
						break;
					}
					break;
				case "Escape":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
					menuKind = MenuKind.None;
					shouldClearTooltip = true;
					break;
				case "Backspace":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
					modelModifier.Delete(
	                    viewModel,
	                    1,
	                    ctrlKey,
	                    TextEditorModel.DeleteKind.Backspace);
	                shouldRevealCursor = true;
	                menuKind = MenuKind.None;
	                shouldClearTooltip = true;
					break;
				case "Delete":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
					modelModifier.Delete(
	                    viewModel,
	                    1,
	                    ctrlKey,
	                    TextEditorModel.DeleteKind.Delete);
					shouldRevealCursor = true;
					menuKind = MenuKind.None;
	                shouldClearTooltip = true;
					break;
				case "Enter":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
					var valueToInsert = modelModifier.LineEndKindPreference.AsCharacters();
			
					// Match indentation on newline keystroke
					var line = modelModifier.GetLineInformation(viewModel.LineIndex);
		
					var cursorPositionIndex = line.Position_StartInclusiveIndex + viewModel.ColumnIndex;
					var indentationPositionIndex = line.Position_StartInclusiveIndex;
		
					_indentationBuilder.Clear();
					
					while (indentationPositionIndex < cursorPositionIndex)
					{
						var possibleIndentationChar = modelModifier.RichCharacterList[indentationPositionIndex++].Value;
		
						if (possibleIndentationChar == '\t' || possibleIndentationChar == ' ')
							_indentationBuilder.Append(possibleIndentationChar);
						else
							break;
					}
		
					valueToInsert += _indentationBuilder.ToString();
					
					if (shiftKey)
					{
						viewModel.SelectionAnchorPositionIndex = -1;
						viewModel.LineIndex = viewModel.LineIndex;
    					viewModel.ColumnIndex = modelModifier.GetLineLength(viewModel.LineIndex);
					}
					
					modelModifier.Insert(
			            valueToInsert,
			            viewModel);
			            
	                shouldRevealCursor = true;
	                menuKind = MenuKind.None;
	                shouldClearTooltip = true;
		            shouldApplySyntaxHighlighting = true;
	                break;
				case "Tab":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
					if (TextEditorSelectionHelper.HasSelectedText(viewModel))
		        	{
		        		if (shiftKey)
			        	{
			        		TextEditorCommandDefaultFunctions.IndentLess(
				                editContext,
				                modelModifier,
				                viewModel);
			        	}
			        	else
			        	{
			        		TextEditorCommandDefaultFunctions.IndentMore(
				                editContext,
				                modelModifier,
				                viewModel);
			        	}
			        	
			        	shouldRevealCursor = true;
			        	break;
		        	}
					else
					{
						if (shiftKey)
			        	{
			        		TextEditorCommandDefaultFunctions.IndentLess(
				                editContext,
				                modelModifier,
				                viewModel);
			        	}
			        	else
			        	{
			        		editContext.TextEditorService.InsertTab(editContext, modelModifier,  viewModel);
			                
			                menuKind = MenuKind.None;
			                shouldClearTooltip = true;
		            		shouldApplySyntaxHighlighting = true;
			            }
			            
			            shouldRevealCursor = true;
		            }
	                break;
				case "Space":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
	            	modelModifier.Insert(
	                    " ",
	                    viewModel);
	                    
	                shouldRevealCursor = true;
	                shouldClearTooltip = true;
	                menuKind = MenuKind.None;
		            shouldApplySyntaxHighlighting = true;
	                break;
				case "Backquote":
				case "BracketLeft":
				case "BracketRight":
				case "Backslash":
				case "Semicolon":
				case "Quote":
				case "Comma":
				case "Period":
				case "Slash":
				case "Minus":
				case "Equal":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
					modelModifier.Insert(
	                    key,
	                    viewModel);
	                
	                if (code == "Minus" && shiftKey)
	                	menuKind = MenuKind.AutoCompleteMenu;
	                else if (code == "Period" && !shiftKey)
	                	menuKind = MenuKind.AutoCompleteMenu;
	                else
	                	menuKind = MenuKind.None;
	                
	                shouldRevealCursor = true;
	                shouldClearTooltip = true;
	                break;
				case "Digit0":
				case "Digit1":
				case "Digit2":
				case "Digit3":
				case "Digit4":
				case "Digit5":
				case "Digit6":
				case "Digit7":
				case "Digit8":
				case "Digit9":
	            case "KeyA":
	            case "KeyB":
	            case "KeyC":
	            case "KeyD":
	            case "KeyE":
	            case "KeyF":
	            case "KeyG":
	            case "KeyH":
	            case "KeyI":
	            case "KeyJ":
	            case "KeyK":
	            case "KeyL":
	            case "KeyM":
	            case "KeyN":
	            case "KeyO":
	            case "KeyP":
	            case "KeyQ":
	            case "KeyR":
	            case "KeyS":
	            case "KeyT":
	            case "KeyU":
	            case "KeyV":
	            case "KeyW":
	            case "KeyX":
	            case "KeyY":
	            case "KeyZ":
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
	            	modelModifier.Insert(
	                    key,
	                    viewModel);
	                shouldRevealCursor = true;
	                menuKind = MenuKind.AutoCompleteMenu;
	                shouldClearTooltip = true;
	                break;
				default:
		        	modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
		        	break;
			}
		}
		
		if (viewModel.PersistentState.MenuKind != menuKind)
		{
			switch (menuKind)
			{
				case MenuKind.None:
					TextEditorCommandDefaultFunctions.RemoveDropdown(
				        editContext,
				        viewModel,
			        	componentData.TextEditorViewModelSlimDisplay.CommonService);
			        break;
				case MenuKind.ContextMenu:
					TextEditorCommandDefaultFunctions.ShowContextMenu(
				        editContext,
				        modelModifier,
				        viewModel,
				        componentData.TextEditorViewModelSlimDisplay.CommonService,
				        componentData);
			        break;
				case MenuKind.AutoCompleteMenu:
					TextEditorCommandDefaultFunctions.ShowAutocompleteMenu(
		        		editContext,
				        modelModifier,
				        viewModel,
				        componentData.TextEditorViewModelSlimDisplay.CommonService,
				        componentData);
			        break;
			}
		}
		
		if (shouldClearTooltip)
		{
			if (viewModel.PersistentState.TooltipModel is not null)
			{
				viewModel.PersistentState.TooltipModel = null;
		        componentData.TextEditorViewModelSlimDisplay.CommonService.SetTooltipModel(viewModel.PersistentState.TooltipModel);
			}
		}
		
		if (shouldRevealCursor)
		{
			viewModel.PersistentState.ShouldRevealCursor = true;
		}
		
		if (shouldApplySyntaxHighlighting)
		{
			componentData.ThrottleApplySyntaxHighlighting(modelModifier);
		}
		
		// TODO: Do this code first so the user gets immediate UI feedback in the event that
		//       their keydown code takes a long time?
		editContext.TextEditorService.ViewModel_StopCursorBlinking();
		
		await editContext.TextEditorService
			.FinalizePost(editContext)
			.ConfigureAwait(false);
    }
}
