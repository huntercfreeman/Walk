using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Enums;
using Walk.Extensions.CompilerServices.Syntax.Nodes;

namespace Walk.CompilerServices.CSharp.ParserCase.Internals;

public static class ParseTypes
{
    /// <summary>
    /// TODO: TypeDefinitionNode(s) should use the expression loop to parse the...
    /// ...generic parameters. They currently use 'ParseTypes.HandleGenericParameters(...);'
    /// </summary>
    public static (SyntaxToken OpenAngleBracketToken, int IndexGenericParameterEntryList, int CountGenericParameterEntryList, SyntaxToken CloseAngleBracketToken) HandleGenericParameters(ref CSharpParserModel parserModel)
    {
        var openAngleBracketToken = parserModel.TokenWalker.Consume();
    
        if (SyntaxKind.CloseAngleBracketToken == parserModel.TokenWalker.Current.SyntaxKind)
        {
            return (openAngleBracketToken, -1, 0, parserModel.TokenWalker.Consume());
        }

        var indexGenericParameterEntryList = parserModel.Binder.GenericParameterList.Count;
        var countGenericParameterEntryList = 0;

        while (true)
        {
            // TypeClause
            var typeClauseNode = MatchTypeClause(ref parserModel);

            if (typeClauseNode.IsFabricated)
                break;

            var genericArgumentEntryNode = new GenericParameter(new TypeReference(typeClauseNode));
            parserModel.Return_TypeClauseNode(typeClauseNode);
            parserModel.Binder.GenericParameterList.Add(genericArgumentEntryNode);
            countGenericParameterEntryList++;

            if (SyntaxKind.CommaToken == parserModel.TokenWalker.Current.SyntaxKind)
            {
                _ = parserModel.TokenWalker.Consume(); // commaToken

                // TODO: Track comma tokens?
                //
                // functionArgumentListing.Add(commaToken);
            }
            else
            {
                break;
            }
        }

        var closeAngleBracketToken = parserModel.TokenWalker.Match(SyntaxKind.CloseAngleBracketToken);

        return (openAngleBracketToken, indexGenericParameterEntryList, countGenericParameterEntryList, closeAngleBracketToken);
    }

    public static TypeClauseNode MatchTypeClause(ref CSharpParserModel parserModel)
    {
        parserModel.TryParseExpressionSyntaxKindList.Add(SyntaxKind.TypeClauseNode);
        if (ParseExpressions.TryParseExpression(ref parserModel, out var expressionNode))
        {
            return (TypeClauseNode)expressionNode;
        }
        else
        {
            var syntaxToken = parserModel.TokenWalker.Match(SyntaxKind.IdentifierToken);
            var typeClauseNode = parserModel.Rent_TypeClauseNode();
            typeClauseNode.TypeIdentifierToken = syntaxToken;
            return typeClauseNode;
        }
    }

    public static void HandlePrimaryConstructorDefinition(
        TypeDefinitionNode typeDefinitionNode,
        ref CSharpParserModel parserModel)
    {
        ParseFunctions.HandleFunctionArguments(typeDefinitionNode, ref parserModel, variableKind: VariableKind.Property);
    }
    
    public static void HandleEnumDefinitionNode(
        TypeDefinitionNode typeDefinitionNode,
        ref CSharpParserModel parserModel)
    {
        while (!parserModel.TokenWalker.IsEof)
        {
            if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenBraceToken)
                break;
                
            _ = parserModel.TokenWalker.Consume();
        }
        
        parserModel.SetCurrentScope_PermitCodeBlockParsing(true);
        
        parserModel.StatementBuilder.FinishStatement(parserModel.TokenWalker.Index, ref parserModel);
        
        ParseTokens.ParseOpenBraceToken(ref parserModel);
        
        var shouldFindIdentifier = true;
        
        while (!parserModel.TokenWalker.IsEof)
        {
            if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CloseBraceToken)
                break;
                
            var token = parserModel.TokenWalker.Consume();
            
            if (shouldFindIdentifier)
            {
                if (UtilityApi.IsConvertibleToIdentifierToken(token.SyntaxKind))
                {
                    var identifierToken = UtilityApi.ConvertToIdentifierToken(ref token, ref parserModel);
                    
                    var variableDeclarationNode = new VariableDeclarationNode(
                        typeDefinitionNode.ToTypeReference(),
                        identifierToken,
                        VariableKind.EnumMember,
                        false,
                        parserModel.ResourceUri);
                        
                    parserModel.BindVariableDeclarationNode(variableDeclarationNode);
                    
                    shouldFindIdentifier = !shouldFindIdentifier;
                }
            }
            else
            {
                if (token.SyntaxKind == SyntaxKind.CommaToken)
                    shouldFindIdentifier = !shouldFindIdentifier;
            }
        }
    }
}
