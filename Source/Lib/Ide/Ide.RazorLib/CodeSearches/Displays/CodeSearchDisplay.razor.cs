using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Resizes.Models;
using Walk.Common.RazorLib.Resizes.Displays;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Ide.RazorLib.CodeSearches.Models;

namespace Walk.Ide.RazorLib.CodeSearches.Displays;

public partial class CodeSearchDisplay : ComponentBase, IDisposable
{
    [Inject]
    private IdeService IdeService { get; set; } = null!;
    
    private readonly ViewModelDisplayOptions _textEditorViewModelDisplayOptions = new()
    {
        HeaderComponentType = null,
    };
    
    private TreeViewContainerParameter _treeViewContainerParameter;
    
    private Func<ElementDimensions, ElementDimensions, (MouseEventArgs firstMouseEventArgs, MouseEventArgs secondMouseEventArgs), Task>? _dragEventHandler;
    private MouseEventArgs? _previousDragMouseEventArgs;

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
        IdeService.IdeStateChanged += OnCodeSearchStateChanged;
        IdeService.CommonService.CommonUiStateChanged += OnCommonUiStateChanged;
        
        _treeViewContainerParameter = new(
            CodeSearchState.TreeViewCodeSearchContainerKey,
            new CodeSearchTreeViewKeyboardEventHandler(IdeService.TextEditorService),
            new CodeSearchTreeViewMouseEventHandler(IdeService.TextEditorService),
            OnTreeViewContextMenuFunc);
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

        IdeService.CommonService.Dropdown_ReduceRegisterAction(dropdownRecord);
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
    
    public async void OnCommonUiStateChanged(CommonUiEventKind commonUiEventKind)
    {
        if (commonUiEventKind == CommonUiEventKind.TreeViewStateChanged)
        {
            await InvokeAsync(StateHasChanged);
        }
        else if (commonUiEventKind == CommonUiEventKind.DragStateChanged)
        {
            if (_dragEventHandler is not null)
            {
                var codeSearchState = IdeService.GetCodeSearchState();
        
                await ResizableRow.Do(
                    IdeService.CommonService,
                    codeSearchState.TopContentElementDimensions,
                    codeSearchState.BottomContentElementDimensions,
                    _dragEventHandler,
                    _previousDragMouseEventArgs,
                    x => _dragEventHandler = x,
                    x => _previousDragMouseEventArgs = x);
                
                await InvokeAsync(StateHasChanged);
            }
        }
    }
    
    public async void OnCodeSearchStateChanged(IdeStateChangedKind ideStateChangedKind)
    {
        if (ideStateChangedKind == IdeStateChangedKind.CodeSearchStateChanged)
        {
            await InvokeAsync(StateHasChanged);
        }
    }
    
    public void SubscribeToDragEvent()
    {
        _dragEventHandler = ResizableRow.DragEventHandlerResizeHandleAsync;
        IdeService.CommonService.Drag_ShouldDisplayAndMouseEventArgsSetAction(true, null);
    }
    
    public void Dispose()
    {
        IdeService.IdeStateChanged -= OnCodeSearchStateChanged;
        IdeService.CommonService.CommonUiStateChanged -= OnCommonUiStateChanged;
    }
}
