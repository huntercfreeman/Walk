namespace Walk.Extensions.CompilerServices.Syntax;

public interface ISyntaxNode : ISyntax
{
    /// <summary>
    /// This should be initialized to -1 as that will imply "null" / that it wasn't set yet.
    ///
    /// This indicates the index that the parent 'ICodeBlockOwner' is at in the 'CSharpCompilationUnit.DefinitionTupleList'.
    ///
    /// This is unsafe, because you must be certain that all data you're interacting with is coming from the same 'CSharpCompilationUnit'.
    /// </summary>
    public int ParentIndexKey { get; set; }
}
