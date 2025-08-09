using Walk.Extensions.CompilerServices;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;

namespace Walk.CompilerServices.CSharp.ParserCase.Internals;

public class ParseDefaultKeywords
{
    public static void HandleAsTokenKeyword(ref CSharpParserModel parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleBaseTokenKeyword(ref CSharpParserModel parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleBoolTokenKeyword(ref CSharpParserModel parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleBreakTokenKeyword(ref CSharpParserModel parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleByteTokenKeyword(ref CSharpParserModel parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleCaseTokenKeyword(ref CSharpParserModel parserModel)
    {
        var caseKeyword = parserModel.TokenWalker.Consume();
        
        parserModel.ExpressionList.Add((SyntaxKind.ColonToken, null));
        var expressionNode = ParseExpressions.ParseExpression(ref parserModel);
        var colonToken = parserModel.TokenWalker.Match(SyntaxKind.ColonToken);
    }

    public static void HandleCatchTokenKeyword(ref CSharpParserModel parserModel)
    {
        var catchKeywordToken = parserModel.TokenWalker.Consume();
        var openParenthesisToken = parserModel.TokenWalker.Match(SyntaxKind.OpenParenthesisToken);
        
        var catchNode = new TryStatementCatchNode(
            // parent: null,
            catchKeywordToken,
            openParenthesisToken,
            closeParenthesisToken: default,
            codeBlock: default);
        
        parserModel.NewScopeAndBuilderFromOwner(
            catchNode,
            parserModel.TokenWalker.Current.TextSpan);
        
        parserModel.ExpressionList.Add((SyntaxKind.CloseParenthesisToken, null));
        var expressionNode = ParseExpressions.ParseExpression(ref parserModel);
        
        var closeParenthesisToken = parserModel.TokenWalker.Match(SyntaxKind.CloseParenthesisToken);
    
        TryStatementNode? tryStatementNode = null;
        
        if (expressionNode.SyntaxKind == SyntaxKind.VariableDeclarationNode)
        {
            var variableDeclarationNode = (VariableDeclarationNode)expressionNode;
            catchNode.VariableDeclarationNode = variableDeclarationNode;
        }
        
        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.WhenTokenContextualKeyword)
        {
            _ = parserModel.TokenWalker.Consume(); // WhenTokenContextualKeyword
            
            parserModel.ExpressionList.Add((SyntaxKind.OpenBraceToken, null));
            _ = ParseExpressions.ParseExpression(ref parserModel);
        }
        
        // Not valid C# -- catch requires brace deliminated code block --, but here for parser recovery.
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenBraceToken)
            parserModel.CurrentCodeBlockOwner.IsImplicitOpenCodeBlockTextSpan = true;
    }

    public static void HandleCharTokenKeyword(ref CSharpParserModel parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleCheckedTokenKeyword(ref CSharpParserModel parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleConstTokenKeyword(ref CSharpParserModel parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleContinueTokenKeyword(ref CSharpParserModel parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleDecimalTokenKeyword(ref CSharpParserModel parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleDefaultTokenKeyword(ref CSharpParserModel parserModel)
    {
        // Switch statement default case.
        if (parserModel.TokenWalker.Next.SyntaxKind == SyntaxKind.ColonToken)
            _ = parserModel.TokenWalker.Consume();
        else
            _ = ParseExpressions.ParseExpression(ref parserModel);
    }

    public static void HandleDelegateTokenKeyword(ref CSharpParserModel parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleDoTokenKeyword(ref CSharpParserModel parserModel)
    {
        var doKeywordToken = parserModel.TokenWalker.Consume();
        
        var doWhileStatementNode = new DoWhileStatementNode(
            doKeywordToken,
            whileKeywordToken: default,
            openParenthesisToken: default,
            closeParenthesisToken: default);
        
        parserModel.NewScopeAndBuilderFromOwner(
            doWhileStatementNode,
            parserModel.TokenWalker.Current.TextSpan);
        
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenBraceToken)
            parserModel.CurrentCodeBlockOwner.IsImplicitOpenCodeBlockTextSpan = true;
    }

    public static void HandleDoubleTokenKeyword(ref CSharpParserModel parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleElseTokenKeyword(ref CSharpParserModel parserModel)
    {
        if (parserModel.TokenWalker.Next.SyntaxKind == SyntaxKind.IfTokenKeyword)
        {
            parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
            return;
        }
    
        var elseTokenKeyword = parserModel.TokenWalker.Consume();
        
        var ifStatementNode = new IfStatementNode(
            elseTokenKeyword,
            default);
        
        parserModel.NewScopeAndBuilderFromOwner(
            ifStatementNode,
            parserModel.TokenWalker.Current.TextSpan);
        
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenBraceToken)
            parserModel.CurrentCodeBlockOwner.IsImplicitOpenCodeBlockTextSpan = true;
    }

    public static void HandleEnumTokenKeyword(ref CSharpParserModel parserModel)
    {
        HandleStorageModifierTokenKeyword(ref parserModel);

        // Why was this method invocation here? (2024-01-23)
        //
        // HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleEventTokenKeyword(ref CSharpParserModel parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleExplicitTokenKeyword(ref CSharpParserModel parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleExternTokenKeyword(ref CSharpParserModel parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleFalseTokenKeyword(ref CSharpParserModel parserModel)
    {
        var expressionNode = ParseExpressions.ParseExpression(ref parserModel);
        parserModel.StatementBuilder.ChildList.Add(expressionNode);
    }

    public static void HandleFinallyTokenKeyword(ref CSharpParserModel parserModel)
    {
        var finallyKeywordToken = parserModel.TokenWalker.Consume();
        
        // TryStatementNode? tryStatementNode = null;
        
        var finallyNode = new TryStatementFinallyNode(
            // tryStatementNode,
            finallyKeywordToken,
            codeBlock: default);
    
        // This was done with CSharpParserModel's SyntaxStack, but that property is now being removed. A different way to accomplish this needs to be done. (2025-02-06)
        // tryStatementNode.SetTryStatementFinallyNode(finallyNode);
        
        parserModel.NewScopeAndBuilderFromOwner(
            finallyNode,
            parserModel.TokenWalker.Current.TextSpan);
    
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenBraceToken)
            parserModel.CurrentCodeBlockOwner.IsImplicitOpenCodeBlockTextSpan = true;
    }

    public static void HandleFixedTokenKeyword(ref CSharpParserModel parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleFloatTokenKeyword(ref CSharpParserModel parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleForTokenKeyword(ref CSharpParserModel parserModel)
    {
        var forKeywordToken = parserModel.TokenWalker.Consume();
        var openParenthesisToken = parserModel.TokenWalker.Match(SyntaxKind.OpenParenthesisToken);
        
        var forStatementNode = new ForStatementNode(
            forKeywordToken,
            openParenthesisToken,
            initializationStatementDelimiterToken: default,
            conditionStatementDelimiterToken: default,
            closeParenthesisToken: default,
            codeBlock: default);
            
        parserModel.NewScopeAndBuilderFromOwner(
            forStatementNode,
            parserModel.TokenWalker.Current.TextSpan);
        
        parserModel.CurrentCodeBlockOwner.IsImplicitOpenCodeBlockTextSpan = false;
        
        for (int i = 0; i < 3; i++)
        {
            parserModel.ExpressionList.Add((SyntaxKind.CloseParenthesisToken, null));
            _ = ParseExpressions.ParseExpression(ref parserModel);
            
            var statementDelimiterToken = parserModel.TokenWalker.Match(SyntaxKind.StatementDelimiterToken);
            
            if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CloseParenthesisToken)
                break;
        }
        
        var closeParenthesisToken = parserModel.TokenWalker.Match(SyntaxKind.CloseParenthesisToken);
        
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenBraceToken)
            parserModel.CurrentCodeBlockOwner.IsImplicitOpenCodeBlockTextSpan = true;
    }

    public static void HandleForeachTokenKeyword(ref CSharpParserModel parserModel)
    {
        var foreachKeywordToken = parserModel.TokenWalker.Consume();
        
        var openParenthesisToken = parserModel.TokenWalker.Match(SyntaxKind.OpenParenthesisToken);
        
        var foreachStatementNode = new ForeachStatementNode(
            foreachKeywordToken,
            openParenthesisToken,
            inKeywordToken: default,
            closeParenthesisToken: default,
            codeBlock: default);
        
        parserModel.NewScopeAndBuilderFromOwner(
            foreachStatementNode,
            parserModel.TokenWalker.Current.TextSpan);
            
        var successParse = ParseExpressions.TryParseVariableDeclarationNode(ref parserModel, out var variableDeclarationNode);
        if (successParse)
            parserModel.BindVariableDeclarationNode(variableDeclarationNode);
        
        var inKeywordToken = parserModel.TokenWalker.Match(SyntaxKind.InTokenKeyword);
        
        parserModel.ExpressionList.Add((SyntaxKind.CloseParenthesisToken, null));
        parserModel.ParserContextKind = CSharpParserContextKind.None;
        var enumerable = ParseExpressions.ParseExpression(ref parserModel);
        
        if (enumerable.ResultTypeReference.IndexGenericParameterEntryList != -1 &&
            variableDeclarationNode is not null &&
            parserModel.GetTextSpanText(variableDeclarationNode.TypeReference.TypeIdentifierToken.TextSpan) == "var")
        {
            if (enumerable.ResultTypeReference.CountGenericParameterEntryList == 1)
                variableDeclarationNode.SetImplicitTypeReference(
                    parserModel.Binder.GenericParameterEntryList[enumerable.ResultTypeReference.IndexGenericParameterEntryList].TypeReference);
        }
        
        if (enumerable.SyntaxKind == SyntaxKind.VariableReferenceNode)
        {
            parserModel.Return_VariableReferenceNode((VariableReferenceNode)enumerable);
        }
        
        var closeParenthesisToken = parserModel.TokenWalker.Match(SyntaxKind.CloseParenthesisToken);
            
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenBraceToken)
            parserModel.CurrentCodeBlockOwner.IsImplicitOpenCodeBlockTextSpan = true;
    }

    public static void HandleGotoTokenKeyword(ref CSharpParserModel parserModel)
    {
        _ = ParseExpressions.ParseExpression(ref parserModel);
    }

    public static void HandleImplicitTokenKeyword(ref CSharpParserModel parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleInTokenKeyword(ref CSharpParserModel parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleIntTokenKeyword(ref CSharpParserModel parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleIsTokenKeyword(ref CSharpParserModel parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleLockTokenKeyword(ref CSharpParserModel parserModel)
    {
        var lockKeywordToken = parserModel.TokenWalker.Consume();
        
        var openParenthesisToken = parserModel.TokenWalker.Match(SyntaxKind.OpenParenthesisToken);
        
        parserModel.ExpressionList.Add((SyntaxKind.CloseParenthesisToken, null));
        _ = ParseExpressions.ParseExpression(ref parserModel);
        
        var closeParenthesisToken = parserModel.TokenWalker.Match(SyntaxKind.CloseParenthesisToken);
        
        var lockStatementNode = new LockStatementNode(
            lockKeywordToken,
            openParenthesisToken,
            closeParenthesisToken,
            codeBlock: default);
            
        parserModel.NewScopeAndBuilderFromOwner(
            lockStatementNode,
            parserModel.TokenWalker.Current.TextSpan);
    
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenBraceToken)
            parserModel.CurrentCodeBlockOwner.IsImplicitOpenCodeBlockTextSpan = true;
    }

    public static void HandleLongTokenKeyword(ref CSharpParserModel parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleNullTokenKeyword(ref CSharpParserModel parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleObjectTokenKeyword(ref CSharpParserModel parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleOperatorTokenKeyword(ref CSharpParserModel parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleOutTokenKeyword(ref CSharpParserModel parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleParamsTokenKeyword(ref CSharpParserModel parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleProtectedTokenKeyword(ref CSharpParserModel parserModel)
    {
        var protectedTokenKeyword = parserModel.TokenWalker.Consume();
        parserModel.StatementBuilder.ChildList.Add(protectedTokenKeyword);
    }

    public static void HandleReadonlyTokenKeyword(ref CSharpParserModel parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleRefTokenKeyword(ref CSharpParserModel parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleSbyteTokenKeyword(ref CSharpParserModel parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleShortTokenKeyword(ref CSharpParserModel parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleSizeofTokenKeyword(ref CSharpParserModel parserModel)
    {
        _ = ParseExpressions.ParseExpression(ref parserModel);
    }

    public static void HandleStackallocTokenKeyword(ref CSharpParserModel parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleStringTokenKeyword(ref CSharpParserModel parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleStructTokenKeyword(ref CSharpParserModel parserModel)
    {
        HandleStorageModifierTokenKeyword(ref parserModel);
    }

    public static void HandleSwitchTokenKeyword(ref CSharpParserModel parserModel)
    {
        var switchKeywordToken = parserModel.TokenWalker.Consume();
        
        var openParenthesisToken = parserModel.TokenWalker.Match(SyntaxKind.OpenParenthesisToken);
        
        parserModel.ExpressionList.Add((SyntaxKind.CloseParenthesisToken, null));
        var expressionNode = ParseExpressions.ParseExpression(ref parserModel);
        
        var closeParenthesisToken = parserModel.TokenWalker.Match(SyntaxKind.CloseParenthesisToken);
        
        var switchStatementNode = new SwitchStatementNode(
            switchKeywordToken,
            openParenthesisToken,
            expressionNode,
            closeParenthesisToken,
            codeBlock: default);
            
        parserModel.NewScopeAndBuilderFromOwner(
            switchStatementNode,
            parserModel.TokenWalker.Current.TextSpan);
    }

    public static void HandleThisTokenKeyword(ref CSharpParserModel parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleThrowTokenKeyword(ref CSharpParserModel parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleTrueTokenKeyword(ref CSharpParserModel parserModel)
    {
        var expressionNode = ParseExpressions.ParseExpression(ref parserModel);
        parserModel.StatementBuilder.ChildList.Add(expressionNode);
    }

    public static void HandleTryTokenKeyword(ref CSharpParserModel parserModel)
    {
        var tryKeywordToken = parserModel.TokenWalker.Consume();
        
        /*var tryStatementNode = new TryStatementNode(
            tryNode: null,
            catchNode: null,
            finallyNode: null);*/
    
        var tryStatementTryNode = new TryStatementTryNode(
            // tryStatementNode,
            tryKeywordToken,
            codeBlock: default);
            
        // tryStatementNode.TryNode = tryStatementTryNode;
            
        // parserModel.CurrentCodeBlockBuilder.AddChild(tryStatementTryNode);
        
        parserModel.NewScopeAndBuilderFromOwner(
            tryStatementTryNode,
            parserModel.TokenWalker.Current.TextSpan);
    
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenBraceToken)
            parserModel.CurrentCodeBlockOwner.IsImplicitOpenCodeBlockTextSpan = true;
    }

    public static void HandleTypeofTokenKeyword(ref CSharpParserModel parserModel)
    {
        _ = ParseExpressions.ParseExpression(ref parserModel);
    }

    public static void HandleUintTokenKeyword(ref CSharpParserModel parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleUlongTokenKeyword(ref CSharpParserModel parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleUncheckedTokenKeyword(ref CSharpParserModel parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleUnsafeTokenKeyword(ref CSharpParserModel parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleUshortTokenKeyword(ref CSharpParserModel parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleVoidTokenKeyword(ref CSharpParserModel parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleVolatileTokenKeyword(ref CSharpParserModel parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleWhileTokenKeyword(ref CSharpParserModel parserModel)
    {
        var whileKeywordToken = parserModel.TokenWalker.Consume();
        
        var openParenthesisToken = parserModel.TokenWalker.Match(SyntaxKind.OpenParenthesisToken);
        
        parserModel.ExpressionList.Add((SyntaxKind.CloseParenthesisToken, null));
        _ = ParseExpressions.ParseExpression(ref parserModel);
        
        var closeParenthesisToken = parserModel.TokenWalker.Match(SyntaxKind.CloseParenthesisToken);
        
        /* This was done with CSharpParserModel's SyntaxStack, but that property is now being removed. A different way to accomplish this needs to be done. (2025-02-06)
        {
            doWhileStatementNode.SetWhileProperties(
                whileKeywordToken,
                openParenthesisToken,
                expressionNode,
                closeParenthesisToken);
            
            return;
        }*/
        
        var whileStatementNode = new WhileStatementNode(
            whileKeywordToken,
            openParenthesisToken,
            closeParenthesisToken,
            codeBlock: default);
            
        parserModel.NewScopeAndBuilderFromOwner(
            whileStatementNode,
            parserModel.TokenWalker.Current.TextSpan);
    
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenBraceToken)
            parserModel.CurrentCodeBlockOwner.IsImplicitOpenCodeBlockTextSpan = true;
    }

    public static void HandleUnrecognizedTokenKeyword(ref CSharpParserModel parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    /// <summary>The 'Default' of this method name is confusing.
    /// It seems to refer to the 'default' of switch statement rather than the 'default' keyword itself?
    /// </summary>
    public static void HandleDefault(ref CSharpParserModel parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleTypeIdentifierKeyword(ref CSharpParserModel parserModel)
    {
        ParseTokens.ParseIdentifierToken(ref parserModel);
    }

    public static void HandleNewTokenKeyword(ref CSharpParserModel parserModel)
    {
        if (parserModel.TokenWalker.Next.SyntaxKind == SyntaxKind.OpenParenthesisToken ||
            UtilityApi.IsConvertibleToIdentifierToken(parserModel.TokenWalker.Next.SyntaxKind))
        {
            var expressionNode = ParseExpressions.ParseExpression(ref parserModel);
            parserModel.StatementBuilder.ChildList.Add(expressionNode);
        }
        else
        {
            parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
        }
    }

    public static void HandlePublicTokenKeyword(ref CSharpParserModel parserModel)
    {
        var publicKeywordToken = parserModel.TokenWalker.Consume();
        parserModel.StatementBuilder.ChildList.Add(publicKeywordToken);
    }

    public static void HandleInternalTokenKeyword(ref CSharpParserModel parserModel)
    {
        var internalTokenKeyword = parserModel.TokenWalker.Consume();
        parserModel.StatementBuilder.ChildList.Add(internalTokenKeyword);
    }

    public static void HandlePrivateTokenKeyword(ref CSharpParserModel parserModel)
    {
        var privateTokenKeyword = parserModel.TokenWalker.Consume();
        parserModel.StatementBuilder.ChildList.Add(privateTokenKeyword);
    }

    public static void HandleStaticTokenKeyword(ref CSharpParserModel parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleOverrideTokenKeyword(ref CSharpParserModel parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleVirtualTokenKeyword(ref CSharpParserModel parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleAbstractTokenKeyword(ref CSharpParserModel parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleSealedTokenKeyword(ref CSharpParserModel parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleIfTokenKeyword(ref CSharpParserModel parserModel)
    {
        var ifTokenKeyword = parserModel.TokenWalker.Consume();
        
        var openParenthesisToken = parserModel.TokenWalker.Match(SyntaxKind.OpenParenthesisToken);
        if (openParenthesisToken.IsFabricated)
            return;

        parserModel.ExpressionList.Add((SyntaxKind.CloseParenthesisToken, null));
        _ = ParseExpressions.ParseExpression(ref parserModel);
        
        var closeParenthesisToken = parserModel.TokenWalker.Match(SyntaxKind.CloseParenthesisToken);

        var ifStatementNode = new IfStatementNode(
            ifTokenKeyword,
            default);
        
        parserModel.NewScopeAndBuilderFromOwner(
            ifStatementNode,
            parserModel.TokenWalker.Current.TextSpan);
        
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenBraceToken)
            parserModel.CurrentCodeBlockOwner.IsImplicitOpenCodeBlockTextSpan = true;
    }

    public static void HandleUsingTokenKeyword(ref CSharpParserModel parserModel)
    {
        var usingKeywordToken = parserModel.TokenWalker.Consume();
        
        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenParenthesisToken)
        {
            HandleUsingCodeBlockOwner(ref usingKeywordToken, ref parserModel);
        }
        else if (parserModel.CurrentCodeBlockOwner.SyntaxKind == SyntaxKind.GlobalCodeBlockNode ||
                 parserModel.CurrentCodeBlockOwner.SyntaxKind == SyntaxKind.NamespaceStatementNode)
        {
            var namespaceIdentifierToken = ParseOthers.HandleNamespaceIdentifier(ref parserModel, isNamespaceStatement: false);
    
            if (!namespaceIdentifierToken.ConstructorWasInvoked)
            {
                // parserModel.Compilation.DiagnosticBag.ReportTodoException(usingKeywordToken.TextSpan, "Expected a namespace identifier.");
                return;
            }
            
            parserModel.BindUsingStatementTuple(usingKeywordToken, namespaceIdentifierToken);
        }
        else
        {
            // Ignore the 'using' keyword in this scenario for now.
        }
    }
    
    public static void HandleUsingCodeBlockOwner(ref SyntaxToken usingKeywordToken, ref CSharpParserModel parserModel)
    {
        var openParenthesisToken = parserModel.TokenWalker.Consume();
        
        var usingStatementNode = new UsingStatementCodeBlockNode(
            usingKeywordToken,
            default);
            
        parserModel.NewScopeAndBuilderFromOwner(
            usingStatementNode,
            parserModel.TokenWalker.Current.TextSpan);
            
        var successParse = ParseExpressions.TryParseVariableDeclarationNode(ref parserModel, out var variableDeclarationNode);
        if (successParse)
        {
            ParseTokens.HandleMultiVariableDeclaration(variableDeclarationNode, ref parserModel);
            var openParenthesisCounter = 1;
            while (true)
            {
                if (parserModel.TokenWalker.IsEof)
                    break;
            
                if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenParenthesisToken)
                {
                    ++openParenthesisCounter;
                }
                else if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CloseParenthesisToken)
                {
                    if (--openParenthesisCounter <= 0)
                        break;
                }
            
                _ = parserModel.TokenWalker.Consume();
            }
        }
        else
        {
            parserModel.ExpressionList.Add((SyntaxKind.CloseParenthesisToken, null));
            _ = ParseExpressions.ParseExpression(ref parserModel);
        }
        
        var closeParenthesisToken = parserModel.TokenWalker.Match(SyntaxKind.CloseParenthesisToken);
        
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenBraceToken)
            parserModel.CurrentCodeBlockOwner.IsImplicitOpenCodeBlockTextSpan = true;
    }

    public static void HandleInterfaceTokenKeyword(ref CSharpParserModel parserModel)
    {
        HandleStorageModifierTokenKeyword(ref parserModel);
    }

    /// <summary>
    /// Example:
    /// public class MyClass { }
    ///              ^
    ///
    /// Given the example the 'MyClass' is the next token
    /// upon invocation of this method.
    ///
    /// Invocation of this method implies the current token was
    /// class, interface, struct, etc...
    /// </summary>
    public static void HandleStorageModifierTokenKeyword(ref CSharpParserModel parserModel)
    {
        var storageModifierToken = parserModel.TokenWalker.Consume();
        
        // Given: public partial class MyClass { }
        // Then: partial
        var hasPartialModifier = false;
        if (parserModel.StatementBuilder.TryPeek(out var syntax) && syntax is SyntaxToken syntaxToken)
        {
            if (syntaxToken.SyntaxKind == SyntaxKind.PartialTokenContextualKeyword)
            {
                _ = parserModel.StatementBuilder.Pop();
                hasPartialModifier = true;
            }
        }
    
        // TODO: Fix; the code that parses the accessModifierKind is a mess
        //
        // Given: public class MyClass { }
        // Then: public
        var accessModifierKind = AccessModifierKind.Public;
        if (parserModel.StatementBuilder.TryPeek(out syntax) && syntax is SyntaxToken firstSyntaxToken)
        {
            var firstOutput = UtilityApi.GetAccessModifierKindFromToken(firstSyntaxToken);

            if (firstOutput != AccessModifierKind.None)
            {
                _ = parserModel.StatementBuilder.Pop();
                accessModifierKind = firstOutput;

                // Given: protected internal class MyClass { }
                // Then: protected internal
                if (parserModel.StatementBuilder.TryPeek(out syntax) && syntax is SyntaxToken secondSyntaxToken)
                {
                    var secondOutput = UtilityApi.GetAccessModifierKindFromToken(secondSyntaxToken);

                    if (secondOutput != AccessModifierKind.None)
                    {
                        _ = parserModel.StatementBuilder.Pop();

                        if ((firstOutput.ToString().ToLower() == "protected" &&
                                secondOutput.ToString().ToLower() == "internal") ||
                            (firstOutput.ToString().ToLower() == "internal" &&
                                secondOutput.ToString().ToLower() == "protected"))
                        {
                            accessModifierKind = AccessModifierKind.ProtectedInternal;
                        }
                        else if ((firstOutput.ToString().ToLower() == "private" &&
                                    secondOutput.ToString().ToLower() == "protected") ||
                                (firstOutput.ToString().ToLower() == "protected" &&
                                    secondOutput.ToString().ToLower() == "private"))
                        {
                            accessModifierKind = AccessModifierKind.PrivateProtected;
                        }
                        // else use the firstOutput.
                    }
                }
            }
        }
    
        // TODO: Fix nullability spaghetti code
        var storageModifierKind = UtilityApi.GetStorageModifierKindFromToken(storageModifierToken);
        if (storageModifierKind == StorageModifierKind.None)
            return;
        if (storageModifierKind == StorageModifierKind.Record &&
            parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.StructTokenKeyword)
        {
            var structKeywordToken = parserModel.TokenWalker.Consume();
            storageModifierKind = StorageModifierKind.RecordStruct;
        }
    
        // Given: public class MyClass<T> { }
        // Then: MyClass
        SyntaxToken identifierToken;
        // Retrospective: What is the purpose of this 'if (contextualKeyword) logic'?
        // Response: maybe it is because 'var' contextual keyword is allowed to be a class name?
        if (UtilityApi.IsContextualKeywordSyntaxKind(parserModel.TokenWalker.Current.SyntaxKind))
        {
            var contextualKeywordToken = parserModel.TokenWalker.Consume();
            // Take the contextual keyword as an identifier
            identifierToken = new SyntaxToken(SyntaxKind.IdentifierToken, contextualKeywordToken.TextSpan);
        }
        else
        {
            identifierToken = parserModel.TokenWalker.Match(SyntaxKind.IdentifierToken);
        }

        // Given: public class MyClass<T> { }
        // Then: <T>
        (SyntaxToken OpenAngleBracketToken, int IndexGenericParameterEntryList, int CountGenericParameterEntryList, SyntaxToken CloseAngleBracketToken) genericParameterListing = default;
        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenAngleBracketToken)
            genericParameterListing = ParseTypes.HandleGenericParameters(ref parserModel);

        var typeDefinitionNode = new TypeDefinitionNode(
            accessModifierKind,
            hasPartialModifier,
            storageModifierKind,
            identifierToken,
            genericParameterListing.OpenAngleBracketToken,
            genericParameterListing.IndexGenericParameterEntryList,
            genericParameterListing.CountGenericParameterEntryList,
            genericParameterListing.CloseAngleBracketToken,
            openParenthesisToken: default,
            indexFunctionArgumentEntryList: -1,
            countFunctionArgumentEntryList: 0,
            closeParenthesisToken: default,
            inheritedTypeReference: TypeFacts.NotApplicable.ToTypeReference(),
            namespaceName: parserModel.GetTextSpanText(parserModel.CurrentNamespaceStatementNode.IdentifierToken.TextSpan),
            parserModel.ResourceUri);
        
        if (typeDefinitionNode.HasPartialModifier)
        {
            if (parserModel.TryGetTypeDefinitionHierarchically(
                    parserModel.ResourceUri,
                    parserModel.Compilation,
                    parserModel.CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
                    parserModel.GetTextSpanText(identifierToken.TextSpan),
                    out TypeDefinitionNode innerTypeDefinitionNode))
            {
                typeDefinitionNode.IndexPartialTypeDefinition = innerTypeDefinitionNode.IndexPartialTypeDefinition;
            }
        }
        
        parserModel.BindTypeDefinitionNode(typeDefinitionNode);
        parserModel.BindTypeIdentifier(identifierToken);
        
        parserModel.StatementBuilder.ChildList.Add(typeDefinitionNode);
        
        parserModel.NewScopeAndBuilderFromOwner(
            typeDefinitionNode,
            parserModel.TokenWalker.Current.TextSpan);
            
        parserModel.CurrentCodeBlockOwner.IsImplicitOpenCodeBlockTextSpan = false;
        
        if (typeDefinitionNode.HasPartialModifier)
        {
            if (typeDefinitionNode.IndexPartialTypeDefinition == -1)
            {
                if (parserModel.Binder.__CompilationUnitMap.TryGetValue(parserModel.ResourceUri, out var previousCompilationUnit))
                {
                    if (typeDefinitionNode.Unsafe_ParentIndexKey < previousCompilationUnit.CountCodeBlockOwnerList)
                    {
                        var previousParent = parserModel.Binder.CodeBlockOwnerList[previousCompilationUnit.IndexCodeBlockOwnerList + typeDefinitionNode.Unsafe_ParentIndexKey];
                        var currentParent = parserModel.GetParent(typeDefinitionNode, parserModel.Compilation);
                        
                        if (currentParent.SyntaxKind == previousParent.SyntaxKind &&
                            parserModel.Binder.GetIdentifierText(currentParent, parserModel.ResourceUri, parserModel.Compilation) == parserModel.Binder.GetIdentifierText(previousParent, parserModel.ResourceUri, previousCompilationUnit))
                        {
                            // All the existing entires will be "emptied"
                            // so don't both with checking whether the arguments are the same here.
                            //
                            // All that matters is that they're put in the same "method group".
                            //
                            var binder = parserModel.Binder;
                            
                            // TODO: Cannot use ref, out, or in...
                            var compilation = parserModel.Compilation;
                            
                            ISyntaxNode? previousNode = null;
                            
                            for (int i = previousCompilationUnit.IndexCodeBlockOwnerList; i < previousCompilationUnit.IndexCodeBlockOwnerList + previousCompilationUnit.CountCodeBlockOwnerList; i++)
                            {
                                var x = parserModel.Binder.CodeBlockOwnerList[i];
                                
                                if (x.Unsafe_ParentIndexKey == previousParent.Unsafe_SelfIndexKey &&
                                    x.SyntaxKind == SyntaxKind.TypeDefinitionNode &&
                                    binder.GetIdentifierText(x, parserModel.ResourceUri, previousCompilationUnit) == binder.GetIdentifierText(typeDefinitionNode, parserModel.ResourceUri, compilation))
                                {
                                    previousNode = x;
                                    break;
                                }
                            }
                            
                            if (previousNode is not null)
                            {
                                var previousTypeDefinitionNode = (TypeDefinitionNode)previousNode;
                                typeDefinitionNode.IndexPartialTypeDefinition = previousTypeDefinitionNode.IndexPartialTypeDefinition;
                            }
                        }
                    }
                }
            }
            
            if (parserModel.ClearedPartialDefinitionHashSet.Add(parserModel.GetTextSpanText(identifierToken.TextSpan)) &&
                typeDefinitionNode.IndexPartialTypeDefinition != -1)
            {
                // Partial definitions of the same type from the same ResourceUri are made contiguous.
                var seenResourceUri = false;
                
                int positionExclusive = typeDefinitionNode.IndexPartialTypeDefinition;
                while (positionExclusive < parserModel.Binder.PartialTypeDefinitionList.Count)
                {
                    if (parserModel.Binder.PartialTypeDefinitionList[positionExclusive].IndexStartGroup == typeDefinitionNode.IndexPartialTypeDefinition)
                    {
                        if (parserModel.Binder.PartialTypeDefinitionList[positionExclusive].ResourceUri == parserModel.ResourceUri)
                        {
                            seenResourceUri = true;
                        
                            var partialTypeDefinitionEntry = parserModel.Binder.PartialTypeDefinitionList[positionExclusive];
                            partialTypeDefinitionEntry.ScopeIndexKey = -1;
                            parserModel.Binder.PartialTypeDefinitionList[positionExclusive] = partialTypeDefinitionEntry;
                            
                            positionExclusive++;
                        }
                        else
                        {
                            if (seenResourceUri)
                                break;
                            positionExclusive++;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        
        if (storageModifierKind == StorageModifierKind.Enum)
        {
            ParseTypes.HandleEnumDefinitionNode(typeDefinitionNode, ref parserModel);
            return;
        }
    
        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenParenthesisToken)
        {
            ParseTypes.HandlePrimaryConstructorDefinition(
                typeDefinitionNode,
                ref parserModel);
        }
        
        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.ColonToken)
        {
            _ = parserModel.TokenWalker.Consume(); // Consume the ColonToken
            var inheritedTypeClauseNode = parserModel.TokenWalker.MatchTypeClauseNode(ref parserModel);
            // parserModel.BindTypeClauseNode(inheritedTypeClauseNode);
            typeDefinitionNode.SetInheritedTypeReference(new TypeReference(inheritedTypeClauseNode));
            parserModel.Return_TypeClauseNode(inheritedTypeClauseNode);
            
            while (!parserModel.TokenWalker.IsEof)
            {
                if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CommaToken)
                {
                    _ = parserModel.TokenWalker.Consume(); // Consume the CommaToken
                
                    var consumeCounter = parserModel.TokenWalker.ConsumeCounter;
                    
                    _ = parserModel.TokenWalker.MatchTypeClauseNode(ref parserModel);
                    // parserModel.BindTypeClauseNode();
                    
                    if (consumeCounter == parserModel.TokenWalker.ConsumeCounter)
                        break;
                }
                else
                {
                    break;
                }
            }
        }
        
        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.WhereTokenContextualKeyword)
        {
            parserModel.ExpressionList.Add((SyntaxKind.OpenBraceToken, null));
            var expressionNode = ParseExpressions.ParseExpression(ref parserModel);
        }
        
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenBraceToken)
            parserModel.CurrentCodeBlockOwner.IsImplicitOpenCodeBlockTextSpan = true;
    
        if (typeDefinitionNode.HasPartialModifier)
            HandlePartialTypeDefinition(typeDefinitionNode, ref parserModel);
    }
    
    public static void HandlePartialTypeDefinition(TypeDefinitionNode typeDefinitionNode, ref CSharpParserModel parserModel)
    {
        var wroteToExistingSlot = false;
    
        int indexForInsertion;
    
        if (typeDefinitionNode.IndexPartialTypeDefinition == -1)
        {
            typeDefinitionNode.IndexPartialTypeDefinition = parserModel.Binder.PartialTypeDefinitionList.Count;
            indexForInsertion = typeDefinitionNode.IndexPartialTypeDefinition;
        }
        else
        {
            var seenResourceUri = false;
        
            int positionExclusive = typeDefinitionNode.IndexPartialTypeDefinition;
            while (positionExclusive < parserModel.Binder.PartialTypeDefinitionList.Count)
            {
                if (parserModel.Binder.PartialTypeDefinitionList[positionExclusive].IndexStartGroup == typeDefinitionNode.IndexPartialTypeDefinition)
                {
                    if (parserModel.Binder.PartialTypeDefinitionList[positionExclusive].ResourceUri == parserModel.ResourceUri)
                    {
                        if (parserModel.Binder.PartialTypeDefinitionList[positionExclusive].ScopeIndexKey == -1)
                        {
                            var partialTypeDefinitionEntry = parserModel.Binder.PartialTypeDefinitionList[positionExclusive];
                            partialTypeDefinitionEntry.ScopeIndexKey = typeDefinitionNode.Unsafe_SelfIndexKey;
                            parserModel.Binder.PartialTypeDefinitionList[positionExclusive] = partialTypeDefinitionEntry;
                            wroteToExistingSlot = true;
                            break;
                        }
                        
                        seenResourceUri = true;
                        positionExclusive++;
                    }
                    else
                    {
                        if (seenResourceUri)
                            break;
                    
                        positionExclusive++;
                    }
                }
                else
                {
                    break;
                }
            }
            
            indexForInsertion = positionExclusive;
        }
        
        if (!wroteToExistingSlot)
        {
            parserModel.Binder.PartialTypeDefinitionList.Insert(
                indexForInsertion,
                new PartialTypeDefinitionEntry(
                    typeDefinitionNode.ResourceUri,
                    typeDefinitionNode.IndexPartialTypeDefinition,
                    typeDefinitionNode.Unsafe_SelfIndexKey));
        
            int positionExclusive = indexForInsertion + 1;
            int lastSeenIndexStartGroup = typeDefinitionNode.IndexPartialTypeDefinition;
            while (positionExclusive < parserModel.Binder.PartialTypeDefinitionList.Count)
            {
                if (parserModel.Binder.PartialTypeDefinitionList[positionExclusive].IndexStartGroup != typeDefinitionNode.IndexPartialTypeDefinition)
                {
                    var partialTypeDefinitionEntry = parserModel.Binder.PartialTypeDefinitionList[positionExclusive];
                    
                    if (lastSeenIndexStartGroup != partialTypeDefinitionEntry.IndexStartGroup)
                    {
                        lastSeenIndexStartGroup = partialTypeDefinitionEntry.IndexStartGroup;
                        
                        if (parserModel.Binder.__CompilationUnitMap.TryGetValue(partialTypeDefinitionEntry.ResourceUri, out var innerCompilationUnit))
                        {
                            ((TypeDefinitionNode)parserModel.Binder.CodeBlockOwnerList[innerCompilationUnit.IndexCodeBlockOwnerList + partialTypeDefinitionEntry.ScopeIndexKey]).IndexPartialTypeDefinition = partialTypeDefinitionEntry.IndexStartGroup + 1;
                        }
                    }
                    
                    partialTypeDefinitionEntry.IndexStartGroup = partialTypeDefinitionEntry.IndexStartGroup + 1;
                    parserModel.Binder.PartialTypeDefinitionList[positionExclusive] = partialTypeDefinitionEntry;
                }
                
                positionExclusive++;
            }
        }
    }

    public static void HandleClassTokenKeyword(ref CSharpParserModel parserModel)
    {
        HandleStorageModifierTokenKeyword(ref parserModel);
    }

    public static void HandleNamespaceTokenKeyword(ref CSharpParserModel parserModel)
    {
        var namespaceKeywordToken = parserModel.TokenWalker.Consume();
        
        var namespaceIdentifier = ParseOthers.HandleNamespaceIdentifier(ref parserModel, isNamespaceStatement: true);

        if (!namespaceIdentifier.ConstructorWasInvoked)
        {
            // parserModel.Compilation.DiagnosticBag.ReportTodoException(namespaceKeywordToken.TextSpan, "Expected a namespace identifier.");
            return;
        }

        var namespaceStatementNode = new NamespaceStatementNode(
            namespaceKeywordToken,
            (SyntaxToken)namespaceIdentifier,
            default,
            parserModel.ResourceUri);

        parserModel.SetCurrentNamespaceStatementNode(namespaceStatementNode);
        
        parserModel.NewScopeAndBuilderFromOwner(
            namespaceStatementNode,
            parserModel.TokenWalker.Current.TextSpan);
        
        // Do not set 'IsImplicitOpenCodeBlockTextSpan' for namespace file scoped.
    }

    public static void HandleReturnTokenKeyword(ref CSharpParserModel parserModel)
    {
        var returnKeywordToken = parserModel.TokenWalker.Consume();
        var expressionNode = ParseExpressions.ParseExpression(ref parserModel);
        var returnStatementNode = new ReturnStatementNode(returnKeywordToken, expressionNode);
        
        parserModel.StatementBuilder.ChildList.Add(returnStatementNode);
    }
}
