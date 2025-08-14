using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Widgets.Models;

namespace Walk.Common.RazorLib.Menus.Displays;

public partial class MenuDisplay : ComponentBase, IDisposable
{
    [Inject]
    private CommonService CommonService { get; set; } = null!;

    [Parameter, EditorRequired]
    public MenuRecord Menu { get; set; } = null!;

    /// <summary>Pixels</summary>
    private int LineHeight => CommonService.Options_LineHeight;
    
    private Guid _guidId = Guid.NewGuid();
    private string _htmlId = null!;

    /// <summary>
    /// Start at -1 so when menu opens user can choose to start at index '0' or 'count - 1' with ArrowDown or ArrowUp.
    /// If MenuRecord.InitialActiveMenuOptionRecordIndex is not -1, then use the provided index as this initial value.
    /// </summary>
    private int _activeIndex = -1;
    
    private readonly HashSet<int> _horizontalRuleElementIndexHashSet = new();
    
    private MenuMeasurements _menuMeasurements;
    private DotNetObjectReference<MenuDisplay>? _dotNetHelper;
    
    /// <summary>In pixels (px)</summary>
    private int _horizontalRuleTotalVerticalMargin = 10;
    /// <summary>In pixels (px)</summary>
    private double _horizontalRuleHeight = 1.5;
    private double HorizontalRuleVerticalOffset => _horizontalRuleTotalVerticalMargin + _horizontalRuleHeight;
    
    private int _seenLineHeight = -1;
    private int _seenWidgetHeight = -1;
    
    private int _indexMenuOptionShouldDisplayWidget = -1;
    private int WidgetHeight => 4 * LineHeight;
    private string _widgetHeightCssStyle;
    
    public string HtmlId => _htmlId;
    
    /// <summary>This property is from the old code, I have to decide on how to rewrite this part</summary>
    private MenuOptionCallbacks MenuOptionCallbacks => new(
        () => HideWidgetAsync(null),
        HideWidgetAsync);
    
    /// <summary>This method is from the old code, I have to decide on how to rewrite this part</summary>
    private async Task HideWidgetAsync(Action? onAfterWidgetHidden)
    {
        _indexMenuOptionShouldDisplayWidget = -1;
        await InvokeAsync(StateHasChanged);

        if (onAfterWidgetHidden is null) // Only hide the widget
        {
            await CommonService.JsRuntimeCommonApi.JsRuntime.InvokeVoidAsync(
                "walkCommon.focusHtmlElementById",
                _htmlId,
                /*preventScroll:*/ true);
        }
        else // Hide the widget AND dispose the menu
        {
            onAfterWidgetHidden.Invoke();
            CommonService.Dropdown_ReduceClearAction();
        }
    }
    
    protected override void OnInitialized()
    {
        if (Menu.InitialActiveMenuOptionRecordIndex != -1)
            _activeIndex = Menu.InitialActiveMenuOptionRecordIndex;
    
        _dotNetHelper = DotNetObjectReference.Create(this);
        _htmlId = $"luth_common_treeview-{_guidId}";
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Do not ConfigureAwait(false) so that the UI doesn't change out from under you
            // before you finish setting up the events?
            // (is this a thing, I'm just presuming this would be true).
            _menuMeasurements = await CommonService.JsRuntimeCommonApi.JsRuntime.InvokeAsync<MenuMeasurements>(
                "walkCommon.menuInitialize",
                _dotNetHelper,
                _htmlId);
            
            if (Menu.ShouldImmediatelyTakeFocus)
            {
                await CommonService.JsRuntimeCommonApi.JsRuntime.InvokeVoidAsync(
                    "walkCommon.focusHtmlElementById",
                    _htmlId,
                    /*preventScroll:*/ true);
            }
        }
    }
    
    public async Task SetFocusAndSetFirstOptionActiveAsync()
    {
        _activeIndex = 0;
        await CommonService.JsRuntimeCommonApi.JsRuntime.InvokeVoidAsync(
            "walkCommon.focusHtmlElementById",
            _htmlId,
            /*preventScroll:*/ true);
        await InvokeAsync(StateHasChanged);
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
                if (_activeIndex >= Menu.MenuOptionList.Count - 1)
                {
                    _activeIndex = 0;
                }
                else
                {
                    _activeIndex++;
                }
                break;
            case "ArrowUp":
                if (_activeIndex <= 0)
                {
                    _activeIndex = Menu.MenuOptionList.Count - 1;
                }
                else
                {
                    _activeIndex--;
                }
                break;
            case "ArrowRight":
                OpenSubmenu();
                break;
            case "ArrowLeft":
                await Close();
                break;
            case "Home":
                _activeIndex = 0;
                break;
            case "End":
                _activeIndex = Menu.MenuOptionList.Count - 1;
                break;
            case "Escape":
                await Close();
                break;
            case "Enter":
            case " ":
                var option = Menu.MenuOptionList[_activeIndex];
                if (option.SubMenu is not null)
                {
                    OpenSubmenu();
                }
                else if (option.OnClickFunc is not null)
                {
                    await option.OnClickFunc.Invoke();
                    await Close();
                }
                else if (option.SimpleWidgetKind != SimpleWidgetKind.None)
                {
                    _indexMenuOptionShouldDisplayWidget = _activeIndex;
                }
                break;
        }
        
        StateHasChanged();
    }
    
    [JSInvokable]
    public void ReceiveOnContextMenu(MenuEventArgsMouseDown eventArgsMouseDown)
    {
        _menuMeasurements = new MenuMeasurements(
            eventArgsMouseDown.ViewWidth,
            eventArgsMouseDown.ViewHeight,
            eventArgsMouseDown.BoundingClientRectLeft,
            eventArgsMouseDown.BoundingClientRectTop);
    
        StateHasChanged();
    }
    
    [JSInvokable]
    public void ReceiveContentOnMouseDown(MenuEventArgsMouseDown eventArgsMouseDown)
    {
        _menuMeasurements = new MenuMeasurements(
            eventArgsMouseDown.ViewWidth,
            eventArgsMouseDown.ViewHeight,
            eventArgsMouseDown.BoundingClientRectLeft,
            eventArgsMouseDown.BoundingClientRectTop);
        
        var indexClicked = GetIndexClicked(eventArgsMouseDown);
        if (indexClicked == -1)
            return;
            
        StateHasChanged();
    }
    
    [JSInvokable]
    public async Task ReceiveOnClick(MenuEventArgsMouseDown eventArgsMouseDown)
    {
        _menuMeasurements = new MenuMeasurements(
            eventArgsMouseDown.ViewWidth,
            eventArgsMouseDown.ViewHeight,
            eventArgsMouseDown.BoundingClientRectLeft,
            eventArgsMouseDown.BoundingClientRectTop);
    
        var indexClicked = GetIndexClicked(eventArgsMouseDown);
        if (indexClicked == -1)
            return;

        _activeIndex = indexClicked;
        var option = Menu.MenuOptionList[indexClicked];
        
        if (option.SubMenu is not null)
        {
            OpenSubmenu();
        }
        else if (option.OnClickFunc is not null)
        {
            await option.OnClickFunc.Invoke();
            await Close();
        }
        else if (option.SimpleWidgetKind != SimpleWidgetKind.None)
        {
            _indexMenuOptionShouldDisplayWidget = indexClicked;
            StateHasChanged();
        }
    }
    
    [JSInvokable]
    public async Task ReceiveOnDoubleClick(MenuEventArgsMouseDown eventArgsMouseDown)
    {
        _menuMeasurements = new MenuMeasurements(
            eventArgsMouseDown.ViewWidth,
            eventArgsMouseDown.ViewHeight,
            eventArgsMouseDown.BoundingClientRectLeft,
            eventArgsMouseDown.BoundingClientRectTop);
        
        var indexClicked = GetIndexClicked(eventArgsMouseDown);
        if (indexClicked == -1)
            return;
    
        StateHasChanged();
    }
    
    /// <summary>
    /// TODO: This seems to be slightly inaccurate...
    /// ...I'm going to try checking if difference is less than 1.1px then return -1, don't do anything.
    /// The -1 trick seems to result in accuracy but having that tiny deadzone is probably going to be annoying.
    ///
    /// Must be on the UI thread so the method safely can read '_horizontalRuleElementIndexHashSet'.
    /// </summary>
    private int GetIndexClicked(MenuEventArgsMouseDown eventArgsMouseDown)
    {
        var relativeY = eventArgsMouseDown.Y - _menuMeasurements.BoundingClientRectTop + eventArgsMouseDown.ScrollTop;
        relativeY = Math.Max(0, relativeY);
        
        double buildHeight = 0.0;
        
        int optionIndex = 0;
        
        for (; optionIndex < Menu.MenuOptionList.Count; optionIndex++)
        {
            if (_horizontalRuleElementIndexHashSet.Contains(optionIndex))
                buildHeight += HorizontalRuleVerticalOffset;
            if (_indexMenuOptionShouldDisplayWidget == optionIndex)
                buildHeight += WidgetHeight;
        
            buildHeight += LineHeight;
            
            if (buildHeight > relativeY)
                break;
        }
        
        if (Math.Abs(buildHeight - relativeY) < 1.1)
            return -1;
        
        return IndexBasicValidation(optionIndex);
    }
    
    /// <summary>
    /// TODO: Don't replicate this method, it is essentially the inverse of 'GetIndexClicked(...)'
    ///
    /// Must be on the UI thread so the method safely can read '_horizontalRuleElementIndexHashSet'.
    /// </summary>
    private double GetTopByIndex(int index)
    {
        double buildHeight = 0.0;
        
        int optionIndex = 0;
        
        for (; optionIndex < index; optionIndex++)
        {
            if (_horizontalRuleElementIndexHashSet.Contains(optionIndex))
                buildHeight += HorizontalRuleVerticalOffset;
            if (_indexMenuOptionShouldDisplayWidget == optionIndex)
                buildHeight += WidgetHeight;
        
            buildHeight += LineHeight;
        }
        
        return buildHeight;
    }
    
    private int IndexBasicValidation(int indexLocal)
    {
        if (indexLocal < 0)
            return 0;
        else if (indexLocal >= Menu.MenuOptionList.Count)
            return Menu.MenuOptionList.Count - 1;
        
        return indexLocal;
    }
    
    private async Task Close()
    {
        CommonService.Dropdown_ReduceClearAction();
        
        if (!string.IsNullOrWhiteSpace(Menu.ElementIdToRestoreFocusToOnClose))
        {
            await CommonService.JsRuntimeCommonApi.JsRuntime.InvokeVoidAsync(
                "walkCommon.focusHtmlElementById",
                Menu.ElementIdToRestoreFocusToOnClose,
                /*preventScroll:*/ true);
        }
    }
    
    private void OpenSubmenu()
    {
        var menuOption = Menu.MenuOptionList[_activeIndex];
        menuOption.SubMenu.ElementIdToRestoreFocusToOnClose = _htmlId;
        
        var topByIndex = GetTopByIndex(_activeIndex);
        
        var submenuDropdown = new DropdownRecord(
            Key<DropdownRecord>.NewKey(),
            leftInitial: _menuMeasurements.BoundingClientRectLeft + _menuMeasurements.ViewWidth,
            topInitial: _menuMeasurements.BoundingClientRectTop + topByIndex,
            typeof(MenuDisplay),
            new Dictionary<string, object?>
            {
                {
                    nameof(Menu),
                    menuOption.SubMenu
                }
            },
            restoreFocusOnClose: null);
        CommonService.Dropdown_ReduceRegisterAction(submenuDropdown);
    }
    
    public void Dispose()
    {
        _dotNetHelper?.Dispose();
    }
}
