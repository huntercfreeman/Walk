using Walk.TextEditor.RazorLib.CompilerServices;

namespace Walk.Extensions.CompilerServices;

public interface IExtendedCompilationUnit : ICompilationUnit
{
    public int IndexCodeBlockOwnerList { get; set; }
    public int CountCodeBlockOwnerList { get; set; }
    
    public int IndexNodeList { get; set; }
    public int CountNodeList { get; set; }
}
