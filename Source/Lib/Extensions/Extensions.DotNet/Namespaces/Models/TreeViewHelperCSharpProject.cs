using Walk.Common.RazorLib.Namespaces.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.CompilerServices.DotNetSolution.Models.Project;
using Walk.Extensions.DotNet.CSharpProjects.Models;
using Walk.Ide.RazorLib;
using Walk.Ide.RazorLib.FileSystems.Models;
using Walk.Ide.RazorLib.Namespaces.Models;

namespace Walk.Extensions.DotNet.Namespaces.Models;

public class TreeViewHelperCSharpProject
{
	public static async Task<List<TreeViewNoType>> LoadChildrenAsync(TreeViewNamespacePath cSharpProjectTreeView)
	{
		var parentDirectoryOfCSharpProject = cSharpProjectTreeView.Item.AbsolutePath.ParentDirectory;
		var ancestorDirectory = parentDirectoryOfCSharpProject;
		var hiddenFiles = IdeFacts.GetHiddenFilesByContainerFileExtension(ExtensionNoPeriodFacts.C_SHARP_PROJECT);

		var directoryPathStringsList = await cSharpProjectTreeView.CommonService.FileSystemProvider.Directory
			.GetDirectoriesAsync(ancestorDirectory)
			.ConfigureAwait(false);

		var childDirectoryTreeViewModelsList = directoryPathStringsList
			.OrderBy(pathString => pathString)
			.Where(x => hiddenFiles.All(hidden => !x.EndsWith(hidden)))
			.Select(x =>
			{
				var absolutePath = cSharpProjectTreeView.CommonService.EnvironmentProvider.AbsolutePathFactory(x, true);

				var namespaceString = cSharpProjectTreeView.Item.Namespace +
					TreeViewNamespaceHelper.NAMESPACE_DELIMITER +
					absolutePath.NameNoExtension;

				return new TreeViewNamespacePath(new NamespacePath(namespaceString, absolutePath),
					cSharpProjectTreeView.CommonService,
					true,
					false);
			});

		var uniqueDirectories = IdeFacts.GetUniqueFilesByContainerFileExtension(ExtensionNoPeriodFacts.C_SHARP_PROJECT);
		var foundUniqueDirectories = new List<TreeViewNamespacePath>();
		var foundDefaultDirectories = new List<TreeViewNamespacePath>();

		foreach (var directoryTreeViewModel in childDirectoryTreeViewModelsList)
		{
			if (uniqueDirectories.Any(unique => directoryTreeViewModel.Item.AbsolutePath.NameNoExtension == unique))
				foundUniqueDirectories.Add(directoryTreeViewModel);
			else
				foundDefaultDirectories.Add(directoryTreeViewModel);
		}

		foundUniqueDirectories = foundUniqueDirectories.OrderBy(x => x.Item.AbsolutePath.NameNoExtension).ToList();
		foundDefaultDirectories = foundDefaultDirectories.OrderBy(x => x.Item.AbsolutePath.NameNoExtension).ToList();

		var filePathStringsList = await cSharpProjectTreeView.CommonService.FileSystemProvider.Directory
			.GetFilesAsync(ancestorDirectory)
			.ConfigureAwait(false);

		var childFileTreeViewModels = filePathStringsList
			.OrderBy(pathString => pathString)
			.Where(x => !x.EndsWith(ExtensionNoPeriodFacts.C_SHARP_PROJECT))
			.Select(x =>
			{
				var absolutePath = cSharpProjectTreeView.CommonService.EnvironmentProvider.AbsolutePathFactory(x, false);
				var namespaceString = cSharpProjectTreeView.Item.Namespace;

				return (TreeViewNoType)new TreeViewNamespacePath(new NamespacePath(namespaceString, absolutePath),
					cSharpProjectTreeView.CommonService,
					false,
					false);
			});

		var cSharpProjectDependenciesTreeViewNode = new TreeViewCSharpProjectDependencies(
			new CSharpProjectDependencies(cSharpProjectTreeView.Item),
			cSharpProjectTreeView.CommonService,
			true,
			false);
	    
	    var result = new List<TreeViewNoType>();
	    result.Add(cSharpProjectDependenciesTreeViewNode);
	    result.AddRange(foundUniqueDirectories);
	    result.AddRange(foundDefaultDirectories);
	    result.AddRange(childFileTreeViewModels);
	    
	    return result;
	}
}