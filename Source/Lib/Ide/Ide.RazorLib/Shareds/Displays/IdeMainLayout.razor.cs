using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Drags.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.TextEditor.RazorLib;
using Walk.Ide.RazorLib.Shareds.Models;

namespace Walk.Ide.RazorLib.Shareds.Displays;

public partial class IdeMainLayout : LayoutComponentBase, IDisposable
{
    [Inject]
    private IDragService DragService { get; set; } = null!;
    [Inject]
    private IPanelService PanelService { get; set; } = null!;
    [Inject]
    private IIdeMainLayoutService IdeMainLayoutService { get; set; } = null!;
    [Inject]
    private TextEditorService TextEditorService { get; set; } = null!;
    [Inject]
    private IAppOptionsService AppOptionsService { get; set; } = null!;
    [Inject]
    private CommonBackgroundTaskApi CommonBackgroundTaskApi { get; set; } = null!;

    private bool _previousDragStateWrapShouldDisplay;
    private ElementDimensions _bodyElementDimensions = new();

    private string UnselectableClassCss => DragService.GetDragState().ShouldDisplay ? "di_unselectable" : string.Empty;

    protected override void OnInitialized()
    {
        DragService.DragStateChanged += DragStateWrapOnStateChanged;
        AppOptionsService.AppOptionsStateChanged += AppOptionsStateWrapOnStateChanged;
        IdeMainLayoutService.IdeMainLayoutStateChanged += OnIdeMainLayoutStateChanged;
        TextEditorService.OptionsApi.StaticStateChanged += TextEditorOptionsStateWrap_StateChanged;

        _bodyElementDimensions.HeightDimensionAttribute.DimensionUnitList.AddRange(new[]
        {
            new DimensionUnit(78, DimensionUnitKind.Percentage),
            new DimensionUnit(
            	AppOptionsService.GetAppOptionsState().Options.ResizeHandleHeightInPixels / 2,
            	DimensionUnitKind.Pixels,
            	DimensionOperatorKind.Subtract),
            new DimensionUnit(
            	SizeFacts.Ide.Header.Height.Value / 2,
            	SizeFacts.Ide.Header.Height.DimensionUnitKind,
            	DimensionOperatorKind.Subtract)
        });

        base.OnInitialized();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await TextEditorService.OptionsApi
                .SetFromLocalStorageAsync()
                .ConfigureAwait(false);

            await AppOptionsService
                .SetFromLocalStorageAsync()
                .ConfigureAwait(false);
        }
    }

    private async void AppOptionsStateWrapOnStateChanged()
    {
        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }

    private async void DragStateWrapOnStateChanged()
    {
        if (_previousDragStateWrapShouldDisplay != DragService.GetDragState().ShouldDisplay)
        {
            _previousDragStateWrapShouldDisplay = DragService.GetDragState().ShouldDisplay;
            await InvokeAsync(StateHasChanged).ConfigureAwait(false);
        }
    }

    private async void OnIdeMainLayoutStateChanged()
    {
    	await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }

    private async void TextEditorOptionsStateWrap_StateChanged()
    {
        await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }

    public void Dispose()
    {
        DragService.DragStateChanged -= DragStateWrapOnStateChanged;
        AppOptionsService.AppOptionsStateChanged -= AppOptionsStateWrapOnStateChanged;
        IdeMainLayoutService.IdeMainLayoutStateChanged -= OnIdeMainLayoutStateChanged;
        TextEditorService.OptionsApi.StaticStateChanged -= TextEditorOptionsStateWrap_StateChanged;
    }
}