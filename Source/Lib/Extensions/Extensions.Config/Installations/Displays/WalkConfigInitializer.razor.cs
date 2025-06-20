using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Ide.RazorLib.ComponentRenderers.Models;
using Walk.Ide.RazorLib.FileSystems.Models;
using Walk.Ide.RazorLib.InputFiles.Displays;
using Walk.Ide.RazorLib.InputFiles.Models;
using Walk.Ide.RazorLib.AppDatas.Models;
using Walk.Ide.RazorLib.Shareds.Models;
using Walk.Extensions.DotNet.BackgroundTasks.Models;
using Walk.Extensions.DotNet.AppDatas.Models;

namespace Walk.Extensions.Config.Installations.Displays;

public partial class WalkConfigInitializer : ComponentBase
{
    [Inject]
    private IFileSystemProvider FileSystemProvider { get; set; } = null!;
    [Inject]
    private ITreeViewService TreeViewService { get; set; } = null!;
    [Inject]
    private IEnvironmentProvider EnvironmentProvider { get; set; } = null!;
    [Inject]
    private IIdeComponentRenderers IdeComponentRenderers { get; set; } = null!;
    [Inject]
    private ICommonComponentRenderers CommonComponentRenderers { get; set; } = null!;
    [Inject]
    private DotNetBackgroundTaskApi DotNetBackgroundTaskApi { get; set; } = null!;
	[Inject]
	private IAppDataService AppDataService { get; set; } = null!;
	[Inject]
	private IInputFileService InputFileService { get; set; } = null!;
	[Inject]
	private BackgroundTaskService BackgroundTaskService { get; set; } = null!;
	[Inject]
	private IIdeMainLayoutService IdeMainLayoutService { get; set; } = null!;

	protected override void OnInitialized()
	{
        BackgroundTaskService.Continuous_EnqueueGroup(new BackgroundTask(
        	Key<IBackgroundTaskGroup>.Empty,
        	Do_InitializeFooterJustifyEndComponents));
		base.OnInitialized();
	}

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
        	var dotNetAppData = await AppDataService
        		.ReadAppDataAsync<DotNetAppData>(
        			DotNetAppData.AssemblyName, DotNetAppData.TypeName, uniqueIdentifier: null, forceRefreshCache: false)
        		.ConfigureAwait(false);
        		
        	await SetSolution(dotNetAppData).ConfigureAwait(false);
        }
    }
    
    public ValueTask Do_InitializeFooterJustifyEndComponents()
    {
        /*_ideMainLayoutService.RegisterFooterJustifyEndComponent(
            new FooterJustifyEndComponent(
                Key<FooterJustifyEndComponent>.NewKey(),
                typeof(GitInteractiveIconDisplay),
                new Dictionary<string, object?>
                {
                    {
                        nameof(GitInteractiveIconDisplay.CssStyleString),
                        "margin-right: 15px;"
                    }
                }));*/

        IdeMainLayoutService.RegisterFooterJustifyEndComponent(
            new FooterJustifyEndComponent(
                Key<FooterJustifyEndComponent>.NewKey(),
                typeof(Walk.TextEditor.RazorLib.Edits.Displays.DirtyResourceUriInteractiveIconDisplay),
                new Dictionary<string, object?>
                {
                    {
                        nameof(Walk.TextEditor.RazorLib.Edits.Displays.DirtyResourceUriInteractiveIconDisplay.CssStyleString),
                        "margin-right: 15px;"
                    }
                }));

        IdeMainLayoutService.RegisterFooterJustifyEndComponent(
            new FooterJustifyEndComponent(
                Key<FooterJustifyEndComponent>.NewKey(),
                typeof(Walk.Common.RazorLib.Notifications.Displays.NotificationsInteractiveIconDisplay),
                ComponentParameterMap: null));

        return ValueTask.CompletedTask;
    }
    
    private async Task SetSolution(DotNetAppData dotNetAppData)
    {
    	var solutionMostRecent = dotNetAppData?.SolutionMostRecent;
    
    	if (solutionMostRecent is null)
    		return;
    
    	var slnAbsolutePath = EnvironmentProvider.AbsolutePathFactory(
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
            var parentDirectoryAbsolutePath = EnvironmentProvider.AbsolutePathFactory(
                parentDirectory,
                true);

            var pseudoRootNode = new TreeViewAbsolutePath(
                parentDirectoryAbsolutePath,
                IdeComponentRenderers,
                CommonComponentRenderers,
                FileSystemProvider,
                EnvironmentProvider,
                true,
                false);

            await pseudoRootNode.LoadChildListAsync().ConfigureAwait(false);

            var adhocRootNode = TreeViewAdhoc.ConstructTreeViewAdhoc(pseudoRootNode.ChildList.ToArray());

            foreach (var child in adhocRootNode.ChildList)
            {
                child.IsExpandable = false;
            }

            var activeNode = adhocRootNode.ChildList.FirstOrDefault();

            if (!TreeViewService.TryGetTreeViewContainer(InputFileContent.TreeViewContainerKey, out var treeViewContainer))
            {
                TreeViewService.ReduceRegisterContainerAction(new TreeViewContainer(
                    InputFileContent.TreeViewContainerKey,
                    adhocRootNode,
                    activeNode is null
                        ? new List<TreeViewNoType>()
                        : new() { activeNode }));
            }
            else
            {
                TreeViewService.ReduceWithRootNodeAction(InputFileContent.TreeViewContainerKey, adhocRootNode);

                TreeViewService.ReduceSetActiveNodeAction(
                    InputFileContent.TreeViewContainerKey,
                    activeNode,
                    true,
                    false);
            }
            await pseudoRootNode.LoadChildListAsync().ConfigureAwait(false);

            InputFileService.SetOpenedTreeViewModel(
                pseudoRootNode,
                IdeComponentRenderers,
                CommonComponentRenderers,
                FileSystemProvider,
                EnvironmentProvider);
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
}