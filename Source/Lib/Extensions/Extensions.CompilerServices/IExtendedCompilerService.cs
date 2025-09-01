using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices;

public interface IExtendedCompilerService : ICompilerService
{
    /// <summary>
    /// unsafe vs safe are duplicates of the same code
    /// Safe implies the "TextEditorEditContext"
    /// </summary>
    public string? UnsafeGetText(string absolutePath, TextEditorTextSpan textSpan);
    /// <summary>
    /// unsafe vs safe are duplicates of the same code
    /// Safe implies the "TextEditorEditContext"
    /// </summary>
    public string? SafeGetText(string absolutePath, TextEditorTextSpan textSpan);

    public IReadOnlyList<Walk.Extensions.CompilerServices.Syntax.Nodes.GenericParameterEntry> GenericParameterEntryList { get; }
    public IReadOnlyList<Walk.Extensions.CompilerServices.Syntax.Nodes.FunctionParameterEntry> FunctionParameterEntryList { get; }
    public IReadOnlyList<Walk.Extensions.CompilerServices.Syntax.Nodes.FunctionArgumentEntry> FunctionArgumentEntryList { get; }

    public ISyntaxNode? GetSyntaxNode(int positionIndex, ResourceUri resourceUri, ICompilerServiceResource? compilerServiceResource);
    public ISyntaxNode? GetDefinitionNode(TextEditorTextSpan textSpan, ResourceUri resourceUri, ICompilerServiceResource compilerServiceResource, Symbol? symbol = null);
    public (CodeBlockValue CodeBlockValue, ICodeBlockOwner? CodeBlockOwner) GetCodeBlockTupleByPositionIndex(ResourceUri resourceUri, int positionIndex);
    public string GetIdentifierText(ISyntaxNode node, ResourceUri resourceUri);
}
