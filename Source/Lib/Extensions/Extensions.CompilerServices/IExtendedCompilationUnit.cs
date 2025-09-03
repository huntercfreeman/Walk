using Walk.TextEditor.RazorLib.CompilerServices;

namespace Walk.Extensions.CompilerServices;

public interface IExtendedCompilationUnit : ICompilationUnit
{
    public int ScopeOffset { get; set; }
    public int ScopeLength { get; set; }
    
    public int NodeOffset { get; set; }
    public int NodeLength { get; set; }
}
