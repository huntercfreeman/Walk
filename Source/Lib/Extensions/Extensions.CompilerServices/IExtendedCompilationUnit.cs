using Walk.TextEditor.RazorLib.CompilerServices;

namespace Walk.Extensions.CompilerServices;

public interface IExtendedCompilationUnit : ICompilationUnit
{
    public int ScopeIndex { get; set; }
    public int ScopeCount { get; set; }
    
    public int IndexNodeList { get; set; }
    public int CountNodeList { get; set; }
}
