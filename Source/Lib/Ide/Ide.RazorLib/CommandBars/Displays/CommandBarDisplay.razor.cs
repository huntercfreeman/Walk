using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Options.Models;
using Walk.Ide.RazorLib.CommandBars.Models;

namespace Walk.Ide.RazorLib.CommandBars.Displays;

public partial class CommandBarDisplay : ComponentBase, IDisposable
{
	[Inject]
	private IdeService IdeService { get; set; } = null!;
	
	public const string INPUT_HTML_ELEMENT_ID = "di_ide_command-bar-input-id";
		
	protected override void OnInitialized()
	{
		IdeService.CommandBarStateChanged += OnCommandBarStateChanged;
	}
	
	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			await CommonUtilityService.JsRuntimeCommonApi
				.FocusHtmlElementById(CommandBarDisplay.INPUT_HTML_ELEMENT_ID)
	            .ConfigureAwait(false);
		}
	}
	
	private void HandleOnKeyDown(KeyboardEventArgs keyboardEventArgs)
	{
		if (keyboardEventArgs.Key == "Enter")
			CommonUtilityService.SetWidget(null);
	}
	
	private async void OnCommandBarStateChanged()
	{
		await InvokeAsync(StateHasChanged);
	}
	
	public void Dispose()
	{
		IdeService.CommandBarStateChanged -= OnCommandBarStateChanged;
	}
}