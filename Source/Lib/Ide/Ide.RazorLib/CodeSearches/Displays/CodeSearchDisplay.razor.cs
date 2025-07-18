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
	private IdeService IdeService { get; set; } = null!;
    [Inject]
	private IServiceProvider ServiceProvider { get; set; } = null!;
	
	private CodeSearchTreeViewKeyboardEventHandler _treeViewKeymap = null!;
	private CodeSearchTreeViewMouseEventHandler _treeViewMouseEventHandler = null!;
    
    private int OffsetPerDepthInPixels => (int)Math.Ceiling(
		IdeService.TextEditorService.CommonUtilityService.GetAppOptionsState().Options.IconSizeInPixels * (2.0 / 3.0));

	private readonly ViewModelDisplayOptions _textEditorViewModelDisplayOptions = new()
	{
		HeaderComponentType = null,
	};

    private string InputValue
	{
		get => IdeService.GetCodeSearchState().Query;
		set
		{
			if (value is null)
				value = string.Empty;

			IdeService.CodeSearch_With(inState => inState with
			{
				Query = value,
			});

			IdeService.CodeSearch_HandleSearchEffect();
		}
	}
	
	protected override void OnInitialized()
	{
		IdeService.CodeSearchStateChanged += OnCodeSearchStateChanged;
		IdeService.CommonUtilityService.TreeViewStateChanged += OnTreeViewStateChanged;
	
		_treeViewKeymap = new CodeSearchTreeViewKeyboardEventHandler(
			IdeService.TextEditorService,
			ServiceProvider);

		_treeViewMouseEventHandler = new CodeSearchTreeViewMouseEventHandler(
			IdeService.TextEditorService,
			ServiceProvider);
	}
	
	protected override void OnAfterRender(bool firstRender)
	{
		IdeService.CodeSearch_updateContentThrottle.Run(_ => IdeService.CodeSearch_UpdateContent(ResourceUri.Empty));
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

		IdeService.CommonUtilityService.Dropdown_ReduceRegisterAction(dropdownRecord);
		return Task.CompletedTask;
	}

	private string GetIsActiveCssClass(CodeSearchFilterKind codeSearchFilterKind)
	{
		return IdeService.GetCodeSearchState().CodeSearchFilterKind == codeSearchFilterKind
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
    	IdeService.CodeSearchStateChanged -= OnCodeSearchStateChanged;
    	IdeService.CommonUtilityService.TreeViewStateChanged -= OnTreeViewStateChanged;
    }
}