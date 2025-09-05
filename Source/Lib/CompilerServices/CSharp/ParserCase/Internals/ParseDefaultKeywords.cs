using Walk.Extensions.CompilerServices;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Enums;
using Walk.Extensions.CompilerServices.Syntax.NodeReferences;
using Walk.Extensions.CompilerServices.Syntax.NodeValues;

namespace Walk.CompilerServices.CSharp.ParserCase.Internals;

public class ParseDefaultKeywords
{
    public static void HandleAsTokenKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleBaseTokenKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleBoolTokenKeyword(ref CSharpParserState parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleBreakTokenKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleByteTokenKeyword(ref CSharpParserState parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleCaseTokenKeyword(ref CSharpParserState parserModel)
    {
        _ = parserModel.TokenWalker.Consume(); // caseKeyword

        parserModel.ExpressionList.Add((SyntaxKind.ColonToken, null));
        _ = ParseExpressions.ParseExpression(ref parserModel);
        _ = parserModel.TokenWalker.Match(SyntaxKind.ColonToken);
    }
    
    public static void HandleCatchTokenKeyword(ref CSharpParserState parserModel)
    {
        _ = parserModel.TokenWalker.Consume(); // catchKeywordToken
        _ = parserModel.TokenWalker.Match(SyntaxKind.OpenParenthesisToken);
        
        parserModel.RegisterScope(
            new Scope(
                ScopeDirectionKind.Down,
                scope_StartInclusiveIndex: parserModel.TokenWalker.Current.TextSpan.StartInclusiveIndex,
                scope_EndExclusiveIndex: -1,
                codeBlock_StartInclusiveIndex: -1,
                codeBlock_EndExclusiveIndex: -1,
                parentScopeSubIndex: parserModel.ScopeCurrentSubIndex,
                selfScopeSubIndex: parserModel.Compilation.ScopeLength,
                nodeSubIndex: -1,
                permitCodeBlockParsing: true,
                isImplicitOpenCodeBlockTextSpan: false,
                ownerSyntaxKind: SyntaxKind.TryStatementCatchNode),
            codeBlockOwner: null);
        
        parserModel.ExpressionList.Add((SyntaxKind.CloseParenthesisToken, null));
        _ = ParseExpressions.ParseExpression(ref parserModel);
        _ = parserModel.TokenWalker.Match(SyntaxKind.CloseParenthesisToken);

        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.WhenTokenContextualKeyword)
        {
            _ = parserModel.TokenWalker.Consume(); // WhenTokenContextualKeyword
            
            parserModel.ExpressionList.Add((SyntaxKind.OpenBraceToken, null));
            _ = ParseExpressions.ParseExpression(ref parserModel);
        }
        
        // Not valid C# -- catch requires brace deliminated code block --, but here for parser recovery.
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenBraceToken)
            parserModel.SetCurrentScope_IsImplicitOpenCodeBlockTextSpan(true);
    }

    public static void HandleCharTokenKeyword(ref CSharpParserState parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleCheckedTokenKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleConstTokenKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleContinueTokenKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleDecimalTokenKeyword(ref CSharpParserState parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleDefaultTokenKeyword(ref CSharpParserState parserModel)
    {
        // Switch statement default case.
        if (parserModel.TokenWalker.Next.SyntaxKind == SyntaxKind.ColonToken)
            _ = parserModel.TokenWalker.Consume();
        else
            _ = ParseExpressions.ParseExpression(ref parserModel);
    }

    public static void HandleDelegateTokenKeyword(ref CSharpParserState parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleDoTokenKeyword(ref CSharpParserState parserModel)
    {
        _ = parserModel.TokenWalker.Consume(); // doKeywordToken

        parserModel.RegisterScope(
        	new Scope(
        		ScopeDirectionKind.Down,
        		scope_StartInclusiveIndex: parserModel.TokenWalker.Current.TextSpan.StartInclusiveIndex,
        		scope_EndExclusiveIndex: -1,
        		codeBlock_StartInclusiveIndex: -1,
        		codeBlock_EndExclusiveIndex: -1,
        		parentScopeSubIndex: parserModel.ScopeCurrentSubIndex,
        		selfScopeSubIndex: parserModel.Compilation.ScopeLength,
        		nodeSubIndex: -1,
        		permitCodeBlockParsing: true,
        		isImplicitOpenCodeBlockTextSpan: false,
        		ownerSyntaxKind: SyntaxKind.DoWhileStatementNode),
            codeBlockOwner: null);
        
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenBraceToken)
            parserModel.SetCurrentScope_IsImplicitOpenCodeBlockTextSpan(true);
    }

    public static void HandleDoubleTokenKeyword(ref CSharpParserState parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleElseTokenKeyword(ref CSharpParserState parserModel)
    {
        if (parserModel.TokenWalker.Next.SyntaxKind == SyntaxKind.IfTokenKeyword)
        {
            parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
            return;
        }
    
        _ = parserModel.TokenWalker.Consume(); // elseTokenKeyword
            
        parserModel.RegisterScope(
        	new Scope(
        		ScopeDirectionKind.Down,
        		scope_StartInclusiveIndex: parserModel.TokenWalker.Current.TextSpan.StartInclusiveIndex,
        		scope_EndExclusiveIndex: -1,
        		codeBlock_StartInclusiveIndex: -1,
        		codeBlock_EndExclusiveIndex: -1,
        		parentScopeSubIndex: parserModel.ScopeCurrentSubIndex,
        		selfScopeSubIndex: parserModel.Compilation.ScopeLength,
        		nodeSubIndex: -1,
        		permitCodeBlockParsing: true,
        		isImplicitOpenCodeBlockTextSpan: false,
        		ownerSyntaxKind: SyntaxKind.IfStatementNode),
            codeBlockOwner: null);
        
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenBraceToken)
            parserModel.SetCurrentScope_IsImplicitOpenCodeBlockTextSpan(true);
    }

    public static void HandleEnumTokenKeyword(ref CSharpParserState parserModel)
    {
        HandleStorageModifierTokenKeyword(ref parserModel);
    }

    public static void HandleEventTokenKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleExplicitTokenKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleExternTokenKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleFalseTokenKeyword(ref CSharpParserState parserModel)
    {
        var expressionNode = ParseExpressions.ParseExpression(ref parserModel);
        parserModel.StatementBuilder.MostRecentNode = expressionNode;
    }

    public static void HandleFinallyTokenKeyword(ref CSharpParserState parserModel)
    {
        _ = parserModel.TokenWalker.Consume(); // finallyKeywordToken

        parserModel.RegisterScope(
            new Scope(
                ScopeDirectionKind.Down,
                scope_StartInclusiveIndex: parserModel.TokenWalker.Current.TextSpan.StartInclusiveIndex,
                scope_EndExclusiveIndex: -1,
                codeBlock_StartInclusiveIndex: -1,
                codeBlock_EndExclusiveIndex: -1,
                parentScopeSubIndex: parserModel.ScopeCurrentSubIndex,
                selfScopeSubIndex: parserModel.Compilation.ScopeLength,
                nodeSubIndex: -1,
                permitCodeBlockParsing: true,
                isImplicitOpenCodeBlockTextSpan: false,
                ownerSyntaxKind: SyntaxKind.TryStatementFinallyNode),
            codeBlockOwner: null);

        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenBraceToken)
            parserModel.SetCurrentScope_IsImplicitOpenCodeBlockTextSpan(true);
    }

    public static void HandleFixedTokenKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleFloatTokenKeyword(ref CSharpParserState parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleForTokenKeyword(ref CSharpParserState parserModel)
    {
        _ = parserModel.TokenWalker.Consume(); // forKeywordToken
        _ = parserModel.TokenWalker.Match(SyntaxKind.OpenParenthesisToken);

        parserModel.RegisterScope(
        	new Scope(
        		ScopeDirectionKind.Down,
        		scope_StartInclusiveIndex: parserModel.TokenWalker.Current.TextSpan.StartInclusiveIndex,
        		scope_EndExclusiveIndex: -1,
        		codeBlock_StartInclusiveIndex: -1,
        		codeBlock_EndExclusiveIndex: -1,
        		parentScopeSubIndex: parserModel.ScopeCurrentSubIndex,
        		selfScopeSubIndex: parserModel.Compilation.ScopeLength,
        		nodeSubIndex: -1,
        		permitCodeBlockParsing: true,
        		isImplicitOpenCodeBlockTextSpan: false,
        		ownerSyntaxKind: SyntaxKind.ForStatementNode),
            codeBlockOwner: null);
        
        parserModel.SetCurrentScope_IsImplicitOpenCodeBlockTextSpan(false);
        
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
        
        _ = parserModel.TokenWalker.Match(SyntaxKind.CloseParenthesisToken);
        
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenBraceToken)
            parserModel.SetCurrentScope_IsImplicitOpenCodeBlockTextSpan(true);
    }

    public static void HandleForeachTokenKeyword(ref CSharpParserState parserModel)
    {
        _ = parserModel.TokenWalker.Consume(); // foreachKeywordToken
        _ = parserModel.TokenWalker.Match(SyntaxKind.OpenParenthesisToken);
        
        parserModel.RegisterScope(
        	new Scope(
        		ScopeDirectionKind.Down,
        		scope_StartInclusiveIndex: parserModel.TokenWalker.Current.TextSpan.StartInclusiveIndex,
        		scope_EndExclusiveIndex: -1,
        		codeBlock_StartInclusiveIndex: -1,
        		codeBlock_EndExclusiveIndex: -1,
        		parentScopeSubIndex: parserModel.ScopeCurrentSubIndex,
        		selfScopeSubIndex: parserModel.Compilation.ScopeLength,
        		nodeSubIndex: -1,
        		permitCodeBlockParsing: true,
        		isImplicitOpenCodeBlockTextSpan: false,
        		ownerSyntaxKind: SyntaxKind.ForeachStatementNode),
            codeBlockOwner: null);
            
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
                    parserModel.Binder.GenericParameterList[enumerable.ResultTypeReference.IndexGenericParameterEntryList].TypeReference);
        }
        
        if (enumerable.SyntaxKind == SyntaxKind.VariableReferenceNode)
        {
            parserModel.Return_VariableReferenceNode((VariableReferenceNode)enumerable);
        }
        else if (enumerable.SyntaxKind == SyntaxKind.FunctionInvocationNode)
        {
            parserModel.Return_FunctionInvocationNode((FunctionInvocationNode)enumerable);
        }
        
        _ = parserModel.TokenWalker.Match(SyntaxKind.CloseParenthesisToken);
            
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenBraceToken)
            parserModel.SetCurrentScope_IsImplicitOpenCodeBlockTextSpan(true);
    }

    public static void HandleGotoTokenKeyword(ref CSharpParserState parserModel)
    {
        _ = ParseExpressions.ParseExpression(ref parserModel);
    }

    public static void HandleImplicitTokenKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleInTokenKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleIntTokenKeyword(ref CSharpParserState parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleIsTokenKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleLockTokenKeyword(ref CSharpParserState parserModel)
    {
        _ = parserModel.TokenWalker.Consume(); // lockKeywordToken
        _ = parserModel.TokenWalker.Match(SyntaxKind.OpenParenthesisToken);
        
        parserModel.ExpressionList.Add((SyntaxKind.CloseParenthesisToken, null));
        var expression = ParseExpressions.ParseExpression(ref parserModel);
        
        if (expression.SyntaxKind == SyntaxKind.VariableReferenceNode)
        {
            parserModel.Return_VariableReferenceNode((VariableReferenceNode)expression);
        }
        
        _ = parserModel.TokenWalker.Match(SyntaxKind.CloseParenthesisToken);
        
        parserModel.RegisterScope(
        	new Scope(
        		ScopeDirectionKind.Down,
        		scope_StartInclusiveIndex: parserModel.TokenWalker.Current.TextSpan.StartInclusiveIndex,
        		scope_EndExclusiveIndex: -1,
        		codeBlock_StartInclusiveIndex: -1,
        		codeBlock_EndExclusiveIndex: -1,
        		parentScopeSubIndex: parserModel.ScopeCurrentSubIndex,
        		selfScopeSubIndex: parserModel.Compilation.ScopeLength,
        		nodeSubIndex: -1,
        		permitCodeBlockParsing: true,
        		isImplicitOpenCodeBlockTextSpan: false,
        		ownerSyntaxKind: SyntaxKind.LockStatementNode),
            codeBlockOwner: null);
    
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenBraceToken)
            parserModel.SetCurrentScope_IsImplicitOpenCodeBlockTextSpan(true);
    }

    public static void HandleLongTokenKeyword(ref CSharpParserState parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleNullTokenKeyword(ref CSharpParserState parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleObjectTokenKeyword(ref CSharpParserState parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleOperatorTokenKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleOutTokenKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleParamsTokenKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleProtectedTokenKeyword(ref CSharpParserState parserModel)
    {
        var protectedTokenKeyword = parserModel.TokenWalker.Consume();
        parserModel.StatementBuilder.ChildList.Add(protectedTokenKeyword);
    }

    public static void HandleReadonlyTokenKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleRefTokenKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleSbyteTokenKeyword(ref CSharpParserState parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleShortTokenKeyword(ref CSharpParserState parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleSizeofTokenKeyword(ref CSharpParserState parserModel)
    {
        _ = ParseExpressions.ParseExpression(ref parserModel);
    }

    public static void HandleStackallocTokenKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleStringTokenKeyword(ref CSharpParserState parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleStructTokenKeyword(ref CSharpParserState parserModel)
    {
        HandleStorageModifierTokenKeyword(ref parserModel);
    }

    public static void HandleSwitchTokenKeyword(ref CSharpParserState parserModel)
    {
        _ = parserModel.TokenWalker.Consume(); // switchKeywordToken
        _ = parserModel.TokenWalker.Match(SyntaxKind.OpenParenthesisToken);
        
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
        
        _ = parserModel.TokenWalker.Match(SyntaxKind.CloseParenthesisToken);
        
        parserModel.RegisterScope(
        	new Scope(
        		ScopeDirectionKind.Down,
        		scope_StartInclusiveIndex: parserModel.TokenWalker.Current.TextSpan.StartInclusiveIndex,
        		scope_EndExclusiveIndex: -1,
        		codeBlock_StartInclusiveIndex: -1,
        		codeBlock_EndExclusiveIndex: -1,
        		parentScopeSubIndex: parserModel.ScopeCurrentSubIndex,
        		selfScopeSubIndex: parserModel.Compilation.ScopeLength,
        		nodeSubIndex: -1,
        		permitCodeBlockParsing: true,
        		isImplicitOpenCodeBlockTextSpan: false,
        		ownerSyntaxKind: SyntaxKind.SwitchStatementNode),
            codeBlockOwner: null);
    }

    public static void HandleThisTokenKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleThrowTokenKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleTrueTokenKeyword(ref CSharpParserState parserModel)
    {
        var expressionNode = ParseExpressions.ParseExpression(ref parserModel);
        parserModel.StatementBuilder.MostRecentNode = expressionNode;
    }

    public static void HandleTryTokenKeyword(ref CSharpParserState parserModel)
    {
        _ = parserModel.TokenWalker.Consume(); // tryKeywordToken

        parserModel.RegisterScope(
        	new Scope(
        		ScopeDirectionKind.Down,
        		scope_StartInclusiveIndex: parserModel.TokenWalker.Current.TextSpan.StartInclusiveIndex,
        		scope_EndExclusiveIndex: -1,
        		codeBlock_StartInclusiveIndex: -1,
        		codeBlock_EndExclusiveIndex: -1,
        		parentScopeSubIndex: parserModel.ScopeCurrentSubIndex,
        		selfScopeSubIndex: parserModel.Compilation.ScopeLength,
        		nodeSubIndex: -1,
        		permitCodeBlockParsing: true,
        		isImplicitOpenCodeBlockTextSpan: false,
        		ownerSyntaxKind: SyntaxKind.TryStatementTryNode),
            codeBlockOwner: null);
        
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenBraceToken)
            parserModel.SetCurrentScope_IsImplicitOpenCodeBlockTextSpan(true);
    }

    public static void HandleTypeofTokenKeyword(ref CSharpParserState parserModel)
    {
        _ = ParseExpressions.ParseExpression(ref parserModel);
    }

    public static void HandleUintTokenKeyword(ref CSharpParserState parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleUlongTokenKeyword(ref CSharpParserState parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleUncheckedTokenKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleUnsafeTokenKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleUshortTokenKeyword(ref CSharpParserState parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleVoidTokenKeyword(ref CSharpParserState parserModel)
    {
        HandleTypeIdentifierKeyword(ref parserModel);
    }

    public static void HandleVolatileTokenKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleWhileTokenKeyword(ref CSharpParserState parserModel)
    {
        _ = parserModel.TokenWalker.Consume(); // whileKeywordToken
        _ = parserModel.TokenWalker.Match(SyntaxKind.OpenParenthesisToken);
        
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
        
        _ = parserModel.TokenWalker.Match(SyntaxKind.CloseParenthesisToken);
        
        /* This was done with CSharpParserModel's SyntaxStack, but that property is now being removed. A different way to accomplish this needs to be done. (2025-02-06)
        {
            doWhileStatementNode.SetWhileProperties(
                whileKeywordToken,
                openParenthesisToken,
                expressionNode,
                closeParenthesisToken);
            
            return;
        }*/
        
        parserModel.RegisterScope(
        	new Scope(
        		ScopeDirectionKind.Down,
        		scope_StartInclusiveIndex: parserModel.TokenWalker.Current.TextSpan.StartInclusiveIndex,
        		scope_EndExclusiveIndex: -1,
        		codeBlock_StartInclusiveIndex: -1,
        		codeBlock_EndExclusiveIndex: -1,
        		parentScopeSubIndex: parserModel.ScopeCurrentSubIndex,
        		selfScopeSubIndex: parserModel.Compilation.ScopeLength,
        		nodeSubIndex: -1,
        		permitCodeBlockParsing: true,
        		isImplicitOpenCodeBlockTextSpan: false,
        		ownerSyntaxKind: SyntaxKind.WhileStatementNode),
            codeBlockOwner: null);
        
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenBraceToken)
            parserModel.SetCurrentScope_IsImplicitOpenCodeBlockTextSpan(true);
    }

    public static void HandleUnrecognizedTokenKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    /// <summary>The 'Default' of this method name is confusing.
    /// It seems to refer to the 'default' of switch statement rather than the 'default' keyword itself?
    /// </summary>
    public static void HandleDefault(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleTypeIdentifierKeyword(ref CSharpParserState parserModel)
    {
        ParseTokens.ParseIdentifierToken(ref parserModel);
    }

    public static void HandleNewTokenKeyword(ref CSharpParserState parserModel)
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

    public static void HandlePublicTokenKeyword(ref CSharpParserState parserModel)
    {
        var publicKeywordToken = parserModel.TokenWalker.Consume();
        parserModel.StatementBuilder.ChildList.Add(publicKeywordToken);
    }

    public static void HandleInternalTokenKeyword(ref CSharpParserState parserModel)
    {
        var internalTokenKeyword = parserModel.TokenWalker.Consume();
        parserModel.StatementBuilder.ChildList.Add(internalTokenKeyword);
    }

    public static void HandlePrivateTokenKeyword(ref CSharpParserState parserModel)
    {
        var privateTokenKeyword = parserModel.TokenWalker.Consume();
        parserModel.StatementBuilder.ChildList.Add(privateTokenKeyword);
    }

    public static void HandleStaticTokenKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleOverrideTokenKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleVirtualTokenKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleAbstractTokenKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleSealedTokenKeyword(ref CSharpParserState parserModel)
    {
        parserModel.StatementBuilder.ChildList.Add(parserModel.TokenWalker.Consume());
    }

    public static void HandleIfTokenKeyword(ref CSharpParserState parserModel)
    {
        _ = parserModel.TokenWalker.Consume(); // ifTokenKeyword

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

        _ = parserModel.TokenWalker.Match(SyntaxKind.CloseParenthesisToken);

        parserModel.RegisterScope(
        	new Scope(
        		ScopeDirectionKind.Down,
        		scope_StartInclusiveIndex: parserModel.TokenWalker.Current.TextSpan.StartInclusiveIndex,
        		scope_EndExclusiveIndex: -1,
        		codeBlock_StartInclusiveIndex: -1,
        		codeBlock_EndExclusiveIndex: -1,
        		parentScopeSubIndex: parserModel.ScopeCurrentSubIndex,
        		selfScopeSubIndex: parserModel.Compilation.ScopeLength,
        		nodeSubIndex: -1,
        		permitCodeBlockParsing: true,
        		isImplicitOpenCodeBlockTextSpan: false,
        		ownerSyntaxKind: SyntaxKind.IfStatementNode),
            codeBlockOwner: null);
        
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenBraceToken)
            parserModel.SetCurrentScope_IsImplicitOpenCodeBlockTextSpan(true);
    }

    public static void HandleUsingTokenKeyword(ref CSharpParserState parserModel)
    {
        var usingKeywordToken = parserModel.TokenWalker.Consume();
        
        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenParenthesisToken)
        {
            HandleUsingCodeBlockOwner(ref usingKeywordToken, ref parserModel);
        }
        else if (parserModel.ScopeCurrent.OwnerSyntaxKind == SyntaxKind.GlobalCodeBlockNode ||
                 parserModel.ScopeCurrent.OwnerSyntaxKind == SyntaxKind.NamespaceStatementNode)
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
    
    public static void HandleUsingCodeBlockOwner(ref SyntaxToken usingKeywordToken, ref CSharpParserState parserModel)
    {
        _ = parserModel.TokenWalker.Consume(); // openParenthesisToken

        parserModel.RegisterScope(
        	new Scope(
        		ScopeDirectionKind.Down,
        		scope_StartInclusiveIndex: parserModel.TokenWalker.Current.TextSpan.StartInclusiveIndex,
        		scope_EndExclusiveIndex: -1,
        		codeBlock_StartInclusiveIndex: -1,
        		codeBlock_EndExclusiveIndex: -1,
        		parentScopeSubIndex: parserModel.ScopeCurrentSubIndex,
        		selfScopeSubIndex: parserModel.Compilation.ScopeLength,
        		nodeSubIndex: -1,
        		permitCodeBlockParsing: true,
        		isImplicitOpenCodeBlockTextSpan: false,
        		ownerSyntaxKind: SyntaxKind.UsingStatementCodeBlockNode),
            codeBlockOwner: null);
        
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

        _ = parserModel.TokenWalker.Match(SyntaxKind.CloseParenthesisToken);

        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenBraceToken)
            parserModel.SetCurrentScope_IsImplicitOpenCodeBlockTextSpan(true);
    }

    public static void HandleInterfaceTokenKeyword(ref CSharpParserState parserModel)
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
    public static void HandleStorageModifierTokenKeyword(ref CSharpParserState parserModel)
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
                _ = parserModel.TokenWalker.Consume(); // classKeywordToken
                storageModifierKind = StorageModifierKind.RecordClass;
            }
            else if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.StructTokenKeyword)
            {
                _ = parserModel.TokenWalker.Consume(); // structKeywordToken
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
            parserModel.ResourceUri);
        
        if (typeDefinitionNode.HasPartialModifier)
        {
            // NOTE: You do indeed use the current compilation unit here...
            // ...there is a different step that checks the previous.
            if (parserModel.TryGetTypeDefinitionHierarchically(
                    parserModel.ResourceUri,
                    parserModel.Compilation,
                    parserModel.ScopeCurrentSubIndex,
                    parserModel.ResourceUri,
                    identifierToken.TextSpan,
                    out SyntaxNodeValue previousTypeDefinitionNode))
            {
                var typeDefinitionMetadata = parserModel.Binder.TypeDefinitionTraitsList[previousTypeDefinitionNode.TraitsIndex];
                typeDefinitionNode.IndexPartialTypeDefinition = typeDefinitionMetadata.IndexPartialTypeDefinition;
            }
        }
        
        parserModel.BindTypeDefinitionNode(typeDefinitionNode);
        parserModel.BindTypeIdentifier(identifierToken);
        
        parserModel.StatementBuilder.MostRecentNode = typeDefinitionNode;
            
        parserModel.RegisterScope(
        	new Scope(
        		ScopeDirectionKind.Both,
        		scope_StartInclusiveIndex: parserModel.TokenWalker.Current.TextSpan.StartInclusiveIndex,
        		scope_EndExclusiveIndex: -1,
        		codeBlock_StartInclusiveIndex: -1,
        		codeBlock_EndExclusiveIndex: -1,
        		parentScopeSubIndex: parserModel.ScopeCurrentSubIndex,
        		selfScopeSubIndex: parserModel.Compilation.ScopeLength,
        		nodeSubIndex: parserModel.Compilation.NodeLength,
        		permitCodeBlockParsing: true,
        		isImplicitOpenCodeBlockTextSpan: false,
        		ownerSyntaxKind: typeDefinitionNode.SyntaxKind),
    	    typeDefinitionNode);
        
        parserModel.SetCurrentScope_IsImplicitOpenCodeBlockTextSpan(false);
        
        if (typeDefinitionNode.HasPartialModifier)
        {
            if (typeDefinitionNode.IndexPartialTypeDefinition == -1)
            {
                if (parserModel.Binder.__CompilationUnitMap.TryGetValue(parserModel.ResourceUri, out var previousCompilationUnit))
                {
                    if (typeDefinitionNode.ParentScopeSubIndex < previousCompilationUnit.ScopeLength)
                    {
                        var previousParent = parserModel.Binder.ScopeList[previousCompilationUnit.ScopeOffset + typeDefinitionNode.ParentScopeSubIndex];
                        var currentParent = parserModel.GetParent(typeDefinitionNode.ParentScopeSubIndex, parserModel.Compilation);
                        
                        if (currentParent.OwnerSyntaxKind == previousParent.OwnerSyntaxKind)
                        {
                            var currentParentIdentifierText = parserModel.Binder.CSharpCompilerService.SafeGetText(
                                parserModel.Binder.NodeList[parserModel.Compilation.NodeOffset + currentParent.NodeSubIndex].ResourceUri.Value,
                                parserModel.Binder.NodeList[parserModel.Compilation.NodeOffset + currentParent.NodeSubIndex].IdentifierToken.TextSpan);
                            
                            var previousParentIdentifierText = parserModel.Binder.CSharpCompilerService.SafeGetText(
                                parserModel.Binder.NodeList[previousCompilationUnit.NodeOffset + previousParent.NodeSubIndex].ResourceUri.Value,
                                parserModel.Binder.NodeList[previousCompilationUnit.NodeOffset + previousParent.NodeSubIndex].IdentifierToken.TextSpan);
                            
                            if (currentParentIdentifierText is not null &&
                                currentParentIdentifierText == previousParentIdentifierText)
                            {
                                // All the existing entires will be "emptied"
                                // so don't both with checking whether the arguments are the same here.
                                //
                                // All that matters is that they're put in the same "group".
                                //
                                var binder = parserModel.Binder;
                                
                                // TODO: Cannot use ref, out, or in...
                                var compilation = parserModel.Compilation;
                                
                                SyntaxNodeValue previousNode = default;
                                
                                for (int i = previousCompilationUnit.ScopeOffset; i < previousCompilationUnit.ScopeOffset + previousCompilationUnit.ScopeLength; i++)
                                {
                                    var scope = parserModel.Binder.ScopeList[i];
                                    
                                    if (scope.ParentScopeSubIndex == previousParent.SelfScopeSubIndex &&
                                        scope.OwnerSyntaxKind == SyntaxKind.TypeDefinitionNode &&
                                        binder.CSharpCompilerService.SafeGetText(
                                                parserModel.Binder.NodeList[previousCompilationUnit.NodeOffset + scope.NodeSubIndex].ResourceUri.Value,
                                                parserModel.Binder.NodeList[previousCompilationUnit.NodeOffset + scope.NodeSubIndex].IdentifierToken.TextSpan) ==
                                            binder.GetIdentifierText(typeDefinitionNode, parserModel.ResourceUri, compilation))
                                    {
                                        previousNode = parserModel.Binder.NodeList[previousCompilationUnit.NodeOffset + scope.NodeSubIndex];
                                        break;
                                    }
                                }
                                
                                if (!previousNode.IsDefault())
                                {
                                    var previousTypeDefinitionNode = previousNode;
                                    var previousTypeDefinitionMetadata = parserModel.Binder.TypeDefinitionTraitsList[previousTypeDefinitionNode.TraitsIndex];
                                    typeDefinitionNode.IndexPartialTypeDefinition = previousTypeDefinitionMetadata.IndexPartialTypeDefinition;
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
                            partialTypeDefinitionEntry.ScopeSubIndex = -1;
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
            typeDefinitionNode.SetInheritedTypeReference(new TypeReferenceValue(inheritedTypeClauseNode));
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
            _ = ParseExpressions.ParseExpression(ref parserModel);
        }
        
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenBraceToken)
            parserModel.SetCurrentScope_IsImplicitOpenCodeBlockTextSpan(true);
    
        if (typeDefinitionNode.HasPartialModifier)
            HandlePartialTypeDefinition(typeDefinitionNode, ref parserModel);
    }
    
    public static void HandlePartialTypeDefinition(TypeDefinitionNode typeDefinitionNode, ref CSharpParserState parserModel)
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
                        if (parserModel.Binder.PartialTypeDefinitionList[positionExclusive].ScopeSubIndex == -1)
                        {
                            var partialTypeDefinitionEntry = parserModel.Binder.PartialTypeDefinitionList[positionExclusive];
                            partialTypeDefinitionEntry.ScopeSubIndex = typeDefinitionNode.SelfScopeSubIndex;
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
                new PartialTypeDefinitionValue(
                    typeDefinitionNode.ResourceUri,
                    typeDefinitionNode.IndexPartialTypeDefinition,
                    typeDefinitionNode.SelfScopeSubIndex));
        
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
                            var innerTypeDefinitionNode = parserModel.Binder.NodeList[
                                innerCompilationUnit.NodeOffset +
                                parserModel.Binder.ScopeList[innerCompilationUnit.ScopeOffset + partialTypeDefinitionEntry.ScopeSubIndex].NodeSubIndex];
                            var innerTypeDefinitionMetadata = parserModel.Binder.TypeDefinitionTraitsList[innerTypeDefinitionNode.TraitsIndex];
                            innerTypeDefinitionMetadata.IndexPartialTypeDefinition = partialTypeDefinitionEntry.IndexStartGroup + 1;
                            parserModel.Binder.TypeDefinitionTraitsList[innerTypeDefinitionNode.TraitsIndex] = innerTypeDefinitionMetadata;
                        }
                    }
                    
                    partialTypeDefinitionEntry.IndexStartGroup = partialTypeDefinitionEntry.IndexStartGroup + 1;
                    parserModel.Binder.PartialTypeDefinitionList[positionExclusive] = partialTypeDefinitionEntry;
                }
                
                positionExclusive++;
            }
        }
    }

    public static void HandleClassTokenKeyword(ref CSharpParserState parserModel)
    {
        HandleStorageModifierTokenKeyword(ref parserModel);
    }

    public static void HandleNamespaceTokenKeyword(ref CSharpParserState parserModel)
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
        
        parserModel.RegisterScope(
        	new Scope(
        		ScopeDirectionKind.Both,
        		scope_StartInclusiveIndex: parserModel.TokenWalker.Current.TextSpan.StartInclusiveIndex,
        		scope_EndExclusiveIndex: -1,
        		codeBlock_StartInclusiveIndex: -1,
        		codeBlock_EndExclusiveIndex: -1,
        		parentScopeSubIndex: parserModel.ScopeCurrentSubIndex,
        		selfScopeSubIndex: parserModel.Compilation.ScopeLength,
        		nodeSubIndex: parserModel.Compilation.NodeLength,
        		permitCodeBlockParsing: true,
        		isImplicitOpenCodeBlockTextSpan: false,
        		ownerSyntaxKind: namespaceStatementNode.SyntaxKind),
    	    namespaceStatementNode);
        
        // Do not set 'IsImplicitOpenCodeBlockTextSpan' for namespace file scoped.
    }

    public static void HandleReturnTokenKeyword(ref CSharpParserState parserModel)
    {
        _ = parserModel.TokenWalker.Consume(); // returnKeywordToken
        var expressionNode = ParseExpressions.ParseExpression(ref parserModel);
        
        if (expressionNode.SyntaxKind == SyntaxKind.VariableReferenceNode)
        {
            parserModel.Return_VariableReferenceNode((VariableReferenceNode)expressionNode);
        }
        else if (expressionNode.SyntaxKind == SyntaxKind.FunctionInvocationNode)
        {
            parserModel.Return_FunctionInvocationNode((FunctionInvocationNode)expressionNode);
        }
        else if (expressionNode.SyntaxKind == SyntaxKind.ConstructorInvocationNode)
        {
            parserModel.Return_ConstructorInvocationExpressionNode((ConstructorInvocationNode)expressionNode);
        }
        else if (expressionNode.SyntaxKind == SyntaxKind.BinaryExpressionNode)
        {
            parserModel.Return_BinaryExpressionNode((BinaryExpressionNode)expressionNode);
        }
    }
}
