using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.JavaScriptObjects.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Drags.Displays;
using Walk.Common.RazorLib.Tabs.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;

namespace Walk.Common.RazorLib.Panels.Models;

public record Panel : IPanelTab, IDialog, IDrag
{
    private readonly Type _dragTabComponentType;

    private readonly Type? _dragDialogComponentType = null;
    private readonly Dictionary<string, object?>? _dragDialogComponentParameterMap = null;

    public Panel(
        string title,
        Key<Panel> key,
        Key<IDynamicViewModel> dynamicViewModelKey,
        Type componentType,
        Dictionary<string, object?>? componentParameterMap,
        CommonService commonService)
    {
        Title = title;
        Key = key;
        DynamicViewModelKey = dynamicViewModelKey;
        ComponentType = componentType;
        ComponentParameterMap = componentParameterMap;

        CommonService = commonService;

        _dragTabComponentType = typeof(DragDisplay);

        DialogFocusPointHtmlElementId = $"di_dialog-focus-point_{DynamicViewModelKey.Guid}";
    }

    public string Title { get; }
    public string TitleVerbose => Title;
    public Key<Panel> Key { get; }
    public Key<IDynamicViewModel> DynamicViewModelKey { get; }
    public CommonService CommonService { get;}
    public Type ComponentType { get; }
    public Dictionary<string, object?>? ComponentParameterMap { get; set; }
    public string? DialogCssClass { get; set; }
    public string? DialogCssStyle { get; set; }
    public ITabGroup? TabGroup { get; set; }

    public bool DialogIsMinimized { get; set; }
    public bool DialogIsMaximized { get; set; }
    public bool DialogIsResizable { get; set; } = true;
    public string? SetFocusOnCloseElementId { get; set; }
    public string DialogFocusPointHtmlElementId { get; init; }
    public ElementDimensions DialogElementDimensions { get; set; } = CommonFacts.ConstructDefaultElementDimensions();

    public List<IDropzone> DropzoneList { get; set; } = new();
    
    public Type DragComponentType => TabGroup is null
        ? _dragDialogComponentType
        : _dragTabComponentType;

    public Dictionary<string, object?>? DragComponentParameterMap => TabGroup is null
        ? _dragDialogComponentParameterMap
        : null;

    public string? DragCssClass { get; set; }
    public string? DragCssStyle { get; set; }
    
    public TabCascadingValueBatch TabCascadingValueBatch { get; set; }

    public IDialog SetDialogIsMaximized(bool isMaximized)
    {
        DialogIsMaximized = isMaximized;
        return this;
    }

    public async Task OnDragStartAsync()
    {
        var dropzoneList = new List<IDropzone>();
        AddFallbackDropzone(dropzoneList);

        var panelGroupHtmlIdTupleList = new (Key<PanelGroup> PanelGroupKey, string HtmlElementId)[]
        {
            (CommonFacts.LeftPanelGroupKey, "di_ide_panel_left_tabs"),
            (CommonFacts.RightPanelGroupKey, "di_ide_panel_right_tabs"),
            (CommonFacts.BottomPanelGroupKey, "di_ide_panel_bottom_tabs"),
        };

        foreach (var panelGroupHtmlIdTuple in panelGroupHtmlIdTupleList)
        {
            var measuredHtmlElementDimensions = await CommonService.JsRuntimeCommonApi
                .MeasureElementById(panelGroupHtmlIdTuple.HtmlElementId)
                .ConfigureAwait(false);

            measuredHtmlElementDimensions = measuredHtmlElementDimensions with
            {
                ZIndex = 1,
            };

            var elementDimensions = new ElementDimensions();

            elementDimensions.ElementPositionKind = ElementPositionKind.Fixed;

            // Width
            elementDimensions.DisableWidth();
            elementDimensions.Width_Base_0 = elementDimensions.Width_Base_0 with
            {
                Value = measuredHtmlElementDimensions.WidthInPixels
            };

            // Height
            elementDimensions.DisableHeight();
            elementDimensions.Height_Base_0 = elementDimensions.Height_Base_0 with
            {
                Value = measuredHtmlElementDimensions.HeightInPixels
            };

            // Left
            elementDimensions.DisableLeft();
            elementDimensions.Left_Base_0 = elementDimensions.Left_Base_0 with
            {
                Value = measuredHtmlElementDimensions.LeftInPixels
            };

            // Top
            elementDimensions.DisableTop();
            elementDimensions.Top_Base_0 = elementDimensions.Top_Base_0 with
            {
                Value = measuredHtmlElementDimensions.TopInPixels
            };

            dropzoneList.Add(new PanelGroupDropzone(
                measuredHtmlElementDimensions,
                panelGroupHtmlIdTuple.PanelGroupKey,
                elementDimensions,
                Key<IDropzone>.NewKey(),
                null,
                null));
        }

        DropzoneList = dropzoneList;
    }

    public Task OnDragEndAsync(MouseEventArgs mouseEventArgs, IDropzone? dropzone)
    {
        var panelGroup = TabGroup as PanelGroup;

        if (dropzone is not PanelGroupDropzone panelGroupDropzone)
            return Task.CompletedTask;

        // Create Dialog
        if (panelGroupDropzone.PanelGroupKey == Key<PanelGroup>.Empty)
        {
            // Delete current UI
            {
                if (panelGroup is not null)
                {
                    CommonService.DisposePanelTab(
                        panelGroup.Key,
                        Key);
                }
                else
                {
                    // Is a dialog
                    //
                    // Already a dialog, so nothing needs to be done
                    return Task.CompletedTask;
                }

                TabGroup = null;
            }

            CommonService.Dialog_ReduceRegisterAction(this);
        }
        
        // Create Panel Tab
        {
            // Delete current UI
            {
                if (panelGroup is not null)
                {
                    CommonService.DisposePanelTab(
                        panelGroup.Key,
                        Key);
                }
                else
                {
                    CommonService.Dialog_ReduceDisposeAction(DynamicViewModelKey);
                }

                TabGroup = null;
            }

            var verticalHalfwayPoint = dropzone.MeasuredHtmlElementDimensions.TopInPixels +
                (dropzone.MeasuredHtmlElementDimensions.HeightInPixels / 2);

            var insertAtIndexZero = mouseEventArgs.ClientY < verticalHalfwayPoint
                ? true
                : false;

            CommonService.RegisterPanelTab(
                panelGroupDropzone.PanelGroupKey,
                this,
                insertAtIndexZero);
        }

        return Task.CompletedTask;
    }

    private void AddFallbackDropzone(List<IDropzone> dropzoneList)
    {
        var fallbackElementDimensions = new ElementDimensions();

        fallbackElementDimensions.ElementPositionKind = ElementPositionKind.Fixed;

        // Width
        fallbackElementDimensions.DisableWidth();
        fallbackElementDimensions.Width_Base_0 = new DimensionUnit(100, DimensionUnitKind.ViewportWidth);

        // Height
        fallbackElementDimensions.DisableHeight();
        fallbackElementDimensions.Height_Base_0 = new DimensionUnit(100, DimensionUnitKind.ViewportHeight);

        // Left
        fallbackElementDimensions.DisableLeft();
        fallbackElementDimensions.Left_Base_0 = new DimensionUnit(0, DimensionUnitKind.Pixels);

        // Top
        fallbackElementDimensions.DisableTop();
        fallbackElementDimensions.Top_Base_0 = new DimensionUnit(0, DimensionUnitKind.Pixels);

        dropzoneList.Add(new PanelGroupDropzone(
            new MeasuredHtmlElementDimensions(0, 0, 0, 0, 0),
            Key<PanelGroup>.Empty,
            fallbackElementDimensions,
            Key<IDropzone>.NewKey(),
            "di_dropzone-fallback",
            null));
    }
    
    public async Task OnClick(MouseEventArgs mouseEventArgs)
    {
        var localTabGroup = TabGroup;
        if (localTabGroup is null)
            return;

        await localTabGroup.OnClickAsync(this, mouseEventArgs);
    }
    
    public async Task HandleOnMouseDownAsync(MouseEventArgs mouseEventArgs)
    {
        if (mouseEventArgs.Button == 0)
            TabCascadingValueBatch.ThinksLeftMouseButtonIsDown = true;
        if (mouseEventArgs.Button == 1)
            await CloseTabOnClickAsync().ConfigureAwait(false);
        else if (mouseEventArgs.Button == 2)
            ManuallyPropagateOnContextMenu(mouseEventArgs);
    }
    
    public async Task HandleOnMouseOutAsync(MouseEventArgs mouseEventArgs)
    {
        if ((mouseEventArgs.Buttons & 1) == 0)
            TabCascadingValueBatch.ThinksLeftMouseButtonIsDown = false;

        if (TabCascadingValueBatch.ThinksLeftMouseButtonIsDown && this is IDrag draggable)
        {
            TabCascadingValueBatch.ThinksLeftMouseButtonIsDown = false;

            // This needs to run synchronously to guarantee `dragState.DragElementDimensions` is in a threadsafe state
            // (keep any awaits after it).
            // (only the "UI thread" touches `dragState.DragElementDimensions`).
            var dragState = CommonService.GetDragState();

            dragState.DragElementDimensions.DisableWidth();

            dragState.DragElementDimensions.DisableHeight();

            dragState.DragElementDimensions.Left_Offset = new DimensionUnit(mouseEventArgs.ClientX, DimensionUnitKind.Pixels);

            dragState.DragElementDimensions.Top_Offset = new DimensionUnit(mouseEventArgs.ClientY, DimensionUnitKind.Pixels);

            dragState.DragElementDimensions.ElementPositionKind = ElementPositionKind.Fixed;

            await draggable.OnDragStartAsync().ConfigureAwait(false);

            TabCascadingValueBatch.SubscribeToDragEventForScrolling.Invoke(draggable);
        }
    }
    
    public void ManuallyPropagateOnContextMenu(MouseEventArgs mouseEventArgs)
    {
        var localHandleTabButtonOnContextMenu = TabCascadingValueBatch?.HandleTabButtonOnContextMenu;
        if (localHandleTabButtonOnContextMenu is null)
            return;

        CommonService.Enqueue(new CommonWorkArgs
        {
            WorkKind = CommonWorkKind.Tab_ManuallyPropagateOnContextMenu,
            HandleTabButtonOnContextMenu = localHandleTabButtonOnContextMenu,
            TabContextMenuEventArgs = new TabContextMenuEventArgs(mouseEventArgs, this, () => Task.CompletedTask),
        });
    }
    
    public async Task CloseTabOnClickAsync()
    {
        var localTabGroup = TabGroup;
        if (localTabGroup is null)
            return;

        await localTabGroup.CloseAsync(this).ConfigureAwait(false);
    }
}
