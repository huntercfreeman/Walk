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
	private DropdownState _dropdownState = new();
	
	public DropdownState GetDropdownState() => _dropdownState;
	
    public void Dropdown_ReduceRegisterAction(DropdownRecord dropdown)
    {
    	var inState = GetDropdownState();
    
		var indexExistingDropdown = inState.DropdownList.FindIndex(
			x => x.Key == dropdown.Key);

		if (indexExistingDropdown != -1)
		{
			CommonUiStateChanged?.Invoke(CommonUiEventKind.DropdownStateChanged);
			return;
		}

		var outDropdownList = new List<DropdownRecord>(inState.DropdownList);
		outDropdownList.Add(dropdown);

        _dropdownState = inState with
        {
            DropdownList = outDropdownList
        };
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.DropdownStateChanged);
		return;
    }

    public void Dropdown_ReduceDisposeAction(Key<DropdownRecord> key)
    {
    	var inState = GetDropdownState();
    
		var indexExistingDropdown = inState.DropdownList.FindIndex(
			x => x.Key == key);

		if (indexExistingDropdown == -1)
		{
			CommonUiStateChanged?.Invoke(CommonUiEventKind.DropdownStateChanged);
			return;
		}
			
		var outDropdownList = new List<DropdownRecord>(inState.DropdownList);
		outDropdownList.RemoveAt(indexExistingDropdown);

        _dropdownState = inState with
        {
            DropdownList = outDropdownList
        };
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.DropdownStateChanged);
		return;
    }

    public void Dropdown_ReduceClearAction()
    {
    	var inState = GetDropdownState();
    
    	var outDropdownList = new List<DropdownRecord>();
    
        _dropdownState = inState with
        {
            DropdownList = outDropdownList
        };
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.DropdownStateChanged);
		return;
    }

    public void Dropdown_ReduceFitOnScreenAction(DropdownRecord dropdown)
    {
    	var inState = GetDropdownState();
    
		var indexExistingDropdown = inState.DropdownList.FindIndex(
			x => x.Key == dropdown.Key);

		if (indexExistingDropdown == -1)
		{
			CommonUiStateChanged?.Invoke(CommonUiEventKind.DropdownStateChanged);
			return;
		}
		
		var inDropdown = inState.DropdownList[indexExistingDropdown];

		var outDropdown = inDropdown with
		{
			Width = dropdown.Width,
			Height = dropdown.Height,
			Left = dropdown.Left,
			Top = dropdown.Top
		};
		
		var outDropdownList = new List<DropdownRecord>(inState.DropdownList);
		outDropdownList[indexExistingDropdown] = outDropdown;

        _dropdownState = inState with
        {
            DropdownList = outDropdownList
        };
        
        CommonUiStateChanged?.Invoke(CommonUiEventKind.DropdownStateChanged);
		return;
    }
}
