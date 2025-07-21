using System.Text;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.TreeViews.Models.Utils;
using Walk.Extensions.DotNet.CommandLines.Models;
using Walk.Extensions.DotNet.Outputs.Models;

namespace Walk.Extensions.DotNet;

public partial class DotNetService
{
	private readonly Throttle _throttleCreateTreeView = new Throttle(TimeSpan.FromMilliseconds(333));

	private OutputState _outputState = new();

	public event Action? OutputStateChanged;

	public OutputState GetOutputState() => _outputState;

	public void ReduceStateHasChangedAction(Guid dotNetRunParseResultId)
	{
		var inState = GetOutputState();

		_outputState = inState with
		{
			DotNetRunParseResultId = dotNetRunParseResultId
		};

		OutputStateChanged?.Invoke();
		return;
	}

	public Task HandleConstructTreeViewEffect()
	{
		_throttleCreateTreeView.Run(async _ => await OutputService_Do_ConstructTreeView());
		return Task.CompletedTask;
	}

	public ValueTask OutputService_Do_ConstructTreeView()
	{
		var dotNetRunParseResult = GetDotNetRunParseResult();

		var treeViewNodeList = dotNetRunParseResult.AllDiagnosticLineList.Select(x =>
			(TreeViewNoType)new TreeViewDiagnosticLine(
				x,
				false,
				false))
			.ToArray();

		var filePathGrouping = treeViewNodeList.GroupBy(
			x => ((TreeViewDiagnosticLine)x).Item.FilePathTextSpan.Text);

		var projectManualGrouping = new Dictionary<string, TreeViewGroup>();
		var treeViewBadStateGroupList = new List<TreeViewNoType>();

		foreach (var group in filePathGrouping)
		{
			var absolutePath = CommonService.EnvironmentProvider.AbsolutePathFactory(group.Key, false);
			var groupEnumerated = group.ToList();
			var groupNameBuilder = new StringBuilder();

			var errorCount = groupEnumerated.Count(x =>
				((TreeViewDiagnosticLine)x).Item.DiagnosticLineKind == DiagnosticLineKind.Error);

			var warningCount = groupEnumerated.Count(x =>
				((TreeViewDiagnosticLine)x).Item.DiagnosticLineKind == DiagnosticLineKind.Warning);

			groupNameBuilder
				.Append(absolutePath.NameWithExtension)
				.Append(" (")
				.Append(errorCount)
				.Append(" errors)")
				.Append(" (")
				.Append(warningCount)
				.Append(" warnings)");

			var treeViewGroup = new TreeViewGroup(
				groupNameBuilder.ToString(),
				true,
				groupEnumerated.Any(x => ((TreeViewDiagnosticLine)x).Item.DiagnosticLineKind == DiagnosticLineKind.Error))
			{
				TitleText = absolutePath.ParentDirectory ?? $"{nameof(AbsolutePath.ParentDirectory)} was null"
			};

			treeViewGroup.ChildList = groupEnumerated;
			treeViewGroup.LinkChildren(new(), treeViewGroup.ChildList);

			var firstEntry = groupEnumerated.FirstOrDefault();

			if (firstEntry is not null)
			{
				var projectText = ((TreeViewDiagnosticLine)firstEntry).Item.ProjectTextSpan.Text;
				var projectAbsolutePath = CommonService.EnvironmentProvider.AbsolutePathFactory(projectText, false);

				if (!projectManualGrouping.ContainsKey(projectText))
				{
					var treeViewGroupProject = new TreeViewGroup(
						projectAbsolutePath.NameWithExtension,
						true,
						true)
					{
						TitleText = absolutePath.ParentDirectory ?? $"{nameof(AbsolutePath.ParentDirectory)} was null"
					};

					projectManualGrouping.Add(projectText, treeViewGroupProject);
				}

				projectManualGrouping[projectText].ChildList.Add(treeViewGroup);
			}
			else
			{
				treeViewBadStateGroupList.Add(treeViewGroup);
			}
		}

		var treeViewProjectGroupList = projectManualGrouping.Values
			.Select(x => (TreeViewNoType)x)
			.ToList();

		// Bad State
		if (treeViewBadStateGroupList.Count != 0)
		{
			var projectText = "Could not find project";

			var treeViewGroupProjectBadState = new TreeViewGroup(
				projectText,
				true,
				true)
			{
				TitleText = projectText
			};

			treeViewGroupProjectBadState.ChildList = treeViewBadStateGroupList;

			treeViewProjectGroupList.Add(treeViewGroupProjectBadState);
		}

		foreach (var treeViewProjectGroup in treeViewProjectGroupList)
		{
			treeViewProjectGroup.LinkChildren(new(), treeViewProjectGroup.ChildList);
		}

		var adhocRoot = TreeViewAdhoc.ConstructTreeViewAdhoc(treeViewProjectGroupList.ToArray());
		var firstNode = treeViewNodeList.FirstOrDefault();

		var activeNodes = firstNode is null
			? new List<TreeViewNoType>()
			: new() { firstNode };

		if (!CommonService.TryGetTreeViewContainer(OutputState.TreeViewContainerKey, out _))
		{
			CommonService.TreeView_RegisterContainerAction(new TreeViewContainer(
				OutputState.TreeViewContainerKey,
				adhocRoot,
				activeNodes));
		}
		else
		{
			CommonService.TreeView_WithRootNodeAction(OutputState.TreeViewContainerKey, adhocRoot);

			CommonService.TreeView_SetActiveNodeAction(
				OutputState.TreeViewContainerKey,
				firstNode,
				true,
				false);
		}

		ReduceStateHasChangedAction(dotNetRunParseResult.Id);
		return ValueTask.CompletedTask;
	}
}
