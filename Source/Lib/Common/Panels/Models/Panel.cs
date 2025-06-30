using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.JavaScriptObjects.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Drags.Displays;

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
        Key<ContextRecord> contextRecordKey,
        Type componentType,
        Dictionary<string, object?>? componentParameterMap,
        ICommonUiService commonUiService,
        CommonBackgroundTaskApi commonBackgroundTaskApi)
    {
        Title = title;
        Key = key;
        DynamicViewModelKey = dynamicViewModelKey;
        ContextRecordKey = contextRecordKey;
        ComponentType = componentType;
        ComponentParameterMap = componentParameterMap;

        CommonUiService = commonUiService;
        CommonBackgroundTaskApi = commonBackgroundTaskApi;

        _dragTabComponentType = typeof(DragDisplay);

        DialogFocusPointHtmlElementId = $"di_dialog-focus-point_{DynamicViewModelKey.Guid}";
    }

    public string Title { get; }
	public string TitleVerbose => Title;
	public Key<Panel> Key { get; }
	public Key<IDynamicViewModel> DynamicViewModelKey { get; }
    public Key<ContextRecord> ContextRecordKey { get; }
	public ICommonUiService CommonUiService { get;}
    public CommonBackgroundTaskApi CommonBackgroundTaskApi { get;}
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
	public ElementDimensions DialogElementDimensions { get; set; } = DialogHelper.ConstructDefaultElementDimensions();

    public List<IDropzone> DropzoneList { get; set; } = new();
    
	public Type DragComponentType => TabGroup is null
        ? _dragDialogComponentType
        : _dragTabComponentType;

    public Dictionary<string, object?>? DragComponentParameterMap => TabGroup is null
        ? _dragDialogComponentParameterMap
        : null;

    public string? DragCssClass { get; set; }
	public string? DragCssStyle { get; set; }

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
			(PanelFacts.LeftPanelGroupKey, "di_ide_panel_left_tabs"),
			(PanelFacts.RightPanelGroupKey, "di_ide_panel_right_tabs"),
			(PanelFacts.BottomPanelGroupKey, "di_ide_panel_bottom_tabs"),
		};

		foreach (var panelGroupHtmlIdTuple in panelGroupHtmlIdTupleList)
		{
			var measuredHtmlElementDimensions = await CommonBackgroundTaskApi.JsRuntimeCommonApi
                .MeasureElementById(panelGroupHtmlIdTuple.HtmlElementId)
                .ConfigureAwait(false);

			measuredHtmlElementDimensions = measuredHtmlElementDimensions with
			{
				ZIndex = 1,
			};

			var elementDimensions = new ElementDimensions();

			elementDimensions.ElementPositionKind = ElementPositionKind.Fixed;

			// Width
			{
				elementDimensions.WidthDimensionAttribute.DimensionUnitList.Clear();
				elementDimensions.WidthDimensionAttribute.DimensionUnitList.Add(new DimensionUnit(
					measuredHtmlElementDimensions.WidthInPixels,
					DimensionUnitKind.Pixels));
			}

			// Height
			{
				elementDimensions.HeightDimensionAttribute.DimensionUnitList.Clear();
				elementDimensions.HeightDimensionAttribute.DimensionUnitList.Add(new DimensionUnit(
					measuredHtmlElementDimensions.HeightInPixels,
					DimensionUnitKind.Pixels));
			}

			// Left
			{
				elementDimensions.LeftDimensionAttribute.DimensionUnitList.Clear();
				elementDimensions.LeftDimensionAttribute.DimensionUnitList.Add(new DimensionUnit(
					measuredHtmlElementDimensions.LeftInPixels,
					DimensionUnitKind.Pixels));
			}

			// Top
			{
				elementDimensions.TopDimensionAttribute.DimensionUnitList.Clear();
				elementDimensions.TopDimensionAttribute.DimensionUnitList.Add(new DimensionUnit(
					measuredHtmlElementDimensions.TopInPixels,
					DimensionUnitKind.Pixels));
			}

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
					CommonUiService.DisposePanelTab(
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

			CommonUiService.Dialog_ReduceRegisterAction(this);
		}
		
		// Create Panel Tab
		{
			// Delete current UI
			{
				if (panelGroup is not null)
				{
					CommonUiService.DisposePanelTab(
						panelGroup.Key,
						Key);
				}
				else
				{
					CommonUiService.Dialog_ReduceDisposeAction(DynamicViewModelKey);
				}

				TabGroup = null;
			}

			var verticalHalfwayPoint = dropzone.MeasuredHtmlElementDimensions.TopInPixels +
				(dropzone.MeasuredHtmlElementDimensions.HeightInPixels / 2);

			var insertAtIndexZero = mouseEventArgs.ClientY < verticalHalfwayPoint
				? true
				: false;

			CommonUiService.RegisterPanelTab(
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
		{
			fallbackElementDimensions.WidthDimensionAttribute.DimensionUnitList.Clear();
			fallbackElementDimensions.WidthDimensionAttribute.DimensionUnitList.Add(new DimensionUnit(100, DimensionUnitKind.ViewportWidth));
		}

		// Height
		{
			fallbackElementDimensions.HeightDimensionAttribute.DimensionUnitList.Clear();
			fallbackElementDimensions.HeightDimensionAttribute.DimensionUnitList.Add(new DimensionUnit(100, DimensionUnitKind.ViewportHeight));
		}

		// Left
		{
			fallbackElementDimensions.LeftDimensionAttribute.DimensionUnitList.Clear();
			fallbackElementDimensions.LeftDimensionAttribute.DimensionUnitList.Add(new DimensionUnit(0, DimensionUnitKind.Pixels));
		}

		// Top
		{
			fallbackElementDimensions.TopDimensionAttribute.DimensionUnitList.Clear();
			fallbackElementDimensions.TopDimensionAttribute.DimensionUnitList.Add(new DimensionUnit(0, DimensionUnitKind.Pixels));
		}

		dropzoneList.Add(new PanelGroupDropzone(
			new MeasuredHtmlElementDimensions(0, 0, 0, 0, 0),
			Key<PanelGroup>.Empty,
			fallbackElementDimensions,
			Key<IDropzone>.NewKey(),
            "di_dropzone-fallback",
			null));
	}
}
