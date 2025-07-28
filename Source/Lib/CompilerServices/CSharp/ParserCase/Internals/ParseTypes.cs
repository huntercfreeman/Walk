using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;

namespace Walk.CompilerServices.CSharp.ParserCase.Internals;

public static class ParseTypes
{
    /// <summary>
    /// TODO: TypeDefinitionNode(s) should use the expression loop to parse the...
    /// ...generic parameters. They currently use 'ParseTypes.HandleGenericParameters(...);'
    /// </summary>
    public static (SyntaxToken OpenAngleBracketToken, List<GenericParameterEntry> GenericParameterEntryList, SyntaxToken CloseAngleBracketToken) HandleGenericParameters(ref CSharpParserModel parserModel)
    {
        var openAngleBracketToken = parserModel.TokenWalker.Consume();
    
        if (SyntaxKind.CloseAngleBracketToken == parserModel.TokenWalker.Current.SyntaxKind)
        {
            return (openAngleBracketToken, new(), parserModel.TokenWalker.Consume());
        }

        var genericParameterList = new List<GenericParameterEntry>();

        while (true)
        {
            // TypeClause
            var typeClauseNode = MatchTypeClause(ref parserModel);

            if (typeClauseNode.IsFabricated)
                break;

            var genericArgumentEntryNode = new GenericParameterEntry(new TypeReference(typeClauseNode));
            genericParameterList.Add(genericArgumentEntryNode);

            if (SyntaxKind.CommaToken == parserModel.TokenWalker.Current.SyntaxKind)
            {
                var commaToken = parserModel.TokenWalker.Consume();

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

        return (openAngleBracketToken, genericParameterList, closeAngleBracketToken);
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
            
            return parserModel.ConstructOrRecycleTypeClauseNode(
                syntaxToken,
                openAngleBracketToken: default,
        		genericParameterEntryList: null,
        		closeAngleBracketToken: default,
                isKeywordType: false);
        }
        
        /*ISyntaxToken syntaxToken;
        
        if (UtilityApi.IsKeywordSyntaxKind(parserModel.TokenWalker.Current.SyntaxKind) &&
                (UtilityApi.IsTypeIdentifierKeywordSyntaxKind(parserModel.TokenWalker.Current.SyntaxKind) ||
                UtilityApi.IsVarContextualKeyword(compilationUnit, parserModel.TokenWalker.Current.SyntaxKind)))
        {
            syntaxToken = parserModel.TokenWalker.Consume();
        }
        else
        {
            syntaxToken = parserModel.TokenWalker.Match(SyntaxKind.IdentifierToken);
        }

        var typeClauseNode = new TypeClauseNode(
            syntaxToken,
            null,
            null);

        parserModel.Binder.BindTypeClauseNode(typeClauseNode, compilationUnit);

        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenAngleBracketToken)
        {
            var genericParametersListingNode = (GenericParametersListingNode)ParseOthers.Force_ParseExpression(
                SyntaxKind.GenericParametersListingNode,
                compilationUnit);
                
            typeClauseNode.SetGenericParametersListingNode(genericParametersListingNode);
        }
        
        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.QuestionMarkToken)
        {
            typeClauseNode.HasQuestionMark = true;
            _ = parserModel.TokenWalker.Consume();
        }
        
        while (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenSquareBracketToken)
        {
            var openSquareBracketToken = parserModel.TokenWalker.Consume();
            var closeSquareBracketToken = parserModel.TokenWalker.Match(SyntaxKind.CloseSquareBracketToken);

            var arraySyntaxTokenTextSpan = syntaxToken.TextSpan with
            {
                EndExclusiveIndex = closeSquareBracketToken.TextSpan.EndExclusiveIndex
            };

            var arraySyntaxToken = new ArraySyntaxToken(arraySyntaxTokenTextSpan);
            var genericParameterEntryNode = new GenericParameterEntryNode(typeClauseNode);

            var genericParametersListingNode = new GenericParametersListingNode(
                new OpenAngleBracketToken(openSquareBracketToken.TextSpan)
                {
                    IsFabricated = true
                },
                new List<GenericParameterEntryNode> { genericParameterEntryNode },
                new CloseAngleBracketToken(closeSquareBracketToken.TextSpan)
                {
                    IsFabricated = true
                });

            return new TypeClauseNode(
                arraySyntaxToken,
                null,
                genericParametersListingNode);

            // TODO: Implement multidimensional arrays. This array logic always returns after finding the first array syntax.
        }

        return typeClauseNode;
        */
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
        
        parserModel.CurrentCodeBlockOwner.PermitCodeBlockParsing = true;
        
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
                        parserModel.Compilation.ResourceUri);
                        
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
