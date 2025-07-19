using System.Text.Json;
using System.Text;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Keymaps.Models;
using Walk.Common.RazorLib.Themes.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.JavaScriptObjects.Models;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.ListExtensions;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.Widgets.Models;
using Walk.Common.RazorLib.JsRuntimes.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Tooltips.Models;
using Walk.Common.RazorLib.Installations.Displays;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Tabs.Models;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Exceptions;

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
