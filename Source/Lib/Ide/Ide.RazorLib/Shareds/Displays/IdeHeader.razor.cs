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

namespace Walk.Ide.RazorLib.Shareds.Displays;

public partial class IdeHeader : ComponentBase, IDisposable
{
	[Inject]
	private ITerminalService TerminalService { get; set; } = null!;
	[Inject]
	private IIdeHeaderService IdeHeaderService { get; set; } = null!;
	[Inject]
	private IAppOptionsService AppOptionsService { get; set; } = null!;
    [Inject]
    private IdeBackgroundTaskApi IdeBackgroundTaskApi { get; set; } = null!;
    [Inject]
    private CommonBackgroundTaskApi CommonBackgroundTaskApi { get; set; } = null!;
    [Inject]
    private WalkHostingInformation WalkHostingInformation { get; set; } = null!;
    [Inject]
    private IPanelService PanelService { get; set; } = null!;
    [Inject]
    private IDropdownService DropdownService { get; set; } = null!;
    [Inject]
    private TextEditorService TextEditorService { get; set; } = null!;
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

    private static readonly Key<IDynamicViewModel> _infoDialogKey = Key<IDynamicViewModel>.NewKey();

    public ElementReference? _buttonFileElementReference;
    public ElementReference? _buttonToolsElementReference;
    public ElementReference? _buttonViewElementReference;
    public ElementReference? _buttonRunElementReference;
    
	protected override void OnInitialized()
	{
		AppOptionsService.AppOptionsStateChanged += OnAppOptionsStateChanged;

		IdeBackgroundTaskApi.Enqueue(new IdeBackgroundTaskApiWorkArgs
		{
			WorkKind = IdeBackgroundTaskApiWorkKind.IdeHeaderOnInit,
			IdeHeader = this,
		});

        base.OnInitialized();
	}

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

    private async void OnAppOptionsStateChanged()
	{
		await InvokeAsync(StateHasChanged);
	}
	
	public void Dispose()
	{
		AppOptionsService.AppOptionsStateChanged -= OnAppOptionsStateChanged;
	}
}