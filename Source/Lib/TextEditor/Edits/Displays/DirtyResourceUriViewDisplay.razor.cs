using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;

namespace Walk.TextEditor.RazorLib.Edits.Displays;

public partial class DirtyResourceUriViewDisplay : ComponentBase, IDisposable
{
    [Inject]
    private TextEditorService TextEditorService { get; set; } = null!;
    
    protected override void OnInitialized()
	{
		TextEditorService.DirtyResourceUriStateChanged += OnDirtyResourceUriStateChanged;
	}

    private Task OpenInEditorOnClick(string filePath)
    {
    	TextEditorService.WorkerArbitrary.PostUnique(async editContext =>
    	{
    		await TextEditorService.OpenInEditorAsync(
    			editContext,
                filePath,
				true,
				null,
				new Category("main"),
				Key<TextEditorViewModel>.NewKey());
    	});
    	return Task.CompletedTask;
    }
    
    public async void OnDirtyResourceUriStateChanged()
    {
    	await InvokeAsync(StateHasChanged);
    }
    
    public void Dispose()
    {
    	TextEditorService.DirtyResourceUriStateChanged -= OnDirtyResourceUriStateChanged;
    }
}