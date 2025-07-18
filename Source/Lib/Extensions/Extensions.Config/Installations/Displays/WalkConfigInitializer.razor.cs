using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Edits.Models;
using Walk.Ide.RazorLib.BackgroundTasks.Models;
using Walk.Ide.RazorLib.ComponentRenderers.Models;
using Walk.Ide.RazorLib.FileSystems.Models;
using Walk.Ide.RazorLib.InputFiles.Displays;
using Walk.Ide.RazorLib.InputFiles.Models;
using Walk.Ide.RazorLib.AppDatas.Models;
using Walk.Ide.RazorLib.Shareds.Models;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.Extensions.DotNet.BackgroundTasks.Models;
using Walk.Extensions.DotNet.AppDatas.Models;

// CompilerServiceRegistry.cs
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.CompilerServices.CSharp.CompilerServiceCase;
using Walk.CompilerServices.CSharpProject.CompilerServiceCase;
using Walk.CompilerServices.Css;
using Walk.CompilerServices.DotNetSolution.CompilerServiceCase;
using Walk.CompilerServices.Json;
using Walk.CompilerServices.Razor.CompilerServiceCase;
using Walk.CompilerServices.Xml;
using Walk.TextEditor.RazorLib.CompilerServices;

// DecorationMapperRegistry.cs
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.CompilerServices.Css.Decoration;
using Walk.CompilerServices.Json.Decoration;
using Walk.CompilerServices.Xml.Html.Decoration;

using Walk.Ide.RazorLib;

namespace Walk.Extensions.Config.Installations.Displays;

public partial class WalkConfigInitializer : ComponentBase
{
    [Inject]
    private IdeService IdeService { get; set; } = null!;
    [Inject]
    private DotNetBackgroundTaskApi DotNetBackgroundTaskApi { get; set; } = null!;
	[Inject]
	private IAppDataService AppDataService { get; set; } = null!;
	[Inject]
	private TextEditorService TextEditorService { get; set; } = null!;
	
    private static Key<IDynamicViewModel> _notificationRecordKey = Key<IDynamicViewModel>.NewKey();

	protected override void OnInitialized()
	{
	    HandleCompilerServicesAndDecorationMappers();
	
        TextEditorService.CommonUtilityService.Continuous_EnqueueGroup(new BackgroundTask(
        	Key<IBackgroundTaskGroup>.Empty,
        	Do_InitializeFooterBadges));
	
	    DotNetBackgroundTaskApi.Enqueue(new DotNetBackgroundTaskApiWorkArgs
		{
			WorkKind = DotNetBackgroundTaskApiWorkKind.WalkExtensionsDotNetInitializerOnInit,
		});
	}

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            DotNetBackgroundTaskApi.Enqueue(new DotNetBackgroundTaskApiWorkArgs
            {
            	WorkKind = DotNetBackgroundTaskApiWorkKind.WalkExtensionsDotNetInitializerOnAfterRender
            });
        
        	var dotNetAppData = await AppDataService
        		.ReadAppDataAsync<DotNetAppData>(
        			DotNetAppData.AssemblyName, DotNetAppData.TypeName, uniqueIdentifier: null, forceRefreshCache: false)
        		.ConfigureAwait(false);
        		
        	await SetSolution(dotNetAppData).ConfigureAwait(false);
        }
    }
    
    public ValueTask Do_InitializeFooterBadges()
    {
        IdeService.Ide_RegisterFooterBadge(
            new DirtyResourceUriBadge(TextEditorService));

        IdeService.Ide_RegisterFooterBadge(
            new NotificationBadge(TextEditorService.CommonUtilityService));

        return ValueTask.CompletedTask;
    }
    
    private async Task SetSolution(DotNetAppData dotNetAppData)
    {
    	var solutionMostRecent = dotNetAppData?.SolutionMostRecent;
    
    	if (solutionMostRecent is null)
    		return;
    
    	var slnAbsolutePath = TextEditorService.CommonUtilityService.EnvironmentProvider.AbsolutePathFactory(
            solutionMostRecent,
            false);

        DotNetBackgroundTaskApi.Enqueue(new DotNetBackgroundTaskApiWorkArgs
        {
        	WorkKind = DotNetBackgroundTaskApiWorkKind.SetDotNetSolution,
        	DotNetSolutionAbsolutePath = slnAbsolutePath,
    	});

        var parentDirectory = slnAbsolutePath.ParentDirectory;
        if (parentDirectory is not null)
        {
            var parentDirectoryAbsolutePath = TextEditorService.CommonUtilityService.EnvironmentProvider.AbsolutePathFactory(
                parentDirectory,
                true);

            var pseudoRootNode = new TreeViewAbsolutePath(
                parentDirectoryAbsolutePath,
                IdeService.IdeComponentRenderers,
                TextEditorService.CommonUtilityService,
                true,
                false);

            await pseudoRootNode.LoadChildListAsync().ConfigureAwait(false);

            var adhocRootNode = TreeViewAdhoc.ConstructTreeViewAdhoc(pseudoRootNode.ChildList.ToArray());

            foreach (var child in adhocRootNode.ChildList)
            {
                child.IsExpandable = false;
            }

            var activeNode = adhocRootNode.ChildList.FirstOrDefault();

            if (!TextEditorService.CommonUtilityService.TryGetTreeViewContainer(InputFileContent.TreeViewContainerKey, out var treeViewContainer))
            {
                TextEditorService.CommonUtilityService.TreeView_RegisterContainerAction(new TreeViewContainer(
                    InputFileContent.TreeViewContainerKey,
                    adhocRootNode,
                    activeNode is null
                        ? new List<TreeViewNoType>()
                        : new() { activeNode }));
            }
            else
            {
                TextEditorService.CommonUtilityService.TreeView_WithRootNodeAction(InputFileContent.TreeViewContainerKey, adhocRootNode);

                TextEditorService.CommonUtilityService.TreeView_SetActiveNodeAction(
                    InputFileContent.TreeViewContainerKey,
                    activeNode,
                    true,
                    false);
            }
            await pseudoRootNode.LoadChildListAsync().ConfigureAwait(false);

            IdeService.InputFile_SetOpenedTreeViewModel(
                pseudoRootNode,
                IdeService.IdeComponentRenderers,
                TextEditorService.CommonUtilityService);
        }

		/*
        if (!string.IsNullOrWhiteSpace(projectPersonalPath) &&
            await FileSystemProvider.File.ExistsAsync(projectPersonalPath).ConfigureAwait(false))
        {
            var projectAbsolutePath = EnvironmentProvider.AbsolutePathFactory(
                projectPersonalPath,
                false);

			var startupControl = StartupControlStateWrap.Value.StartupControlList.FirstOrDefault(
				x => x.StartupProjectAbsolutePath.Value == projectAbsolutePath.Value);
				
			if (startupControl is null)
				return;
			
			Dispatcher.Dispatch(new StartupControlState.SetActiveStartupControlKeyAction(startupControl.Key));	
        }
        */
    }
    
    private void HandleCompilerServicesAndDecorationMappers()
    {
        var cSharpCompilerService = new CSharpCompilerService(TextEditorService);
        var cSharpProjectCompilerService = new CSharpProjectCompilerService(TextEditorService);
        // var javaScriptCompilerService = new JavaScriptCompilerService(TextEditorService);
        var cssCompilerService = new CssCompilerService(TextEditorService);
        var dotNetSolutionCompilerService = new DotNetSolutionCompilerService(TextEditorService);
        var jsonCompilerService = new JsonCompilerService(TextEditorService);
        var razorCompilerService = new RazorCompilerService(TextEditorService, cSharpCompilerService);
        var xmlCompilerService = new XmlCompilerService(TextEditorService);
        var terminalCompilerService = new TerminalCompilerService(IdeService);
        var defaultCompilerService = new CompilerServiceDoNothing();

        TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.HTML, xmlCompilerService);
        TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.XML, xmlCompilerService);
        TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.C_SHARP_PROJECT, cSharpProjectCompilerService);
        TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.C_SHARP_CLASS, cSharpCompilerService);
        // TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.JAVA_SCRIPT, JavaScriptCompilerService);
        TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.RAZOR_CODEBEHIND, cSharpCompilerService);
        TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.RAZOR_MARKUP, razorCompilerService);
        TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.CSHTML_CLASS, razorCompilerService);
        TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.CSS, cssCompilerService);
        TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.JSON, jsonCompilerService);
        TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.DOT_NET_SOLUTION, dotNetSolutionCompilerService);
        TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.DOT_NET_SOLUTION_X, dotNetSolutionCompilerService);
        TextEditorService.RegisterCompilerService(ExtensionNoPeriodFacts.TERMINAL, terminalCompilerService);
        
        //
        // Decoration Mapper starts after this point.
        //
        
        var cssDecorationMapper = new TextEditorCssDecorationMapper();
        var jsonDecorationMapper = new TextEditorJsonDecorationMapper();
        var genericDecorationMapper = new GenericDecorationMapper();
        var htmlDecorationMapper = new TextEditorHtmlDecorationMapper();
        var terminalDecorationMapper = new TerminalDecorationMapper();
        var defaultDecorationMapper = new TextEditorDecorationMapperDefault();

        TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.HTML, htmlDecorationMapper);
        TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.XML, htmlDecorationMapper);
        TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.C_SHARP_PROJECT, htmlDecorationMapper);
        TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.C_SHARP_CLASS, genericDecorationMapper);
        TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.RAZOR_CODEBEHIND, genericDecorationMapper);
        TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.RAZOR_MARKUP, htmlDecorationMapper);
        TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.CSHTML_CLASS, htmlDecorationMapper);
        TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.CSS, cssDecorationMapper);
        TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.JAVA_SCRIPT, genericDecorationMapper);
        TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.JSON, jsonDecorationMapper);
        TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.TYPE_SCRIPT, genericDecorationMapper);
        TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.F_SHARP, genericDecorationMapper);
        TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.C, genericDecorationMapper);
        TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.PYTHON, genericDecorationMapper);
        TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.H, genericDecorationMapper);
        TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.CPP, genericDecorationMapper);
        TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.HPP, genericDecorationMapper);
        TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.DOT_NET_SOLUTION, htmlDecorationMapper);
        TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.DOT_NET_SOLUTION_X, htmlDecorationMapper);
        TextEditorService.RegisterDecorationMapper(ExtensionNoPeriodFacts.TERMINAL, terminalDecorationMapper);
    }
}
