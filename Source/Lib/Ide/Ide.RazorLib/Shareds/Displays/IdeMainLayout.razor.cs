using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Drags.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Clipboards.Models;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Installations.Models;
using Walk.Ide.RazorLib.Shareds.Models;
using Walk.Ide.RazorLib.Shareds.Displays.Internals;
using Walk.Ide.RazorLib.Commands;
using Walk.Ide.RazorLib.BackgroundTasks.Models;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.Ide.RazorLib.JsRuntimes.Models;
using Walk.Ide.RazorLib.Settings.Displays;

namespace Walk.Ide.RazorLib.Shareds.Displays;

public partial class IdeMainLayout : LayoutComponentBase, IDisposable
{
    [Inject]
    private IDragService DragService { get; set; } = null!;
    [Inject]
    private IPanelService PanelService { get; set; } = null!;
    [Inject]
    private IIdeService IdeService { get; set; } = null!;
    [Inject]
    private TextEditorService TextEditorService { get; set; } = null!;
    [Inject]
    private IAppOptionsService AppOptionsService { get; set; } = null!;
    [Inject]
    private CommonBackgroundTaskApi CommonBackgroundTaskApi { get; set; } = null!;
    [Inject]
    private ITerminalService TerminalService { get; set; } = null!;
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
    
    private static readonly Key<IDynamicViewModel> _infoDialogKey = Key<IDynamicViewModel>.NewKey();
    
    public ElementReference? _buttonFileElementReference;
    public ElementReference? _buttonToolsElementReference;
    public ElementReference? _buttonViewElementReference;
    public ElementReference? _buttonRunElementReference;
    
    private IDialog _dialogRecord = new DialogViewModel(
        Key<IDynamicViewModel>.NewKey(),
        "Settings",
        typeof(SettingsDisplay),
        null,
        null,
		true,
		null);

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
        IdeService.IdeStateChanged += OnIdeMainLayoutStateChanged;
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
    
    public Task RenderFileDropdownOnClick()
    {
        return DropdownHelper.RenderDropdownAsync(
            DropdownService,
            CommonBackgroundTaskApi.JsRuntimeCommonApi,
            IdeState.ButtonFileId,
            DropdownOrientation.Bottom,
            IdeState.DropdownKeyFile,
            IdeService.GetIdeState().MenuFile,
            _buttonFileElementReference);
    }
    
    public Task RenderToolsDropdownOnClick()
    {
        return DropdownHelper.RenderDropdownAsync(
            DropdownService,
            CommonBackgroundTaskApi.JsRuntimeCommonApi,
            IdeState.ButtonToolsId,
            DropdownOrientation.Bottom,
            IdeState.DropdownKeyTools,
            IdeService.GetIdeState().MenuTools,
            _buttonToolsElementReference);
    }
    
    public Task RenderViewDropdownOnClick()
    {
        InitializeMenuView();
    
        return DropdownHelper.RenderDropdownAsync(
            DropdownService,
            CommonBackgroundTaskApi.JsRuntimeCommonApi,
            IdeState.ButtonViewId,
            DropdownOrientation.Bottom,
            IdeState.DropdownKeyView,
            IdeService.GetIdeState().MenuView,
            _buttonViewElementReference);
    }
    
    public Task RenderRunDropdownOnClick()
    {
        return DropdownHelper.RenderDropdownAsync(
            DropdownService,
            CommonBackgroundTaskApi.JsRuntimeCommonApi,
            IdeState.ButtonRunId,
            DropdownOrientation.Bottom,
            IdeState.DropdownKeyRun,
            IdeService.GetIdeState().MenuRun,
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
            IdeService.SetMenuView(new MenuRecord(MenuRecord.NoMenuOptionsExistList));
        }
        else
        {
            IdeService.SetMenuView(new MenuRecord(menuOptionsList));
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

    public void DispatchRegisterDialogRecordAction() =>
        DialogService.ReduceRegisterAction(_dialogRecord);

    public void Dispose()
    {
        DragService.DragStateChanged -= DragStateWrapOnStateChanged;
        AppOptionsService.AppOptionsStateChanged -= AppOptionsStateWrapOnStateChanged;
        IdeService.IdeStateChanged -= OnIdeMainLayoutStateChanged;
        TextEditorService.OptionsApi.StaticStateChanged -= TextEditorOptionsStateWrap_StateChanged;
    }
}
