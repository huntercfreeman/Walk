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
            closeParenthesisToken: default);
        
        parserModel.RegisterScopeAndOwner(
            new Scope(
                ScopeDirectionKind.Down,
                scope_StartInclusiveIndex: textSpan.StartInclusiveIndex,
                scope_EndExclusiveIndex: -1,
                codeBlock_StartInclusiveIndex: textSpan.StartInclusiveIndex,
                codeBlock_EndExclusiveIndex: -1,
                parentScopeOffset: parserModel.CurrentScopeOffset,
                selfScopeOffset: parserModel.Binder.ScopeList.Count,
                nodeOffset: parserModel.Binder.NodeList.Count,
                permitCodeBlockParsing: false,
                isImplicitOpenCodeBlockTextSpan: false,
                returnTypeReference: CSharpFacts.Types.Void.ToTypeReference(),
                syntaxKind: catchNode.SyntaxKind),
            catchNode);
        
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
            parserModel.SetCurrentScope_IsImplicitOpenCodeBlockTextSpan_True();
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
        
        parserModel.RegisterScopeAndOwner(
            doWhileStatementNode,
            parserModel.TokenWalker.Current.TextSpan);
        
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenBraceToken)
            parserModel.SetCurrentScope_IsImplicitOpenCodeBlockTextSpan_True();
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
        
        var ifStatementNode = new IfStatementNode(elseTokenKeyword);
        
        parserModel.RegisterScopeAndOwner(
            ifStatementNode,
            parserModel.TokenWalker.Current.TextSpan);
        
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenBraceToken)
            parserModel.SetCurrentScope_IsImplicitOpenCodeBlockTextSpan_True();
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
        parserModel.StatementBuilder.MostRecentNode = expressionNode;
    }

    public static void HandleFinallyTokenKeyword(ref CSharpParserModel parserModel)
    {
        var finallyKeywordToken = parserModel.TokenWalker.Consume();
        
        // TryStatementNode? tryStatementNode = null;
        
        var finallyNode = new TryStatementFinallyNode(
            // tryStatementNode,
            finallyKeywordToken);
    
        // This was done with CSharpParserModel's SyntaxStack, but that property is now being removed. A different way to accomplish this needs to be done. (2025-02-06)
        // tryStatementNode.SetTryStatementFinallyNode(finallyNode);
        
        parserModel.RegisterScopeAndOwner(
            finallyNode,
            parserModel.TokenWalker.Current.TextSpan);
    
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenBraceToken)
            parserModel.SetCurrentScope_IsImplicitOpenCodeBlockTextSpan_True();
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
            closeParenthesisToken: default);
            
        parserModel.RegisterScopeAndOwner(
            forStatementNode,
            parserModel.TokenWalker.Current.TextSpan);
        
        parserModel.CurrentCodeBlockOwner.IsImplicitOpenCodeBlockTextSpan = false;
        
        for (int i = 0; i < 3; i++)
        {
            parserModel.ExpressionList.Add((SyntaxKind.CloseParenthesisToken, null));
            var expression = ParseExpressions.ParseExpression(ref parserModel);
            
            if (expression.SyntaxKind == SyntaxKind.VariableReferenceNode)
            {
                parserModel.Return_VariableReferenceNode((VariableReferenceNode)expression);
            }
            else if (expression.SyntaxKind == SyntaxKind.FunctionInvocationNode)
            {
                parserModel.Return_FunctionInvocationNode((FunctionInvocationNode)expression);
            }
            else if (expression.SyntaxKind == SyntaxKind.BinaryExpressionNode)
            {
                parserModel.Return_BinaryExpressionNode((BinaryExpressionNode)expression);
            }
            
            var statementDelimiterToken = parserModel.TokenWalker.Match(SyntaxKind.StatementDelimiterToken);
            
            if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CloseParenthesisToken)
                break;
        }
        
        var closeParenthesisToken = parserModel.TokenWalker.Match(SyntaxKind.CloseParenthesisToken);
        
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenBraceToken)
            parserModel.SetCurrentScope_IsImplicitOpenCodeBlockTextSpan_True();
    }

    public static void HandleForeachTokenKeyword(ref CSharpParserModel parserModel)
    {
        var foreachKeywordToken = parserModel.TokenWalker.Consume();
        
        var openParenthesisToken = parserModel.TokenWalker.Match(SyntaxKind.OpenParenthesisToken);
        
        var foreachStatementNode = new ForeachStatementNode(
            foreachKeywordToken,
            openParenthesisToken,
            inKeywordToken: default,
            closeParenthesisToken: default);
        
        parserModel.RegisterScopeAndOwner(
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
            parserModel.Binder.CSharpCompilerService.SafeCompareText(parserModel.ResourceUri.Value, "var", variableDeclarationNode.TypeReference.TypeIdentifierToken.TextSpan))
        {
            if (enumerable.ResultTypeReference.CountGenericParameterEntryList == 1)
                variableDeclarationNode.SetImplicitTypeReference(
                    parserModel.Binder.GenericParameterEntryList[enumerable.ResultTypeReference.IndexGenericParameterEntryList].TypeReference);
        }
        
        if (enumerable.SyntaxKind == SyntaxKind.VariableReferenceNode)
        {
            parserModel.Return_VariableReferenceNode((VariableReferenceNode)enumerable);
        }
        else if (enumerable.SyntaxKind == SyntaxKind.FunctionInvocationNode)
        {
            parserModel.Return_FunctionInvocationNode((FunctionInvocationNode)enumerable);
        }
        
        var closeParenthesisToken = parserModel.TokenWalker.Match(SyntaxKind.CloseParenthesisToken);
            
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenBraceToken)
            parserModel.SetCurrentScope_IsImplicitOpenCodeBlockTextSpan_True();
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
        var expression = ParseExpressions.ParseExpression(ref parserModel);
        
        if (expression.SyntaxKind == SyntaxKind.VariableReferenceNode)
        {
            parserModel.Return_VariableReferenceNode((VariableReferenceNode)expression);
        }
        
        var closeParenthesisToken = parserModel.TokenWalker.Match(SyntaxKind.CloseParenthesisToken);
        
        var lockStatementNode = new LockStatementNode(
            lockKeywordToken,
            openParenthesisToken,
            closeParenthesisToken);
            
        parserModel.RegisterScopeAndOwner(
            lockStatementNode,
            parserModel.TokenWalker.Current.TextSpan);
    
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenBraceToken)
            parserModel.SetCurrentScope_IsImplicitOpenCodeBlockTextSpan_True();
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
        
        if (expressionNode.SyntaxKind == SyntaxKind.VariableReferenceNode)
        {
            parserModel.Return_VariableReferenceNode((VariableReferenceNode)expressionNode);
        }
        else if (expressionNode.SyntaxKind == SyntaxKind.FunctionInvocationNode)
        {
            parserModel.Return_FunctionInvocationNode((FunctionInvocationNode)expressionNode);
        }
        
        var closeParenthesisToken = parserModel.TokenWalker.Match(SyntaxKind.CloseParenthesisToken);
        
        var switchStatementNode = new SwitchStatementNode(
            switchKeywordToken,
            openParenthesisToken,
            expressionNode,
            closeParenthesisToken);
            
        parserModel.RegisterScopeAndOwner(
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
        parserModel.StatementBuilder.MostRecentNode = expressionNode;
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
            tryKeywordToken);
            
        // tryStatementNode.TryNode = tryStatementTryNode;
            
        // parserModel.CurrentCodeBlockBuilder.AddChild(tryStatementTryNode);
        
        parserModel.RegisterScopeAndOwner(
            tryStatementTryNode,
            parserModel.TokenWalker.Current.TextSpan);
    
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenBraceToken)
            parserModel.SetCurrentScope_IsImplicitOpenCodeBlockTextSpan_True();
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
        var expression = ParseExpressions.ParseExpression(ref parserModel);
        
        if (expression.SyntaxKind == SyntaxKind.VariableReferenceNode)
        {
            parserModel.Return_VariableReferenceNode((VariableReferenceNode)expression);
        }
        else if (expression.SyntaxKind == SyntaxKind.FunctionInvocationNode)
        {
            parserModel.Return_FunctionInvocationNode((FunctionInvocationNode)expression);
        }
        else if (expression.SyntaxKind == SyntaxKind.BinaryExpressionNode)
        {
            parserModel.Return_BinaryExpressionNode((BinaryExpressionNode)expression);
        }
        
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
            closeParenthesisToken);
            
        parserModel.RegisterScopeAndOwner(
            whileStatementNode,
            parserModel.TokenWalker.Current.TextSpan);
    
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenBraceToken)
            parserModel.SetCurrentScope_IsImplicitOpenCodeBlockTextSpan_True();
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
            parserModel.StatementBuilder.MostRecentNode = expressionNode;
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
        var expression = ParseExpressions.ParseExpression(ref parserModel);
        
        if (expression.SyntaxKind == SyntaxKind.VariableReferenceNode)
        {
            parserModel.Return_VariableReferenceNode((VariableReferenceNode)expression);
        }
        else if (expression.SyntaxKind == SyntaxKind.FunctionInvocationNode)
        {
            parserModel.Return_FunctionInvocationNode((FunctionInvocationNode)expression);
        }
        else if (expression.SyntaxKind == SyntaxKind.BinaryExpressionNode)
        {
            parserModel.Return_BinaryExpressionNode((BinaryExpressionNode)expression);
        }
        
        var closeParenthesisToken = parserModel.TokenWalker.Match(SyntaxKind.CloseParenthesisToken);

        var ifStatementNode = new IfStatementNode(ifTokenKeyword);
        
        parserModel.RegisterScopeAndOwner(
            ifStatementNode,
            parserModel.TokenWalker.Current.TextSpan);
        
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenBraceToken)
            parserModel.SetCurrentScope_IsImplicitOpenCodeBlockTextSpan_True();
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
        
        var usingStatementNode = new UsingStatementCodeBlockNode(usingKeywordToken);
            
        parserModel.RegisterScopeAndOwner(
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
            parserModel.SetCurrentScope_IsImplicitOpenCodeBlockTextSpan_True();
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
        if (parserModel.StatementBuilder.TryPeek(out var token))
        {
            if (token.SyntaxKind == SyntaxKind.PartialTokenContextualKeyword)
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
        if (parserModel.StatementBuilder.TryPeek(out var firstSyntaxToken))
        {
            var firstOutput = UtilityApi.GetAccessModifierKindFromToken(firstSyntaxToken);

            if (firstOutput != AccessModifierKind.None)
            {
                _ = parserModel.StatementBuilder.Pop();
                accessModifierKind = firstOutput;

                // Given: protected internal class MyClass { }
                // Then: protected internal
                if (parserModel.StatementBuilder.TryPeek(out var secondSyntaxToken))
                {
                    var secondOutput = UtilityApi.GetAccessModifierKindFromToken(secondSyntaxToken);

                    if (secondOutput != AccessModifierKind.None)
                    {
                        _ = parserModel.StatementBuilder.Pop();

                        if ((firstOutput == AccessModifierKind.Protected && secondOutput == AccessModifierKind.Internal) ||
                            (firstOutput == AccessModifierKind.Internal && secondOutput == AccessModifierKind.Protected))
                        {
                            accessModifierKind = AccessModifierKind.ProtectedInternal;
                        }
                        else if ((firstOutput == AccessModifierKind.Private && secondOutput == AccessModifierKind.Protected) ||
                                (firstOutput == AccessModifierKind.Protected && secondOutput == AccessModifierKind.Private))
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
        if (storageModifierKind == StorageModifierKind.Record)
        {
            if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.ClassTokenKeyword)
            {
                var classKeywordToken = parserModel.TokenWalker.Consume();
                storageModifierKind = StorageModifierKind.RecordClass;
            }
            else if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.StructTokenKeyword)
            {
                var structKeywordToken = parserModel.TokenWalker.Consume();
                storageModifierKind = StorageModifierKind.RecordStruct;
            }
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
            // NOTE: You do indeed use the current compilation unit here...
            // ...there is a different step that checks the previous.
            if (parserModel.TryGetTypeDefinitionHierarchically(
                    parserModel.ResourceUri,
                    parserModel.Compilation,
                    parserModel.CurrentCodeBlockOwner.Unsafe_SelfIndexKey,
                    parserModel.ResourceUri,
                    identifierToken.TextSpan,
                    out TypeDefinitionNode? previousTypeDefinitionNode))
            {
                typeDefinitionNode.IndexPartialTypeDefinition = previousTypeDefinitionNode.IndexPartialTypeDefinition;
            }
        }
        
        parserModel.BindTypeDefinitionNode(typeDefinitionNode);
        parserModel.BindTypeIdentifier(identifierToken);
        
        parserModel.StatementBuilder.MostRecentNode = typeDefinitionNode;
        
        parserModel.RegisterScopeAndOwner(
            typeDefinitionNode,
            parserModel.TokenWalker.Current.TextSpan);
            
        parserModel.CurrentCodeBlockOwner.IsImplicitOpenCodeBlockTextSpan = false;
        
        if (typeDefinitionNode.HasPartialModifier)
        {
            if (typeDefinitionNode.IndexPartialTypeDefinition == -1)
            {
                if (parserModel.Binder.__CompilationUnitMap.TryGetValue(parserModel.ResourceUri, out var previousCompilationUnit))
                {
                    if (typeDefinitionNode.ParentScopeOffset < previousCompilationUnit.ScopeCount)
                    {
                        var previousParent = parserModel.Binder.CodeBlockOwnerList[previousCompilationUnit.ScopeIndex + typeDefinitionNode.ParentScopeOffset];
                        var currentParent = parserModel.GetParent(typeDefinitionNode, parserModel.Compilation);
                        
                        if (currentParent.SyntaxKind == previousParent.SyntaxKind)
                        {
                            var currentParentIdentifierText = parserModel.Binder.GetIdentifierText(currentParent, parserModel.ResourceUri, parserModel.Compilation);
                            
                            if (currentParentIdentifierText is not null &&
                                currentParentIdentifierText == parserModel.Binder.GetIdentifierText(previousParent, parserModel.ResourceUri, previousCompilationUnit))
                            {
                                // All the existing entires will be "emptied"
                                // so don't both with checking whether the arguments are the same here.
                                //
                                // All that matters is that they're put in the same "group".
                                //
                                var binder = parserModel.Binder;
                                
                                // TODO: Cannot use ref, out, or in...
                                var compilation = parserModel.Compilation;
                                
                                ISyntaxNode? previousNode = null;
                                
                                for (int i = previousCompilationUnit.ScopeIndex; i < previousCompilationUnit.ScopeIndex + previousCompilationUnit.ScopeCount; i++)
                                {
                                    var x = parserModel.Binder.CodeBlockOwnerList[i];
                                    
                                    if (x.ParentScopeOffset == previousParent.Unsafe_SelfIndexKey &&
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
                            partialTypeDefinitionEntry.ScopeOffset = -1;
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
            var inheritedTypeClauseNode = ParseTypes.MatchTypeClause(ref parserModel);
            // parserModel.BindTypeClauseNode(inheritedTypeClauseNode);
            typeDefinitionNode.SetInheritedTypeReference(new TypeReference(inheritedTypeClauseNode));
            parserModel.Return_TypeClauseNode(inheritedTypeClauseNode);
            
            while (!parserModel.TokenWalker.IsEof)
            {
                if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CommaToken)
                {
                    _ = parserModel.TokenWalker.Consume(); // Consume the CommaToken
                
                    var consumeCounter = parserModel.TokenWalker.ConsumeCounter;
                    
                    _ = ParseTypes.MatchTypeClause(ref parserModel);
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
            parserModel.SetCurrentScope_IsImplicitOpenCodeBlockTextSpan_True();
    
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
                        if (parserModel.Binder.PartialTypeDefinitionList[positionExclusive].ScopeOffset == -1)
                        {
                            var partialTypeDefinitionEntry = parserModel.Binder.PartialTypeDefinitionList[positionExclusive];
                            partialTypeDefinitionEntry.ScopeOffset = typeDefinitionNode.SelfScopeOffset;
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
                    typeDefinitionNode.SelfScopeOffset));
        
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
                            ((TypeDefinitionNode)parserModel.Binder.ScopeList[innerCompilationUnit.ScopeIndex + partialTypeDefinitionEntry.ScopeOffset]).IndexPartialTypeDefinition = partialTypeDefinitionEntry.IndexStartGroup + 1;
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
            parserModel.ResourceUri);

        parserModel.SetCurrentNamespaceStatementNode(namespaceStatementNode);
        
        parserModel.RegisterScopeAndOwner(
            namespaceStatementNode,
            parserModel.TokenWalker.Current.TextSpan);
        
        // Do not set 'IsImplicitOpenCodeBlockTextSpan' for namespace file scoped.
    }

    public static void HandleReturnTokenKeyword(ref CSharpParserModel parserModel)
    {
        var returnKeywordToken = parserModel.TokenWalker.Consume();
        var expressionNode = ParseExpressions.ParseExpression(ref parserModel);
        
        if (expressionNode.SyntaxKind == SyntaxKind.VariableReferenceNode)
        {
            parserModel.Return_VariableReferenceNode((VariableReferenceNode)expressionNode);
        }
        else if (expressionNode.SyntaxKind == SyntaxKind.FunctionInvocationNode)
        {
            parserModel.Return_FunctionInvocationNode((FunctionInvocationNode)expressionNode);
        }
        else if (expressionNode.SyntaxKind == SyntaxKind.ConstructorInvocationExpressionNode)
        {
            parserModel.Return_ConstructorInvocationExpressionNode((ConstructorInvocationExpressionNode)expressionNode);
        }
        else if (expressionNode.SyntaxKind == SyntaxKind.BinaryExpressionNode)
        {
            parserModel.Return_BinaryExpressionNode((BinaryExpressionNode)expressionNode);
        }
        
        // var returnStatementNode = new ReturnStatementNode(returnKeywordToken, expressionNode);
        
        // parserModel.StatementBuilder.ChildList.Add(returnStatementNode);
    }
}
