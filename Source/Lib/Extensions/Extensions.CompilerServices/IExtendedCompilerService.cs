using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Values;
using Walk.Extensions.CompilerServices.Syntax.Interfaces;
using Walk.Extensions.CompilerServices.Syntax.NodeValues;

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

    public IReadOnlyList<GenericParameter> GenericParameterEntryList { get; }
    public IReadOnlyList<FunctionParameter> FunctionParameterEntryList { get; }
    public IReadOnlyList<Walk.Extensions.CompilerServices.Syntax.Nodes.FunctionArgument> FunctionArgumentEntryList { get; }
    
    public IReadOnlyList<TypeDefinitionTraits> TypeDefinitionTraitsList { get; }
    public IReadOnlyList<FunctionDefinitionTraits> FunctionDefinitionTraitsList { get; }
    public IReadOnlyList<VariableDeclarationTraits> VariableDeclarationTraitsList { get; }
    public IReadOnlyList<ConstructorDefinitionTraits> ConstructorDefinitionTraitsList { get; }

    public SyntaxNodeValue GetSyntaxNode(int positionIndex, ResourceUri resourceUri, ICompilerServiceResource? compilerServiceResource);
    public SyntaxNodeValue GetDefinitionNodeValue(TextEditorTextSpan textSpan, ResourceUri resourceUri, ICompilerServiceResource compilerServiceResource, Symbol? symbol = null);
    public (Scope Scope, ICodeBlockOwner? CodeBlockOwner) GetCodeBlockTupleByPositionIndex(ResourceUri resourceUri, int positionIndex);
    public string GetIdentifierText(ISyntaxNode node, ResourceUri resourceUri);
}
