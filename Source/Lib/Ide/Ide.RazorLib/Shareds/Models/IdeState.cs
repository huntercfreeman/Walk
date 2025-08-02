using Walk.Common.RazorLib.Badges.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dropdowns.Models;

namespace Walk.Ide.RazorLib.Shareds.Models;

public record struct IdeState(
    MenuRecord MenuFile,
    MenuRecord MenuTools,
    MenuRecord MenuView,
    MenuRecord MenuRun)
{
    public IdeState() : this(
        new MenuRecord(Array.Empty<MenuOptionRecord>()),
        new MenuRecord(Array.Empty<MenuOptionRecord>()),
        new MenuRecord(Array.Empty<MenuOptionRecord>()),
        new MenuRecord(Array.Empty<MenuOptionRecord>()))
    {
    }
    
    public static readonly Key<DropdownRecord> DropdownKeyFile = Key<DropdownRecord>.NewKey();
    public const string ButtonFileId = "di_ide_header-button-file";

    public static readonly Key<DropdownRecord> DropdownKeyTools = Key<DropdownRecord>.NewKey();
    public const string ButtonToolsId = "di_ide_header-button-tools";

    public static readonly Key<DropdownRecord> DropdownKeyView = Key<DropdownRecord>.NewKey();
    public const string ButtonViewId = "di_ide_header-button-view";

    public static readonly Key<DropdownRecord> DropdownKeyRun = Key<DropdownRecord>.NewKey();
    public const string ButtonRunId = "di_ide_header-button-run";
}
