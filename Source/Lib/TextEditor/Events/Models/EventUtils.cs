using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Keymaps.Models;
using Walk.TextEditor.RazorLib.Keymaps.Models;
using Walk.TextEditor.RazorLib.TextEditors.Displays;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;

namespace Walk.TextEditor.RazorLib.Events.Models;

public static class EventUtils
{
    public static bool IsAutocompleteMenuInvoker(KeymapArgs keymapArgs)
    {
        // LetterOrDigit was hit without Ctrl being held
        return !keymapArgs.CtrlKey &&
               !CommonFacts.IsWhitespaceCode(keymapArgs.Code) &&
               !CommonFacts.IsMetaKey(keymapArgs);
    }

    public static bool IsSyntaxHighlightingInvoker(KeymapArgs keymapArgs)
    {
        if (keymapArgs.Key == ";" ||
            CommonFacts.IsWhitespaceCode(keymapArgs.Code))
        {
            if (keymapArgs.CtrlKey && (keymapArgs.Key == " " || keymapArgs.Key == "SPACE"))
            {
                // Working on using the binder to populate the autocomplete menu with the members of the type
                // that a variable reference is a type of. (2025-01-01)
                // ==========================================================================================
                //
                //
                // Introduction to the issue.
                // --------------------------------------------------------------------------------
                // When typing the '.' after the variable reference's identifier, the autocomplete
                // correctly populates with the members using the binder.
                //
                // But, if the cursor is immediately after an already existing '.' and then
                // one presses { 'Ctrl' + 'Space' }, then the autocomplete is empty (with regards to the binder result).
                //
                //
                // Some findings
                // -------------------------------------------------------------------------------------
                // In the first case where you type a '.', the node that is found at the cursor position
                // is a (VariableReferenceNode - need to re-confirm this as I'm speaking from memory), but
                // the second case of { 'Ctrl' + 'Space' } and an existing '.' then the found
                // node is an EmptyExpressionNode.
                //
                // 
                // Conclusion
                // ------------------------------------------------------------------------------------- 
                // This 'if' statement is being added temporarily in order to stop the re-parsing of the
                // text file.
                //
                // Because, it is presumed to be the 're-parsing' of the text file, and some timing issue
                // such that the node cannot be found correctly, which results
                // in no results coming back from binder when asked for the members.
                //
                // This 'if' statement fixes the issue for now.
                // But this is not a good long term solution.
                //
                // The code for the member autocompletion is being worked on,
                // and I don't want to look at the timing issue until I've finished my thoughts
                // with the member autocompletion.
                return false;
            }
        
            return true;
        }
        
        if (keymapArgs.CtrlKey)
        {
            switch (keymapArgs.Key)
            {
                case "s":
                case "v":
                case "z":
                case "y":
                    return true;
            }
        }
        
        return false;
    }

    /// <summary>
    /// All keyboardEventArgs that return true from "IsAutocompleteIndexerInvoker"
    /// are to be 1 character long, as well either whitespace or punctuation.
    /// Therefore 1 character behind might be a word that can be indexed.
    /// </summary>
    public static bool IsAutocompleteIndexerInvoker(KeymapArgs keymapArgs)
    {
        return (CommonFacts.IsWhitespaceCode(keymapArgs.Code) ||
                    CommonFacts.IsPunctuationCharacter(keymapArgs.Key.First())) &&
                !keymapArgs.CtrlKey;
    }

    public static async Task<(int LineIndex, int ColumnIndex, double PositionX, double PositionY)> CalculateLineAndColumnIndex(
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel,
        double clientX,
        double clientY,
        TextEditorComponentData componentData,
        TextEditorEditContext editContext)
    {
        var globalTextEditorOptions = editContext.TextEditorService.Options_GetTextEditorOptionsState().Options;

        if (modelModifier is null || viewModel is null)
            return (0, 0, 0, 0);
            
        var tabWidth = editContext.TextEditorService.Options_GetOptions().TabWidth;
    
        var positionX = clientX - viewModel.PersistentState.TextEditorDimensions.BoundingClientRectLeft;
        var positionY = clientY - viewModel.PersistentState.TextEditorDimensions.BoundingClientRectTop;
    
        // Scroll position offset
        positionX += viewModel.PersistentState.ScrollLeft;
        positionY += viewModel.PersistentState.ScrollTop;
        
        positionX -= viewModel.PersistentState.GutterWidth;
        
        var lineIndex = (int)(positionY / viewModel.PersistentState.CharAndLineMeasurements.LineHeight);
        
        lineIndex = lineIndex > modelModifier.LineCount - 1
            ? modelModifier.LineCount - 1
            : lineIndex;
            
        var columnIndexDouble = positionX / viewModel.PersistentState.CharAndLineMeasurements.CharacterWidth;
        int columnIndexInt = (int)Math.Round(columnIndexDouble, MidpointRounding.AwayFromZero);
        
        var lineLength = modelModifier.GetLineLength(lineIndex);
        
        lineIndex = Math.Max(lineIndex, 0);
        columnIndexInt = Math.Max(columnIndexInt, 0);
        
        var lineInformation = modelModifier.GetLineInformation(lineIndex);
        
        int literalLength = 0;
        int visualLength = 0;
        
        var previousCharacterWidth = 1;
        var previousPosition = -1;
        
        for (int columnIndex = 0; columnIndex < lineLength; columnIndex++)
        {
            if (visualLength >= columnIndexInt)
            {
                if (previousCharacterWidth > 1)
                {
                    var visualLengthPrevious = visualLength - previousCharacterWidth;
                    
                    // If distance from left side to event is smaller than distance from right side to event
                    // then put cursor on the left side
                    // else put cursor on the right side.
                    if (columnIndexDouble - visualLengthPrevious < visualLength - columnIndexDouble)
                    {
                        // Represent a '\t' with 4 character width as "--->",
                        // a cursor by "|",
                        // and the side closest to the event with "==" then:
                        //
                        //  ==
                        // |--->
                        literalLength = literalLength - 1;
                        break;
                    }
                    else
                    {
                        // Represent a '\t' with 4 character width as "--->",
                        // a cursor by "|",
                        // and the side closest to the event with "==" then:
                        //
                        //    ==
                        //  --->|
                        break;
                    }
                }
            
                break;
            }
            
            literalLength += 1;
            
            previousCharacterWidth = GetCharacterWidth(
                modelModifier.RichCharacterList[
                    lineInformation.Position_StartInclusiveIndex + columnIndex]
                .Value);
            
            visualLength += previousCharacterWidth;
        }
        
        int GetCharacterWidth(char character)
        {
            if (character == '\t')
                return tabWidth;
        
            return 1;
        }
        
        columnIndexInt = columnIndexInt > lineLength
            ? lineLength
            : columnIndexInt;
        
        return (lineIndex, literalLength, positionX, positionY);
    }
}
