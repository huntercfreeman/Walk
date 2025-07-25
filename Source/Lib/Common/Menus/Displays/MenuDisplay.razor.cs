using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Common.RazorLib.Menus.Displays;

public partial class MenuDisplay : ComponentBase, IDisposable
{
    [Inject]
    private CommonService CommonService { get; set; } = null!;
    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;

    [Parameter, EditorRequired]
    public MenuRecord Menu { get; set; } = null!;

    /// <summary>Pixels</summary>
    private int _lineHeight = 20;
    
    private Guid _guidId = Guid.NewGuid();
    private string _htmlId = null!;

    private int _activeIndex;
    
    private MenuMeasurements _menuMeasurements;
    private DotNetObjectReference<MenuDisplay>? _dotNetHelper;
    
    protected override void OnInitialized()
    {
        Console.WriteLine("OnInitialized()");
        _dotNetHelper = DotNetObjectReference.Create(this);
        _htmlId = $"luth_common_treeview-{_guidId}";
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        Console.WriteLine("OnAfterRenderAsync()");
        if (firstRender)
        {
            // Do not ConfigureAwait(false) so that the UI doesn't change out from under you
            // before you finish setting up the events?
            // (is this a thing, I'm just presuming this would be true).
            _menuMeasurements = await JsRuntime.InvokeAsync<MenuMeasurements>(
                "walkCommon.menuInitialize",
                _dotNetHelper,
                _htmlId);
            
            if (Menu.ShouldImmediatelyTakeFocus)
            {
                await JsRuntime.InvokeVoidAsync(
                    "walkCommon.focusHtmlElementById",
                    _htmlId,
                    /*preventScroll:*/ true);
            }
        }
    }
    
    [JSInvokable]
    public async Task ReceiveOnKeyDown(MenuEventArgsKeyDown eventArgsKeyDown)
    {
        _menuMeasurements = new MenuMeasurements(
            eventArgsKeyDown.ViewWidth,
            eventArgsKeyDown.ViewHeight,
            eventArgsKeyDown.BoundingClientRectLeft,
            eventArgsKeyDown.BoundingClientRectTop);
    
        switch (eventArgsKeyDown.Key)
        {
            case "ArrowDown":
                _activeIndex++;
                break;
            case "ArrowUp":
                _activeIndex--;
                break;
        }
    }
    
    [JSInvokable]
    public void ReceiveOnContextMenu(MenuEventArgsMouseDown eventArgsMouseDown)
    {
        _menuMeasurements = new MenuMeasurements(
            eventArgsMouseDown.ViewWidth,
            eventArgsMouseDown.ViewHeight,
            eventArgsMouseDown.BoundingClientRectLeft,
            eventArgsMouseDown.BoundingClientRectTop);
    }
    
    [JSInvokable]
    public void ReceiveContentOnMouseDown(MenuEventArgsMouseDown eventArgsMouseDown)
    {
        _menuMeasurements = new MenuMeasurements(
            eventArgsMouseDown.ViewWidth,
            eventArgsMouseDown.ViewHeight,
            eventArgsMouseDown.BoundingClientRectLeft,
            eventArgsMouseDown.BoundingClientRectTop);
    }
    
    [JSInvokable]
    public async Task ReceiveOnClick(MenuEventArgsMouseDown eventArgsMouseDown)
    {
        _menuMeasurements = new MenuMeasurements(
            eventArgsMouseDown.ViewWidth,
            eventArgsMouseDown.ViewHeight,
            eventArgsMouseDown.BoundingClientRectLeft,
            eventArgsMouseDown.BoundingClientRectTop);
    }
    
    [JSInvokable]
    public async Task ReceiveOnDoubleClick(MenuEventArgsMouseDown eventArgsMouseDown)
    {
        _menuMeasurements = new MenuMeasurements(
            eventArgsMouseDown.ViewWidth,
            eventArgsMouseDown.ViewHeight,
            eventArgsMouseDown.BoundingClientRectLeft,
            eventArgsMouseDown.BoundingClientRectTop);
    }
    
    public void Dispose()
    {
        _dotNetHelper?.Dispose();
    }
}
