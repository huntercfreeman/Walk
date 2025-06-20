using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.CompilerServices.CSharp.CompilerServiceCase;
using Walk.CompilerServices.DotNetSolution.CompilerServiceCase;
using Walk.CompilerServices.CSharpProject.CompilerServiceCase;
using Walk.Extensions.DotNet.BackgroundTasks.Models;
using Walk.Extensions.DotNet.DotNetSolutions.Models.Internals;
using Walk.Extensions.DotNet.DotNetSolutions.Displays.Internals;
using Walk.TextEditor.RazorLib.CompilerServices;

namespace Walk.Extensions.DotNet.DotNetSolutions.Displays;

public partial class SolutionVisualizationDisplay : ComponentBase, IDisposable
{
	[Inject]
	private ICompilerServiceRegistry CompilerServiceRegistry { get; set; } = null!;
	[Inject]
	private IAppDimensionService AppDimensionService { get; set; } = null!;
	[Inject]
	private IDropdownService DropdownService { get; set; } = null!;
	[Inject]
	private CommonBackgroundTaskApi CommonBackgroundTaskApi { get; set; } = null!;
	[Inject]
	private DotNetBackgroundTaskApi DotNetBackgroundTaskApi { get; set; } = null!;

	[CascadingParameter]
	public IDialog Dialog { get; set; }

	private readonly Throttle _stateChangedThrottle = new(ThrottleFacts.TwentyFour_Frames_Per_Second);

	private SolutionVisualizationModel _solutionVisualizationModel;
	private DotNetSolutionCompilerService _dotNetSolutionCompilerService;
	private CSharpProjectCompilerService _cSharpProjectCompilerService;
	private CSharpCompilerService _cSharpCompilerService;
	private string _divHtmlElementId;
	private string _svgHtmlElementId;

	public Guid IdSalt { get; } = Guid.NewGuid();

	public string DivHtmlElementId => _divHtmlElementId ??= $"di_ide_solution-visualization-div_{IdSalt}";
	public string SvgHtmlElementId => _svgHtmlElementId ??= $"di_ide_solution-visualization-svg_{IdSalt}";

	protected override void OnInitialized()
	{
		_solutionVisualizationModel = new(null, OnCompilerServiceChanged);

		AppDimensionService.AppDimensionStateChanged += OnAppDimensionStateWrapChanged;
		DotNetBackgroundTaskApi.DotNetSolutionService.DotNetSolutionStateChanged += OnDotNetSolutionStateChanged;

		SubscribeTo_DotNetSolutionCompilerService();
		SubscribeTo_CSharpProjectCompilerService();
		SubscribeTo_CSharpCompilerService();

		base.OnInitialized();
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			OnAppDimensionStateWrapChanged();
			OnDotNetSolutionStateChanged();
			OnCompilerServiceChanged();
		}
	}

	private async void OnAppDimensionStateWrapChanged()
	{
		_solutionVisualizationModel.Dimensions.DivBoundingClientRect = await CommonBackgroundTaskApi.JsRuntimeCommonApi
			.MeasureElementById(DivHtmlElementId)
			.ConfigureAwait(false);

		_solutionVisualizationModel.Dimensions.SvgBoundingClientRect = await CommonBackgroundTaskApi.JsRuntimeCommonApi
			.MeasureElementById(SvgHtmlElementId)
			.ConfigureAwait(false);

		OnCompilerServiceChanged();
	}

	private void OnDotNetSolutionStateChanged()
	{
		_solutionVisualizationModel = new(DotNetBackgroundTaskApi.DotNetSolutionService.GetDotNetSolutionState().DotNetSolutionModel?.AbsolutePath, OnCompilerServiceChanged);
		OnAppDimensionStateWrapChanged();
	}

	private void OnCompilerServiceChanged()
	{
		_stateChangedThrottle.Run(_ =>
		{
			_solutionVisualizationModel = _solutionVisualizationModel.MakeDrawing(
				_dotNetSolutionCompilerService,
				_cSharpProjectCompilerService,
				_cSharpCompilerService);

			return InvokeAsync(StateHasChanged);
		});
	}

	private void HandleOnContextMenu(MouseEventArgs mouseEventArgs)
	{
		var dropdownRecord = new DropdownRecord(
			SolutionVisualizationContextMenu.ContextMenuEventDropdownKey,
			mouseEventArgs.ClientX,
			mouseEventArgs.ClientY,
			typeof(SolutionVisualizationContextMenu),
			new Dictionary<string, object?>
			{
				{
					nameof(SolutionVisualizationContextMenu.MouseEventArgs),
					mouseEventArgs
				},
				{
					nameof(SolutionVisualizationContextMenu.SolutionVisualizationModel),
					_solutionVisualizationModel
				}
			},
			restoreFocusOnClose: null);

		DropdownService.ReduceRegisterAction(dropdownRecord);
	}

	private void SubscribeTo_DotNetSolutionCompilerService()
	{
        _dotNetSolutionCompilerService = (DotNetSolutionCompilerService)CompilerServiceRegistry
			.GetCompilerService(ExtensionNoPeriodFacts.DOT_NET_SOLUTION);

        _dotNetSolutionCompilerService.ResourceRegistered += OnCompilerServiceChanged;
		_dotNetSolutionCompilerService.ResourceParsed += OnCompilerServiceChanged;
		_dotNetSolutionCompilerService.ResourceDisposed += OnCompilerServiceChanged;
	}

	private void DisposeFrom_DotNetSolutionCompilerService()
	{
		_dotNetSolutionCompilerService.ResourceRegistered -= OnCompilerServiceChanged;
		_dotNetSolutionCompilerService.ResourceParsed -= OnCompilerServiceChanged;
		_dotNetSolutionCompilerService.ResourceDisposed -= OnCompilerServiceChanged;
	}

	private void SubscribeTo_CSharpProjectCompilerService()
	{
        _cSharpProjectCompilerService = (CSharpProjectCompilerService)CompilerServiceRegistry
			.GetCompilerService(ExtensionNoPeriodFacts.C_SHARP_PROJECT);

		_cSharpProjectCompilerService.ResourceRegistered += OnCompilerServiceChanged;
		_cSharpProjectCompilerService.ResourceParsed += OnCompilerServiceChanged;
		_cSharpProjectCompilerService.ResourceDisposed += OnCompilerServiceChanged;
	}

	private void DisposeFrom_CSharpProjectCompilerService()
	{
		_cSharpProjectCompilerService.ResourceRegistered -= OnCompilerServiceChanged;
		_cSharpProjectCompilerService.ResourceParsed -= OnCompilerServiceChanged;
		_cSharpProjectCompilerService.ResourceDisposed -= OnCompilerServiceChanged;
	}

	private void SubscribeTo_CSharpCompilerService()
	{
        _cSharpCompilerService = (CSharpCompilerService)CompilerServiceRegistry
			.GetCompilerService(ExtensionNoPeriodFacts.C_SHARP_CLASS);

        _cSharpCompilerService.ResourceRegistered += OnCompilerServiceChanged;
		_cSharpCompilerService.ResourceParsed += OnCompilerServiceChanged;
		_cSharpCompilerService.ResourceDisposed += OnCompilerServiceChanged;
	}

	private void DisposeFrom_CSharpCompilerService()
	{
		_cSharpCompilerService.ResourceRegistered -= OnCompilerServiceChanged;
		_cSharpCompilerService.ResourceParsed -= OnCompilerServiceChanged;
		_cSharpCompilerService.ResourceDisposed -= OnCompilerServiceChanged;
	}

	public void Dispose()
	{
		DisposeFrom_DotNetSolutionCompilerService();
		DisposeFrom_CSharpProjectCompilerService();
		DisposeFrom_CSharpCompilerService();

		AppDimensionService.AppDimensionStateChanged -= OnAppDimensionStateWrapChanged;
		DotNetBackgroundTaskApi.DotNetSolutionService.DotNetSolutionStateChanged -= OnDotNetSolutionStateChanged;
	}
}