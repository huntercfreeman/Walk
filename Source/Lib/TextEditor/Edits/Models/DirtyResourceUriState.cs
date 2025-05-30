using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.TextEditor.RazorLib.Edits.Models;

public record struct DirtyResourceUriState(List<ResourceUri> DirtyResourceUriList)
{
    public DirtyResourceUriState() : this(new())
    {
    }
}
