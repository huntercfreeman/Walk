using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.Ide.RazorLib.CodeSearches.Models;

public interface ICodeSearchService
{
	public Throttle _updateContentThrottle { get; }

	public event Action? CodeSearchStateChanged;
    
    public CodeSearchState GetCodeSearchState();
    
    public void With(Func<CodeSearchState, CodeSearchState> withFunc);
    public void AddResult(string result);
    public void ClearResultList();
    public void InitializeResizeHandleDimensionUnit(DimensionUnit dimensionUnit);

    /// <summary>
    /// TODO: This method makes use of <see cref="IThrottle"/> and yet is accessing...
    ///       ...searchEffect.CancellationToken.
    ///       The issue here is that the search effect parameter to this method
    ///       could be out of date by the time that the throttle delay is completed.
    ///       This should be fixed. (2024-05-02)
    /// </summary>
    /// <param name="searchEffect"></param>
    /// <param name="dispatcher"></param>
    /// <returns></returns>
    public Task HandleSearchEffect(CancellationToken CancellationToken = default);
    
    public Task UpdateContent(ResourceUri providedResourceUri);
}
