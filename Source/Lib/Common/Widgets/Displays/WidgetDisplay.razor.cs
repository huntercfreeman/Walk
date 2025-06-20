using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Widgets.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;

namespace Walk.Common.RazorLib.Widgets.Displays;

public partial class WidgetDisplay : ComponentBase
{
    [Inject]
    private CommonBackgroundTaskApi CommonBackgroundTaskApi { get; set; } = null!;
    [Inject]
    private IWidgetService WidgetService { get; set; } = null!;
    
	[Parameter, EditorRequired]
	public WidgetModel Widget { get; set; } = null!;
	[Parameter, EditorRequired]
    public Func<Task> OnFocusInFunc { get; set; } = null!;
    [Parameter, EditorRequired]
    public Func<Task> OnFocusOutFunc { get; set; } = null!;
    
    private const string WIDGET_HTML_ELEMENT_ID = "di_widget-id";
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await CommonBackgroundTaskApi.JsRuntimeCommonApi
                .FocusHtmlElementById(WIDGET_HTML_ELEMENT_ID)
                .ConfigureAwait(false);
        }
    }

	private Task HandleOnFocusIn()
    {
        return OnFocusInFunc.Invoke();
    }
    
	private Task HandleOnFocusOut()
    {
    	return OnFocusOutFunc.Invoke();
    }
    
    private async Task HandleOnMouseDown()
    {
        await CommonBackgroundTaskApi.JsRuntimeCommonApi
            .FocusHtmlElementById(WIDGET_HTML_ELEMENT_ID)
            .ConfigureAwait(false);
    }
    
    private void HandleOnKeyDown(KeyboardEventArgs keyboardEventArgs)
	{
		if (keyboardEventArgs.Key == "Escape")
			WidgetService.SetWidget(null);
	}
}