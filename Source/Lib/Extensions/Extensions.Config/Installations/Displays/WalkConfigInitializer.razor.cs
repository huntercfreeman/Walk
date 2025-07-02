using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Badges.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib.Edits.Models;
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
    private IIdeComponentRenderers IdeComponentRenderers { get; set; } = null!;
    [Inject]
    private DotNetBackgroundTaskApi DotNetBackgroundTaskApi { get; set; } = null!;
	[Inject]
	private IAppDataService AppDataService { get; set; } = null!;
	[Inject]
	private IInputFileService InputFileService { get; set; } = null!;
	[Inject]
	private IIdeService IdeService { get; set; } = null!;
	[Inject]
	private CommonUtilityService CommonUtilityService { get; set; } = null!;
	[Inject]
	private IDirtyResourceUriService DirtyResourceUriService { get; set; } = null!;
	
    private static Key<IDynamicViewModel> _notificationRecordKey = Key<IDynamicViewModel>.NewKey();

	protected override void OnInitialized()
	{
        CommonUtilityService.Continuous_EnqueueGroup(new BackgroundTask(
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
        IdeService.RegisterFooterBadge(
            new DirtyResourceUriBadge(
                DirtyResourceUriService,
                CommonUtilityService));

        IdeService.RegisterFooterBadge(
            new NotificationBadge(
                CommonUtilityService));

        return ValueTask.CompletedTask;
    }
    
    private async Task SetSolution(DotNetAppData dotNetAppData)
    {
    	var solutionMostRecent = dotNetAppData?.SolutionMostRecent;
    
    	if (solutionMostRecent is null)
    		return;
    
    	var slnAbsolutePath = CommonUtilityService.EnvironmentProvider.AbsolutePathFactory(
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
            var parentDirectoryAbsolutePath = CommonUtilityService.EnvironmentProvider.AbsolutePathFactory(
                parentDirectory,
                true);

            var pseudoRootNode = new TreeViewAbsolutePath(
                parentDirectoryAbsolutePath,
                IdeComponentRenderers,
                CommonUtilityService,
                true,
                false);

            await pseudoRootNode.LoadChildListAsync().ConfigureAwait(false);

            var adhocRootNode = TreeViewAdhoc.ConstructTreeViewAdhoc(pseudoRootNode.ChildList.ToArray());

            foreach (var child in adhocRootNode.ChildList)
            {
                child.IsExpandable = false;
            }

            var activeNode = adhocRootNode.ChildList.FirstOrDefault();

            if (!CommonUtilityService.TryGetTreeViewContainer(InputFileContent.TreeViewContainerKey, out var treeViewContainer))
            {
                CommonUtilityService.TreeView_RegisterContainerAction(new TreeViewContainer(
                    InputFileContent.TreeViewContainerKey,
                    adhocRootNode,
                    activeNode is null
                        ? new List<TreeViewNoType>()
                        : new() { activeNode }));
            }
            else
            {
                CommonUtilityService.TreeView_WithRootNodeAction(InputFileContent.TreeViewContainerKey, adhocRootNode);

                CommonUtilityService.TreeView_SetActiveNodeAction(
                    InputFileContent.TreeViewContainerKey,
                    activeNode,
                    true,
                    false);
            }
            await pseudoRootNode.LoadChildListAsync().ConfigureAwait(false);

            InputFileService.SetOpenedTreeViewModel(
                pseudoRootNode,
                IdeComponentRenderers,
                CommonUtilityService);
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