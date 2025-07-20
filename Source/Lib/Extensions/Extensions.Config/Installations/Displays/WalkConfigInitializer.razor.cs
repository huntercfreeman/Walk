using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.TextEditor.RazorLib.Edits.Models;
using Walk.Ide.RazorLib.FileSystems.Models;
using Walk.Ide.RazorLib.InputFiles.Displays;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.Extensions.DotNet;
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

namespace Walk.Extensions.Config.Installations.Displays;

public partial class WalkConfigInitializer : ComponentBase
{
    [Inject]
    private DotNetService DotNetService { get; set; } = null!;
	
    private static Key<IDynamicViewModel> _notificationRecordKey = Key<IDynamicViewModel>.NewKey();

	protected override void OnInitialized()
	{
	    HandleCompilerServicesAndDecorationMappers();
	
        DotNetService.TextEditorService.CommonService.Continuous_EnqueueGroup(new BackgroundTask(
        	Key<IBackgroundTaskGroup>.Empty,
        	Do_InitializeFooterBadges));
	
	    DotNetService.Enqueue(new DotNetWorkArgs
		{
			WorkKind = DotNetWorkKind.WalkExtensionsDotNetInitializerOnInit,
		});
	}

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            DotNetService.Enqueue(new DotNetWorkArgs
            {
            	WorkKind = DotNetWorkKind.WalkExtensionsDotNetInitializerOnAfterRender
            });
        
        	var dotNetAppData = await DotNetService.AppDataService
        		.ReadAppDataAsync<DotNetAppData>(
        			DotNetAppData.AssemblyName, DotNetAppData.TypeName, uniqueIdentifier: null, forceRefreshCache: false)
        		.ConfigureAwait(false);
        		
        	await SetSolution(dotNetAppData).ConfigureAwait(false);
        }
    }
    
    public ValueTask Do_InitializeFooterBadges()
    {
        DotNetService.IdeService.Ide_RegisterFooterBadge(
            new DirtyResourceUriBadge(DotNetService.TextEditorService));

        DotNetService.IdeService.Ide_RegisterFooterBadge(
            new NotificationBadge(DotNetService.TextEditorService.CommonService));

        return ValueTask.CompletedTask;
    }
    
    private async Task SetSolution(DotNetAppData dotNetAppData)
    {
    	var solutionMostRecent = dotNetAppData?.SolutionMostRecent;
    
    	if (solutionMostRecent is null)
    		return;
    
    	var slnAbsolutePath = DotNetService.TextEditorService.CommonService.EnvironmentProvider.AbsolutePathFactory(
            solutionMostRecent,
            false);

        DotNetService.Enqueue(new DotNetWorkArgs
        {
        	WorkKind = DotNetWorkKind.SetDotNetSolution,
        	DotNetSolutionAbsolutePath = slnAbsolutePath,
    	});

        var parentDirectory = slnAbsolutePath.ParentDirectory;
        if (parentDirectory is not null)
        {
            var parentDirectoryAbsolutePath = DotNetService.TextEditorService.CommonService.EnvironmentProvider.AbsolutePathFactory(
                parentDirectory,
                true);

            var pseudoRootNode = new TreeViewAbsolutePath(
                parentDirectoryAbsolutePath,
                DotNetService.TextEditorService.CommonService,
                true,
                false);

            await pseudoRootNode.LoadChildListAsync().ConfigureAwait(false);

            var adhocRootNode = TreeViewAdhoc.ConstructTreeViewAdhoc(pseudoRootNode.ChildList.ToArray());

            foreach (var child in adhocRootNode.ChildList)
            {
                child.IsExpandable = false;
            }

            var activeNode = adhocRootNode.ChildList.FirstOrDefault();

            if (!DotNetService.TextEditorService.CommonService.TryGetTreeViewContainer(InputFileContent.TreeViewContainerKey, out var treeViewContainer))
            {
                DotNetService.TextEditorService.CommonService.TreeView_RegisterContainerAction(new TreeViewContainer(
                    InputFileContent.TreeViewContainerKey,
                    adhocRootNode,
                    activeNode is null
                        ? new List<TreeViewNoType>()
                        : new() { activeNode }));
            }
            else
            {
                DotNetService.TextEditorService.CommonService.TreeView_WithRootNodeAction(InputFileContent.TreeViewContainerKey, adhocRootNode);

                DotNetService.TextEditorService.CommonService.TreeView_SetActiveNodeAction(
                    InputFileContent.TreeViewContainerKey,
                    activeNode,
                    true,
                    false);
            }
            await pseudoRootNode.LoadChildListAsync().ConfigureAwait(false);

            DotNetService.IdeService.InputFile_SetOpenedTreeViewModel(
                pseudoRootNode,
                DotNetService.TextEditorService.CommonService);
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
}
