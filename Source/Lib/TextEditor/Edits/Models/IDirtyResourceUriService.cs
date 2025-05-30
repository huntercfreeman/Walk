using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.TextEditor.RazorLib.Edits.Models;

public interface IDirtyResourceUriService
{
	public event Action DirtyResourceUriStateChanged;
	
	public DirtyResourceUriState GetDirtyResourceUriState();

    public void AddDirtyResourceUri(ResourceUri resourceUri);
    public void RemoveDirtyResourceUri(ResourceUri resourceUri);
}
