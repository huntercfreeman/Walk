using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Keyboards.Models;
using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Common.RazorLib.Menus.Displays;

public partial class MenuOptionDisplay : ComponentBase
{
    [Inject]
    private CommonService CommonService { get; set; } = null!;

	[CascadingParameter]
	public DropdownRecord? Dropdown { get; set; }

    [Parameter, EditorRequired]
    public MenuOptionRecord MenuOptionRecord { get; set; } = null!;
    [Parameter, EditorRequired]
    public int Index { get; set; }
    [Parameter, EditorRequired]
    public int ActiveMenuOptionRecordIndex { get; set; }

    [Parameter]
    public RenderFragment<MenuOptionRecord>? IconRenderFragment { get; set; }

	private string _menuOptionHtmlElementId => $"di_menu-option-display_{_htmlElementIdSalt}";

    private readonly Key<DropdownRecord> _subMenuDropdownKey = Key<DropdownRecord>.NewKey();
    private readonly Guid _htmlElementIdSalt = Guid.NewGuid();

    private ElementReference? _topmostElementReference;
    private bool _shouldDisplayWidget;

    private bool IsActive => Index == ActiveMenuOptionRecordIndex;

    private bool HasSubmenuActive => CommonService.GetDropdownState().DropdownList.Any(
        x => x.Key == _subMenuDropdownKey);

    private string IsActiveCssClass => IsActive ? "di_active" : string.Empty;

    private string HasWidgetActiveCssClass =>
        _shouldDisplayWidget && MenuOptionRecord.WidgetRendererType is not null
            ? "di_active"
            : string.Empty;

    private MenuOptionCallbacks MenuOptionCallbacks => new(
        () => HideWidgetAsync(null),
        HideWidgetAsync);

    private bool DisplayWidget => _shouldDisplayWidget && MenuOptionRecord.WidgetRendererType is not null;

    protected override async Task OnParametersSetAsync()
    {
        var localHasSubmenuActive = HasSubmenuActive;

        // The following if(s) are not working. They result
        // in one never being able to open a submenu
        //
        // // Close submenu for non active menu option
        // if (!IsActive && localHasSubmenuActive)
        //     Dispatcher.Dispatch(new RemoveActiveDropdownKeyAction(_subMenuDropdownKey));
        //
        // // Hide widget for non active menu option
        // if (!IsActive && _shouldDisplayWidget)
        // {
        //     _shouldDisplayWidget = false;
        // }

        // Set focus to active menu option
        if (IsActive && !localHasSubmenuActive && !DisplayWidget)
        {
            await FocusElementReference();
        }

        await base.OnParametersSetAsync();
    }
    
    private Task RenderDropdownAsync(MenuRecord localSubMenu)
    {
    	return DropdownHelper.RenderDropdownAsync(
    		CommonService,
    		(JsRuntimes.Models.WalkCommonJavaScriptInteropApi)CommonService.JsRuntimeCommonApi,
			_menuOptionHtmlElementId,
			DropdownOrientation.Right,
			_subMenuDropdownKey,
			localSubMenu,
			FocusElementReference);
    }

    private async Task HandleOnClick()
    {
        if (MenuOptionRecord.OnClickFunc is not null)
        {
            var localDropdown = Dropdown;
            
            CommonService.Dropdown_ReduceClearAction();
			
			if (localDropdown is not null)
            	CommonService.Dropdown_ReduceDisposeAction(localDropdown.Key);
            	
            await MenuOptionRecord.OnClickFunc.Invoke().ConfigureAwait(false);
        }

		var localSubMenu = MenuOptionRecord.SubMenu;
        if (localSubMenu is not null)
        {
            if (HasSubmenuActive)
                CommonService.Dropdown_ReduceDisposeAction(_subMenuDropdownKey);
            else
				await RenderDropdownAsync(localSubMenu);
        }

        if (MenuOptionRecord.WidgetRendererType is not null)
        {
            _shouldDisplayWidget = !_shouldDisplayWidget;
        }
    }

    private Task HandleOnKeyDown(KeyboardEventArgs keyboardEventArgs)
    {
        switch (keyboardEventArgs.Key)
        {
            case KeyboardKeyFacts.MovementKeys.ARROW_RIGHT:
            case KeyboardKeyFacts.AlternateMovementKeys.ARROW_RIGHT:
				var localSubMenu = MenuOptionRecord.SubMenu;
                if (localSubMenu is not null)
                    return RenderDropdownAsync(localSubMenu);
                break;
        }

        switch (keyboardEventArgs.Code)
        {
            case KeyboardKeyFacts.WhitespaceCodes.ENTER_CODE:
            case KeyboardKeyFacts.WhitespaceCodes.SPACE_CODE:
				return HandleOnClick();
        }

        return Task.CompletedTask;
    }

	private async Task FocusElementReference()
	{
		var localTopmostElementReference = _topmostElementReference;
	    try
        {
            if (localTopmostElementReference.HasValue)
            {
                await localTopmostElementReference.Value
                    .FocusAsync()
                    .ConfigureAwait(false);
            }
        }
        catch (Exception)
        {
            // TODO: Capture specifically the exception that is fired when the JsRuntime...
			//       ...tries to set focus to an HTML element, but that HTML element
			//       was not found.
        }
	}

    private async Task HideWidgetAsync(Action? onAfterWidgetHidden)
    {
        _shouldDisplayWidget = false;
        await InvokeAsync(StateHasChanged);

        if (onAfterWidgetHidden is null) // Only hide the widget
        {
			await FocusElementReference();
        }
        else // Hide the widget AND dispose the menu
        {
            onAfterWidgetHidden.Invoke();
            CommonService.Dropdown_ReduceClearAction();

			var localDropdown = Dropdown;
			if (localDropdown is not null)
            	CommonService.Dropdown_ReduceDisposeAction(localDropdown.Key);
        }
    }
}