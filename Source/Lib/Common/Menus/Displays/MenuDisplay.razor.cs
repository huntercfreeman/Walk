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

    [CascadingParameter(Name="ReturnFocusToParentFuncAsync")]
    public Func<Task>? ReturnFocusToParentFuncAsync { get; set; }
    [CascadingParameter]
    public DropdownRecord? Dropdown { get; set; }

    [Parameter, EditorRequired]
    public MenuRecord MenuRecord { get; set; } = null!;

    [Parameter]
    public int InitialActiveMenuOptionRecordIndex { get; set; } = -1;
    [Parameter]
    public bool GroupByMenuOptionKind { get; set; } = true;
    [Parameter]
    public bool FocusOnAfterRenderAsync { get; set; } = true;
    [Parameter]
    public RenderFragment<MenuOptionRecord>? IconRenderFragment { get; set; }
}
