using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices;

public interface IExtendedCompilationUnit : ICompilationUnit
{
    /// <summary>
    /// This contains all "relevant" ISyntaxNode that were parsed for the file.
    ///
    /// Essentially, this is a flattened syntax tree.
    ///
    /// As well, the amount of nodes that are kept in this list vary
    /// depending on the "purpose" of the parse.
    ///
    /// Was it a solution wide parse? Then take as little information as necessary.
    /// Did the user open a file? Then take more information.
    /// </summary>
    public List<ICodeBlockOwner> CodeBlockOwnerList { get; }
    
    public int IndexNodeList { get; set; }
    public int CountNodeList { get; set; }
    
    public string SourceText { get; }
}
