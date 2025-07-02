using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Widgets.Models;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Common.RazorLib.Widgets.Displays;

public partial class WidgetDisplay : ComponentBase
{
    [Inject]
    private CommonUtilityService CommonUtilityService { get; set; } = null!;
    
	[Parameter, EditorRequired]
	public WidgetModel Widget { get; set; } = null!;
    
    private const string WIDGET_HTML_ELEMENT_ID = "di_widget-id";
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await CommonUtilityService.JsRuntimeCommonApi
                .FocusHtmlElementById(WIDGET_HTML_ELEMENT_ID)
                .ConfigureAwait(false);
        }
    }

	private async Task HandleOnMouseDown()
    {
        await CommonUtilityService.JsRuntimeCommonApi
            .FocusHtmlElementById(WIDGET_HTML_ELEMENT_ID)
            .ConfigureAwait(false);
    }
    
    private void HandleOnKeyDown(KeyboardEventArgs keyboardEventArgs)
	{
		if (keyboardEventArgs.Key == "Escape")
			CommonUtilityService.SetWidget(null);
	}
}