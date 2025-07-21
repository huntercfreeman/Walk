namespace Walk.TextEditor.RazorLib.Commands.Models.Defaults;

public static class TextEditorCommandDefaultFacts
{
    /*
    public static readonly TextEditorCommand RefreshSyntaxHighlighting = new(
        "Refresh Syntax Highlighting", "defaults_refresh_syntax_highlighting", false, false, TextEditKind.None, null,
        interfaceCommandArgs =>
        {
            var commandArgs = (TextEditorCommandArgs)interfaceCommandArgs;
            
            var modelModifier = commandArgs.EditContext.GetModelModifier(commandArgs.ModelResourceUri);
            var viewModelModifier = commandArgs.EditContext.GetViewModelModifier(commandArgs.ViewModelKey);
            var cursorModifierBag = commandArgs.EditContext.GetCursorModifierBag(viewModelModifier?.ViewModel);
            var primaryCursorModifier = commandArgs.EditContext.GetPrimaryCursorModifier(cursorModifierBag);

            if (modelModifier is null || viewModelModifier is null || !cursorModifierBag.ConstructorWasInvoked || primaryCursorModifier is null)
                return ValueTask.CompletedTask;
                
            modelModifier.CompilerService.ResourceWasModified(
                modelModifier.ResourceUri,
                Array.Empty<TextEditorTextSpan>());
            return ValueTask.CompletedTask;
        });
    */
}
