using Walk.Common.RazorLib.Keys.Models;

namespace Walk.TextEditor.RazorLib.FindAlls.Models;

public interface ITextEditorSearchEngine
{
    public Key<ITextEditorSearchEngine> Key { get; }
    public Type IconComponentRendererType { get; }
    public string DisplayName { get; }

    public Task SearchAsync(string searchQuery, CancellationToken cancellationToken = default);
}
