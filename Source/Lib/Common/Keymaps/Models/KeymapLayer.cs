using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Common.RazorLib.Keymaps.Models;

public record struct KeymapLayer(
    Key<KeymapLayer> Key,
    string DisplayName,
    string InternalName)
{
    public KeymapLayer()
        : this(Key<KeymapLayer>.Empty, string.Empty, string.Empty)
    {

    }
}