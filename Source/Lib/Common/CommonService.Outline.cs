using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.JavaScriptObjects.Models;

namespace Walk.Common.RazorLib;

public partial class CommonService
{
    private OutlineState _outlineState = new();
    
    public OutlineState GetOutlineState() => _outlineState;

    public void SetOutline(
        string? elementId,
        MeasuredHtmlElementDimensions? measuredHtmlElementDimensions,
        bool needsMeasured)
    {
        lock (_stateModificationLock)
        {
            _outlineState = _outlineState with
            {
                ElementId = elementId,
                MeasuredHtmlElementDimensions = measuredHtmlElementDimensions,
                NeedsMeasured = needsMeasured,
            };
        }

        if (needsMeasured && elementId is not null)
        {
            _ = Task.Run(async () =>
            {
                var elementDimensions = await JsRuntimeCommonApi
                    .MeasureElementById(elementId)
                    .ConfigureAwait(false);

                Outline_SetMeasurements(
                    elementId,
                    elementDimensions);
            });

            return; // The state has changed will occur in 'ReduceSetMeasurementsAction'
        }
        else
        {
            goto finalize;
        }

        finalize:
        CommonUiStateChanged?.Invoke(CommonUiEventKind.OutlineStateChanged);
    }
    
    public void Outline_SetMeasurements(
        string? elementId,
        MeasuredHtmlElementDimensions? measuredHtmlElementDimensions)
    {
        lock (_stateModificationLock)
        {
            if (_outlineState.ElementId == elementId)
            {
                _outlineState = _outlineState with
                {
                    MeasuredHtmlElementDimensions = measuredHtmlElementDimensions,
                    NeedsMeasured = false,
                };
            }
        }

        CommonUiStateChanged?.Invoke(CommonUiEventKind.OutlineStateChanged);
    }
}
