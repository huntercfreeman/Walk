using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Dropdowns.Models;
using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Common.RazorLib.Menus.Displays;

public partial class MenuDisplay : ComponentBase
{
    [Inject]
    private CommonService CommonService { get; set; } = null!;

    [Parameter, EditorRequired]
    public MenuRecord MenuRecord { get; set; } = null!;

    private int _activeIndex;
    
    private void HandleOnKeyDown(KeyboardEventArgs keyboardEventArgs)
    {
        switch (keyboardEventArgs.Key)
        {
            case "ArrowDown":
                _activeIndex++;
                break;
            case "ArrowUp":
                _activeIndex--;
                break;
        }
    }
}
