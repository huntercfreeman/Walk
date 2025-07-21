using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Common.RazorLib.Keymaps.Models;

public record struct KeymapLayer(
    int Key,
    string DisplayName,
    string InternalName)
{
    public KeymapLayer()
        : this(-1, string.Empty, string.Empty)
    {

    }
}