using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;

namespace Walk.Common.RazorLib.Contexts.Displays;

public partial class ContextBoundaryMeasurer : ComponentBase, IDisposable
{
    [Inject]
    private CommonBackgroundTaskApi CommonBackgroundTaskApi { get; set; } = null!;
    [Inject]
    private IContextService ContextService { get; set; } = null!;

    [Parameter, EditorRequired]
    public ContextRecord ContextRecord { get; set; } = default!;
    [Parameter, EditorRequired]
    public Func<List<Key<ContextRecord>>> GetContextBoundaryHeirarchy { get; set; } = null!;

    private bool _previousIsSelectingInspectionTarget;

	protected override void OnInitialized()
	{
		ContextService.ContextStateChanged += OnContextStateChanged;
		base.OnInitialized();
	}

    protected override bool ShouldRender()
    {
        var contextState = ContextService.GetContextState();

        if (_previousIsSelectingInspectionTarget != contextState.IsSelectingInspectionTarget)
            return true;

        return false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        var contextState = ContextService.GetContextState();

        if (_previousIsSelectingInspectionTarget != contextState.IsSelectingInspectionTarget)
        {
            _previousIsSelectingInspectionTarget = contextState.IsSelectingInspectionTarget;

            if (contextState.IsSelectingInspectionTarget)
            {
                var measuredHtmlElementDimensions = await CommonBackgroundTaskApi.JsRuntimeCommonApi
                    .MeasureElementById(ContextRecord.ContextElementId)
                    .ConfigureAwait(false);
                
                var contextBoundaryHeirarchy = GetContextBoundaryHeirarchy.Invoke();

                measuredHtmlElementDimensions = measuredHtmlElementDimensions with
                {
                    ZIndex = contextBoundaryHeirarchy.Count
                };

                ContextService.AddInspectableContext(
                    new InspectableContext(
                        new(contextBoundaryHeirarchy),
                        measuredHtmlElementDimensions));
            }
        }
    }
    
    public async void OnContextStateChanged()
    {
    	await InvokeAsync(StateHasChanged);
    }
    
    public void Dispose()
    {
    	ContextService.ContextStateChanged -= OnContextStateChanged;
    }
}