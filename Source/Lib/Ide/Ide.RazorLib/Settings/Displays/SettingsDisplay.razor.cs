using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib;

namespace Walk.Ide.RazorLib.Settings.Displays;

public partial class SettingsDisplay : ComponentBase
{
    [Inject]
    private TextEditorService TextEditorService { get; set; } = null!;

    public static readonly Key<IDynamicViewModel> SettingsDialogKey = Key<IDynamicViewModel>.NewKey();
}
