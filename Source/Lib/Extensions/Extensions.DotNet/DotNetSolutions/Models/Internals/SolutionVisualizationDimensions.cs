using Walk.Common.RazorLib.JavaScriptObjects.Models;

namespace Walk.Extensions.DotNet.DotNetSolutions.Models.Internals;

public class SolutionVisualizationDimensions
{
	private readonly Action _onStateChangedAction;

	public SolutionVisualizationDimensions(Action onStateChangedAction)
	{
		_onStateChangedAction = onStateChangedAction;
	}

	private int _svgWidth = 400;
	private int _svgHeight = 400;
	private int _viewBoxMinX = 0;
	private int _viewBoxMinY = 0;
	private int _viewBoxWidth = 100;
	private int _viewBoxHeight = 100;
	private int _horizontalPadding = 5;
	private int _verticalPadding = 5;

	public int SvgWidth
	{
		get => (int)(DivBoundingClientRect?.WidthInPixels ?? 0);
		set
		{
			throw new NotImplementedException();
			_svgWidth = value;
			_onStateChangedAction.Invoke();
		}
	}

	public int SvgHeight
	{
		get => _svgHeight;
		set
		{
			_svgHeight = value;
			_onStateChangedAction.Invoke();
		}
	}

	public int ViewBoxMinX
	{
		get => _viewBoxMinX;
		set
		{
			_viewBoxMinX = value;
			_onStateChangedAction.Invoke();
		}
	}

	public int ViewBoxMinY
	{
		get => _viewBoxMinY;
		set
		{
			_viewBoxMinY = value;
			_onStateChangedAction.Invoke();
		}
	}

	public int ViewBoxWidth
	{
		get => _viewBoxWidth;
		set
		{
			_viewBoxWidth = value;
			_onStateChangedAction.Invoke();
		}
	}

	public int ViewBoxHeight
	{
		get => _viewBoxHeight;
		set
		{
			_viewBoxHeight = value;
			_onStateChangedAction.Invoke();
		}
	}

	public int HorizontalPadding
	{
		get => _horizontalPadding;
		set
		{
			_horizontalPadding = value;
			_onStateChangedAction.Invoke();
		}
	}

	public int VerticalPadding
	{
		get => _verticalPadding;
		set
		{
			_verticalPadding = value;
			_onStateChangedAction.Invoke();
		}
	}

	public MeasuredHtmlElementDimensions DivBoundingClientRect { get; set; }
	public MeasuredHtmlElementDimensions SvgBoundingClientRect { get; set; }

	public double ScaleX => SvgWidth / (double)ViewBoxWidth;
	public double ScaleY => SvgHeight / (double)ViewBoxHeight;
}
