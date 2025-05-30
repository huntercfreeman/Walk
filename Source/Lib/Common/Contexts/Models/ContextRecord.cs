using Walk.Common.RazorLib.Keymaps.Models;
using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Common.RazorLib.Contexts.Models;

public record struct ContextRecord(
    Key<ContextRecord> ContextKey,
    string DisplayNameFriendly,
    string ContextNameInternal,
    IKeymap Keymap)
{
    public string ContextElementId => $"di_ide_context-{ContextKey.Guid}";
}
