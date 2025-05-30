using Microsoft.Extensions.DependencyInjection;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Installations.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.CompilerServices.DotNetSolution.CompilerServiceCase;
using Walk.CompilerServices.CSharpProject.CompilerServiceCase;
using Walk.TextEditor.RazorLib.CompilerServices;

namespace Walk.Extensions.DotNet.DotNetSolutions.Models.Internals;

public class SolutionVisualizationDrawingCircle<TItem> : ISolutionVisualizationDrawingCircle
{
	public TItem Item { get; set; }
	public SolutionVisualizationDrawingKind SolutionVisualizationDrawingKind => SolutionVisualizationDrawingKind.Circle;
	public SolutionVisualizationItemKind SolutionVisualizationItemKind { get; set; }
	public int CenterX { get; set; }
	public int CenterY { get; set; }
	public int Radius { get; set; }
	public string Fill { get; set; }
	public int RenderCycle { get; set; }
	public int RenderCycleSequence { get; set; }

	object ISolutionVisualizationDrawing.Item => Item;

	public MenuOptionRecord GetMenuOptionRecord(
		SolutionVisualizationModel localSolutionVisualizationModel,
		IEnvironmentProvider environmentProvider,
		WalkTextEditorConfig textEditorConfig,
		IServiceProvider serviceProvider)
	{
		var menuOptionRecordList = new List<MenuOptionRecord>();

		var targetDisplayName = "unknown";

		if (Item is ICompilerServiceResource compilerServiceResource)
		{
			var absolutePath = environmentProvider.AbsolutePathFactory(compilerServiceResource.ResourceUri.Value, false);
			targetDisplayName = absolutePath.NameWithExtension;

			menuOptionRecordList.Add(new MenuOptionRecord(
				"Open in editor",
				MenuOptionKind.Other,
				onClickFunc: () => OpenFileInEditor(
					compilerServiceResource.ResourceUri.Value,
					textEditorConfig,
					serviceProvider)));

			if (SolutionVisualizationItemKind == SolutionVisualizationItemKind.Solution)
			{
				menuOptionRecordList.Add(new MenuOptionRecord(
					"Load Projects",
					MenuOptionKind.Other,
					onClickFunc: () => LoadProjects(
						(DotNetSolutionResource)compilerServiceResource,
						textEditorConfig,
						serviceProvider)));
			}
			else if (SolutionVisualizationItemKind == SolutionVisualizationItemKind.Project)
			{
				menuOptionRecordList.Add(new MenuOptionRecord(
					"Load Classes",
					MenuOptionKind.Other,
					onClickFunc: () => LoadClasses(
						(CSharpProjectResource)compilerServiceResource,
						localSolutionVisualizationModel,
						textEditorConfig,
						serviceProvider)));
			}
		}

		return new MenuOptionRecord(
			targetDisplayName,
			MenuOptionKind.Other,
			subMenu: new MenuRecord(menuOptionRecordList));
	}

	private Task OpenFileInEditor(
		string filePath,
		WalkTextEditorConfig textEditorConfig,
		IServiceProvider serviceProvider)
	{
		var textEditorService = serviceProvider.GetRequiredService<TextEditorService>();
		textEditorService.WorkerArbitrary.PostUnique(async editContext =>
		{
			await textEditorService.OpenInEditorAsync(
				editContext,
				filePath,
				true,
				null,
				new Category("main"),
				Key<TextEditorViewModel>.NewKey());
		});
		return Task.CompletedTask;
	}

	private async Task LoadProjects(
		DotNetSolutionResource dotNetSolutionResource,
		WalkTextEditorConfig textEditorConfig,
		IServiceProvider serviceProvider)
	{
		/*if (textEditorConfig.RegisterModelFunc is null)
			return;

		var dotNetBackgroundTaskApi = serviceProvider.GetRequiredService<DotNetBackgroundTaskApi>();

		var dotNetSolutionModel = dotNetBackgroundTaskApi.DotNetSolutionService.GetDotNetSolutionState().DotNetSolutionsList.FirstOrDefault(x =>
			x.AbsolutePath.Value == dotNetSolutionResource.ResourceUri.Value);

		foreach (var project in dotNetSolutionModel.DotNetProjectList)
		{
			var resourceUri = new ResourceUri(project.AbsolutePath.Value);

			await textEditorConfig.RegisterModelFunc.Invoke(new RegisterModelArgs(
					resourceUri,
					serviceProvider))
				.ConfigureAwait(false);
		}*/
	}

	private async Task LoadClasses(
		CSharpProjectResource cSharpProjectResource,
		SolutionVisualizationModel localSolutionVisualizationModel,
		WalkTextEditorConfig textEditorConfig,
		IServiceProvider serviceProvider)
	{
		/*if (textEditorConfig.RegisterModelFunc is null)
			return;

		var environmentProvider = serviceProvider.GetRequiredService<IEnvironmentProvider>();
		var fileSystemProvider = serviceProvider.GetRequiredService<IFileSystemProvider>();
		var commonComponentRenderers = serviceProvider.GetRequiredService<ICommonComponentRenderers>();
		var notificationService = serviceProvider.GetRequiredService<INotificationService>();

		var projectAbsolutePath = environmentProvider.AbsolutePathFactory(cSharpProjectResource.ResourceUri.Value, false);

		var parentDirectory = projectAbsolutePath.ParentDirectory;
		if (parentDirectory is null)
			return;

		var startingAbsolutePathForSearch = parentDirectory;

		var discoveredFileList = new List<string>();

		await DiscoverFilesRecursively(startingAbsolutePathForSearch, discoveredFileList).ConfigureAwait(false);

		NotificationHelper.DispatchInformative(
			"SolutionVisualizationDrawing",
			$"{projectAbsolutePath.NameWithExtension} discovery {discoveredFileList.Count} C# files",
			commonComponentRenderers,
			notificationService,
			TimeSpan.FromSeconds(6));

		foreach (var file in discoveredFileList)
		{
			if (!localSolutionVisualizationModel.ParentMap.TryAdd(file, this))
				localSolutionVisualizationModel.ParentMap[file] = this;
		}

		foreach (var file in discoveredFileList)
		{
			var resourceUri = new ResourceUri(file);

			await textEditorConfig.RegisterModelFunc.Invoke(new RegisterModelArgs(
					resourceUri,
					serviceProvider))
				.ConfigureAwait(false);
		}

		async Task DiscoverFilesRecursively(string directoryPathParent, List<string> discoveredFileList)
		{
			var directoryPathChildList = await fileSystemProvider.Directory.GetDirectoriesAsync(
					directoryPathParent,
					CancellationToken.None)
				.ConfigureAwait(false);

			var filePathChildList = await fileSystemProvider.Directory.GetFilesAsync(
					directoryPathParent,
					CancellationToken.None)
				.ConfigureAwait(false);

			foreach (var filePathChild in filePathChildList)
			{
				if (filePathChild.EndsWith(".cs"))
					discoveredFileList.Add(filePathChild);
			}

			foreach (var directoryPathChild in directoryPathChildList)
			{
				if (IFileSystemProvider.IsDirectoryIgnored(directoryPathChild))
					continue;

				await DiscoverFilesRecursively(directoryPathChild, discoveredFileList).ConfigureAwait(false);
			}
		}*/
	}
}
