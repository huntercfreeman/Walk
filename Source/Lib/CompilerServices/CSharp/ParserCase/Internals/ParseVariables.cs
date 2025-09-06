using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Enums;
using Walk.Extensions.CompilerServices.Syntax.NodeReferences;
using Walk.Extensions.CompilerServices.Syntax.NodeValues;

namespace Walk.CompilerServices.CSharp.ParserCase.Internals;

public static class ParseVariables
{
    /// <summary>Function invocation which uses the 'out' keyword.</summary>
    public static VariableDeclarationNode? HandleVariableDeclarationExpression(
        TypeClauseNode consumedTypeClauseNode,
        SyntaxToken consumedIdentifierToken,
        VariableKind variableKind,
        ref CSharpParserState parserModel)
    {
        VariableDeclarationNode variableDeclarationNode;

        if ((parserModel.Compilation.CompilationUnitKind == CompilationUnitKind.SolutionWide_DefinitionsOnly ||
            parserModel.Compilation.CompilationUnitKind == CompilationUnitKind.SolutionWide_MinimumLocalsData) &&
            variableKind == VariableKind.Local)
        {
            variableDeclarationNode = parserModel.Rent_TemporaryLocalVariableDeclarationNode();
            variableDeclarationNode.TypeReference = new TypeReferenceValue(consumedTypeClauseNode);
            variableDeclarationNode.IdentifierToken = consumedIdentifierToken;
            variableDeclarationNode.VariableKind = variableKind;
            variableDeclarationNode.IsInitialized = false;
            variableDeclarationNode.ResourceUri = parserModel.ResourceUri;
            variableDeclarationNode._isFabricated = false;
        }
        else
        {
            variableDeclarationNode = new VariableDeclarationNode(
                new TypeReferenceValue(consumedTypeClauseNode),
                consumedIdentifierToken,
                variableKind,
                false,
                parserModel.ResourceUri);
        }
        
        parserModel.Return_TypeClauseNode(consumedTypeClauseNode);
        
        parserModel.BindVariableDeclarationNode(variableDeclarationNode);
        return variableDeclarationNode;
    }
}
