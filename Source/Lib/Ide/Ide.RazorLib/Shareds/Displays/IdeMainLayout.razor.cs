using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Drags.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.TextEditor.RazorLib;
using Walk.Ide.RazorLib.Shareds.Models;

/* Start Body */
using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.StateHasChangedBoundaries.Displays;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
/* End Body */

/* Start Header */
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Clipboards.Models;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Installations.Models;
using Walk.Ide.RazorLib.Shareds.Displays.Internals;
using Walk.Ide.RazorLib.Commands;
using Walk.Ide.RazorLib.BackgroundTasks.Models;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.Ide.RazorLib.Shareds.Models;
/* End Header */

/* Start IdeMainLayout */
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Ide.RazorLib.JsRuntimes.Models;
using Walk.Ide.RazorLib.BackgroundTasks.Models;
/* End IdeMainLayout */

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
    /* Start Header */
    [Inject]
    private ITerminalService TerminalService { get; set; } = null!;
    [Inject]
    private IIdeHeaderService IdeHeaderService { get; set; } = null!;
    [Inject]
    private IdeBackgroundTaskApi IdeBackgroundTaskApi { get; set; } = null!;
    [Inject]
    private WalkHostingInformation WalkHostingInformation { get; set; } = null!;
    [Inject]
    private IDropdownService DropdownService { get; set; } = null!;
    [Inject]
    private IDialogService DialogService { get; set; } = null!;
    [Inject]
    private ICommandFactory CommandFactory { get; set; } = null!;
    [Inject]
    private IClipboardService ClipboardService { get; set; } = null!;
    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;
    [Inject]
    private IServiceProvider ServiceProvider { get; set; } = null!;
    [Inject]
    private BackgroundTaskService BackgroundTaskService { get; set; } = null!;
    [Inject]
    private WalkTextEditorConfig TextEditorConfig { get; set; } = null!;
    /* End Header */
    
    private bool _previousDragStateWrapShouldDisplay;
    private ElementDimensions _bodyElementDimensions = new();
    private ElementDimensions _editorElementDimensions = new();

    private string UnselectableClassCss => _previousDragStateWrapShouldDisplay ? "di_unselectable" : string.Empty;
    
    /// <summary>
    /// This can only be set from the "UI thread".
    /// </summary>
    private bool _shouldRecalculateCssStrings = true;
    
    private string _classCssString;
    private string _styleCssString;
    private string _headerCssStyle;
    
    /* Start Header */
    private static readonly Key<IDynamicViewModel> _infoDialogKey = Key<IDynamicViewModel>.NewKey();
    
    public ElementReference? _buttonFileElementReference;
    public ElementReference? _buttonToolsElementReference;
    public ElementReference? _buttonViewElementReference;
    public ElementReference? _buttonRunElementReference;
    /* End Header */

    protected override void OnInitialized()
    {
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
        
        _editorElementDimensions.WidthDimensionAttribute.DimensionUnitList.AddRange(new[]
        {
            new DimensionUnit(
            	33.3333,
            	DimensionUnitKind.Percentage),
            new DimensionUnit(
            	AppOptionsService.GetAppOptionsState().Options.ResizeHandleWidthInPixels / 2,
            	DimensionUnitKind.Pixels,
            	DimensionOperatorKind.Subtract)
        });
    
        DragService.DragStateChanged += DragStateWrapOnStateChanged;
        AppOptionsService.AppOptionsStateChanged += AppOptionsStateWrapOnStateChanged;
        IdeMainLayoutService.IdeMainLayoutStateChanged += OnIdeMainLayoutStateChanged;
        TextEditorService.OptionsApi.StaticStateChanged += TextEditorOptionsStateWrap_StateChanged;

    	IdeBackgroundTaskApi.Enqueue(new IdeBackgroundTaskApiWorkArgs
    	{
    		WorkKind = IdeBackgroundTaskApiWorkKind.IdeHeaderOnInit,
    		IdeMainLayout = this,
    	});
    	
    	IdeBackgroundTaskApi.Enqueue(new IdeBackgroundTaskApiWorkArgs
        {
        	WorkKind = IdeBackgroundTaskApiWorkKind.WalkIdeInitializerOnInit,
        });
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
                
            if (WalkHostingInformation.WalkHostingKind == WalkHostingKind.Photino)
			{
				await JsRuntime.GetWalkIdeApi()
					.PreventDefaultBrowserKeybindings()
					.ConfigureAwait(false);
			}
        }
    }

    private async void AppOptionsStateWrapOnStateChanged()
    {
        await InvokeAsync(() =>
        {
            _shouldRecalculateCssStrings = true;
            StateHasChanged();
        }).ConfigureAwait(false);
    }

    private async void DragStateWrapOnStateChanged()
    {
        if (_previousDragStateWrapShouldDisplay != DragService.GetDragState().ShouldDisplay)
        {
            _previousDragStateWrapShouldDisplay = DragService.GetDragState().ShouldDisplay;
            await InvokeAsync(() =>
            {
                _shouldRecalculateCssStrings = true;
                StateHasChanged();
            }).ConfigureAwait(false);
        }
    }

    private async void OnIdeMainLayoutStateChanged()
    {
    	await InvokeAsync(StateHasChanged).ConfigureAwait(false);
    }

    private async void TextEditorOptionsStateWrap_StateChanged()
    {
        await InvokeAsync(() =>
        {
            _shouldRecalculateCssStrings = true;
            StateHasChanged();
        }).ConfigureAwait(false);
    }
    
    /// <summary>
    /// This can only be invoked from the UI thread due to the usage of `CommonBackgroundTaskApi.UiStringBuilder`.
    /// </summary>
    private void CreateCssStrings()
    {
        if (_shouldRecalculateCssStrings)
        {
            _shouldRecalculateCssStrings = false;
        
            var uiStringBuilder = CommonBackgroundTaskApi.UiStringBuilder;
            
            uiStringBuilder.Clear();
            uiStringBuilder.Append("di_ide_main-layout ");
            uiStringBuilder.Append(UnselectableClassCss);
            uiStringBuilder.Append(" ");
            uiStringBuilder.Append(AppOptionsService.ThemeCssClassString);
            uiStringBuilder.Append(" ");
            uiStringBuilder.Append(TextEditorService.ThemeCssClassString);
            _classCssString = uiStringBuilder.ToString();
            
            uiStringBuilder.Clear();
            uiStringBuilder.Append(AppOptionsService.FontSizeCssStyleString);
            uiStringBuilder.Append(" ");
            uiStringBuilder.Append(AppOptionsService.FontFamilyCssStyleString);
            uiStringBuilder.Append(" ");
            uiStringBuilder.Append(AppOptionsService.ColorSchemeCssStyleString);
            _styleCssString = uiStringBuilder.ToString();
            
        	uiStringBuilder.Clear();
            uiStringBuilder.Append("display: flex; justify-content: space-between; border-bottom: ");
            uiStringBuilder.Append(AppOptionsService.GetAppOptionsState().Options.ResizeHandleHeightInPixels);
            uiStringBuilder.Append("px solid var(--di_primary-border-color);");
            _headerCssStyle = uiStringBuilder.ToString();
        }
    }
    
    /* Start Header */
    public Task RenderFileDropdownOnClick()
    {
        return DropdownHelper.RenderDropdownAsync(
            DropdownService,
            CommonBackgroundTaskApi.JsRuntimeCommonApi,
            IdeHeaderState.ButtonFileId,
            DropdownOrientation.Bottom,
            IdeHeaderState.DropdownKeyFile,
            IdeHeaderService.GetIdeHeaderState().MenuFile,
            _buttonFileElementReference);
    }
    
    public Task RenderToolsDropdownOnClick()
    {
        return DropdownHelper.RenderDropdownAsync(
            DropdownService,
            CommonBackgroundTaskApi.JsRuntimeCommonApi,
            IdeHeaderState.ButtonToolsId,
            DropdownOrientation.Bottom,
            IdeHeaderState.DropdownKeyTools,
            IdeHeaderService.GetIdeHeaderState().MenuTools,
            _buttonToolsElementReference);
    }
    
    public Task RenderViewDropdownOnClick()
    {
        InitializeMenuView();
    
        return DropdownHelper.RenderDropdownAsync(
            DropdownService,
            CommonBackgroundTaskApi.JsRuntimeCommonApi,
            IdeHeaderState.ButtonViewId,
            DropdownOrientation.Bottom,
            IdeHeaderState.DropdownKeyView,
            IdeHeaderService.GetIdeHeaderState().MenuView,
            _buttonViewElementReference);
    }
    
    public Task RenderRunDropdownOnClick()
    {
        return DropdownHelper.RenderDropdownAsync(
            DropdownService,
            CommonBackgroundTaskApi.JsRuntimeCommonApi,
            IdeHeaderState.ButtonRunId,
            DropdownOrientation.Bottom,
            IdeHeaderState.DropdownKeyRun,
            IdeHeaderService.GetIdeHeaderState().MenuRun,
            _buttonRunElementReference);
    }
    
    public void InitializeMenuView()
    {
        var menuOptionsList = new List<MenuOptionRecord>();
        var panelState = PanelService.GetPanelState();
        var dialogState = DialogService.GetDialogState();
    
        foreach (var panel in panelState.PanelList)
        {
            var menuOptionPanel = new MenuOptionRecord(
                panel.Title,
                MenuOptionKind.Delete,
                async () =>
                {
                    var panelGroup = panel.TabGroup as PanelGroup;
    
                    if (panelGroup is not null)
                    {
                        PanelService.SetActivePanelTab(panelGroup.Key, panel.Key);
    
                        var contextRecord = ContextFacts.AllContextsList.FirstOrDefault(x => x.ContextKey == panel.ContextRecordKey);
    
                        if (contextRecord != default)
                        {
                            var command = ContextHelper.ConstructFocusContextElementCommand(
                                contextRecord,
                                nameof(ContextHelper.ConstructFocusContextElementCommand),
                                nameof(ContextHelper.ConstructFocusContextElementCommand),
                                CommonBackgroundTaskApi.JsRuntimeCommonApi,
                                PanelService);
    
                            await command.CommandFunc.Invoke(null).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        var existingDialog = dialogState.DialogList.FirstOrDefault(
                            x => x.DynamicViewModelKey == panel.DynamicViewModelKey);
    
                        if (existingDialog is not null)
                        {
                            DialogService.ReduceSetActiveDialogKeyAction(existingDialog.DynamicViewModelKey);
    
                            await CommonBackgroundTaskApi.JsRuntimeCommonApi
                                .FocusHtmlElementById(existingDialog.DialogFocusPointHtmlElementId)
                                .ConfigureAwait(false);
                        }
                        else
                        {
                            PanelService.RegisterPanelTab(PanelFacts.LeftPanelGroupKey, panel, true);
                            PanelService.SetActivePanelTab(PanelFacts.LeftPanelGroupKey, panel.Key);
    
                            var contextRecord = ContextFacts.AllContextsList.FirstOrDefault(x => x.ContextKey == panel.ContextRecordKey);
    
                            if (contextRecord != default)
                            {
                                var command = ContextHelper.ConstructFocusContextElementCommand(
                                    contextRecord,
                                    nameof(ContextHelper.ConstructFocusContextElementCommand),
                                    nameof(ContextHelper.ConstructFocusContextElementCommand),
                                    CommonBackgroundTaskApi.JsRuntimeCommonApi,
                                    PanelService);
    
                                await command.CommandFunc.Invoke(null).ConfigureAwait(false);
                            }
                        }
                    }
                });
    
            menuOptionsList.Add(menuOptionPanel);
        }
    
        if (menuOptionsList.Count == 0)
        {
            IdeHeaderService.SetMenuView(new MenuRecord(MenuRecord.NoMenuOptionsExistList));
        }
        else
        {
            IdeHeaderService.SetMenuView(new MenuRecord(menuOptionsList));
        }
    }
    
    private Task OpenInfoDialogOnClick()
    {
        var dialogRecord = new DialogViewModel(
            _infoDialogKey,
            "Info",
            typeof(IdeInfoDisplay),
            null,
            null,
            true,
            null);
    
        DialogService.ReduceRegisterAction(dialogRecord);
        return Task.CompletedTask;
    }
    /* End Header */

    public void Dispose()
    {
        DragService.DragStateChanged -= DragStateWrapOnStateChanged;
        AppOptionsService.AppOptionsStateChanged -= AppOptionsStateWrapOnStateChanged;
        IdeMainLayoutService.IdeMainLayoutStateChanged -= OnIdeMainLayoutStateChanged;
        TextEditorService.OptionsApi.StaticStateChanged -= TextEditorOptionsStateWrap_StateChanged;
    }
}
