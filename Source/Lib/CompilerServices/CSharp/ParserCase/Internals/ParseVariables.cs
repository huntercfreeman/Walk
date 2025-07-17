using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.CompilerServices.CSharp.CompilerServiceCase;

namespace Walk.CompilerServices.CSharp.ParserCase.Internals;

public static class ParseVariables
{
    /// <summary>Function invocation which uses the 'out' keyword.</summary>
    public static VariableDeclarationNode? HandleVariableDeclarationExpression(
        TypeClauseNode consumedTypeClauseNode,
        SyntaxToken consumedIdentifierToken,
        VariableKind variableKind,
        CSharpCompilationUnit compilationUnit,
        ref CSharpParserModel parserModel)
    {
    	VariableDeclarationNode variableDeclarationNode;

		variableDeclarationNode = new VariableDeclarationNode(
	        new TypeReference(consumedTypeClauseNode),
	        consumedIdentifierToken,
	        variableKind,
	        false,
	        compilationUnit.ResourceUri);

        parserModel.Binder.BindVariableDeclarationNode(variableDeclarationNode, ref parserModel);
        return variableDeclarationNode;
    }
}
