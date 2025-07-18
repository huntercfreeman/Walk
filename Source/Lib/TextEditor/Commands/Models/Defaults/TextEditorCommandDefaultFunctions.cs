using System.Text;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Keyboards.Models;
using Walk.Common.RazorLib.JsRuntimes.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Menus.Displays;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Keymaps.Models;
using Walk.TextEditor.RazorLib.JavaScriptObjects.Models;
using Walk.TextEditor.RazorLib.Cursors.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;
using Walk.TextEditor.RazorLib.Characters.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.Installations.Models;
using Walk.TextEditor.RazorLib.Events.Models;

namespace Walk.TextEditor.RazorLib.Commands.Models.Defaults;

public class TextEditorCommandDefaultFunctions
{
    public static void DoNothingDiscard()
    {
        return;
    }

    public static async ValueTask CopyAsync(
	    TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel)
    {
        var selectedText = TextEditorSelectionHelper.GetSelectedText(viewModel, modelModifier);
        selectedText ??= modelModifier.GetLineTextRange(viewModel.LineIndex, 1);

        await editContext.TextEditorService.CommonService.SetClipboard(selectedText).ConfigureAwait(false);
        await viewModel.FocusAsync().ConfigureAwait(false);
    }

    public static async ValueTask CutAsync(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel)
    {
    	if (!TextEditorSelectionHelper.HasSelectedText(viewModel))
        {
            var positionIndex = modelModifier.GetPositionIndex(viewModel);
            var lineInformation = modelModifier.GetLineInformationFromPositionIndex(positionIndex);

            viewModel.SelectionAnchorPositionIndex = lineInformation.Position_StartInclusiveIndex;
            viewModel.SelectionEndingPositionIndex = lineInformation.Position_EndExclusiveIndex;
        }

        var selectedText = TextEditorSelectionHelper.GetSelectedText(viewModel, modelModifier) ?? string.Empty;
        await editContext.TextEditorService.CommonService.SetClipboard(selectedText).ConfigureAwait(false);

        await viewModel.FocusAsync().ConfigureAwait(false);

        modelModifier.HandleKeyboardEvent(
            new KeymapArgs { Key = KeyboardKeyFacts.MetaKeys.DELETE },
            viewModel);
    }

    public static async ValueTask PasteAsync(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel)
    {
    	var clipboard = await editContext.TextEditorService.CommonService.ReadClipboard().ConfigureAwait(false);
        modelModifier.Insert(clipboard, viewModel);
    }

    public static void TriggerSave(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel,
        ICommonComponentRenderers commonComponentRenderers,
        CommonService commonService)
    {
    	if (viewModel.PersistentState.OnSaveRequested is null)
    	{
    		NotificationHelper.DispatchError(
		        nameof(TriggerSave),
		        $"{nameof(TriggerSave)} was null",
				commonService,
		        TimeSpan.FromSeconds(7));
    	}
    	else
    	{
    		viewModel.PersistentState.OnSaveRequested.Invoke(modelModifier);
        	modelModifier.SetIsDirtyFalse();
    	}
    }

    public static void SelectAll(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel)
    {
    	viewModel.SelectionAnchorPositionIndex = 0;
        viewModel.SelectionEndingPositionIndex = modelModifier.CharCount;
    }

    public static void Undo(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel)
    {
        modelModifier.UndoEditWithCursor(viewModel);
    }

    public static void Redo(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel)
    {
		modelModifier.RedoEditWithCursor(viewModel);
    }

    public static void TriggerRemeasure(
        TextEditorEditContext editContext,
        TextEditorViewModel viewModel)
    {
    	editContext.TextEditorService.CommonService.AppDimension_NotifyIntraAppResize();
    }

    public static void ScrollLineDown(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel)
    {
        editContext.TextEditorService.ViewModel_MutateScrollVerticalPosition(
    		editContext,
	        viewModel,
	        viewModel.PersistentState.CharAndLineMeasurements.LineHeight);
    }

    public static void ScrollLineUp(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel)
    {
        editContext.TextEditorService.ViewModel_MutateScrollVerticalPosition(
            editContext,
	        viewModel,
	        -1 * viewModel.PersistentState.CharAndLineMeasurements.LineHeight);
    }

    public static void ScrollPageDown(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel)
    {
        editContext.TextEditorService.ViewModel_MutateScrollVerticalPosition(
            editContext,
	        viewModel,
	        viewModel.PersistentState.TextEditorDimensions.Height);
    }

    public static void ScrollPageUp(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel)
    {
        editContext.TextEditorService.ViewModel_MutateScrollVerticalPosition(
            editContext,
	        viewModel,
	        -1 * viewModel.PersistentState.TextEditorDimensions.Height);
    }

    public static void CursorMovePageBottom(
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

    public static void CursorMovePageTop(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel)
    {
        if (viewModel.Virtualization.Count > 0)
        {
        	var firstEntry = viewModel.Virtualization.EntryList[0];
            viewModel.LineIndex = firstEntry.LineIndex;
            viewModel.ColumnIndex = 0;
        }
    }

    public static void Duplicate(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel)
    {
        var selectedText = TextEditorSelectionHelper.GetSelectedText(viewModel, modelModifier);
        
        var before_LineIndex = viewModel.LineIndex;
		var before_ColumnIndex = viewModel.ColumnIndex;
		var before_PreferredColumnIndex = viewModel.PreferredColumnIndex;
		var before_SelectionAnchorPositionIndex = viewModel.SelectionAnchorPositionIndex;
		var before_SelectionEndingPositionIndex = viewModel.SelectionEndingPositionIndex;
		
		viewModel.SelectionAnchorPositionIndex = -1;
		viewModel.SelectionEndingPositionIndex = 0;

        if (selectedText is null)
        {
            // Select line
            selectedText = modelModifier.GetLineTextRange(viewModel.LineIndex, 1);
			viewModel.SetColumnIndexAndPreferred(0);
        }

        modelModifier.Insert(
            selectedText,
            viewModel);
    
    	viewModel.SelectionAnchorPositionIndex = before_SelectionAnchorPositionIndex;
		viewModel.SelectionEndingPositionIndex = before_SelectionEndingPositionIndex;
		
		viewModel.ColumnIndex = before_ColumnIndex;
		viewModel.PreferredColumnIndex = before_PreferredColumnIndex;
    }

    public static void IndentMore(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel)
    {
        if (!TextEditorSelectionHelper.HasSelectedText(viewModel))
            return;

        var selectionBoundsInPositionIndexUnits = TextEditorSelectionHelper.GetSelectionBounds(viewModel);

        var selectionBoundsInLineIndexUnits = TextEditorSelectionHelper.ConvertSelectionOfPositionIndexUnitsToLineIndexUnits(
            modelModifier,
            selectionBoundsInPositionIndexUnits);
            
        var before_LineIndex = viewModel.LineIndex;
		var before_ColumnIndex = viewModel.ColumnIndex;
		var before_PreferredColumnIndex = viewModel.PreferredColumnIndex;
		var before_SelectionAnchorPositionIndex = viewModel.SelectionAnchorPositionIndex;
		var before_SelectionEndingPositionIndex = viewModel.SelectionEndingPositionIndex;
		
		viewModel.SelectionAnchorPositionIndex = -1;
		viewModel.SelectionEndingPositionIndex = 0;

        for (var i = selectionBoundsInLineIndexUnits.Line_LowerInclusiveIndex;
             i < selectionBoundsInLineIndexUnits.Line_UpperExclusiveIndex;
             i++)
        {
        	viewModel.LineIndex = i;
        	viewModel.SetColumnIndexAndPreferred(0);

            editContext.TextEditorService.InsertTab(editContext, modelModifier,  viewModel);
        }

        viewModel.LineIndex = before_LineIndex;
		viewModel.ColumnIndex = before_ColumnIndex;
		viewModel.PreferredColumnIndex = before_PreferredColumnIndex;
		viewModel.SelectionAnchorPositionIndex = before_SelectionAnchorPositionIndex;
		viewModel.SelectionEndingPositionIndex = before_SelectionEndingPositionIndex;
        
        int lowerBoundPositionIndexChange;
        if (editContext.TextEditorService.Options_GetOptions().TabKeyBehavior)
        {
		    lowerBoundPositionIndexChange = 1;
		    
		    var upperBoundPositionIndexChange = selectionBoundsInLineIndexUnits.Line_UpperExclusiveIndex -
	            selectionBoundsInLineIndexUnits.Line_LowerInclusiveIndex;
	
	        if (viewModel.SelectionAnchorPositionIndex < viewModel.SelectionEndingPositionIndex)
	        {
	            viewModel.SelectionAnchorPositionIndex += lowerBoundPositionIndexChange;
	            viewModel.SelectionEndingPositionIndex += upperBoundPositionIndexChange;
	        }
	        else
	        {
	            viewModel.SelectionAnchorPositionIndex += upperBoundPositionIndexChange;
	            viewModel.SelectionEndingPositionIndex += lowerBoundPositionIndexChange;
	        }
	
	        viewModel.SetColumnIndexAndPreferred(1 + viewModel.ColumnIndex);
		}
        else
        {
            lowerBoundPositionIndexChange = editContext.TextEditorService.TabKeyBehavior_TabSpaces.Length;
            
            var upperBoundPositionIndexChange = selectionBoundsInLineIndexUnits.Line_UpperExclusiveIndex -
	            selectionBoundsInLineIndexUnits.Line_LowerInclusiveIndex;
	            
	        upperBoundPositionIndexChange *= editContext.TextEditorService.TabKeyBehavior_TabSpaces.Length;
	
	        if (viewModel.SelectionAnchorPositionIndex < viewModel.SelectionEndingPositionIndex)
	        {
	            viewModel.SelectionAnchorPositionIndex += lowerBoundPositionIndexChange;
	            viewModel.SelectionEndingPositionIndex += upperBoundPositionIndexChange;
	        }
	        else
	        {
	            viewModel.SelectionAnchorPositionIndex += upperBoundPositionIndexChange;
	            viewModel.SelectionEndingPositionIndex += lowerBoundPositionIndexChange;
	        }
	
	        viewModel.SetColumnIndexAndPreferred(editContext.TextEditorService.TabKeyBehavior_TabSpaces.Length + viewModel.ColumnIndex);
        }
    }

    public static void IndentLess(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel)
    {
    	(int Line_LowerIndexInclusive, int Line_UpperIndexExclusive) selectionBoundsInLineIndexUnits;
    
    	if (viewModel.SelectionAnchorPositionIndex == -1)
    	{
    		selectionBoundsInLineIndexUnits = (viewModel.LineIndex, viewModel.LineIndex + 1);
    	}
    	else
    	{
	        var selectionBoundsInPositionIndexUnits = TextEditorSelectionHelper.GetSelectionBounds(viewModel);
	
	        selectionBoundsInLineIndexUnits = TextEditorSelectionHelper.ConvertSelectionOfPositionIndexUnitsToLineIndexUnits(
	            modelModifier,
	            selectionBoundsInPositionIndexUnits);
        }
        
        var before_LineIndex = viewModel.LineIndex;
		var before_ColumnIndex = viewModel.ColumnIndex;
		var before_PreferredColumnIndex = viewModel.PreferredColumnIndex;
		var before_SelectionAnchorPositionIndex = viewModel.SelectionAnchorPositionIndex;
		var before_SelectionEndingPositionIndex = viewModel.SelectionEndingPositionIndex;
		
		viewModel.SelectionAnchorPositionIndex = -1;
		viewModel.SelectionEndingPositionIndex = 0;

        bool isFirstLoop = true;
        var tabWidth = editContext.TextEditorService.Options_GetOptions().TabWidth;

        for (var i = selectionBoundsInLineIndexUnits.Line_LowerIndexInclusive;
             i < selectionBoundsInLineIndexUnits.Line_UpperIndexExclusive;
             i++)
        {
            var rowPositionIndex = modelModifier.GetPositionIndex(i, 0);
            var characterReadCount = tabWidth;
            var lengthOfLine = modelModifier.GetLineLength(i);

            characterReadCount = Math.Min(lengthOfLine, characterReadCount);

            var readResult = modelModifier.GetString(rowPositionIndex, characterReadCount);
            var removeCharacterCount = 0;

            if (readResult.StartsWith(KeyboardKeyFacts.WhitespaceCharacters.TAB))
            {
                removeCharacterCount = 1;

				viewModel.LineIndex = i;
				viewModel.SetColumnIndexAndPreferred(0);

                modelModifier.DeleteByRange(
                    removeCharacterCount, // Delete a single "Tab" character
                    viewModel);
            }
            else if (readResult.StartsWith(KeyboardKeyFacts.WhitespaceCharacters.SPACE))
            {
            	viewModel.LineIndex = i;
				viewModel.SetColumnIndexAndPreferred(0);
				
                var contiguousSpaceCount = 0;

                foreach (var character in readResult)
                {
                    if (character == KeyboardKeyFacts.WhitespaceCharacters.SPACE)
                        contiguousSpaceCount++;
                }

                removeCharacterCount = contiguousSpaceCount;

                modelModifier.DeleteByRange(
                    removeCharacterCount,
                    viewModel);
            }

            // Modify the lower bound of user's text selection
            if (isFirstLoop)
            {
                isFirstLoop = false;

                if (before_SelectionAnchorPositionIndex < before_SelectionEndingPositionIndex)
                    before_SelectionAnchorPositionIndex -= removeCharacterCount;
                else
                    before_SelectionEndingPositionIndex -= removeCharacterCount;
            }

            // Modify the upper bound of user's text selection
            if (before_SelectionAnchorPositionIndex < before_SelectionEndingPositionIndex)
                before_SelectionEndingPositionIndex -= removeCharacterCount;
            else
                before_SelectionAnchorPositionIndex -= removeCharacterCount;

            // Modify the column index of user's cursor
            if (i == before_LineIndex)
            {
                var nextColumnIndex = before_ColumnIndex - removeCharacterCount;

                before_LineIndex = before_LineIndex;
                before_ColumnIndex = Math.Max(0, nextColumnIndex);
            }
        }
        
        viewModel.LineIndex = before_LineIndex;
		viewModel.ColumnIndex = before_ColumnIndex;
		viewModel.PreferredColumnIndex = before_PreferredColumnIndex;
		viewModel.SelectionAnchorPositionIndex = before_SelectionAnchorPositionIndex;
		viewModel.SelectionEndingPositionIndex = before_SelectionEndingPositionIndex;
    }

    public static void ClearTextSelection(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel)
    {
        viewModel.SelectionAnchorPositionIndex = -1;
    }

    public static void NewLineBelow(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel)
    {
        viewModel.SelectionAnchorPositionIndex = -1;

        var lengthOfLine = modelModifier.GetLineLength(viewModel.LineIndex);

        viewModel.LineIndex = viewModel.LineIndex;
        viewModel.ColumnIndex = lengthOfLine;
        
        // NOTE: keep the value to insert as '\n' because this will be changed to the user's
        //       preferred line ending upon insertion.
        var valueToInsert = "\n";
        
        // GOAL: Match indentation on newline keystroke (2024-07-07)
        {
			var line = modelModifier.GetLineInformation(viewModel.LineIndex);

			var cursorPositionIndex = line.Position_StartInclusiveIndex + viewModel.ColumnIndex;
			var indentationPositionIndex = line.Position_StartInclusiveIndex;

			var indentationBuilder = new StringBuilder();

			while (indentationPositionIndex < cursorPositionIndex)
			{
				var possibleIndentationChar = modelModifier.RichCharacterList[indentationPositionIndex++].Value;

				if (possibleIndentationChar == '\t' || possibleIndentationChar == ' ')
					indentationBuilder.Append(possibleIndentationChar);
				else
					break;
			}

			valueToInsert += indentationBuilder.ToString();
        }

        modelModifier.Insert(valueToInsert, viewModel);
    }

    public static void NewLineAbove(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel)
    {
        viewModel.SelectionAnchorPositionIndex = -1;
            
        var originalColumnIndex = viewModel.ColumnIndex;

        viewModel.LineIndex = viewModel.LineIndex;
        viewModel.ColumnIndex = 0;
        
        // NOTE: keep the value to insert as '\n' because this will be changed to the user's
        //       preferred line ending upon insertion.
        var valueToInsert = "\n";
        
        var indentationLength = 0;
        
        // GOAL: Match indentation on newline keystroke (2024-07-07)
        {
			var line = modelModifier.GetLineInformation(viewModel.LineIndex);

			var cursorPositionIndex = line.Position_StartInclusiveIndex + originalColumnIndex;
			var indentationPositionIndex = line.Position_StartInclusiveIndex;

			var indentationBuilder = new StringBuilder();

			while (indentationPositionIndex < cursorPositionIndex)
			{
				var possibleIndentationChar = modelModifier.RichCharacterList[indentationPositionIndex++].Value;

				if (possibleIndentationChar == '\t' || possibleIndentationChar == ' ')
					indentationBuilder.Append(possibleIndentationChar);
				else
					break;
			}

			valueToInsert = indentationBuilder.ToString() + valueToInsert;
			indentationLength = indentationBuilder.Length;
        }

        modelModifier.Insert(valueToInsert, viewModel);

        if (viewModel.LineIndex > 1)
        {
            viewModel.LineIndex--;
            viewModel.ColumnIndex = indentationLength;
        }
    }
    
    public static void MoveLineDown(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel)
    {
        var lineIndexOriginal = viewModel.LineIndex;
        var columnIndexOriginal = viewModel.ColumnIndex;

		var nextLineIndex = lineIndexOriginal + 1;
		var nextLineInformation = modelModifier.GetLineInformation(nextLineIndex);

		// Insert
		{
			var currentLineContent = modelModifier.GetLineTextRange(lineIndexOriginal, 1);
		
			viewModel.LineIndex = nextLineIndex + 1;
			viewModel.ColumnIndex = 0;
			
			modelModifier.Insert(
				value: currentLineContent,
				viewModel);
		}

		// Delete
		{
			viewModel.LineIndex = lineIndexOriginal;
			viewModel.ColumnIndex = 0;
			
			var currentLineInformation = modelModifier.GetLineInformation(viewModel.LineIndex);
			var columnCount = currentLineInformation.Position_EndExclusiveIndex -
				currentLineInformation.Position_StartInclusiveIndex;

			modelModifier.Delete(
		        viewModel,
		        columnCount,
		        false,
		        TextEditorModel.DeleteKind.Delete);
		}
		
		viewModel.LineIndex = lineIndexOriginal + 1;
		viewModel.ColumnIndex = 0;
    }
    
    public static void MoveLineUp(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel)
    {
        var lineIndexOriginal = viewModel.LineIndex;
		var columnIndexOriginal = viewModel.ColumnIndex;
			
		var previousLineIndex = lineIndexOriginal - 1;
		var previousLineInformation = modelModifier.GetLineInformation(previousLineIndex);

		// Insert
		{
			var currentLineContent = modelModifier.GetLineTextRange(lineIndexOriginal, 1);
		
			viewModel.LineIndex = previousLineIndex;
			viewModel.ColumnIndex = 0;

			modelModifier.Insert(
				value: currentLineContent,
				viewModel);
		}

		// Delete
		{
			// Add 1 because a line was inserted
			viewModel.LineIndex = lineIndexOriginal + 1;
			viewModel.ColumnIndex = 0;
			
			var currentLineInformation = modelModifier.GetLineInformation(viewModel.LineIndex);
			var columnCount = currentLineInformation.Position_EndExclusiveIndex -
				currentLineInformation.Position_StartInclusiveIndex;

			modelModifier.Delete(
		        viewModel,
		        columnCount,
		        false,
		        TextEditorModel.DeleteKind.Delete);
		}
		
		viewModel.LineIndex = lineIndexOriginal - 1;
		viewModel.ColumnIndex = 0;
    }

    public static void GoToMatchingCharacter(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel,
        bool shouldSelectText)
    {
        var cursorPositionIndex = modelModifier.GetPositionIndex(viewModel);

        if (shouldSelectText)
        {
            if (!TextEditorSelectionHelper.HasSelectedText(viewModel))
                viewModel.SelectionAnchorPositionIndex = cursorPositionIndex;
        }
        else
        {
            viewModel.SelectionAnchorPositionIndex = -1;
        }

        var previousCharacter = modelModifier.GetCharacter(cursorPositionIndex - 1);
        var currentCharacter = modelModifier.GetCharacter(cursorPositionIndex);

        char? characterToMatch = null;
        char? match = null;

        var fallbackToPreviousCharacter = false;

        if (CharacterKindHelper.CharToCharacterKind(currentCharacter) == CharacterKind.Punctuation)
        {
            // Prefer current character
            match = KeyboardKeyFacts.MatchPunctuationCharacter(currentCharacter);

            if (match is not null)
                characterToMatch = currentCharacter;
        }

        if (characterToMatch is null && CharacterKindHelper.CharToCharacterKind(previousCharacter) == CharacterKind.Punctuation)
        {
            // Fallback to the previous current character
            match = KeyboardKeyFacts.MatchPunctuationCharacter(previousCharacter);

            if (match is not null)
            {
                characterToMatch = previousCharacter;
                fallbackToPreviousCharacter = true;
            }
        }

        if (characterToMatch is null || match is null)
            return;

        var directionToFindMatchingPunctuationCharacter = KeyboardKeyFacts.DirectionToFindMatchingPunctuationCharacter(
            characterToMatch.Value);

        if (directionToFindMatchingPunctuationCharacter is null)
            return;

        var unmatchedCharacters = fallbackToPreviousCharacter && -1 == directionToFindMatchingPunctuationCharacter
            ? 0
            : 1;

        while (true)
        {
            KeymapArgs keymapArgs;

            if (directionToFindMatchingPunctuationCharacter == -1)
            {
                keymapArgs = new KeymapArgs
                {
                    Key = KeyboardKeyFacts.MovementKeys.ARROW_LEFT,
                    ShiftKey = shouldSelectText,
                };
            }
            else
            {
                keymapArgs = new KeymapArgs
                {
                    Key = KeyboardKeyFacts.MovementKeys.ARROW_RIGHT,
                    ShiftKey = shouldSelectText,
                };
            }

            editContext.TextEditorService.ViewModel_MoveCursorUnsafe(
        		keymapArgs.Key,
                keymapArgs.Code,
                keymapArgs.CtrlKey,
                keymapArgs.ShiftKey,
                keymapArgs.AltKey,
		        editContext,
		        modelModifier,
		        viewModel);

            var positionIndex = modelModifier.GetPositionIndex(viewModel);
            var characterAt = modelModifier.GetCharacter(positionIndex);

            if (characterAt == match)
                unmatchedCharacters--;
            else if (characterAt == characterToMatch)
                unmatchedCharacters++;

            if (unmatchedCharacters == 0)
                break;

            if (positionIndex <= 0 || positionIndex >= modelModifier.CharCount)
                break;
        }

        if (shouldSelectText)
            viewModel.SelectionEndingPositionIndex = modelModifier.GetPositionIndex(viewModel);
    }

    public static async ValueTask RelatedFilesQuickPick(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel,
        WalkCommonJavaScriptInteropApi jsRuntimeCommonApi,
        IEnvironmentProvider environmentProvider,
        IFileSystemProvider fileSystemProvider,
        TextEditorService textEditorService,
        CommonService commonService)
    {
    	var componentData = viewModel.PersistentState.ComponentData;
    	if (componentData is null)
    		return;
    
		var cursorDimensions = await jsRuntimeCommonApi
			.MeasureElementById(componentData.PrimaryCursorContentId)
			.ConfigureAwait(false);

		var resourceAbsolutePath = environmentProvider.AbsolutePathFactory(modelModifier.PersistentState.ResourceUri.Value, false);
		var parentDirectoryAbsolutePath = environmentProvider.AbsolutePathFactory(resourceAbsolutePath.ParentDirectory, true);
	
		var siblingFileStringList = Array.Empty<string>();
		
		try
		{
			siblingFileStringList = await fileSystemProvider.Directory
				.GetFilesAsync(parentDirectoryAbsolutePath.Value)
				.ConfigureAwait(false);
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
		}
		
		var menuOptionList = new List<MenuOptionRecord>();
		
		siblingFileStringList = siblingFileStringList.OrderBy(x => x).ToArray();
		
		var initialActiveMenuOptionRecordIndex = -1;
		
		for (int i = 0; i < siblingFileStringList.Length; i++)
		{
			var file = siblingFileStringList[i];
			
			var siblingAbsolutePath = environmentProvider.AbsolutePathFactory(file, false);
			
			menuOptionList.Add(new MenuOptionRecord(
				siblingAbsolutePath.NameWithExtension,
				MenuOptionKind.Other,
				onClickFunc: async () => 
				{
					textEditorService.WorkerArbitrary.PostUnique(async editContext =>
			    	{
			    		await textEditorService.OpenInEditorAsync(
			    			editContext,
			                file,
							true,
							null,
							new Category("main"),
							Key<TextEditorViewModel>.NewKey());
			    	});
				}));
					
			if (siblingAbsolutePath.NameWithExtension == resourceAbsolutePath.NameWithExtension)
				initialActiveMenuOptionRecordIndex = i;
		}
		
		MenuRecord menu;
		
		if (menuOptionList.Count == 0)
			menu = new MenuRecord(MenuRecord.NoMenuOptionsExistList);
		else
			menu = new MenuRecord(menuOptionList);
		
		var dropdownRecord = new DropdownRecord(
			Key<DropdownRecord>.NewKey(),
			cursorDimensions.LeftInPixels,
			cursorDimensions.TopInPixels + cursorDimensions.HeightInPixels,
			typeof(MenuDisplay),
			new Dictionary<string, object?>
			{
				{
					nameof(MenuDisplay.MenuRecord),
					menu
				},
				{
					nameof(MenuDisplay.InitialActiveMenuOptionRecordIndex),
					initialActiveMenuOptionRecordIndex
				}
			},
			// TODO: this callback when the dropdown closes is suspect.
			//       The editContext is supposed to live the lifespan of the
			//       Post. But what if the Post finishes before the dropdown is closed?
			async () => 
			{
				// TODO: Even if this '.single or default' to get the main group works it is bad and I am ashamed...
				//       ...I'm too tired at the moment, need to make this sensible.
				//	   The key is in the IDE project yet its circular reference if I do so, gotta
				//       make groups more sensible I'm not sure what to say here I'm super tired and brain checked out.
				//       |
				//       I ran this and it didn't work. Its for the best that it doesn't.
				//	   maybe when I wake up tomorrow I'll realize what im doing here.
				var mainEditorGroup = textEditorService.Group_GetTextEditorGroupState().GroupList.SingleOrDefault();
				
				if (mainEditorGroup is not null &&
					mainEditorGroup.ActiveViewModelKey != Key<TextEditorViewModel>.Empty)
				{
					var activeViewModel = textEditorService.ViewModel_GetOrDefault(mainEditorGroup.ActiveViewModelKey);

					if (activeViewModel is not null)
						await activeViewModel.FocusAsync();
				}
				
				await viewModel.FocusAsync();
			});

        commonService.Dropdown_ReduceRegisterAction(dropdownRecord);
    }
    
    public static async ValueTask QuickActionsSlashRefactor(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel,
        WalkCommonJavaScriptInteropApi jsRuntimeCommonApi,
        TextEditorService textEditorService,
        CommonService commonService)
    {
    	var componentData = viewModel.PersistentState.ComponentData;
    	if (componentData is null)
    		return;
    
		var cursorDimensions = await jsRuntimeCommonApi
			.MeasureElementById(componentData.PrimaryCursorContentId)
			.ConfigureAwait(false);

		var compilerService = modelModifier.PersistentState.CompilerService;

		var menu = await compilerService.GetQuickActionsSlashRefactorMenu(
			editContext,
	        modelModifier,
	        viewModel);

		var dropdownRecord = new DropdownRecord(
			Key<DropdownRecord>.NewKey(),
			cursorDimensions.LeftInPixels,
			cursorDimensions.TopInPixels + cursorDimensions.HeightInPixels,
			typeof(MenuDisplay),
			new Dictionary<string, object?>
			{
				{
					nameof(MenuDisplay.MenuRecord),
					menu
				}
			},
			// TODO: this callback when the dropdown closes is suspect.
			//       The editContext is supposed to live the lifespan of the
			//       Post. But what if the Post finishes before the dropdown is closed?
			async () => 
			{
				// TODO: Even if this '.single or default' to get the main group works it is bad and I am ashamed...
				//       ...I'm too tired at the moment, need to make this sensible.
				//	   The key is in the IDE project yet its circular reference if I do so, gotta
				//       make groups more sensible I'm not sure what to say here I'm super tired and brain checked out.
				//       |
				//       I ran this and it didn't work. Its for the best that it doesn't.
				//	   maybe when I wake up tomorrow I'll realize what im doing here.
				var mainEditorGroup = textEditorService.Group_GetTextEditorGroupState().GroupList.SingleOrDefault();
				
				if (mainEditorGroup is not null &&
					mainEditorGroup.ActiveViewModelKey != Key<TextEditorViewModel>.Empty)
				{
					var activeViewModel = textEditorService.ViewModel_GetOrDefault(mainEditorGroup.ActiveViewModelKey);

					if (activeViewModel is not null)
						await activeViewModel.FocusAsync();
				}
				
				await viewModel.FocusAsync();
			});

        commonService.Dropdown_ReduceRegisterAction(dropdownRecord);
    }
    
    public static void GoToDefinition(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel,
        Category category,
        int positionIndex)
    {
    	modelModifier.PersistentState.CompilerService.GoToDefinition(
			editContext,
	        modelModifier,
	        viewModel,
	        category,
            positionIndex);
    }

    public static void ShowFindAllDialog(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel,
        TextEditorService textEditorService)
    {
        textEditorService.Options_ShowFindAllDialog();
    }

    public static async ValueTask ShowTooltipByCursorPositionAsync(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel,
        TextEditorService textEditorService,
        TextEditorComponentData componentData)
    {
    	componentData = viewModel.PersistentState.ComponentData;
    	if (componentData is null)
    		return;
    		
        var elementPositionInPixels = await textEditorService.JsRuntimeTextEditorApi
            .GetBoundingClientRect(componentData.PrimaryCursorContentId)
            .ConfigureAwait(false);

        elementPositionInPixels = elementPositionInPixels with
        {
            Top = elementPositionInPixels.Top +
                (.9 * viewModel.PersistentState.CharAndLineMeasurements.LineHeight)
        };

        await HandleMouseStoppedMovingEventAsync(
        		editContext,
        		modelModifier,
        		viewModel,
        		elementPositionInPixels.Left,
        		elementPositionInPixels.Top,
        		shiftKey: false,
        		ctrlKey: false,
        		altKey: false,
				componentData,
				modelModifier.PersistentState.ResourceUri)
			.ConfigureAwait(false);
    }
    
    /// <summary>
    /// Returns whether you should goto finalize.
    /// </summary>
    public static bool ToggleCollapsePoint(
    	int lineIndex,
    	TextEditorModel modelModifier,
    	TextEditorViewModel viewModel)
    {
        var virtualizedIndexCollapsePoint = viewModel.PersistentState.VirtualizedCollapsePointList.FindIndex(x => x.AppendToLineIndex == lineIndex);
		if (virtualizedIndexCollapsePoint != -1)
		{
			var allIndexCollapsePoint = viewModel.PersistentState.AllCollapsePointList.FindIndex(x => x.AppendToLineIndex == lineIndex);
			if (allIndexCollapsePoint != -1)
			{
				var virtualizedCollapsePoint = viewModel.PersistentState.VirtualizedCollapsePointList[virtualizedIndexCollapsePoint];
				virtualizedCollapsePoint.IsCollapsed = !virtualizedCollapsePoint.IsCollapsed;
				viewModel.PersistentState.VirtualizedCollapsePointList[virtualizedIndexCollapsePoint] = virtualizedCollapsePoint;
				
				var allCollapsePoint = viewModel.PersistentState.AllCollapsePointList[allIndexCollapsePoint];
				allCollapsePoint.IsCollapsed = virtualizedCollapsePoint.IsCollapsed;
				viewModel.PersistentState.AllCollapsePointList[allIndexCollapsePoint] = allCollapsePoint;
				
				if (allCollapsePoint.IsCollapsed)
				{
					var firstToHideLineIndex = allCollapsePoint.AppendToLineIndex + 1;
					var upperExclusiveLimit = allCollapsePoint.EndExclusiveLineIndex - allCollapsePoint.AppendToLineIndex - 1;
					for (var lineOffset = 0; lineOffset < upperExclusiveLimit; lineOffset++)
					{
						viewModel.PersistentState.HiddenLineIndexHashSet.Add(firstToHideLineIndex + lineOffset);
						
						if (viewModel.LineIndex == firstToHideLineIndex + lineOffset)
						{
							var shouldMoveCursor = true;
						
							if (lineOffset == upperExclusiveLimit - 1)
							{
								var loopLineInformation = modelModifier.GetLineInformation(firstToHideLineIndex + lineOffset);
								
								if (viewModel.ColumnIndex == loopLineInformation.LastValidColumnIndex)
									shouldMoveCursor = false;
							}
							
							if (shouldMoveCursor)
							{
								var appendToLineInformation = modelModifier.GetLineInformation(virtualizedCollapsePoint.AppendToLineIndex);
								viewModel.LineIndex = allCollapsePoint.AppendToLineIndex;
								viewModel.SetColumnIndexAndPreferred(appendToLineInformation.LastValidColumnIndex);
							}
						}
					}
				}
				else
				{
					viewModel.PersistentState.HiddenLineIndexHashSet.Clear();
					foreach (var collapsePoint in viewModel.PersistentState.AllCollapsePointList)
					{
						if (!collapsePoint.IsCollapsed)
							continue;
						var firstToHideLineIndex = collapsePoint.AppendToLineIndex + 1;
						for (var lineOffset = 0; lineOffset < collapsePoint.EndExclusiveLineIndex - collapsePoint.AppendToLineIndex - 1; lineOffset++)
						{
							viewModel.PersistentState.HiddenLineIndexHashSet.Add(firstToHideLineIndex + lineOffset);
						}
					}
				}
				
				if (virtualizedCollapsePoint.IsCollapsed)
    			{
    				virtualizedIndexCollapsePoint = viewModel.PersistentState.VirtualizedCollapsePointList.FindIndex(x => x.AppendToLineIndex == lineIndex);
    				
    				var lineInformation = modelModifier.GetLineInformation(virtualizedCollapsePoint.AppendToLineIndex);
    				
    				var inlineUi = new InlineUi(
    					positionIndex: lineInformation.UpperLineEnd.Position_StartInclusiveIndex,
    					InlineUiKind.ThreeDotsExpandInlineUiThing);
    				
    				/*// TODO: Increment position of any InlineUi that have a position >= the new inlineUi.
    				for (int i = 0; i < viewModel.InlineUiList.Count; i++)
    				{
    					var inlineUiTuple = viewModel.InlineUiList[i];
    					
    					if (inlineUiTuple.InlineUi.PositionIndex >= lineInformation.UpperLineEnd.Position_StartInclusiveIndex)
    					{
    						inlineUiTuple.InlineUi = inlineUiTuple.InlineUi.WithIncrementPositionIndex(3);
    						viewModel.InlineUiList[i] = inlineUiTuple;
    					}
    				}*/
    				
    				viewModel.PersistentState.InlineUiList.Add(
    					(
    						inlineUi,
            				Tag: virtualizedCollapsePoint.Identifier
            			));
    			}
    			else
    			{
    				// TODO: Bad, this only permits one name regardless of scope
    				var indexTagMatchedInlineUi = viewModel.PersistentState.InlineUiList.FindIndex(
    					x => x.Tag == virtualizedCollapsePoint.Identifier);
    					
    				// var inlineUiTupleToRemove = viewModel.InlineUiList[indexTagMatchedInlineUi];
    					
    				if (indexTagMatchedInlineUi != -1)
        				viewModel.PersistentState.InlineUiList.RemoveAt(indexTagMatchedInlineUi);
        				
    				/*// TODO: Decrement position of any InlineUi that have a position >= the removed inlineUi.
    				for (int i = 0; i < viewModel.InlineUiList.Count; i++)
    				{
    					var inlineUiTuple = viewModel.InlineUiList[i];
    					
    					if (inlineUiTuple.InlineUi.PositionIndex >= inlineUiTupleToRemove.InlineUi.PositionIndex)
    					{
    						inlineUiTuple.InlineUi = inlineUiTuple.InlineUi.WithDecrementPositionIndex(3);
    						viewModel.InlineUiList[i] = inlineUiTuple;
    					}
    				}*/
    			}
				
				viewModel.Virtualization.ShouldCalculateVirtualizationResult = true;
				viewModel.PersistentState.VirtualizedCollapsePointListVersion++;
				viewModel.PersistentState.ComponentData.LineIndexCache.Clear();
				return true;
			}
		}
		
		return false;
    }

	/// <summary>The default <see cref="AfterOnKeyDownAsync"/> will provide syntax highlighting, and autocomplete.<br/><br/>The syntax highlighting occurs on ';', whitespace, paste, undo, redo<br/><br/>The autocomplete occurs on LetterOrDigit typed or { Ctrl + Space }. Furthermore, the autocomplete is done via <see cref="IAutocompleteService"/> and the one can provide their own implementation when registering the Walk.TextEditor services using <see cref="WalkTextEditorConfig.AutocompleteServiceFactory"/></summary>
	public static async ValueTask HandleAfterOnKeyDownAsync(
		TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel,
        KeymapArgs keymapArgs,
		TextEditorComponentData componentData)
    {
        if (EventUtils.IsAutocompleteMenuInvoker(keymapArgs))
        {
        	ShowAutocompleteMenu(
        		editContext,
		        modelModifier,
		        viewModel,
		        componentData.TextEditorViewModelSlimDisplay.CommonService,
		        componentData);
        }
        else if (EventUtils.IsSyntaxHighlightingInvoker(keymapArgs))
        {
            componentData.ThrottleApplySyntaxHighlighting(modelModifier);
        }
    }

	/// <summary>
	/// This method was being used in the 'OnKeyDownBatch.cs' class, which no longer exists.
	/// The replacement for 'OnKeyDownBatch.cs' is 'OnKeyDownLateBatching.cs'.
	///
	/// But, during the replacement process, this method was overlooked.
	///
	/// One would likely want to use this method when appropriate because
	/// it permits every batched keyboard event to individually be given a chance
	/// to trigger 'HandleAfterOnKeyDownAsyncFactory(...)'
	///
	/// Example: a 'space' keyboard event, batched with the letter 'a' keyboard event.
	/// Depending on what 'OnKeyDownLateBatching.cs' does, perhaps it takes the last keyboard event
	/// and uses that to fire 'HandleAfterOnKeyDownAsyncFactory(...)'.
	///
	/// Well, a 'space' keyboard event would have trigger syntax highlighting to be refreshed.
	/// Whereas, the letter 'a' keyboard event won't do anything beyond inserting the letter.
	/// Therefore, the syntax highlighting was erroneously not refreshed due to batching.
	/// This method is intended to solve this problem, but it was forgotten at some point.
	/// </summary>
	public static async ValueTask HandleAfterOnKeyDownRangeAsync(
		TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel,
        KeymapArgs[] batchKeymapArgsList,
        int batchKeymapArgsListLength,
		TextEditorComponentData componentData,
		ViewModelDisplayOptions viewModelDisplayOptions)
    {
        if (viewModelDisplayOptions.AfterOnKeyDownRangeAsync is not null)
        {
            await viewModelDisplayOptions.AfterOnKeyDownRangeAsync.Invoke(
                editContext,
		        modelModifier,
		        viewModel,
                batchKeymapArgsList,
                batchKeymapArgsListLength,
                componentData);
            return;
        }

        var seenIsAutocompleteIndexerInvoker = false;
        var seenIsAutocompleteMenuInvoker = false;
        var seenIsSyntaxHighlightingInvoker = false;

        for (int i = 0; i < batchKeymapArgsListLength; i++)
        {
            var keymapArgs = batchKeymapArgsList[i];

            if (!seenIsAutocompleteIndexerInvoker && EventUtils.IsAutocompleteIndexerInvoker(keymapArgs))
                seenIsAutocompleteIndexerInvoker = true;

            if (!seenIsAutocompleteMenuInvoker && EventUtils.IsAutocompleteMenuInvoker(keymapArgs))
                seenIsAutocompleteMenuInvoker = true;
            else if (!seenIsSyntaxHighlightingInvoker && EventUtils.IsSyntaxHighlightingInvoker(keymapArgs))
                seenIsSyntaxHighlightingInvoker = true;
        }

        if (seenIsAutocompleteMenuInvoker)
        {
        	ShowAutocompleteMenu(
        		editContext,
		        modelModifier,
		        viewModel,
		        componentData.TextEditorViewModelSlimDisplay.CommonService,
		        componentData);
        }

        if (seenIsSyntaxHighlightingInvoker)
        {
            componentData.ThrottleApplySyntaxHighlighting(modelModifier);
        }
    }

	public static ValueTask HandleMouseStoppedMovingEventAsync(
		TextEditorEditContext editContext,
		TextEditorModel modelModifier,
		TextEditorViewModel viewModel,
		double clientX,
		double clientY,
		bool shiftKey,
        bool ctrlKey,
        bool altKey,
		TextEditorComponentData componentData,
        ResourceUri resourceUri)
    {
    	return modelModifier.PersistentState.CompilerService.OnInspect(
			editContext,
			modelModifier,
			viewModel,
			clientX,
    		clientY,
    		shiftKey,
            ctrlKey,
            altKey,
			componentData,
	        resourceUri);
    }
    
    /// <summary>
    /// If left or top is left as null, it will be set to whatever the primary cursor's left or top is respectively.
    /// The left and top values are offsets from the text editor's bounding client rect.
    ///
    /// Use the method <see cref="RemoveDropdown"/> to un-render the dropdown programmatically.
    /// </summary>
    public static void ShowDropdown(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel,
        CommonService commonService,
        double? leftOffset,
        double? topOffset,
        Type componentType,
        Dictionary<string, object?>? componentParameters)
    {
        var dropdownKey = new Key<DropdownRecord>(viewModel.PersistentState.ViewModelKey.Guid);
        var tabWidth = editContext.TextEditorService.Options_GetOptions().TabWidth;
        
        if (leftOffset is null)
        {
            leftOffset = viewModel.PersistentState.GutterWidth +
                         viewModel.ColumnIndex *
                         viewModel.PersistentState.CharAndLineMeasurements.CharacterWidth;
	        
	        // Tab key column offset
            var tabsOnSameLineBeforeCursor = modelModifier.GetTabCountOnSameLineBeforeCursor(
                viewModel.LineIndex,
                viewModel.ColumnIndex);

            // 1 of the character width is already accounted for
            var extraWidthPerTabKey = tabWidth - 1;

            leftOffset += extraWidthPerTabKey *
                tabsOnSameLineBeforeCursor *
                viewModel.PersistentState.CharAndLineMeasurements.CharacterWidth;
                
            leftOffset -= viewModel.PersistentState.ScrollLeft;
        }
        
        if (topOffset is null)
        {
        	var hiddenLineCount = 0;
	
			for (int i = 0; i < viewModel.LineIndex; i++)
			{
				if (viewModel.PersistentState.HiddenLineIndexHashSet.Contains(i))
					hiddenLineCount++;
			}
        
        	topOffset ??= ((viewModel.LineIndex - hiddenLineCount) + 1) *
	        	viewModel.PersistentState.CharAndLineMeasurements.LineHeight -
	        	viewModel.PersistentState.ScrollTop;
        }
		
		var dropdownRecord = new DropdownRecord(
			dropdownKey,
			viewModel.PersistentState.TextEditorDimensions.BoundingClientRectLeft + leftOffset.Value,
			viewModel.PersistentState.TextEditorDimensions.BoundingClientRectTop + topOffset.Value,
			componentType,
			componentParameters,
			async () => await viewModel.FocusAsync())
		{
			ShouldShowOutOfBoundsClickDisplay = false
		};

        commonService.Dropdown_ReduceRegisterAction(dropdownRecord);
	}
	
	public static void RemoveDropdown(
        TextEditorEditContext editContext,
        TextEditorViewModel viewModel,
        CommonService commonService)
    {
    	viewModel.PersistentState.MenuKind = MenuKind.None;
    
		var dropdownKey = new Key<DropdownRecord>(viewModel.PersistentState.ViewModelKey.Guid);
		commonService.Dropdown_ReduceDisposeAction(dropdownKey);
	}
	
	public static void ShowContextMenu(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel,
        CommonService commonService,
        TextEditorComponentData componentData)
    {
    	viewModel.PersistentState.MenuKind = MenuKind.ContextMenu;
    
    	ShowDropdown(
    		editContext,
	        modelModifier,
	        viewModel,
	        commonService,
	        leftOffset: null,
	        topOffset: null,
	        typeof(Walk.TextEditor.RazorLib.TextEditors.Displays.Internals.ContextMenu),
	        new Dictionary<string, object?>
			{
				{
					nameof(Walk.TextEditor.RazorLib.TextEditors.Displays.Internals.ContextMenu.ComponentDataKey),
					componentData.ComponentDataKey
				},
			});
	}
	
	public static void ShowAutocompleteMenu(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel,
        CommonService commonService,
        TextEditorComponentData componentData)
    {
    	viewModel.PersistentState.MenuKind = MenuKind.AutoCompleteMenu;
    
    	ShowDropdown(
    		editContext,
	        modelModifier,
	        viewModel,
	        commonService,
	        leftOffset: null,
	        topOffset: null,
	        typeof(Walk.TextEditor.RazorLib.TextEditors.Displays.Internals.AutocompleteMenu),
	        new Dictionary<string, object?>
			{
				{
					nameof(Walk.TextEditor.RazorLib.TextEditors.Displays.Internals.AutocompleteMenu.ComponentDataKey),
					componentData.ComponentDataKey
				},
			});
	}
	
	public static async ValueTask ShowFindOverlay(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel,
        WalkCommonJavaScriptInteropApi commonJavaScriptInteropApi)
    {
		// If the user has an active text selection,
		// then populate the find overlay with their selection.
		
        var selectedText = TextEditorSelectionHelper.GetSelectedText(viewModel, modelModifier);
		if (selectedText is not null)
		{
			viewModel.PersistentState.FindOverlayValue = selectedText;
            viewModel.PersistentState.FindOverlayValueExternallyChangedMarker = !viewModel.PersistentState.FindOverlayValueExternallyChangedMarker;
		}

        if (viewModel.PersistentState.ShowFindOverlay)
        {
        	var componentData = viewModel.PersistentState.ComponentData;
	    	if (componentData is null)
	    		return;
    		
            await commonJavaScriptInteropApi
                .FocusHtmlElementById(componentData.FindOverlayId)
                .ConfigureAwait(false);
        }
        else
        {
            viewModel.PersistentState.ShowFindOverlay = true;
        }
    }
    
    public static void PopulateSearchFindAll(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel)
    {
		// If the user has an active text selection,
		// then populate the find overlay with their selection.
		
        if (modelModifier is null || viewModel is null)
            return;

        var selectedText = TextEditorSelectionHelper.GetSelectedText(viewModel, modelModifier);
		if (selectedText is null)
			return;
			
		editContext.TextEditorService.SetSearchQuery(selectedText);
    }
    
    public static Task OnWheel(Walk.Common.RazorLib.Tooltips.Models.ITooltipModel tooltipModel, WheelEventArgs wheelEventArgs)
    {
        // (TextEditorService TextEditorService, Key<TextEditorViewModel> ViewModelKey) tuple
        // ValueTuple<TextEditorService, Key<TextEditorViewModel>> tuple
        if (tooltipModel.ItemUntyped is not ValueTuple<TextEditorService, Key<TextEditorViewModel>, int> tuple)
            return Task.CompletedTask;
        
        tuple.Item1.WorkerArbitrary.PostUnique(editContext =>
        {
            var viewModel = editContext.GetViewModelModifier(tuple.Item2);
        
            var componentData = viewModel.PersistentState.ComponentData;
            if (componentData is null)
                return ValueTask.CompletedTask;
                
            tuple.Item1.WorkerUi.Enqueue(
    	        new TextEditorEventArgs
    	        {
    	            X = wheelEventArgs.DeltaX,
                    Y = wheelEventArgs.DeltaY,
                    ShiftKey = wheelEventArgs.ShiftKey,
    	        },
            	componentData,
            	viewModel.PersistentState.ViewModelKey,
    	        Walk.TextEditor.RazorLib.BackgroundTasks.Models.TextEditorWorkUiKind.OnWheel);
        
            return ValueTask.CompletedTask;
        });
        
        return Task.CompletedTask;
    }
}
