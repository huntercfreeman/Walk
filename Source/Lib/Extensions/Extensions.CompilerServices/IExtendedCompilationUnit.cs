using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices;

public interface IExtendedCompilationUnit : ICompilationUnit
{
    public int IndexCodeBlockOwnerList { get; set; }
    public int CountCodeBlockOwnerList { get; set; }
    
    public int IndexNodeList { get; set; }
    public int CountNodeList { get; set; }
}
