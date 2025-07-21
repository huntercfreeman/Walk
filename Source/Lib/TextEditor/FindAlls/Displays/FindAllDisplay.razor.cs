using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.TextEditor.RazorLib.FindAlls.Models;

namespace Walk.TextEditor.RazorLib.FindAlls.Displays;

public partial class FindAllDisplay : ComponentBase, IDisposable
{
    [Inject]
    private IServiceProvider ServiceProvider { get; set; } = null!;
    [Inject]
    private TextEditorService TextEditorService { get; set; } = null!;
    
    private FindAllTreeViewKeyboardEventHandler _treeViewKeymap = null!;
    private FindAllTreeViewMouseEventHandler _treeViewMouseEventHandler = null!;
    
    private int OffsetPerDepthInPixels => (int)Math.Ceiling(
        TextEditorService.CommonService.GetAppOptionsState().Options.IconSizeInPixels * (2.0 / 3.0));

    private string SearchQuery
    {
        get => TextEditorService.GetFindAllState().SearchQuery;
        set
        {
            if (value is not null)
                TextEditorService.SetSearchQuery(value);
        }
    }

    private string StartingDirectoryPath
    {
        get => TextEditorService.GetFindAllState().StartingDirectoryPath;
        set
        {
            if (value is not null)
                TextEditorService.SetStartingDirectoryPath(value);
        }
    }
    
    protected override void OnInitialized()
    {
        TextEditorService.FindAllStateChanged += OnFindAllStateChanged;
    
        _treeViewKeymap = new FindAllTreeViewKeyboardEventHandler(
            TextEditorService,
            ServiceProvider,
            TextEditorService.CommonService);

        _treeViewMouseEventHandler = new FindAllTreeViewMouseEventHandler(
            TextEditorService,
            ServiceProvider,
            TextEditorService.CommonService);
    }
    
    private Task OnTreeViewContextMenuFunc(TreeViewCommandArgs treeViewCommandArgs)
    {
        var dropdownRecord = new DropdownRecord(
            FindAllContextMenu.ContextMenuEventDropdownKey,
            treeViewCommandArgs.ContextMenuFixedPosition.LeftPositionInPixels,
            treeViewCommandArgs.ContextMenuFixedPosition.TopPositionInPixels,
            typeof(FindAllContextMenu),
            new Dictionary<string, object?>
            {
                {
                    nameof(FindAllContextMenu.TreeViewCommandArgs),
                    treeViewCommandArgs
                }
            },
            null);

        TextEditorService.CommonService.Dropdown_ReduceRegisterAction(dropdownRecord);
        return Task.CompletedTask;
    }

    private void DoSearchOnClick()
    {
        TextEditorService.HandleStartSearchAction();
    }

    private void CancelSearchOnClick()
    {
        TextEditorService.CancelSearch();
    }
    
    public async void OnFindAllStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }
    
    public void Dispose()
    {
        TextEditorService.FindAllStateChanged -= OnFindAllStateChanged;
    }
}
