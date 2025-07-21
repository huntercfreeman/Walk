using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Keymaps.Models;

namespace Walk.Common.RazorLib.Keymaps.Displays;

public partial class KeymapDisplay : ComponentBase
{
    [Inject]
    private CommonService CommonService { get; set; } = null!;

    [Parameter, EditorRequired]
    public IKeymap Keymap { get; set; } = null!;
}
