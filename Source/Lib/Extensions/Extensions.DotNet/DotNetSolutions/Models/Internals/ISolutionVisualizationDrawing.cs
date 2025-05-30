using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.TextEditor.RazorLib.Installations.Models;

namespace Walk.Extensions.DotNet.DotNetSolutions.Models.Internals;

public interface ISolutionVisualizationDrawing
{
	public object Item { get; }
	public SolutionVisualizationDrawingKind SolutionVisualizationDrawingKind { get; }
	public SolutionVisualizationItemKind SolutionVisualizationItemKind { get; set; }

	/// <summary>
	/// Macro-filter for the order that things render.
	/// 0 is the first "render cycle", and anything with that value
	/// will render first.
	/// </summary>
	public int RenderCycle { get; set; }
	/// <summary>
	/// Micro-filter for the order that things render.
	/// 0 is the first "render cycle sequence", and anything with that value
	/// would have been the first in a given render cycle to have rendered.
	/// </summary>
	public int RenderCycleSequence { get; set; }

	public MenuOptionRecord GetMenuOptionRecord(
		SolutionVisualizationModel localSolutionVisualizationModel,
		IEnvironmentProvider environmentProvider,
		WalkTextEditorConfig textEditorConfig,
		IServiceProvider serviceProvider);
}
