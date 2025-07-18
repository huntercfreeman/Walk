using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Ide.RazorLib.CodeSearches.Models;
using Walk.Ide.RazorLib.BackgroundTasks.Models;

namespace Walk.Ide.RazorLib.CodeSearches.Displays;

public partial class CodeSearchDisplay : ComponentBase, IDisposable
{
	[Inject]
	private IdeBackgroundTaskApi IdeBackgroundTaskApi { get; set; } = null!;
    [Inject]
	private IServiceProvider ServiceProvider { get; set; } = null!;
	
	private CodeSearchTreeViewKeyboardEventHandler _treeViewKeymap = null!;
	private CodeSearchTreeViewMouseEventHandler _treeViewMouseEventHandler = null!;
    
    private int OffsetPerDepthInPixels => (int)Math.Ceiling(
		IdeBackgroundTaskApi.TextEditorService.CommonUtilityService.GetAppOptionsState().Options.IconSizeInPixels * (2.0 / 3.0));

	private readonly ViewModelDisplayOptions _textEditorViewModelDisplayOptions = new()
	{
		HeaderComponentType = null,
	};

    private string InputValue
	{
		get => IdeBackgroundTaskApi.GetCodeSearchState().Query;
		set
		{
			if (value is null)
				value = string.Empty;

			IdeBackgroundTaskApi.CodeSearch_With(inState => inState with
			{
				Query = value,
			});

			IdeBackgroundTaskApi.CodeSearch_HandleSearchEffect();
		}
	}
	
	protected override void OnInitialized()
	{
		IdeBackgroundTaskApi.CodeSearchStateChanged += OnCodeSearchStateChanged;
		IdeBackgroundTaskApi.CommonUtilityService.TreeViewStateChanged += OnTreeViewStateChanged;
	
		_treeViewKeymap = new CodeSearchTreeViewKeyboardEventHandler(
			IdeBackgroundTaskApi.TextEditorService,
			ServiceProvider);

		_treeViewMouseEventHandler = new CodeSearchTreeViewMouseEventHandler(
			IdeBackgroundTaskApi.TextEditorService,
			ServiceProvider);
	}
	
	protected override void OnAfterRender(bool firstRender)
	{
		IdeBackgroundTaskApi.CodeSearch_updateContentThrottle.Run(_ => IdeBackgroundTaskApi.CodeSearch_UpdateContent(ResourceUri.Empty));
	}
	
	private Task OnTreeViewContextMenuFunc(TreeViewCommandArgs treeViewCommandArgs)
	{
		var dropdownRecord = new DropdownRecord(
			CodeSearchContextMenu.ContextMenuEventDropdownKey,
			treeViewCommandArgs.ContextMenuFixedPosition.LeftPositionInPixels,
			treeViewCommandArgs.ContextMenuFixedPosition.TopPositionInPixels,
			typeof(CodeSearchContextMenu),
			new Dictionary<string, object?>
			{
				{
					nameof(CodeSearchContextMenu.TreeViewCommandArgs),
					treeViewCommandArgs
				}
			},
			null);

		IdeBackgroundTaskApi.CommonUtilityService.Dropdown_ReduceRegisterAction(dropdownRecord);
		return Task.CompletedTask;
	}

	private string GetIsActiveCssClass(CodeSearchFilterKind codeSearchFilterKind)
	{
		return IdeBackgroundTaskApi.GetCodeSearchState().CodeSearchFilterKind == codeSearchFilterKind
			? "di_active"
			: string.Empty;
	}

	private async Task HandleResizableRowReRenderAsync()
	{
		await InvokeAsync(StateHasChanged);
	}
    
    public async void OnTreeViewStateChanged()
    {
    	await InvokeAsync(StateHasChanged);
    }
    
    public async void OnCodeSearchStateChanged()
    {
    	await InvokeAsync(StateHasChanged);
    }
    
    public void Dispose()
    {
    	IdeBackgroundTaskApi.CodeSearchStateChanged -= OnCodeSearchStateChanged;
    	IdeBackgroundTaskApi.CommonUtilityService.TreeViewStateChanged -= OnTreeViewStateChanged;
    }
}