namespace Walk.Common.RazorLib.Keymaps.Models;

public record struct KeymapLayer(int Key)
{
    public KeymapLayer()
        : this(-1)
    {

    }
}
