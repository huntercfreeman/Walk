namespace Walk.Extensions.DotNet.DotNetSolutions.Models.Internals;

public interface ISolutionVisualizationDrawingLine : ISolutionVisualizationDrawing
{
	public (int x, int y) StartPoint { get; set; }
	public (int x, int y) EndPoint { get; set; }
	public string Stroke { get; set; }
}
