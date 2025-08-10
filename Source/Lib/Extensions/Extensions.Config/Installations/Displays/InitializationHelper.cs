using Microsoft.JSInterop;
using System.Text;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.FileSystems.Displays;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.ListExtensions;
using Walk.CompilerServices.CSharp.CompilerServiceCase;
using Walk.CompilerServices.CSharpProject.CompilerServiceCase;
using Walk.CompilerServices.Css;
using Walk.CompilerServices.Css.Decoration;
using Walk.CompilerServices.DotNetSolution.CompilerServiceCase;
using Walk.CompilerServices.Json;
using Walk.CompilerServices.Json.Decoration;
using Walk.CompilerServices.Razor.CompilerServiceCase;
using Walk.CompilerServices.Xml;
using Walk.CompilerServices.Xml.Html.Decoration;
using Walk.Extensions.DotNet;
using Walk.Extensions.DotNet.AppDatas.Models;
using Walk.Extensions.DotNet.CommandLines.Models;
using Walk.Extensions.DotNet.DotNetSolutions.Models;
using Walk.Ide.RazorLib;
using Walk.Ide.RazorLib.BackgroundTasks.Models;
using Walk.Ide.RazorLib.CodeSearches.Displays;
using Walk.Ide.RazorLib.FolderExplorers.Displays;
using Walk.Ide.RazorLib.Shareds.Displays.Internals;
using Walk.Ide.RazorLib.Shareds.Models;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.TextEditor.RazorLib.Commands.Models.Defaults;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;

namespace Walk.Extensions.Config.Installations.Displays;

public static class InitializationHelper
{
    public static readonly Key<IDynamicViewModel> _permissionsDialogKey = Key<IDynamicViewModel>.NewKey();
    public static readonly Key<IDynamicViewModel> _backgroundTaskDialogKey = Key<IDynamicViewModel>.NewKey();
    public static readonly Key<IDynamicViewModel> _solutionVisualizationDialogKey = Key<IDynamicViewModel>.NewKey();
    public static readonly Key<IDynamicViewModel> _infoDialogKey = Key<IDynamicViewModel>.NewKey();
    
    public static Key<IDynamicViewModel> _notificationRecordKey = Key<IDynamicViewModel>.NewKey();

    public static async Task InitializeOnAfterRenderFirstRender(
        DotNetService DotNetService,
        BrowserResizeInterop BrowserResizeInterop,
        CancellationTokenSource _workerCancellationTokenSource)
    {
        var menuOptionOpenDotNetSolution = new MenuOptionRecord(
            ".NET Solution",
            MenuOptionKind.Other,
            () =>
            {
                DotNetSolutionState.ShowInputFile(DotNetService.IdeService, DotNetService);
                return Task.CompletedTask;
            });

        DotNetService.IdeService.Ide_ModifyMenuFile(
            inMenu =>
            {
                var indexMenuOptionOpen = inMenu.MenuOptionList.FindIndex(x => x.DisplayName == "Open");

                if (indexMenuOptionOpen == -1)
                {
                    var copyList = new List<MenuOptionRecord>(inMenu.MenuOptionList);
                    copyList.Add(menuOptionOpenDotNetSolution);
                    return inMenu with
                    {
                        MenuOptionList = copyList
                    };
                }

                var menuOptionOpen = inMenu.MenuOptionList[indexMenuOptionOpen];

                if (menuOptionOpen.SubMenu is null)
                    menuOptionOpen.SubMenu = new MenuRecord(new List<MenuOptionRecord>());

                // UI foreach enumeration was modified nightmare. (2025-02-07)
                var copySubMenuList = new List<MenuOptionRecord>(menuOptionOpen.SubMenu.MenuOptionList);
                copySubMenuList.Add(menuOptionOpenDotNetSolution);

                menuOptionOpen.SubMenu = menuOptionOpen.SubMenu with
                {
                    MenuOptionList = copySubMenuList
                };

                // Menu Option New
                {
                    var menuOptionNewDotNetSolution = new MenuOptionRecord(
                        ".NET Solution",
                        MenuOptionKind.Other,
                        DotNetService.OpenNewDotNetSolutionDialog);

                    var menuOptionNew = new MenuOptionRecord(
                        "New",
                        MenuOptionKind.Other,
                        subMenu: new MenuRecord(new List<MenuOptionRecord> { menuOptionNewDotNetSolution }));

                    var copyMenuOptionList = new List<MenuOptionRecord>(inMenu.MenuOptionList);
                    copyMenuOptionList.Insert(0, menuOptionNew);

                    return inMenu with
                    {
                        MenuOptionList = copyMenuOptionList
                    };
                }
            });

        InitializeMenuRun(DotNetService);

        var compilerService = DotNetService.IdeService.TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.C_SHARP_CLASS);

        /*if (compilerService is CSharpCompilerService cSharpCompilerService)
        {
            cSharpCompilerService.SetSymbolRendererType(typeof(Walk.Extensions.DotNet.TextEditors.Displays.CSharpSymbolDisplay));
        }*/

        DotNetService.IdeService.TextEditorService.UpsertHeader("cs", typeof(Walk.Extensions.CompilerServices.Displays.TextEditorCompilerServiceHeaderDisplay));

        var dotNetAppData = await DotNetService.AppDataService
            .ReadAppDataAsync<DotNetAppData>(
                DotNetAppData.AssemblyName, DotNetAppData.TypeName, uniqueIdentifier: null, forceRefreshCache: false)
            .ConfigureAwait(false);

        await SetSolution(DotNetService, dotNetAppData).ConfigureAwait(false);

        

        await DotNetService.TextEditorService.Options_SetFromLocalStorageAsync()
            .ConfigureAwait(false);

        await DotNetService.CommonService.
            Options_SetFromLocalStorageAsync()
            .ConfigureAwait(false);

        if (DotNetService.CommonService.WalkHostingInformation.WalkHostingKind == WalkHostingKind.Photino)
        {
            await DotNetService.CommonService.JsRuntimeCommonApi.JsRuntime.InvokeVoidAsync("walkIde.preventDefaultBrowserKeybindings").ConfigureAwait(false);
        }

        var token = _workerCancellationTokenSource.Token;

        if (DotNetService.CommonService.Continuous_StartAsyncTask is null)
        {
            DotNetService.CommonService.Continuous_StartAsyncTask = Task.Run(
                () => DotNetService.CommonService.Continuous_ExecuteAsync(token),
                token);
        }

        if (DotNetService.CommonService.WalkHostingInformation.WalkPurposeKind == WalkPurposeKind.Ide)
        {
            if (DotNetService.CommonService.Indefinite_StartAsyncTask is null)
            {
                DotNetService.CommonService.Indefinite_StartAsyncTask = Task.Run(
                    () => DotNetService.CommonService.Indefinite_ExecuteAsync(token),
                    token);
            }
        }

        BrowserResizeInterop.SubscribeWindowSizeChanged(DotNetService.CommonService.JsRuntimeCommonApi);
    }
    
    public static void InitializeMenuRun(DotNetService DotNetService)
    {
        var menuOptionsList = new List<MenuOptionRecord>();

        // Menu Option Build Project (startup project)
        menuOptionsList.Add(new MenuOptionRecord(
            "Build Project (startup project)",
            MenuOptionKind.Create,
            () =>
            {
                var startupControlState = DotNetService.IdeService.GetIdeStartupControlState();
                var activeStartupControl = startupControlState.StartupControlList.FirstOrDefault(
                    x => x.Key == startupControlState.ActiveStartupControlKey);

                if (activeStartupControl?.StartupProjectAbsolutePath is not null)
                    BuildProjectOnClick(DotNetService, activeStartupControl.StartupProjectAbsolutePath.Value);
                else
                    NotificationHelper.DispatchError(nameof(BuildProjectOnClick), "activeStartupControl?.StartupProjectAbsolutePath was null", DotNetService.CommonService, TimeSpan.FromSeconds(6));
                return Task.CompletedTask;
            }));

        // Menu Option Clean (startup project)
        menuOptionsList.Add(new MenuOptionRecord(
            "Clean Project (startup project)",
            MenuOptionKind.Create,
            () =>
            {
                var startupControlState = DotNetService.IdeService.GetIdeStartupControlState();
                var activeStartupControl = startupControlState.StartupControlList.FirstOrDefault(
                    x => x.Key == startupControlState.ActiveStartupControlKey);

                if (activeStartupControl?.StartupProjectAbsolutePath is not null)
                    CleanProjectOnClick(DotNetService, activeStartupControl.StartupProjectAbsolutePath.Value);
                else
                    NotificationHelper.DispatchError(nameof(CleanProjectOnClick), "activeStartupControl?.StartupProjectAbsolutePath was null", DotNetService.CommonService, TimeSpan.FromSeconds(6));
                return Task.CompletedTask;
            }));

        // Menu Option Build Solution
        menuOptionsList.Add(new MenuOptionRecord(
            "Build Solution",
            MenuOptionKind.Delete,
            () =>
            {
                var dotNetSolutionState = DotNetService.GetDotNetSolutionState();
                var dotNetSolutionModel = dotNetSolutionState.DotNetSolutionModel;

                if (dotNetSolutionModel?.AbsolutePath is not null)
                    BuildSolutionOnClick(DotNetService, dotNetSolutionModel.AbsolutePath.Value);
                else
                    NotificationHelper.DispatchError(nameof(BuildSolutionOnClick), "dotNetSolutionModel?.AbsolutePath was null", DotNetService.CommonService, TimeSpan.FromSeconds(6));
                return Task.CompletedTask;
            }));

        // Menu Option Clean Solution
        menuOptionsList.Add(new MenuOptionRecord(
            "Clean Solution",
            MenuOptionKind.Delete,
            () =>
            {
                var dotNetSolutionState = DotNetService.GetDotNetSolutionState();
                var dotNetSolutionModel = dotNetSolutionState.DotNetSolutionModel;

                if (dotNetSolutionModel?.AbsolutePath is not null)
                    CleanSolutionOnClick(DotNetService, dotNetSolutionModel.AbsolutePath.Value);
                else
                    NotificationHelper.DispatchError(nameof(CleanSolutionOnClick), "dotNetSolutionModel?.AbsolutePath was null", DotNetService.CommonService, TimeSpan.FromSeconds(6));
                return Task.CompletedTask;
            }));

        DotNetService.IdeService.Ide_ModifyMenuRun(inMenu =>
        {
            // UI foreach enumeration was modified nightmare. (2025-02-07)
            var copyMenuOptionList = new List<MenuOptionRecord>(inMenu.MenuOptionList);
            copyMenuOptionList.AddRange(menuOptionsList);
            return inMenu with
            {
                MenuOptionList = copyMenuOptionList
            };
        });
    }

    public static void BuildProjectOnClick(DotNetService DotNetService, string projectAbsolutePathString)
    {
        var formattedCommand = DotNetCliCommandFormatter.FormatDotnetBuildProject(projectAbsolutePathString);
        var solutionAbsolutePath = DotNetService.CommonService.EnvironmentProvider.AbsolutePathFactory(projectAbsolutePathString, false, tokenBuilder: new StringBuilder(), formattedBuilder: new StringBuilder());

        var localParentDirectory = solutionAbsolutePath.ParentDirectory;
        if (localParentDirectory is null)
            return;

        var terminalCommandRequest = new TerminalCommandRequest(
            formattedCommand.Value,
            localParentDirectory)
        {
            BeginWithFunc = parsedCommand =>
            {
                DotNetService.ParseOutputEntireDotNetRun(
                    string.Empty,
                    "Build-Project_started");
                return Task.CompletedTask;
            },
            ContinueWithFunc = parsedCommand =>
            {
                DotNetService.ParseOutputEntireDotNetRun(
                    parsedCommand.OutputCache.ToString(),
                    "Build-Project_completed");
                return Task.CompletedTask;
            }
        };

        DotNetService.IdeService.GetTerminalState().TerminalMap[IdeFacts.GENERAL_KEY].EnqueueCommand(terminalCommandRequest);
    }

    public static void CleanProjectOnClick(DotNetService DotNetService, string projectAbsolutePathString)
    {
        var formattedCommand = DotNetCliCommandFormatter.FormatDotnetCleanProject(projectAbsolutePathString);
        var solutionAbsolutePath = DotNetService.CommonService.EnvironmentProvider.AbsolutePathFactory(projectAbsolutePathString, false, tokenBuilder: new StringBuilder(), formattedBuilder: new StringBuilder());

        var localParentDirectory = solutionAbsolutePath.ParentDirectory;
        if (localParentDirectory is null)
            return;

        var terminalCommandRequest = new TerminalCommandRequest(
            formattedCommand.Value,
            localParentDirectory)
        {
            BeginWithFunc = parsedCommand =>
            {
                DotNetService.ParseOutputEntireDotNetRun(
                    string.Empty,
                    "Clean-Project_started");
                return Task.CompletedTask;
            },
            ContinueWithFunc = parsedCommand =>
            {
                DotNetService.ParseOutputEntireDotNetRun(
                    parsedCommand.OutputCache.ToString(),
                    "Clean-Project_completed");
                return Task.CompletedTask;
            }
        };

        DotNetService.IdeService.GetTerminalState().TerminalMap[IdeFacts.GENERAL_KEY].EnqueueCommand(terminalCommandRequest);
    }

    public static void BuildSolutionOnClick(DotNetService DotNetService, string solutionAbsolutePathString)
    {
        var formattedCommand = DotNetCliCommandFormatter.FormatDotnetBuildSolution(solutionAbsolutePathString);
        var solutionAbsolutePath = DotNetService.CommonService.EnvironmentProvider.AbsolutePathFactory(solutionAbsolutePathString, false, tokenBuilder: new StringBuilder(), formattedBuilder: new StringBuilder());

        var localParentDirectory = solutionAbsolutePath.ParentDirectory;
        if (localParentDirectory is null)
            return;

        var terminalCommandRequest = new TerminalCommandRequest(
            formattedCommand.Value,
            localParentDirectory)
        {
            BeginWithFunc = parsedCommand =>
            {
                DotNetService.ParseOutputEntireDotNetRun(
                    string.Empty,
                    "Build-Solution_started");
                return Task.CompletedTask;
            },
            ContinueWithFunc = parsedCommand =>
            {
                DotNetService.ParseOutputEntireDotNetRun(
                    parsedCommand.OutputCache.ToString(),
                    "Build-Solution_completed");
                return Task.CompletedTask;
            }
        };

        DotNetService.IdeService.GetTerminalState().TerminalMap[IdeFacts.GENERAL_KEY].EnqueueCommand(terminalCommandRequest);
    }

    public static void CleanSolutionOnClick(DotNetService DotNetService, string solutionAbsolutePathString)
    {
        var formattedCommand = DotNetCliCommandFormatter.FormatDotnetCleanSolution(solutionAbsolutePathString);
        var solutionAbsolutePath = DotNetService.CommonService.EnvironmentProvider.AbsolutePathFactory(solutionAbsolutePathString, false, tokenBuilder: new StringBuilder(), formattedBuilder: new StringBuilder());

        var localParentDirectory = solutionAbsolutePath.ParentDirectory;
        if (localParentDirectory is null)
            return;

        var terminalCommandRequest = new TerminalCommandRequest(
            formattedCommand.Value,
            localParentDirectory)
        {
            BeginWithFunc = parsedCommand =>
            {
                DotNetService.ParseOutputEntireDotNetRun(
                    string.Empty,
                    "Clean-Solution_started");
                return Task.CompletedTask;
            },
            ContinueWithFunc = parsedCommand =>
            {
                DotNetService.ParseOutputEntireDotNetRun(
                    parsedCommand.OutputCache.ToString(),
                    "Clean-Solution_completed");
                return Task.CompletedTask;
            }
        };

        DotNetService.IdeService.GetTerminalState().TerminalMap[IdeFacts.GENERAL_KEY].EnqueueCommand(terminalCommandRequest);
    }

    public static async Task EnqueueOnInitializedSteps(DotNetService DotNetService)
    {
        await DotNetService.CommonService.Options_SetFromLocalStorageAsync();
        await DotNetService.TextEditorService.Options_SetFromLocalStorageAsync();

        InitializeMenuFile(DotNetService);
        InitializeMenuTools(DotNetService);
        InitializeMenuView(DotNetService);
    }

    public static async Task SetSolution(DotNetService DotNetService, DotNetAppData dotNetAppData)
    {
        var solutionMostRecent = dotNetAppData?.SolutionMostRecent;

        if (solutionMostRecent is null)
            return;

        var slnAbsolutePath = DotNetService.TextEditorService.CommonService.EnvironmentProvider.AbsolutePathFactory(
            solutionMostRecent,
            false,
            tokenBuilder: new StringBuilder(),
            formattedBuilder: new StringBuilder());

        DotNetService.Enqueue(new DotNetWorkArgs
        {
            WorkKind = DotNetWorkKind.SetDotNetSolution,
            DotNetSolutionAbsolutePath = slnAbsolutePath,
        });
    }
    
    public static void InitializeMenuView(DotNetService DotNetService)
    {
        var menuOptionsList = new List<MenuOptionRecord>();
        var panelState = DotNetService.CommonService.GetPanelState();
        var dialogState = DotNetService.CommonService.GetDialogState();
    
        foreach (var panel in panelState.PanelList)
        {
            var menuOptionPanel = new MenuOptionRecord(
                panel.Title,
                MenuOptionKind.Delete,
                () => DotNetService.CommonService.ShowOrAddPanelTab(panel));
    
            menuOptionsList.Add(menuOptionPanel);
        }
    
        if (menuOptionsList.Count == 0)
        {
            DotNetService.IdeService.Ide_SetMenuView(new MenuRecord(MenuRecord.NoMenuOptionsExistList));
        }
        else
        {
            DotNetService.IdeService.Ide_SetMenuView(new MenuRecord(menuOptionsList));
        }
    }

    public static void DispatchRegisterDialogRecordAction(DotNetService DotNetService, IDialog dialog) =>
        DotNetService.CommonService.Dialog_ReduceRegisterAction(dialog);

    public static void InitializeMenuFile(DotNetService DotNetService)
    {
        var menuOptionsList = new List<MenuOptionRecord>();

        // Menu Option Open
        var menuOptionOpenFile = new MenuOptionRecord(
            "File",
            MenuOptionKind.Other,
            () =>
            {
                DotNetService.IdeService.Editor_ShowInputFile();
                return Task.CompletedTask;
            });

        var menuOptionOpenDirectory = new MenuOptionRecord(
            "Directory",
            MenuOptionKind.Other,
            () =>
            {
                DotNetService.IdeService.FolderExplorer_ShowInputFile();
                return Task.CompletedTask;
            });

        var menuOptionOpen = new MenuOptionRecord(
            "Open",
            MenuOptionKind.Other,
            subMenu: new MenuRecord(new List<MenuOptionRecord>()
            {
                menuOptionOpenFile,
                menuOptionOpenDirectory,
            }));

        menuOptionsList.Add(menuOptionOpen);

        var menuOptionSave = new MenuOptionRecord(
            "Save (Ctrl s)",
            MenuOptionKind.Other,
            () =>
            {
                TextEditorCommandDefaultFunctions.TriggerSave_NoTextEditorFocus(DotNetService.TextEditorService, Walk.Ide.RazorLib.IdeService.EditorTextEditorGroupKey);
                return Task.CompletedTask;
            });
        menuOptionsList.Add(menuOptionSave);

        var menuOptionSaveAll = new MenuOptionRecord(
            "Save All (Ctrl Shift s)",
            MenuOptionKind.Other,
            () =>
            {
                TextEditorCommandDefaultFunctions.TriggerSaveAll(DotNetService.TextEditorService, Walk.Ide.RazorLib.IdeService.EditorTextEditorGroupKey);
                return Task.CompletedTask;
            });
        menuOptionsList.Add(menuOptionSaveAll);

        // Menu Option Permissions
        var menuOptionPermissions = new MenuOptionRecord(
            "Permissions",
            MenuOptionKind.Delete,
            () => ShowPermissionsDialog(DotNetService));

        menuOptionsList.Add(menuOptionPermissions);

        DotNetService.IdeService.Ide_SetMenuFile(new MenuRecord(menuOptionsList));
    }

    public static void InitializeMenuTools(DotNetService DotNetService)
    {
        var menuOptionsList = new List<MenuOptionRecord>();

        // Menu Option Find All
        var menuOptionFindAll = new MenuOptionRecord(
            "Find All (Ctrl Shift f)",
            MenuOptionKind.Delete,
            () =>
            {
                DotNetService.TextEditorService.Options_ShowFindAllDialog();
                return Task.CompletedTask;
            });

        menuOptionsList.Add(menuOptionFindAll);

        // Menu Option Code Search
        var menuOptionCodeSearch = new MenuOptionRecord(
            "Code Search (Ctrl ,)",
            MenuOptionKind.Delete,
            () =>
            {
                DotNetService.IdeService.CodeSearchDialog ??= new DialogViewModel(
                    Key<IDynamicViewModel>.NewKey(),
                    "Code Search",
                    typeof(CodeSearchDisplay),
                    null,
                    null,
                    true,
                    null);

                DotNetService.CommonService.Dialog_ReduceRegisterAction(DotNetService.IdeService.CodeSearchDialog);
                return Task.CompletedTask;
            });

        menuOptionsList.Add(menuOptionCodeSearch);

        DotNetService.IdeService.Ide_SetMenuTools(new MenuRecord(menuOptionsList));
    }

    public static Task ShowPermissionsDialog(DotNetService DotNetService)
    {
        var dialogRecord = new DialogViewModel(
            _permissionsDialogKey,
            "Permissions",
            typeof(PermissionsDisplay),
            null,
            null,
            true,
            null);

        DotNetService.CommonService.Dialog_ReduceRegisterAction(dialogRecord);
        return Task.CompletedTask;
    }
    
    public static void HandleCompilerServicesAndDecorationMappers(DotNetService DotNetService)
    {
        var cSharpCompilerService = new CSharpCompilerService(DotNetService.TextEditorService);
        var cSharpProjectCompilerService = new CSharpProjectCompilerService(DotNetService.TextEditorService);
        // var javaScriptCompilerService = new JavaScriptCompilerService(TextEditorService);
        var cssCompilerService = new CssCompilerService(DotNetService.TextEditorService);
        var dotNetSolutionCompilerService = new DotNetSolutionCompilerService(DotNetService.TextEditorService);
        var jsonCompilerService = new JsonCompilerService(DotNetService.TextEditorService);
        var razorCompilerService = new RazorCompilerService(DotNetService.TextEditorService, cSharpCompilerService);
        var xmlCompilerService = new XmlCompilerService(DotNetService.TextEditorService);
        var terminalCompilerService = new TerminalCompilerService(DotNetService.IdeService);
        var defaultCompilerService = new CompilerServiceDoNothing();

        DotNetService.TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.HTML, xmlCompilerService);
        DotNetService.TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.XML, xmlCompilerService);
        DotNetService.TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.C_SHARP_PROJECT, cSharpProjectCompilerService);
        DotNetService.TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.C_SHARP_CLASS, cSharpCompilerService);
        // DotNetService.TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.JAVA_SCRIPT, JavaScriptCompilerService);
        DotNetService.TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.RAZOR_CODEBEHIND, cSharpCompilerService);
        DotNetService.TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.RAZOR_MARKUP, razorCompilerService);
        DotNetService.TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.CSHTML_CLASS, razorCompilerService);
        DotNetService.TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.CSS, cssCompilerService);
        DotNetService.TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.JSON, jsonCompilerService);
        DotNetService.TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.DOT_NET_SOLUTION, dotNetSolutionCompilerService);
        DotNetService.TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.DOT_NET_SOLUTION_X, dotNetSolutionCompilerService);
        DotNetService.TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.TERMINAL, terminalCompilerService);
        
        //
        // Decoration Mapper starts after this point.
        //
        
        var cssDecorationMapper = new TextEditorCssDecorationMapper();
        var jsonDecorationMapper = new TextEditorJsonDecorationMapper();
        var genericDecorationMapper = new GenericDecorationMapper();
        var htmlDecorationMapper = new TextEditorHtmlDecorationMapper();
        var terminalDecorationMapper = new TerminalDecorationMapper();
        var defaultDecorationMapper = new TextEditorDecorationMapperDefault();

        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.HTML, htmlDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.XML, htmlDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.C_SHARP_PROJECT, htmlDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.C_SHARP_CLASS, genericDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.RAZOR_CODEBEHIND, genericDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.RAZOR_MARKUP, htmlDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.CSHTML_CLASS, htmlDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.CSS, cssDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.JAVA_SCRIPT, genericDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.JSON, jsonDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.TYPE_SCRIPT, genericDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.F_SHARP, genericDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.C, genericDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.PYTHON, genericDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.H, genericDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.CPP, genericDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.HPP, genericDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.DOT_NET_SOLUTION, htmlDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.DOT_NET_SOLUTION_X, htmlDecorationMapper);
        DotNetService.TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.TERMINAL, terminalDecorationMapper);
    }
    
    public static void InitializePanelTabs(DotNetService DotNetService)
    {
        var panelState = DotNetService.CommonService.GetPanelState();
        var appOptionsState = DotNetService.CommonService.GetAppOptionsState();
    
        // InitializeLeftPanelTabs();
        var leftPanel = panelState.TopLeftPanelGroup;
        leftPanel.CommonService = DotNetService.CommonService;
    
        // solutionExplorerPanel
        var solutionExplorerPanel = new Panel(
            "Solution Explorer",
            Key<Panel>.NewKey(),
            Key<IDynamicViewModel>.NewKey(),
            typeof(Walk.Extensions.DotNet.DotNetSolutions.Displays.SolutionExplorerDisplay),
            null,
            DotNetService.CommonService);
        ((List<Panel>)panelState.PanelList).Add(solutionExplorerPanel);
        ((List<IPanelTab>)leftPanel.TabList).Add(solutionExplorerPanel);
        
        // folderExplorerPanel
        var folderExplorerPanel = new Panel(
            "Folder Explorer",
            Key<Panel>.NewKey(),
            Key<IDynamicViewModel>.NewKey(),
            typeof(FolderExplorerDisplay),
            null,
            DotNetService.CommonService);
        ((List<Panel>)panelState.PanelList).Add(folderExplorerPanel);
        ((List<IPanelTab>)leftPanel.TabList).Add(folderExplorerPanel);
        
        // InitializeRightPanelTabs();
        var rightPanel = panelState.TopRightPanelGroup;
        rightPanel.CommonService = DotNetService.CommonService;
        Panel_InitializeResizeHandleDimensionUnit(
            DotNetService,
            rightPanel.Key,
            new DimensionUnit(
                () => appOptionsState.Options.ResizeHandleWidthInPixels / 2,
                DimensionUnitKind.Pixels,
                DimensionOperatorKind.Subtract,
                DimensionUnitPurposeKind.ResizableHandleColumn));
        
        // InitializeBottomPanelTabs();
        var bottomPanel = panelState.BottomPanelGroup;
        bottomPanel.CommonService = DotNetService.CommonService;
        Panel_InitializeResizeHandleDimensionUnit(
            DotNetService,
            bottomPanel.Key,
            new DimensionUnit(
                () => appOptionsState.Options.ResizeHandleHeightInPixels / 2,
                DimensionUnitKind.Pixels,
                DimensionOperatorKind.Subtract,
                DimensionUnitPurposeKind.ResizableHandleRow));

        // terminalGroupPanel
        var terminalGroupPanel = new Panel(
            "Terminal",
            Key<Panel>.NewKey(),
            Key<IDynamicViewModel>.NewKey(),
            typeof(Walk.Ide.RazorLib.Terminals.Displays.TerminalGroupDisplay),
            null,
            DotNetService.CommonService);
        ((List<Panel>)panelState.PanelList).Add(terminalGroupPanel);
        ((List<IPanelTab>)bottomPanel.TabList).Add(terminalGroupPanel);

        // SetActivePanelTabAction
        //_panelService.SetActivePanelTab(bottomPanel.Key, terminalGroupPanel.Key);

        // outputPanel
        var outputPanel = new Panel(
            "Output",
            Key<Panel>.NewKey(),
            Key<IDynamicViewModel>.NewKey(),
            typeof(Walk.Extensions.DotNet.Outputs.Displays.OutputPanelDisplay),
            null,
            DotNetService.CommonService);
        ((List<Panel>)panelState.PanelList).Add(outputPanel);
        ((List<IPanelTab>)bottomPanel.TabList).Add(outputPanel);

        // testExplorerPanel
        var testExplorerPanel = new Panel(
            "Test Explorer",
            Key<Panel>.NewKey(),
            Key<IDynamicViewModel>.NewKey(),
            typeof(Walk.Extensions.DotNet.TestExplorers.Displays.TestExplorerDisplay),
            null,
            DotNetService.CommonService);
        ((List<Panel>)panelState.PanelList).Add(testExplorerPanel);
        ((List<IPanelTab>)bottomPanel.TabList).Add(testExplorerPanel);

        // nuGetPanel
        var nuGetPanel = new Panel(
            "NuGet",
            Key<Panel>.NewKey(),
            Key<IDynamicViewModel>.NewKey(),
            typeof(Walk.Extensions.DotNet.Nugets.Displays.NuGetPackageManager),
            null,
            DotNetService.CommonService);
        ((List<Panel>)panelState.PanelList).Add(nuGetPanel);
        ((List<IPanelTab>)bottomPanel.TabList).Add(nuGetPanel);
        
        CodeSearch_InitializeResizeHandleDimensionUnit(
            DotNetService,
            new DimensionUnit(
                () => appOptionsState.Options.ResizeHandleHeightInPixels / 2,
                DimensionUnitKind.Pixels,
                DimensionOperatorKind.Subtract,
                DimensionUnitPurposeKind.ResizableHandleRow));
    
        Panel_InitializeResizeHandleDimensionUnit(
            DotNetService,
            leftPanel.Key,
            new DimensionUnit(
                () => appOptionsState.Options.ResizeHandleWidthInPixels / 2,
                DimensionUnitKind.Pixels,
                DimensionOperatorKind.Subtract,
                DimensionUnitPurposeKind.ResizableHandleColumn));
        
        // terminalGroupPanel: This UI has resizable parts that need to be initialized.
        TerminalGroup_InitializeResizeHandleDimensionUnit(
            DotNetService,
            new DimensionUnit(
                () => appOptionsState.Options.ResizeHandleWidthInPixels / 2,
                DimensionUnitKind.Pixels,
                DimensionOperatorKind.Subtract,
                DimensionUnitPurposeKind.ResizableHandleColumn));
        
        // testExplorerPanel: This UI has resizable parts that need to be initialized.
        ReduceInitializeResizeHandleDimensionUnitAction(
            DotNetService,
            new DimensionUnit(
                () => appOptionsState.Options.ResizeHandleWidthInPixels / 2,
                DimensionUnitKind.Pixels,
                DimensionOperatorKind.Subtract,
                DimensionUnitPurposeKind.ResizableHandleColumn));
    
        // SetActivePanelTabAction
        DotNetService.CommonService.SetActivePanelTab(leftPanel.Key, solutionExplorerPanel.Key);
        
        // SetActivePanelTabAction
        DotNetService.CommonService.SetActivePanelTab(bottomPanel.Key, outputPanel.Key);
    }

    public static void Panel_InitializeResizeHandleDimensionUnit(DotNetService DotNetService, Key<PanelGroup> panelGroupKey, DimensionUnit dimensionUnit)
    {
        var inState = DotNetService.CommonService.GetPanelState();

        PanelGroup inPanelGroup;
            
        if (panelGroupKey == inState.TopLeftPanelGroup.Key)
        {
            inPanelGroup = inState.TopLeftPanelGroup;
        }
        else if (panelGroupKey == inState.TopRightPanelGroup.Key)
        {
            inPanelGroup = inState.TopRightPanelGroup;
        }
        else if (panelGroupKey == inState.BottomPanelGroup.Key)
        {
            inPanelGroup = inState.BottomPanelGroup;
        }
        else
        {
            return;
        }

        if (inPanelGroup is not null)
        {
            if (dimensionUnit.Purpose == DimensionUnitPurposeKind.ResizableHandleRow ||
                dimensionUnit.Purpose == DimensionUnitPurposeKind.ResizableHandleColumn)
            {
                if (dimensionUnit.Purpose == DimensionUnitPurposeKind.ResizableHandleRow)
                {
                    if (inPanelGroup.ElementDimensions.HeightDimensionAttribute.DimensionUnitList is not null)
                    {
                        var existingDimensionUnit = inPanelGroup.ElementDimensions.HeightDimensionAttribute.DimensionUnitList
                            .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);
        
                        if (existingDimensionUnit.Purpose == DimensionUnitPurposeKind.None)
                            inPanelGroup.ElementDimensions.HeightDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
                    }
                }
                else if (dimensionUnit.Purpose != DimensionUnitPurposeKind.ResizableHandleColumn)
                {
                    if (inPanelGroup.ElementDimensions.WidthDimensionAttribute.DimensionUnitList is not null)
                    {
                        var existingDimensionUnit = inPanelGroup.ElementDimensions.WidthDimensionAttribute.DimensionUnitList
                            .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);
        
                        if (existingDimensionUnit.Purpose == DimensionUnitPurposeKind.None)
                            inPanelGroup.ElementDimensions.WidthDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
                    }
                }
            }
        }
    }
    
    public static void ReduceInitializeResizeHandleDimensionUnitAction(DotNetService DotNetService, DimensionUnit dimensionUnit)
    {
        var inState = DotNetService.GetTestExplorerState();

        if (dimensionUnit.Purpose != DimensionUnitPurposeKind.ResizableHandleColumn)
        {
            return;
        }

        // TreeViewElementDimensions
        {
            if (inState.TreeViewElementDimensions.WidthDimensionAttribute.DimensionUnitList is null)
            {
                return;
            }

            var existingDimensionUnit = inState.TreeViewElementDimensions.WidthDimensionAttribute.DimensionUnitList
                .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);

            if (existingDimensionUnit.Purpose != DimensionUnitPurposeKind.None)
            {
                return;
            }

            inState.TreeViewElementDimensions.WidthDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
        }

        // DetailsElementDimensions
        {
            if (inState.DetailsElementDimensions.WidthDimensionAttribute.DimensionUnitList is null)
            {
                return;
            }

            var existingDimensionUnit = inState.DetailsElementDimensions.WidthDimensionAttribute.DimensionUnitList
                .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);

            if (existingDimensionUnit.Purpose != DimensionUnitPurposeKind.None)
            {
                return;
            }

            inState.DetailsElementDimensions.WidthDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
        }
    }
    
    public static void CodeSearch_InitializeResizeHandleDimensionUnit(DotNetService DotNetService, DimensionUnit dimensionUnit)
    {
        var codeSearchState = DotNetService.IdeService.GetCodeSearchState();
    
        if (dimensionUnit.Purpose == DimensionUnitPurposeKind.ResizableHandleRow)
        {
            if (codeSearchState.TopContentElementDimensions.HeightDimensionAttribute.DimensionUnitList is not null)
            {
                var existingDimensionUnit = codeSearchState.TopContentElementDimensions.HeightDimensionAttribute.DimensionUnitList
                    .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);

                if (existingDimensionUnit.Purpose == DimensionUnitPurposeKind.None)
                    codeSearchState.TopContentElementDimensions.HeightDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
            }

            if (codeSearchState.BottomContentElementDimensions.HeightDimensionAttribute.DimensionUnitList is not null)
            {
                var existingDimensionUnit = codeSearchState.BottomContentElementDimensions.HeightDimensionAttribute.DimensionUnitList
                    .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);

                if (existingDimensionUnit.Purpose == DimensionUnitPurposeKind.None)
                    codeSearchState.BottomContentElementDimensions.HeightDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
            }
        }
    }
    
    public static void TerminalGroup_InitializeResizeHandleDimensionUnit(DotNetService DotNetService, DimensionUnit dimensionUnit)
    {
        var terminalGroupState = DotNetService.IdeService.GetTerminalGroupState();
    
        if (dimensionUnit.Purpose == DimensionUnitPurposeKind.ResizableHandleColumn)
        {
            if (terminalGroupState.BodyElementDimensions.WidthDimensionAttribute.DimensionUnitList is not null)
            {
                var existingDimensionUnit = terminalGroupState.BodyElementDimensions.WidthDimensionAttribute.DimensionUnitList
                    .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);

                if (existingDimensionUnit.Purpose == DimensionUnitPurposeKind.None)
                    terminalGroupState.BodyElementDimensions.WidthDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
            }

            if (terminalGroupState.TabsElementDimensions.WidthDimensionAttribute.DimensionUnitList is not null)
            {
                var existingDimensionUnit = terminalGroupState.TabsElementDimensions.WidthDimensionAttribute.DimensionUnitList
                    .FirstOrDefault(x => x.Purpose == dimensionUnit.Purpose);

                if (existingDimensionUnit.Purpose == DimensionUnitPurposeKind.None)
                    terminalGroupState.TabsElementDimensions.WidthDimensionAttribute.DimensionUnitList.Add(dimensionUnit);
            }
        }
    }

    // private const string TEST_STRING_FOR_MEASUREMENT = "abcdefghijklmnopqrstuvwxyz0123456789";

    // TODO: Just define all this in JavaScript to avoid the JSInterop for the parameter?
    //
    //      CharacterWidth         | LineHeight
    // 99,000 => 5.021239618406285 | 20
    // 60,000 => 7.146064814814815 | 20
    // 30,000 => 7.146064814814815 | 20
    // 25,000 => 7.146064444444445 | 20
    // 18,000 => 7.146064814814815 | 20
    //  9,000 => 7.146064814814815 | 20
    //  5,000 => 7.146066666666667 | 20
    //  1,000 => 7.146055555555556 | 20
    //    100 => 7.146111111111111 | 20
    //     40 => 7.145833333333333 | 20
    //     20 => 7.145833333333333 | 20
    //     11 => 7.146464646464646 | 20
    //     10 => 7.147222222222222 | 20
    //      9 => 7.145061728395062 | 20
    //      8 => 7.145833333333333 | 20
    //      7 => 7.146825396825397 | 20
    //      6 => 7.148148148148148 | 20
    //      5 => 7.144444444444445 | 20
    //      4 => 7.145833333333333 | 20
    //      3 => 7.148148148148148 | 20
    //      2 => 7.152777777777778 | 20
    //      1 => 7.138888888888889 | 20
    // private const int TEST_STRING_REPEAT_COUNT = 11;
}
