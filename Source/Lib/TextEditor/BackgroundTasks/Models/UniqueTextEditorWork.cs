namespace Walk.TextEditor.RazorLib.BackgroundTasks.Models;

public struct UniqueTextEditorWork
{
    private readonly Func<TextEditorEditContext, ValueTask> _textEditorFunc;

    public UniqueTextEditorWork(
        TextEditorService textEditorService,
        Func<TextEditorEditContext, ValueTask> textEditorFunc)
    {
        _textEditorFunc = textEditorFunc;
        TextEditorService = textEditorService;
    }

    public TextEditorService TextEditorService { get; }

    public async ValueTask HandleEvent()
    {
    	var editContext = new TextEditorEditContext(TextEditorService);
    
		await _textEditorFunc
            .Invoke(editContext)
            .ConfigureAwait(false);
            
        await editContext.TextEditorService
        	.FinalizePost(editContext)
        	.ConfigureAwait(false);
    }
}
