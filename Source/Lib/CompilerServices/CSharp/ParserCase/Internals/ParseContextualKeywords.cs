namespace Walk.CompilerServices.CSharp.ParserCase.Internals;

public class ParseContextualKeywords
{
    public static void HandleVarTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        if (parserModel.StatementBuilder.StatementIsEmpty)
            ParseTokens.ParseIdentifierToken(ref parserModel);
        else
            _ = ParseExpressions.ParseExpression(ref parserModel);
    }

    public static void HandlePartialTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleAddTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleAndTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleAliasTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleAscendingTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleArgsTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleAsyncTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleAwaitTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleByTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleDescendingTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleDynamicTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleEqualsTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleFileTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleFromTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleGetTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleGlobalTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleGroupTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleInitTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleIntoTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleJoinTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleLetTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleManagedTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleNameofTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        var expression = ParseExpressions.ParseExpression(ref parserModel);
        
        if (expression.SyntaxKind == Walk.Extensions.CompilerServices.Syntax.SyntaxKind.VariableReferenceNode)
        {
            parserModel.Return_VariableReferenceNode((Walk.Extensions.CompilerServices.Syntax.Nodes.VariableReferenceNode)expression);
        }
        else if (expression.SyntaxKind == Walk.Extensions.CompilerServices.Syntax.SyntaxKind.FunctionInvocationNode)
        {
            parserModel.Return_FunctionInvocationNode((Walk.Extensions.CompilerServices.Syntax.Nodes.FunctionInvocationNode)expression);
        }
    }

    public static void HandleNintTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleNotTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleNotnullTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleNuintTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleOnTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleOrTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleOrderbyTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleRecordTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        ParseDefaultKeywords.HandleStorageModifierTokenKeyword(ref parserModel);
    }

    public static void HandleRemoveTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleRequiredTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleScopedTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleSelectTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleSetTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleUnmanagedTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleValueTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleWhenTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleWhereTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleWithTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleYieldTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleUnrecognizedTokenContextualKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }
}
