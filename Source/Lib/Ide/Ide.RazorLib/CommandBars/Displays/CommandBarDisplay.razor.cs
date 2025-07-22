using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Walk.Ide.RazorLib.CommandBars.Displays;

public partial class CommandBarDisplay : ComponentBase, IDisposable
{
    [Inject]
    private IdeService IdeService { get; set; } = null!;
    
    public const string INPUT_HTML_ELEMENT_ID = "di_ide_command-bar-input-id";
        
    protected override void OnInitialized()
    {
        IdeService.IdeStateChanged += OnCommandBarStateChanged;
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await IdeService.CommonService.JsRuntimeCommonApi
                .FocusHtmlElementById(CommandBarDisplay.INPUT_HTML_ELEMENT_ID)
                .ConfigureAwait(false);
        }
    }
    
    private void HandleOnKeyDown(KeyboardEventArgs keyboardEventArgs)
    {
        if (keyboardEventArgs.Key == "Enter")
            IdeService.CommonService.SetWidget(null);
    }
    
    private async void OnCommandBarStateChanged(IdeStateChangedKind ideStateChangedKind)
    {
        if (ideStateChangedKind == IdeStateChangedKind.CommandBarStateChanged)
        {
            await InvokeAsync(StateHasChanged);
        }
    }
    
    public void Dispose()
    {
        IdeService.IdeStateChanged -= OnCommandBarStateChanged;
    }
}
