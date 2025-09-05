using Walk.CompilerServices.CSharp.CompilerServiceCase;
using Walk.CompilerServices.CSharp.Facts;
using Walk.Extensions.CompilerServices;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.TextEditor.RazorLib.Exceptions;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.CompilerServices.CSharp.ParserCase.Internals;

public static class ParseExpressions
{
    /// <summary>
    /// Invoke this method when 'parserModel.TokenWalker.Current' is the first token of the expression to be parsed.
    ///
    /// In the case where the first token of the expression had already been 'Consume()'-ed then 'parserModel.TokenWalker.Backtrack();'
    /// might be of use in order to move the parserModel.TokenWalker backwards prior to invoking this method.
    /// </summary>
    public static IExpressionNode ParseExpression(ref CSharpParserModel parserModel)
    {
        parserModel.ExpressionPrimary = parserModel.ForceParseExpressionInitialPrimaryExpression;
        var indexToken = parserModel.TokenWalker.Index;
        var forceExit = false;
        
        var indexTokenRoot = parserModel.TokenWalker.Index;
        var expressionPrimaryPreviousRoot = parserModel.ExpressionPrimary;
        
        while (true)
        {
            if (SyntaxIsEndDelimiter(parserModel.TokenWalker.Current.SyntaxKind)) // Check if the tokenCurrent is a token that is used as a end-delimiter before iterating the list?
            {
                for (int i = parserModel.ExpressionList.Count - 1; i > -1; i--)
                {
                    var delimiterExpressionTuple = parserModel.ExpressionList[i];
                    
                    if (delimiterExpressionTuple.DelimiterSyntaxKind == parserModel.TokenWalker.Current.SyntaxKind)
                    {
                        if (delimiterExpressionTuple.ExpressionNode is null)
                        {
                            forceExit = true;
                            break;
                        }
                        
                        parserModel.ExpressionPrimary = BubbleUpParseExpression(i, ref parserModel);
                        break;
                    }
                }
            }
            
            // The while loop used to be 'while (!parserModel.TokenWalker.IsEof)'
            // This caused an issue where 'BubbleUpParseExpression(...)' would not run
            // if the end of file was reached.
            //
            // Given how this parser is written, adding 'SyntaxKind.EndOfFile' to 'parserModel.ExpressionList'
            // would follow the pattern of how 'SyntaxKind.StatementDelimiterToken' is written.
            //
            // But, the 'while (true)' loop makes me extremely uncomfortable.
            //
            // So I added '|| parserModel.TokenWalker.IsEof' here.
            //
            // If upon further inspection on way or the other is deemed safe then this redundancy can be removed.
            if (forceExit || parserModel.TokenWalker.IsEof) // delimiterExpressionTuple.ExpressionNode is null
            {
                parserModel.ExpressionPrimary = BubbleUpParseExpression(0, ref parserModel);
                break;
            }
            
            // parserModel.ExpressionPrimary = ParseExpressions.AnyMergeToken(parserModel.ExpressionPrimary, ref parserModel);
            if (parserModel.ParserContextKind != CSharpParserContextKind.ForceParseGenericParameters &&
                UtilityApi.IsBinaryOperatorSyntaxKind(parserModel.TokenWalker.Current.SyntaxKind))
            {
                parserModel.ExpressionPrimary = HandleBinaryOperator(parserModel.ExpressionPrimary, ref parserModel);
            }
            else
            {
                switch (parserModel.ExpressionPrimary.SyntaxKind)
                {
                    case SyntaxKind.EmptyExpressionNode:
                        parserModel.ExpressionPrimary = EmptyMergeToken((EmptyExpressionNode)parserModel.ExpressionPrimary, ref parserModel);
                        break;
                    case SyntaxKind.CollectionInitializationNode:
                        parserModel.ExpressionPrimary = CollectionInitializationMergeToken((CollectionInitializationNode)parserModel.ExpressionPrimary, ref parserModel);
                        break;
                    case SyntaxKind.LiteralExpressionNode:
                        parserModel.ExpressionPrimary = LiteralMergeToken((LiteralExpressionNode)parserModel.ExpressionPrimary, ref parserModel);
                        break;
                    case SyntaxKind.InterpolatedStringNode:
                        parserModel.ExpressionPrimary = InterpolatedStringMergeToken((InterpolatedStringNode)parserModel.ExpressionPrimary, ref parserModel);
                        break;
                    case SyntaxKind.FunctionDefinitionNode:
                        var functionDefinitionNode = (FunctionDefinitionNode)parserModel.ExpressionPrimary;
                        if (functionDefinitionNode.IsParsingGenericParameters)
                            parserModel.ExpressionPrimary = GenericParametersListingMergeToken(functionDefinitionNode, ref parserModel);
                        else
                            parserModel.ExpressionPrimary = FunctionDefinitionMergeToken(functionDefinitionNode, ref parserModel);
                        break;
                    case SyntaxKind.ConstructorDefinitionNode:
                    case SyntaxKind.TypeDefinitionNode:
                        parserModel.ExpressionPrimary = FunctionDefinitionMergeToken((IFunctionDefinitionNode)parserModel.ExpressionPrimary, ref parserModel);
                        break;
                    case SyntaxKind.BinaryExpressionNode:
                        parserModel.ExpressionPrimary = BinaryMergeToken((BinaryExpressionNode)parserModel.ExpressionPrimary, ref parserModel);
                        break;
                    case SyntaxKind.ParenthesizedExpressionNode:
                        parserModel.ExpressionPrimary = ParenthesizedMergeToken((ParenthesizedExpressionNode)parserModel.ExpressionPrimary, ref parserModel);
                        break;
                    case SyntaxKind.FunctionInvocationNode:
                        parserModel.ExpressionPrimary = FunctionInvocationMergeToken((FunctionInvocationNode)parserModel.ExpressionPrimary, ref parserModel);
                        break;
                    case SyntaxKind.LambdaExpressionNode:
                        parserModel.ExpressionPrimary = LambdaMergeToken((LambdaExpressionNode)parserModel.ExpressionPrimary, ref parserModel);
                        break;
                    case SyntaxKind.ConstructorInvocationExpressionNode:
                        parserModel.ExpressionPrimary = ConstructorInvocationMergeToken((ConstructorInvocationExpressionNode)parserModel.ExpressionPrimary, ref parserModel);
                        break;
                    case SyntaxKind.WithExpressionNode:
                        parserModel.ExpressionPrimary = WithMergeToken((WithExpressionNode)parserModel.ExpressionPrimary, ref parserModel);
                        break;
                    case SyntaxKind.ExplicitCastNode:
                        parserModel.ExpressionPrimary = ExplicitCastMergeToken((ExplicitCastNode)parserModel.ExpressionPrimary, ref parserModel);
                        break;
                    case SyntaxKind.TupleExpressionNode:
                        parserModel.ExpressionPrimary = TupleMergeToken((TupleExpressionNode)parserModel.ExpressionPrimary, ref parserModel);
                        break;
                    case SyntaxKind.AmbiguousParenthesizedExpressionNode:
                        parserModel.ExpressionPrimary = AmbiguousParenthesizedMergeToken((AmbiguousParenthesizedExpressionNode)parserModel.ExpressionPrimary, ref parserModel);
                        break;
                    case SyntaxKind.AmbiguousIdentifierExpressionNode:
                        parserModel.ExpressionPrimary = AmbiguousIdentifierMergeToken((AmbiguousIdentifierExpressionNode)parserModel.ExpressionPrimary, ref parserModel);
                        break;
                    case SyntaxKind.VariableReferenceNode:
                        parserModel.ExpressionPrimary = VariableReferenceMergeToken((VariableReferenceNode)parserModel.ExpressionPrimary, ref parserModel);
                        break;
                    case SyntaxKind.TypeClauseNode:
                        parserModel.ExpressionPrimary = TypeClauseMergeToken((TypeClauseNode)parserModel.ExpressionPrimary, ref parserModel);
                        break;
                    case SyntaxKind.ReturnStatementNode:
                        parserModel.ExpressionPrimary = ReturnStatementMergeToken((ReturnStatementNode)parserModel.ExpressionPrimary, ref parserModel);
                        break;
                    case SyntaxKind.KeywordFunctionOperatorNode:
                        parserModel.ExpressionPrimary = KeywordFunctionOperatorMergeToken((KeywordFunctionOperatorNode)parserModel.ExpressionPrimary, ref parserModel);
                        break;
                    case SyntaxKind.SwitchExpressionNode:
                        parserModel.ExpressionPrimary = SwitchExpressionMergeToken((SwitchExpressionNode)parserModel.ExpressionPrimary, ref parserModel);
                        break;
                    case SyntaxKind.BadExpressionNode:
                        parserModel.ExpressionPrimary = BadMergeToken((BadExpressionNode)parserModel.ExpressionPrimary, ref parserModel);
                        break;
                    default:
                        parserModel.ExpressionPrimary = parserModel.Binder.Shared_BadExpressionNode;
                        break;
                };
            }
            
            if (parserModel.TokenWalker.Index == indexToken)
                _ = parserModel.TokenWalker.Consume();
            if (parserModel.TokenWalker.Index < indexToken)
                throw new WalkTextEditorException($"Infinite loop in {nameof(ParseExpression)}");
            
            indexToken = parserModel.TokenWalker.Index;
            
            if (parserModel.NoLongerRelevantExpressionNode is not null) // try finally is not needed to guarantee setting 'parserModel.NoLongerRelevantExpressionNode = null;' because this is an object reference comparison 'Object.ReferenceEquals'. Versus something more general that would break future parses if not properly cleared, like a SyntaxKind.
            {
                ParseExpressions.ClearFromExpressionList(parserModel.NoLongerRelevantExpressionNode, ref parserModel);
                parserModel.NoLongerRelevantExpressionNode = null;
            }
            
            if (parserModel.TryParseExpressionSyntaxKindList.Count != 0)
            {
                var isExpressionRoot = true;
                var rootSyntaxKind = SyntaxKind.EmptyExpressionNode;
                
                foreach (var tuple in parserModel.ExpressionList)
                {
                    if (tuple.ExpressionNode is null)
                        continue;
                        
                    isExpressionRoot = false;
                    rootSyntaxKind = tuple.ExpressionNode.SyntaxKind;
                    break;
                }
                
                var success = true;
                
                if (isExpressionRoot)
                {
                    success = parserModel.TryParseExpressionSyntaxKindList.Contains(parserModel.ExpressionPrimary.SyntaxKind);
                    
                    if (success)
                    {
                        expressionPrimaryPreviousRoot = parserModel.ExpressionPrimary;
                        indexTokenRoot = parserModel.TokenWalker.Index;
                    }
                }
                else
                {
                    success = parserModel.TryParseExpressionSyntaxKindList.Contains(rootSyntaxKind);
                }
                
                if (!success)
                {
                    var distance = parserModel.TokenWalker.Index - indexTokenRoot;
                    
                    for (int i = 0; i < distance; i++)
                    {
                        parserModel.TokenWalker.BacktrackNoReturnValue();
                    }
                    
                    if (parserModel.ExpressionPrimary.SyntaxKind == SyntaxKind.FunctionInvocationNode)
                    {
                        parserModel.Return_FunctionInvocationNode((FunctionInvocationNode)parserModel.ExpressionPrimary);
                    }
                    
                    parserModel.ExpressionPrimary = expressionPrimaryPreviousRoot;
                    
                    foreach (var tuple in parserModel.ExpressionList)
                    {
                        if (tuple.ExpressionNode is null)
                            continue;
                            
                        if (tuple.ExpressionNode != parserModel.ExpressionPrimary)
                        {
                            switch (tuple.ExpressionNode.SyntaxKind)
                            {
                                case SyntaxKind.BinaryExpressionNode:
                                    parserModel.Return_BinaryExpressionNode((BinaryExpressionNode)tuple.ExpressionNode);
                                    break;
                                /*case SyntaxKind.TypeClauseNode:
                                    parserModel.Return_TypeClauseNode((TypeClauseNode)tuple.ExpressionNode);
                                    break;
                                case SyntaxKind.VariableReferenceNode:
                                    parserModel.Return_VariableReferenceNode((VariableReferenceNode)tuple.ExpressionNode);
                                    break;
                                case SyntaxKind.NamespaceClauseNode:
                                    parserModel.Return_NamespaceClauseNode((NamespaceClauseNode)tuple.ExpressionNode);
                                    break;
                                case SyntaxKind.AmbiguousIdentifierExpressionNode:
                                    parserModel.Return_AmbiguousIdentifierExpressionNode((AmbiguousIdentifierExpressionNode)tuple.ExpressionNode);
                                    break;
                                case SyntaxKind.FunctionInvocationNode:
                                    parserModel.Return_FunctionInvocationNode((FunctionInvocationNode)tuple.ExpressionNode);
                                    break;
                                case SyntaxKind.ConstructorInvocationExpressionNode:
                                    parserModel.Return_ConstructorInvocationExpressionNode((ConstructorInvocationExpressionNode)tuple.ExpressionNode);
                                    break;*/
                            }
                        }
                    }
                    
                    forceExit = true;
                }
            }
            
            if (forceExit) // parserModel.ForceParseExpressionSyntaxKind
                break;
        }
        
        // It is vital that this 'clear' and 'add' are done in a way that permits an invoker of the 'ParseExpression' method to 'add' a similar 'forceExit' delimiter
        //     Example: 'parserModel.ExpressionList.Add((SyntaxKind.CloseParenthesisToken, null));'
        //
        // CSharpParserModel constructor needs to duplicate the same additions for the first parse.
        //
        parserModel.ExpressionList.Clear();
        parserModel.ExpressionList.Add((SyntaxKind.EndOfFileToken, null));
        parserModel.ExpressionList.Add((SyntaxKind.CloseBraceToken, null));
        parserModel.ExpressionList.Add((SyntaxKind.StatementDelimiterToken, null));
        
        if (parserModel.ExpressionPrimary.SyntaxKind == SyntaxKind.AmbiguousIdentifierExpressionNode)
        {
            parserModel.ExpressionPrimary = ForceDecisionAmbiguousIdentifier(
                EmptyExpressionNode.Empty,
                (AmbiguousIdentifierExpressionNode)parserModel.ExpressionPrimary,
                ref parserModel);
        }
        
        return parserModel.ExpressionPrimary;
    }

    public static bool SyntaxIsEndDelimiter(SyntaxKind syntaxKind)
    {
        switch (syntaxKind)
        {
            case SyntaxKind.OpenParenthesisToken:
            case SyntaxKind.CloseParenthesisToken:
            case SyntaxKind.CommaToken:
            case SyntaxKind.CloseAngleBracketToken:
            case SyntaxKind.OpenBraceToken:
            case SyntaxKind.CloseBraceToken:
            case SyntaxKind.EqualsToken:
            case SyntaxKind.StatementDelimiterToken:
            case SyntaxKind.ColonToken:
            case SyntaxKind.CloseSquareBracketToken:
            case SyntaxKind.StringInterpolatedEndToken:
            case SyntaxKind.StringInterpolatedContinueToken:
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// 'BubbleUpParseExpression(i, compilationUnit);'
    /// 
    /// This is to have SyntaxKind.StatementDelimiterToken break out of the expression.
    /// The parser is adding as the 0th item that
    /// 'SyntaxKind.StatementDelimiterToken' returns the primary expression to be 'null'.
    ///
    /// One isn't supposed to deal with nulls here, instead using EmptyExpressionNode.
    /// So, if delimiterExpressionTuple.ExpressionNode is null then
    /// this special case to break out of the expresion logic exists.
    ///
    /// It needs to be part of the session.ShortCircuitList however,
    /// because if an expression uses 'SyntaxKind.StatementDelimiterToken'
    /// in their expression, they can override this 0th index entry
    /// and have primary expression "short circuit" to their choosing
    /// and the loop will continue parsing more expressions.
    ///
    /// LambdaExpressionNode for example, needs to override 'SyntaxKind.StatementDelimiterToken'.
    /// </summary>
    private static IExpressionNode BubbleUpParseExpression(int indexTriggered, ref CSharpParserModel parserModel)
    {
        var triggeredDelimiterTuple = parserModel.ExpressionList[indexTriggered];
        IExpressionNode? previousDelimiterExpressionNode = null;
        
        var initialExpressionListCount = parserModel.ExpressionList.Count;
        
        for (int i = initialExpressionListCount - 1; i > indexTriggered - 1; i--)
        {
            var delimiterExpressionTuple = parserModel.ExpressionList[i];
            parserModel.ExpressionList.RemoveAt(i);
            
            if (delimiterExpressionTuple.ExpressionNode is null)
                break; // This implies to forcibly return back to the statement while loop.
            if (previousDelimiterExpressionNode == delimiterExpressionTuple.ExpressionNode)
                continue; // This implies that an individual IExpressionNode existed in the list for more than one SyntaxKind. All entries for a node are continguous, so if the previous node were the same object, then it was already handled.
            if (triggeredDelimiterTuple.ExpressionNode == delimiterExpressionTuple.ExpressionNode &&
                triggeredDelimiterTuple.DelimiterSyntaxKind != delimiterExpressionTuple.DelimiterSyntaxKind)
            {
                continue; // This implies that the triggered syntax kind was not the first syntax kind found for the given 'triggeredDelimiterTuple.ExpressionNode'. (example: a FunctionParametersListingNode might make two entries in the list. 1 for SyntaxKind.CloseParenthesisToken, another for SyntaxKind.CommaToken. If 'SyntaxKind.CloseParenthesisToken' is triggered the 'SyntaxKind.CommaToken' will be hit by this loop first. So it would need to be skipped.
            }
            
            previousDelimiterExpressionNode = delimiterExpressionTuple.ExpressionNode;
            
            var expressionSecondary = parserModel.ExpressionPrimary;
            parserModel.ExpressionPrimary = delimiterExpressionTuple.ExpressionNode;
            
            switch (parserModel.ExpressionPrimary.SyntaxKind)
            {
                case SyntaxKind.CollectionInitializationNode:
                    parserModel.ExpressionPrimary = CollectionInitializationMergeExpression(expressionSecondary, ref parserModel);   
                    break;
                case SyntaxKind.BinaryExpressionNode:
                    parserModel.ExpressionPrimary = BinaryMergeExpression(expressionSecondary, ref parserModel);
                    break;
                case SyntaxKind.InterpolatedStringNode:
                    parserModel.ExpressionPrimary = InterpolatedStringMergeExpression(expressionSecondary, ref parserModel);
                    break;
                case SyntaxKind.FunctionDefinitionNode:
                    var functionDefinitionNode = (FunctionDefinitionNode)parserModel.ExpressionPrimary;
                    if (functionDefinitionNode.IsParsingGenericParameters)
                    {
                        parserModel.ExpressionPrimary = GenericParametersListingMergeExpression(
                            functionDefinitionNode, expressionSecondary, ref parserModel);
                        break;
                    }
                    parserModel.ExpressionPrimary = FunctionDefinitionMergeExpression(functionDefinitionNode, expressionSecondary, ref parserModel);
                    break;
                case SyntaxKind.ConstructorDefinitionNode:
                case SyntaxKind.TypeDefinitionNode:
                    parserModel.ExpressionPrimary = FunctionDefinitionMergeExpression((IFunctionDefinitionNode)parserModel.ExpressionPrimary, expressionSecondary, ref parserModel);
                    break;
                case SyntaxKind.ParenthesizedExpressionNode:
                    parserModel.ExpressionPrimary = ParenthesizedMergeExpression(expressionSecondary, ref parserModel);
                    break;
                case SyntaxKind.FunctionInvocationNode:
                    parserModel.ExpressionPrimary = FunctionInvocationMergeExpression(expressionSecondary, ref parserModel);
                    break;
                case SyntaxKind.LambdaExpressionNode:
                    parserModel.ExpressionPrimary = LambdaMergeExpression(expressionSecondary, ref parserModel);
                    break;
                case SyntaxKind.ConstructorInvocationExpressionNode:
                    parserModel.ExpressionPrimary = ConstructorInvocationMergeExpression(expressionSecondary, ref parserModel);
                    break;
                case SyntaxKind.WithExpressionNode:
                    parserModel.ExpressionPrimary = WithMergeExpression(expressionSecondary, ref parserModel);
                    break;
                case SyntaxKind.TupleExpressionNode:
                    parserModel.ExpressionPrimary = TupleMergeExpression(expressionSecondary, ref parserModel);
                    break;
                case SyntaxKind.AmbiguousParenthesizedExpressionNode:
                    parserModel.ExpressionPrimary = AmbiguousParenthesizedMergeExpression(expressionSecondary, ref parserModel);
                    break;
                case SyntaxKind.AmbiguousIdentifierExpressionNode:
                    parserModel.ExpressionPrimary = AmbiguousIdentifierMergeExpression(expressionSecondary, ref parserModel);
                    break;
                case SyntaxKind.VariableReferenceNode:
                    parserModel.ExpressionPrimary = VariableReferenceMergeExpression(expressionSecondary, ref parserModel);
                    break;
                case SyntaxKind.TypeClauseNode:
                    parserModel.ExpressionPrimary = TypeClauseMergeExpression(expressionSecondary, ref parserModel);
                    break;
                case SyntaxKind.ReturnStatementNode:
                    parserModel.ExpressionPrimary = ReturnStatementMergeExpression(expressionSecondary, ref parserModel);
                    break;
                case SyntaxKind.KeywordFunctionOperatorNode:
                    parserModel.ExpressionPrimary = KeywordFunctionOperatorMergeExpression(expressionSecondary, ref parserModel);
                    break;
                case SyntaxKind.SwitchExpressionNode:
                    parserModel.ExpressionPrimary = SwitchExpressionMergeExpression(expressionSecondary, ref parserModel);
                    break;
                case SyntaxKind.BadExpressionNode:
                    parserModel.ExpressionPrimary = BadMergeExpression(expressionSecondary, ref parserModel);
                    break;
                default:
                    parserModel.ExpressionPrimary = parserModel.Binder.Shared_BadExpressionNode;
                    break;
            };
        }
        
        if (parserModel.NoLongerRelevantExpressionNode is not null) // try finally is not needed to guarantee setting 'parserModel.NoLongerRelevantExpressionNode = null;' because this is an object reference comparison 'Object.ReferenceEquals'. Versus something more general that would break future parses if not properly cleared, like a SyntaxKind.
        {
            ClearFromExpressionList(parserModel.NoLongerRelevantExpressionNode, ref parserModel);
            parserModel.NoLongerRelevantExpressionNode = null;
        }
        
        return parserModel.ExpressionPrimary;
    }
    
    public static IExpressionNode HandleBinaryOperator(
        IExpressionNode expressionPrimary, ref CSharpParserModel parserModel)
    {
        SyntaxToken token;
    
        // TODO: MemberAccessToken should be treated the same as any other operator.
        //       This feels very "special case" the way it is written.
        //       This seems similar to the most precedence being assigned to it.
        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.MemberAccessToken)
        {
            token = parserModel.TokenWalker.Current;
            return ParseMemberAccessToken(expressionPrimary, ref token, ref parserModel);
        }
    
        // In order to disambiguate '<' between when the 'expressionPrimary' is an 'AmbiguousIdentifierExpressionNode'
        //     - Less than operator
        //     - GenericParametersListingNode
        // 
        // Invoke 'ForceDecisionAmbiguousIdentifier(...)' to determine what the true SyntaxKind is.
        //
        // If its true SyntaxKind is a TypeClauseNode, then parse the '<'
        // to be the start of a 'GenericParametersListingNode'.
        //
        // Otherwise presume that the '<' is the 'less than operator'.
        if (expressionPrimary.SyntaxKind == SyntaxKind.AmbiguousIdentifierExpressionNode)
        {
            expressionPrimary = ForceDecisionAmbiguousIdentifier(
                EmptyExpressionNode.Empty,
                (AmbiguousIdentifierExpressionNode)expressionPrimary,
                ref parserModel);
        }
        
        // TODO: This isn't great. The ConstructorInvocationExpressionNode after reading 'new'...
        // if then has to read the TypeClauseNode, it actually does this inside of the 'ConstructorInvocationExpressionNode'.
        // It is sort of duplicated logic, and this case for the 'TypeClauseNode' needs repeating too.
        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenAngleBracketToken || parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CloseAngleBracketToken)
        {
            if (expressionPrimary.SyntaxKind == SyntaxKind.ConstructorInvocationExpressionNode)
                return ConstructorInvocationMergeToken((ConstructorInvocationExpressionNode)expressionPrimary, ref parserModel);
            else if (expressionPrimary.SyntaxKind == SyntaxKind.TypeClauseNode)
                return TypeClauseMergeToken((TypeClauseNode)expressionPrimary, ref parserModel);
            else if (expressionPrimary.SyntaxKind == SyntaxKind.AmbiguousIdentifierExpressionNode)
                return AmbiguousIdentifierMergeToken((AmbiguousIdentifierExpressionNode)expressionPrimary, ref parserModel);
            else if (expressionPrimary.SyntaxKind == SyntaxKind.FunctionInvocationNode)
                return FunctionInvocationMergeToken((FunctionInvocationNode)expressionPrimary, ref parserModel);
            else if (expressionPrimary.SyntaxKind == SyntaxKind.FunctionDefinitionNode)
                return FunctionDefinitionMergeToken((FunctionDefinitionNode)expressionPrimary, ref parserModel);
        }
        
        token = parserModel.TokenWalker.Current;
        
        var expressionAntecedent = GetParentNode(expressionPrimary, ref parserModel);
        if (expressionAntecedent.SyntaxKind == SyntaxKind.BinaryExpressionNode)
        {
            var binaryExpressionAntecedent = (BinaryExpressionNode)expressionAntecedent;
            
            var precedenceAntecedent = UtilityApi.GetOperatorPrecedence(binaryExpressionAntecedent.OperatorToken.SyntaxKind);
            var precedencePrecedent = UtilityApi.GetOperatorPrecedence(token.SyntaxKind);
            
            if (!binaryExpressionAntecedent.RightExpressionNodeWasSet)
            {
                if (precedenceAntecedent >= precedencePrecedent)
                {
                    // Antecedent takes 'primaryExpression' as its right node.
                    // Precedent takes antecedent as its left node.
                    // Precedent becomes "subtree-root".
                    binaryExpressionAntecedent.RightExpressionResultTypeReference = expressionPrimary.ResultTypeReference;
                    
                    var typeClauseNode = expressionPrimary.ResultTypeReference;
                    
                    var binaryExpressionPrecedent = parserModel.Rent_BinaryExpressionNode();
                    binaryExpressionPrecedent.LeftOperandTypeReference = typeClauseNode;
                    binaryExpressionPrecedent.OperatorToken = token;
                    binaryExpressionPrecedent.RightOperandTypeReference = typeClauseNode;
                    binaryExpressionPrecedent.ResultTypeReference = typeClauseNode;
                    
                    // It is important that the primitive recursion does not
                    // set 'binaryExpressionAntecedent' as the primaryExpression in the future
                    // because it is now the left node of 'binaryExpressionPrecedent'.
                    ClearFromExpressionList(binaryExpressionAntecedent, ref parserModel);
                    
                    parserModel.ExpressionList.Add((SyntaxKind.EndOfFileToken, binaryExpressionPrecedent));
                    
                    if (expressionPrimary.SyntaxKind == SyntaxKind.VariableReferenceNode)
                    {
                        parserModel.Return_VariableReferenceNode((VariableReferenceNode)expressionPrimary);
                    }
                    else if (expressionPrimary.SyntaxKind == SyntaxKind.FunctionInvocationNode)
                    {
                        parserModel.Return_FunctionInvocationNode((FunctionInvocationNode)expressionPrimary);
                    }
                    
                    parserModel.Return_BinaryExpressionNode(binaryExpressionAntecedent);
                    
                    return EmptyExpressionNode.Empty;
                }
                else
                {
                    // Precedent takes 'primaryExpression' as its left node.
                    // Antecedent takes precedent as its right node.
                    var typeClauseNode = expressionPrimary.ResultTypeReference;
                    
                    var binaryExpressionNodePrecedent = parserModel.Rent_BinaryExpressionNode();
                    binaryExpressionNodePrecedent.LeftOperandTypeReference = typeClauseNode;
                    binaryExpressionNodePrecedent.OperatorToken = token;
                    binaryExpressionNodePrecedent.RightOperandTypeReference = typeClauseNode;
                    binaryExpressionNodePrecedent.ResultTypeReference = typeClauseNode;
                    
                    binaryExpressionAntecedent.RightExpressionResultTypeReference = binaryExpressionNodePrecedent.ResultTypeReference;
                    
                    parserModel.ExpressionList.Add((SyntaxKind.EndOfFileToken, binaryExpressionNodePrecedent));
                    
                    if (expressionPrimary.SyntaxKind == SyntaxKind.VariableReferenceNode)
                    {
                        parserModel.Return_VariableReferenceNode((VariableReferenceNode)expressionPrimary);
                    }
                    else if (expressionPrimary.SyntaxKind == SyntaxKind.FunctionInvocationNode)
                    {
                        parserModel.Return_FunctionInvocationNode((FunctionInvocationNode)expressionPrimary);
                    }
                    
                    return EmptyExpressionNode.Empty;
                }
            }
            else
            {
                // Weird situation?
                // This sounds like it just wouldn't compile.
                //
                // Something like:
                //     1 + 2 3 + 4
                //
                // NOTE: There is no operator between the '2' and the '3'.
                //       It is just two binary expressions side by side.
                //
                // I think you'd want to pretend that the parent binary expression didn't exist
                // for the sake of parser recovery.
                ClearFromExpressionList(expressionPrimary, ref parserModel);
                ClearFromExpressionList(binaryExpressionAntecedent, ref parserModel);
                
                if (expressionPrimary.SyntaxKind == SyntaxKind.VariableReferenceNode)
                {
                    parserModel.Return_VariableReferenceNode((VariableReferenceNode)expressionPrimary);
                }
                else if (expressionPrimary.SyntaxKind == SyntaxKind.FunctionInvocationNode)
                {
                    parserModel.Return_FunctionInvocationNode((FunctionInvocationNode)expressionPrimary);
                }
                    
                parserModel.Return_BinaryExpressionNode(binaryExpressionAntecedent);
                
                return parserModel.Binder.Shared_BadExpressionNode;
            }
        }
        else
        {
            var typeClauseNode = expressionPrimary.ResultTypeReference;
            
            var binaryExpressionNode = parserModel.Rent_BinaryExpressionNode();
            binaryExpressionNode.LeftOperandTypeReference = typeClauseNode;
            binaryExpressionNode.OperatorToken = token;
            binaryExpressionNode.RightOperandTypeReference = typeClauseNode;
            binaryExpressionNode.ResultTypeReference = typeClauseNode;
            
            parserModel.ExpressionList.Add((SyntaxKind.EndOfFileToken, binaryExpressionNode));
            
            if (expressionPrimary.SyntaxKind == SyntaxKind.VariableReferenceNode)
            {
                parserModel.Return_VariableReferenceNode((VariableReferenceNode)expressionPrimary);
            }
            else if (expressionPrimary.SyntaxKind == SyntaxKind.FunctionInvocationNode)
            {
                parserModel.Return_FunctionInvocationNode((FunctionInvocationNode)expressionPrimary);
            }
            
            return EmptyExpressionNode.Empty;
        }
    }

    public static IExpressionNode AmbiguousParenthesizedMergeToken(
        AmbiguousParenthesizedExpressionNode ambiguousParenthesizedExpressionNode, ref CSharpParserModel parserModel)
    {
        var token = parserModel.TokenWalker.Current;
        
        if (token.SyntaxKind == SyntaxKind.CommaToken)
        {
            if (ambiguousParenthesizedExpressionNode.IsParserContextKindForceStatementExpression)
                parserModel.ParserContextKind = CSharpParserContextKind.ForceStatementExpression;
        
            parserModel.ExpressionList.Add((SyntaxKind.CommaToken, ambiguousParenthesizedExpressionNode));
            return EmptyExpressionNode.Empty;
        }
        else if (token.SyntaxKind == SyntaxKind.CloseParenthesisToken)
        {
            parserModel.ParserContextKind = CSharpParserContextKind.None;
        
            if (parserModel.TokenWalker.Next.SyntaxKind == SyntaxKind.EqualsCloseAngleBracketToken)
            {
                return AmbiguousParenthesizedExpressionTransformTo_LambdaExpressionNode(ambiguousParenthesizedExpressionNode, ref parserModel);
            }
            else if (!ambiguousParenthesizedExpressionNode.HasDecidedShouldMatch)
            {
                var parenthesizedExpressionNode = new ParenthesizedExpressionNode(
                    ambiguousParenthesizedExpressionNode.OpenParenthesisToken,
                    CSharpFacts.Types.Void.ToTypeReference());
                    
                parserModel.NoLongerRelevantExpressionNode = ambiguousParenthesizedExpressionNode;
                    
                return parenthesizedExpressionNode;
            }
            else if (ambiguousParenthesizedExpressionNode.ShouldMatchVariableDeclarationNodes &&
                     ambiguousParenthesizedExpressionNode.CountAmbiguousParenthesizedExpressionNodeChildList >= 1)
            {
                return AmbiguousParenthesizedExpressionTransformTo_TypeClauseNode(ambiguousParenthesizedExpressionNode, ref token, ref parserModel);
            }
            else if (!ambiguousParenthesizedExpressionNode.ShouldMatchVariableDeclarationNodes)
            {
                if (ambiguousParenthesizedExpressionNode.CountAmbiguousParenthesizedExpressionNodeChildList > 1)
                {
                    if (ambiguousParenthesizedExpressionNode.IsParserContextKindForceStatementExpression)
                    {
                        var allChildrenAreTypeClauseNode = true;
                        
                        for (int i = ambiguousParenthesizedExpressionNode.IndexAmbiguousParenthesizedExpressionNodeChildList; i < ambiguousParenthesizedExpressionNode.IndexAmbiguousParenthesizedExpressionNodeChildList + ambiguousParenthesizedExpressionNode.CountAmbiguousParenthesizedExpressionNodeChildList; i++)
                        {
                            var node = parserModel.Binder.AmbiguousParenthesizedExpressionNodeChildList[i];
                            if (node.SyntaxKind != SyntaxKind.TypeClauseNode)
                            {
                                allChildrenAreTypeClauseNode = false;
                                break;
                            }
                        }
                        
                        if (allChildrenAreTypeClauseNode)
                            return AmbiguousParenthesizedExpressionTransformTo_TypeClauseNode(ambiguousParenthesizedExpressionNode, ref token, ref parserModel);
                    }
                    
                    return AmbiguousParenthesizedExpressionTransformTo_TupleExpressionNode(ambiguousParenthesizedExpressionNode, expressionSecondary: null, ref parserModel);
                }
                else if (ambiguousParenthesizedExpressionNode.CountAmbiguousParenthesizedExpressionNodeChildList == 1 &&
                         UtilityApi.IsConvertibleToTypeClauseNode(parserModel.Binder.AmbiguousParenthesizedExpressionNodeChildList[ambiguousParenthesizedExpressionNode.IndexAmbiguousParenthesizedExpressionNodeChildList].SyntaxKind) ||
                         parserModel.Binder.AmbiguousParenthesizedExpressionNodeChildList[ambiguousParenthesizedExpressionNode.IndexAmbiguousParenthesizedExpressionNodeChildList].SyntaxKind == SyntaxKind.AmbiguousIdentifierExpressionNode ||
                         parserModel.Binder.AmbiguousParenthesizedExpressionNodeChildList[ambiguousParenthesizedExpressionNode.IndexAmbiguousParenthesizedExpressionNodeChildList].SyntaxKind == SyntaxKind.VariableReferenceNode)
                {
                    return AmbiguousParenthesizedExpressionTransformTo_ExplicitCastNode(
                        ambiguousParenthesizedExpressionNode, (IExpressionNode)parserModel.Binder.AmbiguousParenthesizedExpressionNodeChildList[ambiguousParenthesizedExpressionNode.IndexAmbiguousParenthesizedExpressionNodeChildList], ref token, ref parserModel);
                }
            }
        }
        
        return parserModel.Binder.Shared_BadExpressionNode;
    }
    
    public static IExpressionNode AmbiguousParenthesizedMergeExpression(
        IExpressionNode expressionSecondary, ref CSharpParserModel parserModel)
    {
        var ambiguousParenthesizedExpressionNode = (AmbiguousParenthesizedExpressionNode)parserModel.ExpressionPrimary;
        
        switch (expressionSecondary.SyntaxKind)
        {
            case SyntaxKind.VariableDeclarationNode:
                if (!ambiguousParenthesizedExpressionNode.HasDecidedShouldMatch)
                {
                    ambiguousParenthesizedExpressionNode.ShouldMatchVariableDeclarationNodes = true;
                    ambiguousParenthesizedExpressionNode.HasDecidedShouldMatch = true;
                }
                if (!ambiguousParenthesizedExpressionNode.ShouldMatchVariableDeclarationNodes)
                    break;
            
                if (ambiguousParenthesizedExpressionNode.IsParserContextKindForceStatementExpression)
                    parserModel.ParserContextKind = CSharpParserContextKind.ForceStatementExpression;
                
                parserModel.Binder.AmbiguousParenthesizedExpressionNodeChildList.Insert(
                    ambiguousParenthesizedExpressionNode.IndexAmbiguousParenthesizedExpressionNodeChildList + ambiguousParenthesizedExpressionNode.CountAmbiguousParenthesizedExpressionNodeChildList,
                    expressionSecondary);
                ++ambiguousParenthesizedExpressionNode.CountAmbiguousParenthesizedExpressionNodeChildList;
                return ambiguousParenthesizedExpressionNode;
            case SyntaxKind.AmbiguousIdentifierExpressionNode:
            case SyntaxKind.TypeClauseNode:
            case SyntaxKind.VariableReferenceNode:
                if (!ambiguousParenthesizedExpressionNode.HasDecidedShouldMatch)
                {
                    ambiguousParenthesizedExpressionNode.ShouldMatchVariableDeclarationNodes = false;
                    ambiguousParenthesizedExpressionNode.HasDecidedShouldMatch = true;
                }
                if (ambiguousParenthesizedExpressionNode.ShouldMatchVariableDeclarationNodes)
                    break;
            
                if (ambiguousParenthesizedExpressionNode.IsParserContextKindForceStatementExpression)
                    parserModel.ParserContextKind = CSharpParserContextKind.ForceStatementExpression;
                
                if (expressionSecondary.SyntaxKind == SyntaxKind.AmbiguousIdentifierExpressionNode)
                {
                    var ambiguousIdentifierExpressionNode = (AmbiguousIdentifierExpressionNode)expressionSecondary;
                    expressionSecondary = new AmbiguousIdentifierExpressionNode(
                        ambiguousIdentifierExpressionNode.Token,
                        ambiguousIdentifierExpressionNode.OpenAngleBracketToken,
                        ambiguousIdentifierExpressionNode.IndexGenericParameterEntryList,
                        ambiguousIdentifierExpressionNode.CountGenericParameterEntryList,
                        ambiguousIdentifierExpressionNode.CloseAngleBracketToken,
                        ambiguousIdentifierExpressionNode.ResultTypeReference)
                    {
                        FollowsMemberAccessToken = ambiguousIdentifierExpressionNode.FollowsMemberAccessToken
                    };
                }
                
                parserModel.Binder.AmbiguousParenthesizedExpressionNodeChildList.Insert(
                    ambiguousParenthesizedExpressionNode.IndexAmbiguousParenthesizedExpressionNodeChildList + ambiguousParenthesizedExpressionNode.CountAmbiguousParenthesizedExpressionNodeChildList,
                    expressionSecondary);
                ++ambiguousParenthesizedExpressionNode.CountAmbiguousParenthesizedExpressionNodeChildList;
                return ambiguousParenthesizedExpressionNode;
            case SyntaxKind.AmbiguousParenthesizedExpressionNode:
                // The 'AmbiguousParenthesizedExpressionNode' merging with 'SyntaxToken' method will
                // return the existing 'AmbiguousParenthesizedExpressionNode' in various situations.
                //
                // One of which is to signify the closing brace token.
                return ambiguousParenthesizedExpressionNode;
        }
        
        // 'ambiguousParenthesizedExpressionNode.NodeList.Count > 0' because the current was never added,
        // so if there already is 1, then there'd be many expressions.
        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CommaToken || ambiguousParenthesizedExpressionNode.CountAmbiguousParenthesizedExpressionNodeChildList > 0)
        {
            return AmbiguousParenthesizedExpressionTransformTo_TupleExpressionNode(ambiguousParenthesizedExpressionNode, expressionSecondary, ref parserModel);
        }
        else
        {
            if (expressionSecondary.SyntaxKind == SyntaxKind.EmptyExpressionNode)
                return ambiguousParenthesizedExpressionNode; // '() => ...;
            else
                return AmbiguousParenthesizedExpressionTransformTo_ParenthesizedExpressionNode(ambiguousParenthesizedExpressionNode, expressionSecondary, ref parserModel);
        }
    }
    
    public static IExpressionNode AmbiguousIdentifierMergeToken(
        AmbiguousIdentifierExpressionNode ambiguousIdentifierExpressionNode, ref CSharpParserModel parserModel)
    {
        if (ambiguousIdentifierExpressionNode.IsParsingGenericParameters)
        {
            return GenericParametersListingMergeToken(
                ambiguousIdentifierExpressionNode, ref parserModel);
        }
    
        switch (parserModel.TokenWalker.Current.SyntaxKind)
        {
            case SyntaxKind.OpenParenthesisToken:
            {
                if (ambiguousIdentifierExpressionNode.Token.SyntaxKind == SyntaxKind.IdentifierToken)
                {
                    // TODO: ContextualKeywords as the function identifier?
                    
                    var functionInvocationNode = parserModel.Rent_FunctionInvocationNode();
                    
                    functionInvocationNode.FunctionInvocationIdentifierToken = ambiguousIdentifierExpressionNode.Token;
                    functionInvocationNode.OpenAngleBracketToken = ambiguousIdentifierExpressionNode.OpenAngleBracketToken;
                    functionInvocationNode.IndexGenericParameterEntryList = ambiguousIdentifierExpressionNode.IndexGenericParameterEntryList;
                    functionInvocationNode.CountGenericParameterEntryList = ambiguousIdentifierExpressionNode.CountGenericParameterEntryList;
                    functionInvocationNode.CloseAngleBracketToken = ambiguousIdentifierExpressionNode.CloseAngleBracketToken;
                    functionInvocationNode.OpenParenthesisToken = parserModel.TokenWalker.Current;
                    functionInvocationNode.IndexFunctionParameterEntryList = parserModel.Binder.FunctionParameterEntryList.Count;
                    
                    parserModel.BindFunctionInvocationNode(functionInvocationNode);
                    
                    return ParseFunctionParameterListing_Start(
                        functionInvocationNode, ref parserModel);
                }
                
                goto default;
            }
            case SyntaxKind.OpenAngleBracketToken:
            {
                var token = parserModel.TokenWalker.Current;
                return ParseGenericParameterNode_Start(ambiguousIdentifierExpressionNode, ref token, ref parserModel);
            }
            case SyntaxKind.CloseAngleBracketToken:
            {
                if (ambiguousIdentifierExpressionNode.OpenAngleBracketToken.ConstructorWasInvoked)
                {
                    ambiguousIdentifierExpressionNode.CloseAngleBracketToken = parserModel.TokenWalker.Current;
                    return ambiguousIdentifierExpressionNode;
                }
            
                goto default;
            }
            case SyntaxKind.OpenSquareBracketToken:
            {
                var decidedNode = ForceDecisionAmbiguousIdentifier(
                    EmptyExpressionNode.Empty,
                    ambiguousIdentifierExpressionNode,
                    ref parserModel);
                    
                if (decidedNode.SyntaxKind == SyntaxKind.VariableReferenceNode)
                    return VariableReferenceMergeToken((VariableReferenceNode)decidedNode, ref parserModel);
            
                goto default;
            }
            case SyntaxKind.EqualsToken:
            {
                // TODO: Is this code ever hit?
                parserModel.ExpressionList.Add((SyntaxKind.CommaToken, ambiguousIdentifierExpressionNode));
                return EmptyExpressionNode.Empty;
            }
            case SyntaxKind.EqualsCloseAngleBracketToken:
            {
                var lambdaExpressionNode = new LambdaExpressionNode(CSharpFacts.Types.Void.ToTypeReference());
                SetLambdaExpressionNodeVariableDeclarationNodeList(lambdaExpressionNode, ambiguousIdentifierExpressionNode, ref parserModel);
                
                SyntaxToken openBraceToken;
                
                if (parserModel.TokenWalker.Next.SyntaxKind == SyntaxKind.OpenBraceToken)
                    openBraceToken = parserModel.TokenWalker.Next;
                else
                    openBraceToken = new SyntaxToken(SyntaxKind.OpenBraceToken, parserModel.TokenWalker.Current.TextSpan);
                
                return ParseLambdaExpressionNode(lambdaExpressionNode, ref openBraceToken, ref parserModel);
            }
            case SyntaxKind.IsTokenKeyword:
            {
                var decidedNode = ForceDecisionAmbiguousIdentifier(
                    EmptyExpressionNode.Empty,
                    ambiguousIdentifierExpressionNode,
                    ref parserModel);
                    
                if (decidedNode.SyntaxKind == SyntaxKind.VariableReferenceNode)
                    return VariableReferenceMergeToken((VariableReferenceNode)decidedNode, ref parserModel);
                
                // The goal is to move all the code to VariableReferenceMergeToken(...)
                // when ForceDecisionAmbiguousIdentifier(...) a VariableReferenceNode.
                //
                // I'm keeping this Consume() logic here for now though because
                // I have to fully understand what the result of removing it would be.
                
                _ = parserModel.TokenWalker.Consume(); // Consume the IsTokenKeyword
                
                if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.NotTokenContextualKeyword)
                    _ = parserModel.TokenWalker.Consume(); // Consume the NotTokenKeyword
                    
                if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.NullTokenKeyword)
                {
                    _ = parserModel.TokenWalker.Consume(); // Consume the NullTokenKeyword
                }
                
                return EmptyExpressionNode.Empty;
            }
            case SyntaxKind.AsTokenKeyword:
            {
                var decidedNode = ForceDecisionAmbiguousIdentifier(
                    EmptyExpressionNode.Empty,
                    ambiguousIdentifierExpressionNode,
                    ref parserModel);
                    
                if (decidedNode.SyntaxKind == SyntaxKind.VariableReferenceNode)
                    return VariableReferenceMergeToken((VariableReferenceNode)decidedNode, ref parserModel);
                
                goto default;
            }
            case SyntaxKind.WithTokenContextualKeyword:
            {
                var decidedNode = ForceDecisionAmbiguousIdentifier(
                    EmptyExpressionNode.Empty,
                    ambiguousIdentifierExpressionNode,
                    ref parserModel);
                
                if (decidedNode.SyntaxKind == SyntaxKind.VariableReferenceNode)
                    return VariableReferenceMergeToken((VariableReferenceNode)decidedNode, ref parserModel);
                
                goto default;
            }
            case SyntaxKind.SwitchTokenKeyword:
            {
                var decidedNode = ForceDecisionAmbiguousIdentifier(
                    EmptyExpressionNode.Empty,
                    ambiguousIdentifierExpressionNode,
                    ref parserModel);
                
                if (decidedNode.SyntaxKind == SyntaxKind.VariableReferenceNode)
                    return VariableReferenceMergeToken((VariableReferenceNode)decidedNode, ref parserModel);
                
                goto default;
            }
            case SyntaxKind.PlusPlusToken:
            {
                var decidedNode = ForceDecisionAmbiguousIdentifier(
                    EmptyExpressionNode.Empty,
                    ambiguousIdentifierExpressionNode,
                    ref parserModel);
                
                if (decidedNode.SyntaxKind == SyntaxKind.VariableReferenceNode)
                    return VariableReferenceMergeToken((VariableReferenceNode)decidedNode, ref parserModel);
                
                goto default;
            }
            case SyntaxKind.MinusMinusToken:
            {
                var decidedNode = ForceDecisionAmbiguousIdentifier(
                    EmptyExpressionNode.Empty,
                    ambiguousIdentifierExpressionNode,
                    ref parserModel);
                    
                if (decidedNode.SyntaxKind == SyntaxKind.VariableReferenceNode)
                    return VariableReferenceMergeToken((VariableReferenceNode)decidedNode, ref parserModel);
                
                goto default;
            }
            case SyntaxKind.BangToken:
            case SyntaxKind.QuestionMarkToken:
            {
                var copyAmbiguousIdentifierExpressionNode = ambiguousIdentifierExpressionNode.GetClone();
                
                var decidedNode = ForceDecisionAmbiguousIdentifier(
                    EmptyExpressionNode.Empty,
                    copyAmbiguousIdentifierExpressionNode,
                    ref parserModel);
            
                if (decidedNode.SyntaxKind == SyntaxKind.VariableReferenceNode)
                    return VariableReferenceMergeToken((VariableReferenceNode)decidedNode, ref parserModel);
                
                // The goal is to move all the code to VariableReferenceMergeToken(...)
                // when ForceDecisionAmbiguousIdentifier(...) a VariableReferenceNode.
                //
                // I'm keeping this Consume() logic here for now though because
                // I have to fully understand what the result of removing it would be.
                
                if (parserModel.TokenWalker.Next.SyntaxKind == SyntaxKind.MemberAccessToken)
                    return ambiguousIdentifierExpressionNode;
                
                if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.QuestionMarkToken)
                {
                    ambiguousIdentifierExpressionNode.HasQuestionMark = true;
                    return ambiguousIdentifierExpressionNode;
                }
                
                goto default;
            }
            case SyntaxKind.IdentifierToken:
            {
                var decidedExpression = ForceDecisionAmbiguousIdentifier(
                    EmptyExpressionNode.Empty,
                    ambiguousIdentifierExpressionNode,
                    ref parserModel);
                
                if (decidedExpression.SyntaxKind != SyntaxKind.TypeClauseNode)
                    return decidedExpression;
            
                var identifierToken = parserModel.TokenWalker.Match(SyntaxKind.IdentifierToken);
                
                var variableDeclarationNode = ParseVariables.HandleVariableDeclarationExpression(
                    (TypeClauseNode)decidedExpression,
                    identifierToken,
                    VariableKind.Local,
                    ref parserModel);
                    
                return variableDeclarationNode;
            }
            default:
                return parserModel.Binder.Shared_BadExpressionNode;
        }
    }

    public static IExpressionNode AmbiguousIdentifierMergeExpression(
        IExpressionNode expressionSecondary, ref CSharpParserModel parserModel)
    {
        var ambiguousIdentifierExpressionNode = (AmbiguousIdentifierExpressionNode)parserModel.ExpressionPrimary;
        
        if (ambiguousIdentifierExpressionNode.IsParsingGenericParameters)
        {
            return GenericParametersListingMergeExpression(
                ambiguousIdentifierExpressionNode, expressionSecondary, ref parserModel);
        }
    
        if (ambiguousIdentifierExpressionNode.OpenAngleBracketToken.ConstructorWasInvoked &&
            !ambiguousIdentifierExpressionNode.CloseAngleBracketToken.ConstructorWasInvoked)
        {
            return ambiguousIdentifierExpressionNode;
        }
        
        return parserModel.Binder.Shared_BadExpressionNode;
    }
    
    /// <summary>
    /// This logic duplicated from `ForceDecisionAmbiguousIdentifier`.
    /// 
    /// The issue is:
    /// - ExplicitCast
    /// - GenericParameters
    /// - SyntaxKind.AsTokenKeyword
    ///
    /// will forcibly make a TypeClauseNode under certain scenarios.
    /// But, the current token is not necessarily in the correct place during these scenarios.
    /// Thus invoking `ForceDecisionAmbiguousIdentifier` needs to be looked into further.
    /// </summary>
    public static TypeClauseNode ExplicitCastAndGenericParametersForceType(
        ref SyntaxToken token,
        ref CSharpParserModel parserModel)
    {
        return parserModel.Rent_TypeClauseNode();
        /*_ = parserModel.TryGetTypeDefinitionHierarchically(
                parserModel.ResourceUri,
                parserModel.Compilation,
                parserModel.ScopeCurrentSubIndex,
                parserModel.ResourceUri,
                token.TextSpan,
                out var typeDefinitionNode);
        
        var typeClauseNode = UtilityApi.ConvertTokenToTypeClauseNode(ref token, ref parserModel);
        
        if (!typeDefinitionNode.IsDefault())
        {
            typeClauseNode.ExplicitDefinitionTextSpan = typeDefinitionNode.IdentifierToken.TextSpan;
            typeClauseNode.ExplicitDefinitionResourceUri = typeDefinitionNode.ResourceUri;
        }
        else
        {
            typeClauseNode.ExplicitDefinitionTextSpan = token.TextSpan;
            typeClauseNode.ExplicitDefinitionResourceUri = parserModel.ResourceUri;
        }
        
        if (!typeClauseNode.IsKeywordType)
        {
            var symbolId = parserModel.GetNextSymbolId();
            
            parserModel.Binder.SymbolList.Insert(
                parserModel.Compilation.SymbolOffset + parserModel.Compilation.SymbolLength,
                new Symbol(
                    SyntaxKind.TypeSymbol,
                    symbolId,
                    typeClauseNode.TypeIdentifierToken.TextSpan with
                    {
                        DecorationByte = (byte)GenericDecorationKind.Type
                    }));
            ++parserModel.Compilation.SymbolLength;
            
            if (parserModel.Binder.SymbolIdToExternalTextSpanMap.TryGetValue(parserModel.ResourceUri.Value, out var symbolIdToExternalTextSpanMap) &&
                !typeDefinitionNode.IsDefault() &&
                typeClauseNode.ExplicitDefinitionResourceUri != parserModel.ResourceUri)
            {
                symbolIdToExternalTextSpanMap.TryAdd(
                    symbolId,
                    (typeDefinitionNode.ResourceUri, typeDefinitionNode.IdentifierToken.TextSpan.StartInclusiveIndex));
            }
        }
        
        return typeClauseNode;*/
    }
    
    /// <summary>
    /// TODO: Combine searches for Types, Functions, etc... where possible?
    /// TODO: This method needs to be verified and then made clear that it looks the current tokens.
    /// </summary>
    public static IExpressionNode ForceDecisionAmbiguousIdentifier(
        IExpressionNode expressionPrimary,
        AmbiguousIdentifierExpressionNode ambiguousIdentifierExpressionNode,
        ref CSharpParserModel parserModel,
        bool forceVariableReferenceNode = false,
        bool allowFabricatedUndefinedNode = true)
    {
        IExpressionNode result;
    
        if (parserModel.ParserContextKind == CSharpParserContextKind.ForceStatementExpression)
        {
            parserModel.ParserContextKind = CSharpParserContextKind.None;
            
            if ((parserModel.TokenWalker.Next.SyntaxKind == SyntaxKind.OpenAngleBracketToken ||
                     UtilityApi.IsConvertibleToIdentifierToken(parserModel.TokenWalker.Next.SyntaxKind)) &&
                 parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.MemberAccessToken)
            {
                parserModel.ParserContextKind = CSharpParserContextKind.ForceParseNextIdentifierAsTypeClauseNode;
            }
        }

        if (parserModel.ParserContextKind != CSharpParserContextKind.ForceParseNextIdentifierAsTypeClauseNode &&
            UtilityApi.IsConvertibleToIdentifierToken(ambiguousIdentifierExpressionNode.Token.SyntaxKind))
        {
            if (parserModel.TryGetVariableDeclarationHierarchically(
                    parserModel.ResourceUri,
                    parserModel.Compilation,
                    parserModel.ScopeCurrentSubIndex,
                    parserModel.ResourceUri,
                    ambiguousIdentifierExpressionNode.Token.TextSpan,
                    out var existingVariableDeclarationNode))
            {
                var token = ambiguousIdentifierExpressionNode.Token;
                var identifierToken = UtilityApi.ConvertToIdentifierToken(ref token, ref parserModel);
                
                var variableReferenceNode = parserModel.ConstructAndBindVariableReferenceNode(identifierToken, shouldCreateSymbol: false);
                var symbolId = parserModel.CreateVariableSymbol(variableReferenceNode.VariableIdentifierToken, VariableKind.Local/*existingVariableDeclarationNode.VariableKind*/);
                
                if (parserModel.Binder.SymbolIdToExternalTextSpanMap.TryGetValue(parserModel.ResourceUri.Value, out var symbolIdToExternalTextSpanMap) &&
                    existingVariableDeclarationNode.ResourceUri != parserModel.ResourceUri)
                {
                    symbolIdToExternalTextSpanMap.TryAdd(
                        symbolId,
                        (existingVariableDeclarationNode.ResourceUri, existingVariableDeclarationNode.IdentifierToken.TextSpan.StartInclusiveIndex));
                }
                
                result = variableReferenceNode;
                goto finalize;
            }
        }

        if (!forceVariableReferenceNode && UtilityApi.IsConvertibleToTypeClauseNode(ambiguousIdentifierExpressionNode.Token.SyntaxKind))
        {
            if (parserModel.TryGetTypeDefinitionHierarchically(
                    parserModel.ResourceUri,
                    parserModel.Compilation,
                    parserModel.ScopeCurrentSubIndex,
                    parserModel.ResourceUri,
                    ambiguousIdentifierExpressionNode.Token.TextSpan,
                    out var typeDefinitionNode))
            {
                var token = ambiguousIdentifierExpressionNode.Token;
                
                TypeClauseNode typeClauseNode;
                
                typeClauseNode = UtilityApi.ConvertTokenToTypeClauseNode(ref token, ref parserModel);
                
                typeClauseNode.HasQuestionMark = ambiguousIdentifierExpressionNode.HasQuestionMark;

                typeClauseNode.ExplicitDefinitionTextSpan = typeDefinitionNode.IdentifierToken.TextSpan;
                typeClauseNode.ExplicitDefinitionResourceUri = typeDefinitionNode.ResourceUri;
                
                if (!typeClauseNode.IsKeywordType)
                {
                    var symbolId = parserModel.GetNextSymbolId();
                
                    parserModel.Binder.SymbolList.Insert(
                        parserModel.Compilation.SymbolOffset + parserModel.Compilation.SymbolLength,
                        new Symbol(
                            SyntaxKind.TypeSymbol,
                            symbolId,
                            typeClauseNode.TypeIdentifierToken.TextSpan with
                            {
                                DecorationByte = (byte)GenericDecorationKind.Type
                            }));
                    ++parserModel.Compilation.SymbolLength;
                    
                    if (parserModel.Binder.SymbolIdToExternalTextSpanMap.TryGetValue(parserModel.ResourceUri.Value, out var symbolIdToExternalTextSpanMap) &&
                        typeClauseNode.ExplicitDefinitionResourceUri != parserModel.ResourceUri)
                    {
                        symbolIdToExternalTextSpanMap.TryAdd(
                            symbolId,
                            (typeDefinitionNode.ResourceUri, typeDefinitionNode.IdentifierToken.TextSpan.StartInclusiveIndex));
                    }
                }
                
                result = typeClauseNode;
                goto finalize;
            }
        }

        if (ambiguousIdentifierExpressionNode.Token.SyntaxKind == SyntaxKind.IdentifierToken &&
            ambiguousIdentifierExpressionNode.Token.TextSpan.Length == 1 &&
            // 95 is ASCII code for '_'
            parserModel.TokenWalker.Current.TextSpan.CharIntSum == 95)
        {
            if (!parserModel.TryGetVariableDeclarationHierarchically(
                    parserModel.ResourceUri,
                    parserModel.Compilation,
                    parserModel.ScopeCurrentSubIndex,
                    parserModel.ResourceUri,
                    ambiguousIdentifierExpressionNode.Token.TextSpan,
                    out _))
            {
                parserModel.BindDiscard(ambiguousIdentifierExpressionNode.Token);
                result = ambiguousIdentifierExpressionNode.GetClone();
                goto finalize;
            }
        }

        if (!forceVariableReferenceNode &&
            parserModel.ParserContextKind != CSharpParserContextKind.ForceParseNextIdentifierAsTypeClauseNode &&
            UtilityApi.IsConvertibleToIdentifierToken(ambiguousIdentifierExpressionNode.Token.SyntaxKind))
        {
            if (parserModel.TryGetFunctionHierarchically(
                    parserModel.ResourceUri,
                    parserModel.Compilation,
                    parserModel.ScopeCurrentSubIndex,
                    parserModel.ResourceUri,
                    ambiguousIdentifierExpressionNode.Token.TextSpan,
                    out var functionDefinitionNode))
            {
                var functionInvocationNode = parserModel.Rent_FunctionInvocationNode();
                functionInvocationNode.FunctionInvocationIdentifierToken = ambiguousIdentifierExpressionNode.Token;
                functionInvocationNode.ResultTypeReference = /*functionDefinitionNode?.ReturnTypeReference ??*/ CSharpFacts.Types.Void.ToTypeReference();

                parserModel.Binder.SymbolList.Insert(
                    parserModel.Compilation.SymbolOffset + parserModel.Compilation.SymbolLength,
                    new Symbol(
                        SyntaxKind.FunctionSymbol,
                        parserModel.GetNextSymbolId(),
                        functionInvocationNode.FunctionInvocationIdentifierToken.TextSpan with
                        {
                            DecorationByte = (byte)GenericDecorationKind.Function
                        }));
                ++parserModel.Compilation.SymbolLength;
                
                // TODO: Method groups

                result = functionInvocationNode;
                goto finalize;
            }

            var node = parserModel.Binder.FindPrefix(
                parserModel.Binder.NamespacePrefixTree.__Root,
                ambiguousIdentifierExpressionNode.Token.TextSpan,
                parserModel.ResourceUri.Value);
            if (node is not null)
            {
                var namespaceClauseNode = parserModel.Rent_NamespaceClauseNode();
                namespaceClauseNode.IdentifierToken = ambiguousIdentifierExpressionNode.Token;
                result = namespaceClauseNode;
                
                parserModel.Binder.SymbolList.Insert(
                    parserModel.Compilation.SymbolOffset + parserModel.Compilation.SymbolLength,
                    new Symbol(
                        SyntaxKind.NamespaceSymbol,
                        parserModel.GetNextSymbolId(),
                        ambiguousIdentifierExpressionNode.Token.TextSpan));
                ++parserModel.Compilation.SymbolLength;
                    
                goto finalize;
            }
            
            if (parserModel.TryGetLabelDeclarationHierarchically(
                    parserModel.ResourceUri,
                    parserModel.Compilation,
                    parserModel.ScopeCurrentSubIndex,
                    parserModel.ResourceUri,
                    ambiguousIdentifierExpressionNode.Token.TextSpan,
                    out var labelDefinitionNode))
            {
                var labelReferenceNode = new LabelReferenceNode(ambiguousIdentifierExpressionNode.Token);
                
                parserModel.BindLabelReferenceNode(labelReferenceNode);
                
                result = labelReferenceNode;
                goto finalize;
            }
        }
        
        if (allowFabricatedUndefinedNode)
        {
            // Bind an undefined-(TypeClauseNode or NamespaceClauseNode)
            if (!forceVariableReferenceNode ||
                UtilityApi.IsConvertibleToTypeClauseNode(ambiguousIdentifierExpressionNode.Token.SyntaxKind))
            {
                var token = ambiguousIdentifierExpressionNode.Token;
                
                if (parserModel.TokenWalker.Next.SyntaxKind == SyntaxKind.MemberAccessToken)
                {
                    var namespaceClauseNode = parserModel.Rent_NamespaceClauseNode();
                    namespaceClauseNode.IdentifierToken = token;
                    result = namespaceClauseNode;
                    parserModel.Binder.SymbolList.Insert(
                        parserModel.Compilation.SymbolOffset + parserModel.Compilation.SymbolLength,
                        new Symbol(
                            SyntaxKind.NamespaceSymbol,
                            parserModel.GetNextSymbolId(),
                            ambiguousIdentifierExpressionNode.Token.TextSpan));
                    ++parserModel.Compilation.SymbolLength;
                }
                else
                {
                    TypeClauseNode typeClauseNode = UtilityApi.ConvertTokenToTypeClauseNode(ref token, ref parserModel);

                    typeClauseNode.ExplicitDefinitionTextSpan = ambiguousIdentifierExpressionNode.Token.TextSpan;
                    typeClauseNode.ExplicitDefinitionResourceUri = parserModel.ResourceUri;
    
                    typeClauseNode.HasQuestionMark = ambiguousIdentifierExpressionNode.HasQuestionMark;
                    parserModel.BindTypeClauseNode(typeClauseNode);
                    result = typeClauseNode;
                }

                
                goto finalize;
            }

            // Bind an undefined-variable
            if (UtilityApi.IsConvertibleToIdentifierToken(ambiguousIdentifierExpressionNode.Token.SyntaxKind))
            {
                var token = ambiguousIdentifierExpressionNode.Token;
                var identifierToken = UtilityApi.ConvertToIdentifierToken(ref token, ref parserModel);

                var variableReferenceNode = parserModel.ConstructAndBindVariableReferenceNode(identifierToken);

                result = variableReferenceNode;
                goto finalize;
            }
        }
        
        result = ambiguousIdentifierExpressionNode.GetClone();
        
        goto finalize;
        
        finalize:
        
        parserModel.Return_AmbiguousIdentifierExpressionNode(ambiguousIdentifierExpressionNode);
        
        if (parserModel.TokenWalker.Next.SyntaxKind == SyntaxKind.MemberAccessToken &&
            UtilityApi.IsConvertibleToIdentifierToken(parserModel.TokenWalker.Current.SyntaxKind))
        {
            _ = parserModel.TokenWalker.Consume();
            var token = parserModel.TokenWalker.Current;
            result = ParseMemberAccessToken(result, ref token, ref parserModel);
        }
        
        return result;
    }
    
    public static IExpressionNode VariableReferenceMergeToken(
        VariableReferenceNode variableReferenceNode, ref CSharpParserModel parserModel)
    {
        var token = parserModel.TokenWalker.Current;
        
        switch (token.SyntaxKind)
        {
            case SyntaxKind.EqualsToken:
            {
                // TODO: Is this code ever hit?
                parserModel.ExpressionList.Add((SyntaxKind.CommaToken, variableReferenceNode));
                return EmptyExpressionNode.Empty;
            }
            case SyntaxKind.IsTokenKeyword:
            {
                _ = parserModel.TokenWalker.Consume(); // Consume the IsTokenKeyword
                
                if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.NotTokenContextualKeyword)
                    _ = parserModel.TokenWalker.Consume(); // Consume the NotTokenKeyword
                    
                if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.NullTokenKeyword)
                {
                    _ = parserModel.TokenWalker.Consume(); // Consume the NullTokenKeyword
                }
                
                parserModel.Return_VariableReferenceNode(variableReferenceNode);
                
                return EmptyExpressionNode.Empty;
            }
            case SyntaxKind.AsTokenKeyword:
            {
                _ = parserModel.TokenWalker.Consume(); // Consume the AsTokenKeyword
                
                if (UtilityApi.IsConvertibleToTypeClauseNode(parserModel.TokenWalker.Current.SyntaxKind))
                {
                    var nameableToken = parserModel.TokenWalker.Consume();
                    
                    var typeClauseNode = ExplicitCastAndGenericParametersForceType(
                        ref nameableToken,
                        ref parserModel);
                
                    var typeReference = new TypeReference(typeClauseNode);
                    parserModel.Return_TypeClauseNode(typeClauseNode);
                    parserModel.Return_VariableReferenceNode(variableReferenceNode);
                    return new VariableReferenceNode(
                        nameableToken,
                        new VariableDeclarationNode(
                            typeReference,
                            nameableToken,
                            VariableKind.Local,
                            isInitialized: true,
                            parserModel.ResourceUri));
                }
                
                return EmptyExpressionNode.Empty;
            }
            case SyntaxKind.WithTokenContextualKeyword:
            {
                return new WithExpressionNode(
                    parserModel.Return_VariableReferenceNode_ToStruct(variableReferenceNode));
            }
            case SyntaxKind.SwitchTokenKeyword:
            {
                parserModel.Return_VariableReferenceNode(variableReferenceNode);
                return new SwitchExpressionNode();
            }
            case SyntaxKind.PlusPlusToken:
            {
                return variableReferenceNode;
            }
            case SyntaxKind.MinusMinusToken:
            {
                return variableReferenceNode;
            }
            case SyntaxKind.BangToken:
            case SyntaxKind.QuestionMarkToken:
            {
                return variableReferenceNode;
            }
            case SyntaxKind.CommaToken:
            {
                parserModel.ExpressionList.Add((SyntaxKind.CommaToken, variableReferenceNode));
                return EmptyExpressionNode.Empty;
            }
            case SyntaxKind.OpenSquareBracketToken:
            {
                parserModel.ExpressionList.Add((SyntaxKind.CloseSquareBracketToken, variableReferenceNode));
                parserModel.ExpressionList.Add((SyntaxKind.CommaToken, variableReferenceNode));
                return EmptyExpressionNode.Empty;
            }
            case SyntaxKind.CloseSquareBracketToken:
            {
                if (variableReferenceNode.ResultTypeReference.IndexGenericParameterEntryList != -1 &&
                    variableReferenceNode.ResultTypeReference.CountGenericParameterEntryList == 1)
                {
                    var indexGenericParameterEntryList = variableReferenceNode.ResultTypeReference.IndexGenericParameterEntryList;
                    parserModel.Return_VariableReferenceNode(variableReferenceNode);
                    return new VariableReferenceNode(
                        token,
                        new VariableDeclarationNode(
                            parserModel.Binder.GenericParameterEntryList[indexGenericParameterEntryList].TypeReference,
                            token,
                            VariableKind.Local,
                            isInitialized: true,
                            parserModel.ResourceUri));
                }
                else
                {
                    parserModel.Return_VariableReferenceNode(variableReferenceNode);
                    return new VariableReferenceNode(
                        token,
                        new VariableDeclarationNode(
                            CSharpFacts.Types.Var.ToTypeReference(),
                            token,
                            VariableKind.Local,
                            isInitialized: true,
                            parserModel.ResourceUri));
                }
            }
            default:
                parserModel.Return_VariableReferenceNode(variableReferenceNode);
                return parserModel.Binder.Shared_BadExpressionNode;
        }
    }
    
    public static IExpressionNode VariableReferenceMergeExpression(
        IExpressionNode expressionSecondary, ref CSharpParserModel parserModel)
    {
        var variableReferenceNode = (VariableReferenceNode)parserModel.ExpressionPrimary;
        
        if (expressionSecondary.SyntaxKind == SyntaxKind.AmbiguousIdentifierExpressionNode)
        {
            expressionSecondary = ForceDecisionAmbiguousIdentifier(
                EmptyExpressionNode.Empty,
                (AmbiguousIdentifierExpressionNode)expressionSecondary,
                ref parserModel);
        }
    
        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CloseSquareBracketToken ||
            parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CommaToken)
        {
            if (expressionSecondary.SyntaxKind == SyntaxKind.VariableReferenceNode)
            {
                parserModel.Return_VariableReferenceNode((VariableReferenceNode)expressionSecondary);
            }
            else if (expressionSecondary.SyntaxKind == SyntaxKind.FunctionInvocationNode)
            {
                parserModel.Return_FunctionInvocationNode((FunctionInvocationNode)expressionSecondary);
            }
            else if (expressionSecondary.SyntaxKind == SyntaxKind.BinaryExpressionNode)
            {
                parserModel.Return_BinaryExpressionNode((BinaryExpressionNode)expressionSecondary);
            }
            
            return variableReferenceNode;
        }
    
        parserModel.Return_VariableReferenceNode(variableReferenceNode);
        return parserModel.Binder.Shared_BadExpressionNode;
    }
        
    public static IExpressionNode BadMergeToken(
        BadExpressionNode badExpressionNode, ref CSharpParserModel parserModel)
    {
        // (2025-01-31)
        // ============
        // 'if (typeof(string))' is breaking any text parsed after it in 'CSharpBinder.Main.cs'.
        //
        // There is more to it than just 'if (typeof(string))',
        // the issue actually occurs due to two consecutive 'if (typeof(string))'.
        //
        // Because the parser can recover from this under certain conditions,
        // but the nested 'if (typeof(string))' in 'CSharpBinder.Main.cs' results
        // in plain text syntax highlighting for any code that appears in the remaining methods.
        //
        // The issue is that 'if (...)' adds to 'parserModel.ExpressionList.Add((SyntaxKind.CloseParenthesisToken, null));
        //
        // This results in the expression loop returning (back to the statement loop) upon encountering an unmatched 'CloseParenthesisToken'.
        // But, 'typeof(string)' is not understood by the expression loop.
        //
        // It only will create a FunctionInvocationNode if the "function name is an IdentifierToken / convertible to an IdentifierToken".
        //
        // And the keyword 'typeof' cannot be converted to an IdentifierToken, so it makes a bad expression node.
        //
        // Following that, the bad expression node goes to merge with an 'OpenParenthesisToken',
        // and under normal circumstances an 'OpenParenthesisToken' would 'parserModel.ExpressionList.Add((SyntaxKind.CloseParenthesisToken, ambiguousParenthesizedExpressionNode));'
        //
        // But, when merging with the 'bad expression node' the 'OpenParenthesisToken' does not do this.
        //
        // Thus, the statement loop picks back up at the first 'CloseParenthesisToken' of 'if (typeof(string))'
        // when it should've picked back up at the second 'CloseParenthesisToken'.
        //
        // The statement loop then goes on to presume that the first 'CloseParenthesisToken'
        // was the closing delimiter of the if statement's predicate.
        //
        // So it Matches a 'CloseParenthesisToken', then sets the next token to be the start of the if statement's code block.
        // But, that next token is another 'CloseParenthesisToken'.
        //
        // From here it is presumed that errors start to cascade, and therefore the details are only relevant if wanting to
        // add 'recovery' logic.
        //
        // I think the best 'recovery' logic for this would that an unmatched 'CloseBraceToken' should
        // return to the statement loop.
        //
        // But, as for a fix, the bad expression node needs to 'match' the Parenthesis tokens so that
        // the statement loop picks back up at the second 'CloseParenthesisToken'.
        
        var token = parserModel.TokenWalker.Current;
        
        switch (token.SyntaxKind)
        {
            case SyntaxKind.OpenParenthesisToken:
                parserModel.ExpressionList.Add((SyntaxKind.CloseParenthesisToken, badExpressionNode));
                break;
            case SyntaxKind.OpenBraceToken:
                parserModel.ExpressionList.Add((SyntaxKind.CloseBraceToken, badExpressionNode));
                break;
        }
        
        #if DEBUG
        badExpressionNode.ClobberCount++;
        #endif
        
        return badExpressionNode;
    }

    public static IExpressionNode BadMergeExpression(
        IExpressionNode expressionSecondary, ref CSharpParserModel parserModel)
    {
        var badExpressionNode = (BadExpressionNode)parserModel.ExpressionPrimary;
        
        #if DEBUG
        badExpressionNode.ClobberCount++;
        #endif
        
        if (expressionSecondary.SyntaxKind == SyntaxKind.VariableReferenceNode)
        {
            parserModel.Return_VariableReferenceNode((VariableReferenceNode)expressionSecondary);
        }
        else if (expressionSecondary.SyntaxKind == SyntaxKind.FunctionInvocationNode)
        {
            parserModel.Return_FunctionInvocationNode((FunctionInvocationNode)expressionSecondary);
        }
        else if (expressionSecondary.SyntaxKind == SyntaxKind.BinaryExpressionNode)
        {
            parserModel.Return_BinaryExpressionNode((BinaryExpressionNode)expressionSecondary);
        }
        
        return badExpressionNode;
    }

    public static IExpressionNode BinaryMergeToken(
        BinaryExpressionNode binaryExpressionNode, ref CSharpParserModel parserModel)
    {
        var token = parserModel.TokenWalker.Current;
    
        switch (token.SyntaxKind)
        {
            case SyntaxKind.NumericLiteralToken:
            case SyntaxKind.StringLiteralToken:
            case SyntaxKind.StringInterpolatedStartToken:
            case SyntaxKind.CharLiteralToken:
            case SyntaxKind.FalseTokenKeyword:
            case SyntaxKind.TrueTokenKeyword:
                TypeReference tokenTypeReference;
                
                if (token.SyntaxKind == SyntaxKind.NumericLiteralToken)
                    tokenTypeReference = CSharpFacts.Types.Int.ToTypeReference();
                else if (token.SyntaxKind == SyntaxKind.StringLiteralToken || token.SyntaxKind == SyntaxKind.StringInterpolatedStartToken)
                    tokenTypeReference = CSharpFacts.Types.String.ToTypeReference();
                else if (token.SyntaxKind == SyntaxKind.CharLiteralToken)
                    tokenTypeReference = CSharpFacts.Types.Char.ToTypeReference();
                else if (token.SyntaxKind == SyntaxKind.FalseTokenKeyword || token.SyntaxKind == SyntaxKind.TrueTokenKeyword)
                    tokenTypeReference = CSharpFacts.Types.Bool.ToTypeReference();
                else
                    goto default;
                    
                IExpressionNode rightExpressionNode;
                    
                if (token.SyntaxKind == SyntaxKind.StringInterpolatedStartToken)
                {
                    rightExpressionNode = new InterpolatedStringNode(
                        token,
                        stringInterpolatedEndToken: default,
                        toBeExpressionPrimary: binaryExpressionNode,
                        resultTypeReference: CSharpFacts.Types.String.ToTypeReference());
                }
                else
                {
                    rightExpressionNode = new LiteralExpressionNode(token, tokenTypeReference);
                }
                
                binaryExpressionNode.RightExpressionResultTypeReference = rightExpressionNode.ResultTypeReference;
                
                if (token.SyntaxKind == SyntaxKind.StringInterpolatedStartToken)
                {
                    // Awkwardly double checking the 'token.SyntaxKind' here to avoid duplicating 'binaryExpressionNode.SetRightExpressionNode(rightExpressionNode);'
                    return ParseInterpolatedStringNode((InterpolatedStringNode)rightExpressionNode, ref parserModel);
                }
                
                return binaryExpressionNode;
            case SyntaxKind.PlusToken:
            case SyntaxKind.MinusToken:
            case SyntaxKind.StarToken:
            case SyntaxKind.DivisionToken:
            case SyntaxKind.EqualsEqualsToken:
                // TODO: More generally, the result will be a number, so all that matters is what operators a number can interact with instead of duplicating this code.
                // RETROSPECTIVE: This code reads like nonsense to me. Shouldn't you check '==' not '!='? This 'if' is backwards?
                if (binaryExpressionNode.RightExpressionNodeWasSet)
                {
                    var typeClauseNode = binaryExpressionNode.ResultTypeReference;
                    
                    var newBinaryExpressionNode = parserModel.Rent_BinaryExpressionNode();
                    newBinaryExpressionNode.LeftOperandTypeReference = typeClauseNode;
                    newBinaryExpressionNode.OperatorToken = token;
                    newBinaryExpressionNode.RightOperandTypeReference = typeClauseNode;
                    newBinaryExpressionNode.ResultTypeReference = typeClauseNode;
                    return newBinaryExpressionNode;
                }
                else
                {
                    goto default;
                }
            default:
                return parserModel.Binder.Shared_BadExpressionNode;
        }
    }
    
    public static IExpressionNode BinaryMergeExpression(
        IExpressionNode expressionSecondary, ref CSharpParserModel parserModel)
    {
        var binaryExpressionNode = (BinaryExpressionNode)parserModel.ExpressionPrimary;
    
        if (!binaryExpressionNode.RightExpressionNodeWasSet)
        {
            if (expressionSecondary.SyntaxKind == SyntaxKind.AmbiguousIdentifierExpressionNode)
            {
                expressionSecondary = ForceDecisionAmbiguousIdentifier(
                    EmptyExpressionNode.Empty,
                    (AmbiguousIdentifierExpressionNode)expressionSecondary,
                    ref parserModel);
            }
                
            binaryExpressionNode.RightExpressionResultTypeReference = expressionSecondary.ResultTypeReference;
            
            if (expressionSecondary.SyntaxKind == SyntaxKind.FunctionInvocationNode)
            {
                parserModel.Return_FunctionInvocationNode((FunctionInvocationNode)expressionSecondary);
            }
            else if (expressionSecondary.SyntaxKind == SyntaxKind.VariableReferenceNode)
            {
                parserModel.Return_VariableReferenceNode((VariableReferenceNode)expressionSecondary);
            }
            else if (expressionSecondary.SyntaxKind == SyntaxKind.ConstructorInvocationExpressionNode)
            {
                parserModel.Return_ConstructorInvocationExpressionNode((ConstructorInvocationExpressionNode)expressionSecondary);
            }
            
            return binaryExpressionNode;
        }
    
        if (expressionSecondary.SyntaxKind == SyntaxKind.FunctionInvocationNode)
        {
            parserModel.Return_FunctionInvocationNode((FunctionInvocationNode)expressionSecondary);
        }
        else if (expressionSecondary.SyntaxKind == SyntaxKind.VariableReferenceNode)
        {
            parserModel.Return_VariableReferenceNode((VariableReferenceNode)expressionSecondary);
        }
        
        return parserModel.Binder.Shared_BadExpressionNode;
    }
    
    public static IExpressionNode CollectionInitializationMergeToken(
        CollectionInitializationNode collectionInitializationNode, ref CSharpParserModel parserModel)
    {
        var token = parserModel.TokenWalker.Current;
    
        switch (token.SyntaxKind)
        {
            case SyntaxKind.CloseBraceToken:
            {
                collectionInitializationNode.IsClosed = true;
                return collectionInitializationNode;
            }
            case SyntaxKind.CommaToken:
            {
                if (collectionInitializationNode.IsClosed)
                    goto default;
                
                parserModel.ExpressionList.Add((SyntaxKind.CommaToken, collectionInitializationNode));
                return EmptyExpressionNode.Empty;
            }
            default:
            {
                return parserModel.Binder.Shared_BadExpressionNode;
            }
        }
    }
    
    public static IExpressionNode CollectionInitializationMergeExpression(
         IExpressionNode expressionSecondary, ref CSharpParserModel parserModel)
    {
        var collectionInitializationNode = (CollectionInitializationNode)parserModel.ExpressionPrimary;
    
        if (expressionSecondary.SyntaxKind == SyntaxKind.VariableReferenceNode)
        {
            parserModel.Return_VariableReferenceNode((VariableReferenceNode)expressionSecondary);
        }
        else if (expressionSecondary.SyntaxKind == SyntaxKind.FunctionInvocationNode)
        {
            parserModel.Return_FunctionInvocationNode((FunctionInvocationNode)expressionSecondary);
        }
        else if (expressionSecondary.SyntaxKind == SyntaxKind.ConstructorInvocationExpressionNode)
        {
            parserModel.Return_ConstructorInvocationExpressionNode((ConstructorInvocationExpressionNode)expressionSecondary);
        }
    
        if (collectionInitializationNode.IsClosed)
            return parserModel.Binder.Shared_BadExpressionNode;

        return collectionInitializationNode;
    }
    
    public static IExpressionNode ConstructorInvocationMergeToken(
        ConstructorInvocationExpressionNode constructorInvocationExpressionNode, ref CSharpParserModel parserModel)
    {
        SyntaxToken token;
        
        if (constructorInvocationExpressionNode.IsParsingFunctionParameters)
        {
            token = parserModel.TokenWalker.Current;
            return ParseFunctionParameterListing_Token(constructorInvocationExpressionNode, ref token, ref parserModel);
        }
        
        if (constructorInvocationExpressionNode.ResultTypeReference.IsDefault() &&
            parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenParenthesisToken)
        {
            constructorInvocationExpressionNode.ConstructorInvocationStageKind = ConstructorInvocationStageKind.Type;
            parserModel.ParserContextKind = CSharpParserContextKind.ForceStatementExpression;
            
            parserModel.ExpressionList.Add((SyntaxKind.OpenParenthesisToken, constructorInvocationExpressionNode));
            parserModel.ExpressionList.Add((SyntaxKind.OpenBraceToken, constructorInvocationExpressionNode));
            return EmptyMergeToken(EmptyExpressionNode.Empty, ref parserModel);
        }
        
        switch (parserModel.TokenWalker.Current.SyntaxKind)
        {
            case SyntaxKind.IdentifierToken:
                goto default;
            case SyntaxKind.OpenParenthesisToken:
                constructorInvocationExpressionNode.OpenParenthesisToken = parserModel.TokenWalker.Current;
                constructorInvocationExpressionNode.IndexFunctionParameterEntryList = parserModel.Binder.FunctionParameterEntryList.Count;
        		constructorInvocationExpressionNode.CountFunctionParameterEntryList = 0;
                constructorInvocationExpressionNode.CloseParenthesisToken = default;
                constructorInvocationExpressionNode.ConstructorInvocationStageKind = ConstructorInvocationStageKind.FunctionParameters;
                
                return ParseFunctionParameterListing_Start(constructorInvocationExpressionNode, ref parserModel);
            case SyntaxKind.CloseParenthesisToken:
                if (constructorInvocationExpressionNode.OpenParenthesisToken.ConstructorWasInvoked)
                {
                    constructorInvocationExpressionNode.CloseParenthesisToken = parserModel.TokenWalker.Current;
                    return constructorInvocationExpressionNode;
                }
                else
                {
                    goto default;
                }
            case SyntaxKind.CloseAngleBracketToken:
                constructorInvocationExpressionNode.ConstructorInvocationStageKind = ConstructorInvocationStageKind.Unset;
                return constructorInvocationExpressionNode;
            case SyntaxKind.OpenBraceToken:
                constructorInvocationExpressionNode.ConstructorInvocationStageKind = ConstructorInvocationStageKind.ObjectInitializationParameters;
                parserModel.ExpressionList.Add((SyntaxKind.CloseBraceToken, constructorInvocationExpressionNode));
                parserModel.ExpressionList.Add((SyntaxKind.CommaToken, constructorInvocationExpressionNode));
                token = parserModel.TokenWalker.Current;
                return ParseObjectInitialization(constructorInvocationExpressionNode, ref token, ref parserModel);
            case SyntaxKind.CloseBraceToken:
                if (constructorInvocationExpressionNode.ConstructorInvocationStageKind == ConstructorInvocationStageKind.ObjectInitializationParameters)
                {
                    constructorInvocationExpressionNode.ConstructorInvocationStageKind = ConstructorInvocationStageKind.Unset;
                    return constructorInvocationExpressionNode;
                }
                
                goto default;
            case SyntaxKind.CommaToken:
                parserModel.ExpressionList.Add((SyntaxKind.CommaToken, constructorInvocationExpressionNode));
                token = parserModel.TokenWalker.Current;
                return ParseObjectInitialization(constructorInvocationExpressionNode, ref token, ref parserModel);
            default:
                return parserModel.Binder.Shared_BadExpressionNode;
        }
    }
    
    public static IExpressionNode ConstructorInvocationMergeExpression(
        IExpressionNode expressionSecondary, ref CSharpParserModel parserModel)
    {
        var constructorInvocationExpressionNode = (ConstructorInvocationExpressionNode)parserModel.ExpressionPrimary;
    
        if (constructorInvocationExpressionNode.IsParsingFunctionParameters)
        {
            return ParseFunctionParameterListing_Expression(
                constructorInvocationExpressionNode, expressionSecondary, ref parserModel);
        }
    
        if (expressionSecondary.SyntaxKind == SyntaxKind.AmbiguousIdentifierExpressionNode)
        {
            expressionSecondary = ForceDecisionAmbiguousIdentifier(
                constructorInvocationExpressionNode,
                (AmbiguousIdentifierExpressionNode)expressionSecondary,
                ref parserModel);
        }
    
        if (expressionSecondary.SyntaxKind == SyntaxKind.EmptyExpressionNode)
            return constructorInvocationExpressionNode;
            
        switch (constructorInvocationExpressionNode.ConstructorInvocationStageKind)
        {
            case ConstructorInvocationStageKind.GenericParameters:
            {
                if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CloseAngleBracketToken &&
                    expressionSecondary is TypeClauseNode typeClauseNode)
                {
                    typeClauseNode.CloseAngleBracketToken = parserModel.TokenWalker.Current;
                    constructorInvocationExpressionNode.ResultTypeReference = new TypeReference(typeClauseNode);
                    parserModel.Return_TypeClauseNode(typeClauseNode);
                    return constructorInvocationExpressionNode;
                }
                
                goto default;
            }
            case ConstructorInvocationStageKind.FunctionParameters:
            {
                if (constructorInvocationExpressionNode.OpenParenthesisToken.ConstructorWasInvoked)
                    return constructorInvocationExpressionNode;
                goto default;
            }
            case ConstructorInvocationStageKind.ObjectInitializationParameters:
            {
                return constructorInvocationExpressionNode;
            }
            case ConstructorInvocationStageKind.Type:
            {
                if (expressionSecondary is TypeClauseNode typeClauseNode)
                {
                    constructorInvocationExpressionNode.ResultTypeReference = new TypeReference(typeClauseNode);
                    parserModel.Return_TypeClauseNode(typeClauseNode);
                }
                else
                {
                    constructorInvocationExpressionNode.ResultTypeReference = CSharpFacts.Types.Void.ToTypeReference();
                }
                
                constructorInvocationExpressionNode.ConstructorInvocationStageKind = ConstructorInvocationStageKind.Unset;
                parserModel.ParserContextKind = CSharpParserContextKind.None;
                
                if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenBraceToken)
                    ClearFromExpressionList(constructorInvocationExpressionNode, ref parserModel);
                
                return constructorInvocationExpressionNode;
            }
            default:
            {
                return parserModel.Binder.Shared_BadExpressionNode;
            }
        }
    }
    
    /// <summary>
    /// CurrentToken is to either be 'OpenBraceToken', or 'CommaToken' when invoking this method.
    /// </summary>
    public static IExpressionNode ParseObjectInitialization(
        ConstructorInvocationExpressionNode constructorInvocationExpressionNode, ref SyntaxToken token, ref CSharpParserModel parserModel)
    {
        // Consume either 'OpenBraceToken', or 'CommaToken'
        _ = parserModel.TokenWalker.Consume();
        
        var isVoidType = false;
        
        // TODO: This is not a good solution...:
        //
        // Out of the following 5 examples of object initialization.
        // person1 to person4 inclusive work WITHOUT the need for this "void" check code.
        // This "void" check code only pertains to this case: 'Person person5 = new() { FirstName = "Jane" };'
        //
        // var person1 = new Person() { FirstName = "Jane" };
        // var person2 = new Person { FirstName = "Jane" };
        // Person person3 = new Person() { FirstName = "Jane" };
        // Person person4 = new Person { FirstName = "Jane" };
        // Person person5 = new() { FirstName = "Jane" };
        //
        // The issue is that constructor invocation syntax marks that the type being constructed has been
        // constructed. And in this person5 case, a 'void' type is assigned
        // if no type had been provided, AND the position index no longer is valid to define the type.
        //
        // Since 'void' for some reason ends up being used in person5 case,
        // then the 'constructorInvocationExpressionNode.ResultTypeReference == default'
        // doesn't trigger the 'parserModel.MostRecentLeftHandSideAssignmentExpressionTypeClauseNode'.
        //
        if (parserModel.Binder.__CompilationUnitMap.TryGetValue(constructorInvocationExpressionNode.ResultTypeReference.ExplicitDefinitionResourceUri, out var innerCompilationUnit))
        {
            if (parserModel.Binder.CSharpCompilerService.SafeCompareText(constructorInvocationExpressionNode.ResultTypeReference.ExplicitDefinitionResourceUri.Value, "void", constructorInvocationExpressionNode.ResultTypeReference.TypeIdentifierToken.TextSpan))
                isVoidType = true;
        }
        
        if (constructorInvocationExpressionNode.ResultTypeReference.IsDefault() || isVoidType)
            constructorInvocationExpressionNode.ResultTypeReference = parserModel.MostRecentLeftHandSideAssignmentExpressionTypeClauseNode;
        
        if (UtilityApi.IsConvertibleToIdentifierToken(parserModel.TokenWalker.Current.SyntaxKind) &&
            parserModel.TokenWalker.Next.SyntaxKind == SyntaxKind.EqualsToken)
        {
            var memberAccessToken = new SyntaxToken(
                SyntaxKind.MemberAccessToken,
                new TextEditorTextSpan(
                    0,
                    0,
                    0))
                {
                    IsFabricated = true
                };
        
            return ParseMemberAccessToken(new TypeClauseNode(constructorInvocationExpressionNode.ResultTypeReference), ref memberAccessToken, ref parserModel);
        }
    
        return EmptyExpressionNode.Empty;
    }
    
    public static IExpressionNode WithMergeToken(
        WithExpressionNode withExpressionNode, ref CSharpParserModel parserModel)
    {
        var token = parserModel.TokenWalker.Current;
        
        switch (token.SyntaxKind)
        {
            case SyntaxKind.OpenBraceToken:
                parserModel.ExpressionList.Add((SyntaxKind.CloseBraceToken, withExpressionNode));
                parserModel.ExpressionList.Add((SyntaxKind.CommaToken, withExpressionNode));
                return ParseWithExpressionNode(withExpressionNode, ref token, ref parserModel);
            case SyntaxKind.CloseBraceToken:                
                return withExpressionNode;
            case SyntaxKind.CommaToken:
                parserModel.ExpressionList.Add((SyntaxKind.CommaToken, withExpressionNode));
                return ParseWithExpressionNode(withExpressionNode, ref token, ref parserModel);
            default:
                return parserModel.Binder.Shared_BadExpressionNode;
        }
    }
    
    public static IExpressionNode WithMergeExpression(
        IExpressionNode expressionSecondary, ref CSharpParserModel parserModel)
    {
        var withExpressionNode = (WithExpressionNode)parserModel.ExpressionPrimary;
        return withExpressionNode;
    }
    
    public static IExpressionNode ParseWithExpressionNode(
        WithExpressionNode withExpressionNode,
        ref SyntaxToken token,
        ref CSharpParserModel parserModel)
    {
        // Consume either 'OpenBraceToken', or 'CommaToken'
        _ = parserModel.TokenWalker.Consume();
        
        if (UtilityApi.IsConvertibleToIdentifierToken(parserModel.TokenWalker.Current.SyntaxKind) &&
            parserModel.TokenWalker.Next.SyntaxKind == SyntaxKind.EqualsToken)
        {
            var memberAccessToken = new SyntaxToken(
                SyntaxKind.MemberAccessToken,
                new TextEditorTextSpan(
                    0,
                    0,
                    0))
                {
                    IsFabricated = true
                };
        
            return ParseMemberAccessToken(new TypeClauseNode(withExpressionNode.ResultTypeReference), ref memberAccessToken, ref parserModel);
        }
    
        return EmptyExpressionNode.Empty;
    }
    
    public static IExpressionNode HandleKeywordFunctionOperator(
        EmptyExpressionNode emptyExpressionNode, ref SyntaxToken token, ref CSharpParserModel parserModel)
    {
        if (parserModel.TokenWalker.Next.SyntaxKind != SyntaxKind.OpenParenthesisToken)
            return emptyExpressionNode;
        
        token = parserModel.TokenWalker.Consume(); // keyword
        _ = parserModel.TokenWalker.Consume(); // OpenParenthesisToken
        
        TypeReference typeReference;
        
        if (token.SyntaxKind == SyntaxKind.SizeofTokenKeyword)
            typeReference = CSharpFacts.Types.Int.ToTypeReference();
        else if (token.SyntaxKind == SyntaxKind.TypeofTokenKeyword)
            typeReference = CSharpFacts.Types.Var.ToTypeReference();
        else if (token.SyntaxKind == SyntaxKind.DefaultTokenKeyword)
            typeReference = CSharpFacts.Types.Var.ToTypeReference();
        else if (token.SyntaxKind == SyntaxKind.NameofTokenContextualKeyword)
            typeReference = CSharpFacts.Types.String.ToTypeReference();
        else
            typeReference = CSharpFacts.Types.Var.ToTypeReference();
        
        var variableDeclarationNode = new VariableDeclarationNode(
            typeReference,
            token,
            VariableKind.Local,
            isInitialized: true,
            parserModel.ResourceUri);
        
        var keywordFunctionOperatorNode = new KeywordFunctionOperatorNode(token, variableDeclarationNode);
        
        parserModel.ExpressionList.Add((SyntaxKind.CloseParenthesisToken, keywordFunctionOperatorNode));
        return EmptyExpressionNode.Empty;
    }
    
    public static IExpressionNode EmptyMergeToken(
        EmptyExpressionNode emptyExpressionNode, ref CSharpParserModel parserModel)
    {
        var token = parserModel.TokenWalker.Current;
    
        if (UtilityApi.IsConvertibleToTypeClauseNode(token.SyntaxKind) && token.SyntaxKind != SyntaxKind.NameofTokenContextualKeyword)
        {
            var ambiguousExpressionNode = parserModel.Rent_AmbiguousIdentifierExpressionNode();
            ambiguousExpressionNode.Token = token;
            ambiguousExpressionNode.FollowsMemberAccessToken = emptyExpressionNode.FollowsMemberAccessToken;
            
            if (parserModel.TokenWalker.Next.SyntaxKind == SyntaxKind.StatementDelimiterToken && !ambiguousExpressionNode.FollowsMemberAccessToken ||
                parserModel.TryParseExpressionSyntaxKindList.Contains(SyntaxKind.TypeClauseNode) && parserModel.TokenWalker.Next.SyntaxKind != SyntaxKind.WithTokenContextualKeyword &&
                parserModel.TokenWalker.Next.SyntaxKind != SyntaxKind.EqualsCloseAngleBracketToken)
            {
                return ForceDecisionAmbiguousIdentifier(
                    emptyExpressionNode,
                    ambiguousExpressionNode,
                    ref parserModel);
            }
            
            return ambiguousExpressionNode;
        }
    
        switch (token.SyntaxKind)
        {
            case SyntaxKind.NumericLiteralToken:
            case SyntaxKind.StringLiteralToken:
            case SyntaxKind.StringInterpolatedStartToken:
            case SyntaxKind.CharLiteralToken:
            case SyntaxKind.FalseTokenKeyword:
            case SyntaxKind.TrueTokenKeyword:
                TypeReference tokenTypeReference;
                
                if (token.SyntaxKind == SyntaxKind.NumericLiteralToken)
                    tokenTypeReference = CSharpFacts.Types.Int.ToTypeReference();
                else if (token.SyntaxKind == SyntaxKind.StringLiteralToken || token.SyntaxKind == SyntaxKind.StringInterpolatedStartToken)
                    tokenTypeReference = CSharpFacts.Types.String.ToTypeReference();
                else if (token.SyntaxKind == SyntaxKind.CharLiteralToken)
                    tokenTypeReference = CSharpFacts.Types.Char.ToTypeReference();
                else if (token.SyntaxKind == SyntaxKind.FalseTokenKeyword || token.SyntaxKind == SyntaxKind.TrueTokenKeyword)
                    tokenTypeReference = CSharpFacts.Types.Bool.ToTypeReference();
                else
                    goto default;
                
                if (token.SyntaxKind == SyntaxKind.StringInterpolatedStartToken)
                {
                    var interpolatedStringNode = new InterpolatedStringNode(
                        token,
                        stringInterpolatedEndToken: default,
                        toBeExpressionPrimary: null,
                        resultTypeReference: CSharpFacts.Types.String.ToTypeReference());
                    
                    return ParseInterpolatedStringNode(interpolatedStringNode, ref parserModel);
                }
                    
                return new LiteralExpressionNode(token, tokenTypeReference);
            case SyntaxKind.OpenParenthesisToken:
                return ShareEmptyExpressionNodeIntoOpenParenthesisTokenCase(ref token, ref parserModel);
            case SyntaxKind.OpenBraceToken:
                var collectionInitializationNode = new CollectionInitializationNode();
                parserModel.ExpressionList.Add((SyntaxKind.CloseBraceToken, collectionInitializationNode));
                parserModel.ExpressionList.Add((SyntaxKind.CommaToken, collectionInitializationNode));
                return EmptyExpressionNode.Empty;
            case SyntaxKind.NewTokenKeyword:
                var constructorInvocationNode = parserModel.Rent_ConstructorInvocationExpressionNode();
                constructorInvocationNode.NewKeywordToken = token;
                return constructorInvocationNode;
            case SyntaxKind.AwaitTokenContextualKeyword:
                return emptyExpressionNode;
            case SyntaxKind.AsyncTokenContextualKeyword:
                return emptyExpressionNode;
                // return new LambdaExpressionNode(CSharpFacts.Types.Void.ToTypeReference());
            case SyntaxKind.DollarSignToken:
            case SyntaxKind.AtToken:
                return emptyExpressionNode;
            case SyntaxKind.OutTokenKeyword:
                parserModel.ParameterModifierKind = ParameterModifierKind.Out;
                return emptyExpressionNode;
            case SyntaxKind.InTokenKeyword:
                parserModel.ParameterModifierKind = ParameterModifierKind.In;
                return emptyExpressionNode;
            case SyntaxKind.RefTokenKeyword:
                parserModel.ParameterModifierKind = ParameterModifierKind.Ref;
                return emptyExpressionNode;
            case SyntaxKind.ParamsTokenKeyword:
                parserModel.ParameterModifierKind = ParameterModifierKind.Params;
                return emptyExpressionNode;
            case SyntaxKind.ThisTokenKeyword:
                parserModel.ParameterModifierKind = ParameterModifierKind.This;
                return emptyExpressionNode;
            case SyntaxKind.ReadonlyTokenKeyword:
                // TODO: Is the readonly keyword valid C# here?
                parserModel.ParameterModifierKind = ParameterModifierKind.Readonly;
                return emptyExpressionNode;
            case SyntaxKind.SizeofTokenKeyword:
            case SyntaxKind.DefaultTokenKeyword:
            case SyntaxKind.TypeofTokenKeyword:
            case SyntaxKind.NameofTokenContextualKeyword:
                return HandleKeywordFunctionOperator(emptyExpressionNode, ref token, ref parserModel);
            case SyntaxKind.OpenAngleBracketToken:
                // TODO: If text is "<Apple>" it no longer parses as generic parameters...
                // ...now there needs to be something prior to the OpenAngleBracketToken that opens the possibility
                // for generic parameters. (2025-03-16)
                //
                /*var genericParameterListing = new GenericParameterListing(
                    token,
                    new List<GenericParameterEntry>(),
                    closeAngleBracketToken: default);
                
                parserModel.ExpressionList.Add((SyntaxKind.CloseAngleBracketToken, genericParameterListing));
                parserModel.ExpressionList.Add((SyntaxKind.CommaToken, genericParameterListing));
                
                return genericParameterListing;*/
                goto default;
            case SyntaxKind.GotoTokenKeyword:
            {
                if (parserModel.TokenWalker.Peek(2).SyntaxKind == SyntaxKind.StatementDelimiterToken)
                {
                    _ = parserModel.TokenWalker.Consume(); // Consume 'goto'
                    
                    if (UtilityApi.IsConvertibleToIdentifierToken(parserModel.TokenWalker.Current.SyntaxKind))
                    {
                        var nameableToken = parserModel.TokenWalker.Consume(); // Consume 'NameableToken'
                        var identifierToken = UtilityApi.ConvertToIdentifierToken(ref nameableToken, ref parserModel);
                        
                        var labelReferenceNode = new LabelReferenceNode(identifierToken);
                        
                        parserModel.BindLabelReferenceNode(labelReferenceNode);
                    }
                }
                
                return EmptyExpressionNode.Empty;
            }
            case SyntaxKind.ReturnTokenKeyword:
                var returnStatementNode = new ReturnStatementNode(token, EmptyExpressionNode.Empty);
                parserModel.ExpressionList.Add((SyntaxKind.EndOfFileToken, returnStatementNode));
                return EmptyExpressionNode.Empty;
            case SyntaxKind.BangToken:
            case SyntaxKind.PipeToken:
            case SyntaxKind.PipePipeToken:
            case SyntaxKind.AmpersandToken:
            case SyntaxKind.AmpersandAmpersandToken:
            case SyntaxKind.PlusPlusToken:
            case SyntaxKind.MinusMinusToken:
                return emptyExpressionNode;
            default:
                return parserModel.Binder.Shared_BadExpressionNode;
        }
    }
    
    public static IExpressionNode ShareEmptyExpressionNodeIntoOpenParenthesisTokenCase(
        ref SyntaxToken token, ref CSharpParserModel parserModel)
    {
        // This conditional branch is meant for '(2)' where the parenthesized expression node is
        // wrapping a numeric literal node / etc...
        //
        // First check if for NOT equaling '()' due to empty parameters for a lambda expression.
        if (parserModel.TokenWalker.Next.SyntaxKind != SyntaxKind.CloseParenthesisToken &&
            !UtilityApi.IsConvertibleToTypeClauseNode(parserModel.TokenWalker.Next.SyntaxKind))
        {
            var parenthesizedExpressionNode = new ParenthesizedExpressionNode(
                token,
                CSharpFacts.Types.Void.ToTypeReference());
            
            parserModel.ExpressionList.Add((SyntaxKind.CloseParenthesisToken, parenthesizedExpressionNode));
            parserModel.ExpressionList.Add((SyntaxKind.CommaToken, parenthesizedExpressionNode));
            
            return EmptyExpressionNode.Empty;
        }
    
        var ambiguousParenthesizedExpressionNode = new AmbiguousParenthesizedExpressionNode(
            token,
            isParserContextKindForceStatementExpression: parserModel.ParserContextKind == CSharpParserContextKind.ForceStatementExpression ||
                // '(List<(int, bool)>)' required the following hack because the CSharpParserContextKind.ForceStatementExpression enum
                // is reset after the first TypeClauseNode in a statement is made, and there was no clear way to set it back again in this situation.;
                // TODO: Don't do this '(List<(int, bool)>)', instead figure out how to have CSharpParserContextKind.ForceStatementExpression live longer in a statement that has many TypeClauseNode(s).
                parserModel.ExpressionList.Any(x => x.ExpressionNode is IGenericParameterNode genericParameterNode && genericParameterNode.IsParsingGenericParameters));
            
        parserModel.ExpressionList.Add((SyntaxKind.CloseParenthesisToken, ambiguousParenthesizedExpressionNode));
        parserModel.ExpressionList.Add((SyntaxKind.CommaToken, ambiguousParenthesizedExpressionNode));
        return EmptyExpressionNode.Empty;
    }
    
    public static IExpressionNode ExplicitCastMergeToken(
        ExplicitCastNode explicitCastNode, ref CSharpParserModel parserModel)
    {
        var token = parserModel.TokenWalker.Current;
        
        switch (token.SyntaxKind)
        {
            case SyntaxKind.OpenParenthesisToken:
                return EmptyMergeToken(EmptyExpressionNode.Empty, ref parserModel);
            case SyntaxKind.CloseParenthesisToken:
                explicitCastNode.CloseParenthesisToken = token;
                return explicitCastNode;
            case SyntaxKind.IdentifierToken:
                var expressionNode = ForceDecisionAmbiguousIdentifier(
                    EmptyExpressionNode.Empty,
                    new AmbiguousIdentifierExpressionNode(
                        token,
                        openAngleBracketToken: default,
                		indexGenericParameterEntryList: -1,
                        countGenericParameterEntryList: 0,
                		closeAngleBracketToken: default,
                        resultTypeReference: default),
                        ref parserModel);
            
                return new VariableReferenceNode(
                    token,
                    new VariableDeclarationNode(
                        explicitCastNode.ResultTypeReference,
                        token,
                        VariableKind.Local,
                        isInitialized: true,
                        parserModel.ResourceUri));
            default:
                return parserModel.Binder.Shared_BadExpressionNode;
        }
    }
    
    public static IExpressionNode ReturnStatementMergeToken(
        ReturnStatementNode returnStatementNode, ref CSharpParserModel parserModel)
    {
        var token = parserModel.TokenWalker.Current;
        
        switch (token.SyntaxKind)
        {
            default:
                return parserModel.Binder.Shared_BadExpressionNode;
        }
    }
    
    public static IExpressionNode ReturnStatementMergeExpression(
        IExpressionNode expressionSecondary, ref CSharpParserModel parserModel)
    {
        var returnStatementNode = (ReturnStatementNode)parserModel.ExpressionPrimary;
        return parserModel.Binder.Shared_BadExpressionNode;
    }
    
    public static IExpressionNode KeywordFunctionOperatorMergeToken(
        KeywordFunctionOperatorNode keywordFunctionOperatorNode, ref CSharpParserModel parserModel)
    {
        var token = parserModel.TokenWalker.Current;
        
        if (token.SyntaxKind == SyntaxKind.CloseParenthesisToken)
            _ = parserModel.TokenWalker.Consume();
        
        return keywordFunctionOperatorNode.ExpressionNodeToMakePrimary;
    }
    
    public static IExpressionNode KeywordFunctionOperatorMergeExpression(
        IExpressionNode expressionSecondary, ref CSharpParserModel parserModel)
    {
        var keywordFunctionOperatorNode = (KeywordFunctionOperatorNode)parserModel.ExpressionPrimary;
        
        if (expressionSecondary.SyntaxKind == SyntaxKind.AmbiguousIdentifierExpressionNode)
        {
            ForceDecisionAmbiguousIdentifier(
                EmptyExpressionNode.Empty,
                (AmbiguousIdentifierExpressionNode)expressionSecondary,
                ref parserModel);
        }
        
        if (expressionSecondary.SyntaxKind == SyntaxKind.VariableReferenceNode)
        {
            parserModel.Return_VariableReferenceNode((VariableReferenceNode)expressionSecondary);
        }
        else if (expressionSecondary.SyntaxKind == SyntaxKind.FunctionInvocationNode)
        {
            parserModel.Return_FunctionInvocationNode((FunctionInvocationNode)expressionSecondary);
        }
    
        return keywordFunctionOperatorNode;
    }
    
    public static IExpressionNode SwitchExpressionMergeToken(
        SwitchExpressionNode switchExpressionNode, ref CSharpParserModel parserModel)
    {
        var token = parserModel.TokenWalker.Current;
        
        if (token.SyntaxKind == SyntaxKind.OpenBraceToken)
        {
            parserModel.ExpressionList.Add((SyntaxKind.CloseBraceToken, switchExpressionNode));
            return EmptyExpressionNode.Empty;
        }
        
        return parserModel.Binder.Shared_BadExpressionNode;
    }
    
    public static IExpressionNode SwitchExpressionMergeExpression(
        IExpressionNode expressionSecondary, ref CSharpParserModel parserModel)
    {
        var switchExpressionNode = (SwitchExpressionNode)parserModel.ExpressionPrimary;
        
        if (expressionSecondary.SyntaxKind == SyntaxKind.AmbiguousIdentifierExpressionNode)
        {
            ForceDecisionAmbiguousIdentifier(
                EmptyExpressionNode.Empty,
                (AmbiguousIdentifierExpressionNode)expressionSecondary,
                ref parserModel);
        }
        
        if (expressionSecondary.SyntaxKind == SyntaxKind.VariableReferenceNode)
        {
            parserModel.Return_VariableReferenceNode((VariableReferenceNode)expressionSecondary);
        }
        else if (expressionSecondary.SyntaxKind == SyntaxKind.FunctionInvocationNode)
        {
            parserModel.Return_FunctionInvocationNode((FunctionInvocationNode)expressionSecondary);
        }
        
        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CloseBraceToken)
            return switchExpressionNode;
    
        return parserModel.Binder.Shared_BadExpressionNode;
    }
    
    public static IExpressionNode LambdaMergeToken(
        LambdaExpressionNode lambdaExpressionNode, ref CSharpParserModel parserModel)
    {
        var token = parserModel.TokenWalker.Current;
    
        if (token.SyntaxKind == SyntaxKind.EqualsCloseAngleBracketToken)
        {
            var textSpan = new TextEditorTextSpan(
                token.TextSpan.StartInclusiveIndex,
                token.TextSpan.EndExclusiveIndex,
                (byte)GenericDecorationKind.None);
        
            parserModel.Binder.SymbolList.Insert(
                parserModel.Compilation.SymbolOffset + parserModel.Compilation.SymbolLength,
                new Symbol(SyntaxKind.LambdaSymbol, parserModel.GetNextSymbolId(), textSpan));
            ++parserModel.Compilation.SymbolLength;
        
            if (parserModel.TokenWalker.Next.SyntaxKind == SyntaxKind.OpenBraceToken)
            {
                lambdaExpressionNode.CodeBlockNodeIsExpression = false;
            
                parserModel.ExpressionList.Add((SyntaxKind.CloseBraceToken, lambdaExpressionNode));
                parserModel.ExpressionList.Add((SyntaxKind.StatementDelimiterToken, lambdaExpressionNode));
                return EmptyExpressionNode.Empty;
            }
            
            parserModel.ExpressionList.Add((SyntaxKind.StatementDelimiterToken, lambdaExpressionNode));
            return EmptyExpressionNode.Empty;
        }
        else if (token.SyntaxKind == SyntaxKind.StatementDelimiterToken)
        {
            if (lambdaExpressionNode.CodeBlockNodeIsExpression)
            {
                return lambdaExpressionNode;
            }
            else
            {
                parserModel.ExpressionList.Add((SyntaxKind.StatementDelimiterToken, lambdaExpressionNode));
                return EmptyExpressionNode.Empty;
            }
        }
        else if (token.SyntaxKind == SyntaxKind.CloseBraceToken)
        {
            if (lambdaExpressionNode.CodeBlockNodeIsExpression)
            {
                return parserModel.Binder.Shared_BadExpressionNode;
            }
            else
            {
                return lambdaExpressionNode;
            }
        }
        else if (token.SyntaxKind == SyntaxKind.OpenParenthesisToken)
        {
            if (lambdaExpressionNode.HasReadParameters)
            {
                return parserModel.Binder.Shared_BadExpressionNode;
            }
            else
            {
                lambdaExpressionNode.HasReadParameters = true;
                parserModel.ExpressionList.Add((SyntaxKind.CloseParenthesisToken, lambdaExpressionNode));
                parserModel.ExpressionList.Add((SyntaxKind.CommaToken, lambdaExpressionNode));
                return EmptyExpressionNode.Empty;
            }
        }
        else if (token.SyntaxKind == SyntaxKind.CloseParenthesisToken)
        {
            return lambdaExpressionNode;
        }
        else if (token.SyntaxKind == SyntaxKind.EqualsToken)
        {
            if (parserModel.TokenWalker.Next.SyntaxKind == SyntaxKind.CloseAngleBracketToken)
                return lambdaExpressionNode;
            
            return parserModel.Binder.Shared_BadExpressionNode;
        }
        else if (token.SyntaxKind == SyntaxKind.CommaToken)
        {
            parserModel.ExpressionList.Add((SyntaxKind.CommaToken, lambdaExpressionNode));
            return EmptyExpressionNode.Empty;
        }
        else if (token.SyntaxKind == SyntaxKind.IdentifierToken)
        {
            if (lambdaExpressionNode.HasReadParameters)
            {
                return parserModel.Binder.Shared_BadExpressionNode;
            }
            else
            {
                lambdaExpressionNode.HasReadParameters = true;
                return lambdaExpressionNode;
            }
        }
        else
        {
            return parserModel.Binder.Shared_BadExpressionNode;
        }
    }
    
    public static IExpressionNode LambdaMergeExpression(
        IExpressionNode expressionSecondary, ref CSharpParserModel parserModel)
    {
        var lambdaExpressionNode = (LambdaExpressionNode)parserModel.ExpressionPrimary;
    
        if (expressionSecondary.SyntaxKind == SyntaxKind.AmbiguousIdentifierExpressionNode)
        {
            expressionSecondary = ForceDecisionAmbiguousIdentifier(
                EmptyExpressionNode.Empty,
                (AmbiguousIdentifierExpressionNode)expressionSecondary,
                ref parserModel);
        }
        
        if (expressionSecondary.SyntaxKind == SyntaxKind.VariableReferenceNode)
        {
            parserModel.Return_VariableReferenceNode((VariableReferenceNode)expressionSecondary);
        }
        else if (expressionSecondary.SyntaxKind == SyntaxKind.FunctionInvocationNode)
        {
            parserModel.Return_FunctionInvocationNode((FunctionInvocationNode)expressionSecondary);
        }
        else if (expressionSecondary.SyntaxKind == SyntaxKind.BinaryExpressionNode)
        {
            parserModel.Return_BinaryExpressionNode((BinaryExpressionNode)expressionSecondary);
        }
    
        if (parserModel.Binder.ScopeList[parserModel.Compilation.ScopeOffset + lambdaExpressionNode.SelfScopeSubIndex].CodeBlock_StartInclusiveIndex == -1)
            CloseLambdaExpressionScope(lambdaExpressionNode, ref parserModel);
        
        return lambdaExpressionNode;
    }

    public static IExpressionNode LiteralMergeToken(
        LiteralExpressionNode literalExpressionNode, ref CSharpParserModel parserModel)
    {
        return parserModel.Binder.Shared_BadExpressionNode;
    }
    
    public static IExpressionNode InterpolatedStringMergeToken(
        InterpolatedStringNode interpolatedStringNode, ref CSharpParserModel parserModel)
    {
        var token = parserModel.TokenWalker.Current;
    
        if (token.SyntaxKind == SyntaxKind.StringInterpolatedEndToken)
        {
            return interpolatedStringNode;
        }
        else if (token.SyntaxKind == SyntaxKind.StringInterpolatedContinueToken)
        {
            parserModel.ExpressionList.Add((SyntaxKind.StringInterpolatedContinueToken, interpolatedStringNode));
            return EmptyExpressionNode.Empty;
        }

        return parserModel.Binder.Shared_BadExpressionNode;
    }
    
    public static IExpressionNode InterpolatedStringMergeExpression(
        IExpressionNode expressionSecondary, ref CSharpParserModel parserModel)
    {
        var interpolatedStringNode = (InterpolatedStringNode)parserModel.ExpressionPrimary;
    
        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.StringInterpolatedEndToken)
        {
            if (expressionSecondary.SyntaxKind == SyntaxKind.AmbiguousIdentifierExpressionNode)
                expressionSecondary = ForceDecisionAmbiguousIdentifier(EmptyExpressionNode.Empty, (AmbiguousIdentifierExpressionNode)expressionSecondary, ref parserModel);

            interpolatedStringNode.StringInterpolatedEndToken = parserModel.TokenWalker.Current;
            
            if (expressionSecondary.SyntaxKind == SyntaxKind.VariableReferenceNode)
            {
                parserModel.Return_VariableReferenceNode((VariableReferenceNode)expressionSecondary);
            }
            else if (expressionSecondary.SyntaxKind == SyntaxKind.FunctionInvocationNode)
            {
                parserModel.Return_FunctionInvocationNode((FunctionInvocationNode)expressionSecondary);
            }

            // Interpolated strings have their interpolated expressions inserted into the syntax token list
            // immediately following the StringInterpolatedStartToken itself.
            //
            // They are deliminated by StringInterpolatedEndToken,
            // upon which this 'LiteralMergeExpression' will be invoked.
            //
            // Just return back the 'interpolatedStringNode.ToBeExpressionPrimary'.
            return interpolatedStringNode.ToBeExpressionPrimary ?? interpolatedStringNode;
        }
        else if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.StringInterpolatedContinueToken)
        {
            if (expressionSecondary.SyntaxKind == SyntaxKind.AmbiguousIdentifierExpressionNode)
                expressionSecondary = ForceDecisionAmbiguousIdentifier(EmptyExpressionNode.Empty, (AmbiguousIdentifierExpressionNode)expressionSecondary, ref parserModel);

            if (expressionSecondary.SyntaxKind == SyntaxKind.VariableReferenceNode)
            {
                parserModel.Return_VariableReferenceNode((VariableReferenceNode)expressionSecondary);
            }
            else if (expressionSecondary.SyntaxKind == SyntaxKind.FunctionInvocationNode)
            {
                parserModel.Return_FunctionInvocationNode((FunctionInvocationNode)expressionSecondary);
            }
            else if (expressionSecondary.SyntaxKind == SyntaxKind.BinaryExpressionNode)
            {
                parserModel.Return_BinaryExpressionNode((BinaryExpressionNode)expressionSecondary);
            }
            
            return interpolatedStringNode;
        }
        else
        {
            if (expressionSecondary.SyntaxKind == SyntaxKind.VariableReferenceNode)
            {
                parserModel.Return_VariableReferenceNode((VariableReferenceNode)expressionSecondary);
            }
            else if (expressionSecondary.SyntaxKind == SyntaxKind.FunctionInvocationNode)
            {
                parserModel.Return_FunctionInvocationNode((FunctionInvocationNode)expressionSecondary);
            }
            
            return parserModel.Binder.Shared_BadExpressionNode;
        }
    }
    
    public static IExpressionNode ParenthesizedMergeToken(
        ParenthesizedExpressionNode parenthesizedExpressionNode, ref CSharpParserModel parserModel)
    {
        switch (parserModel.TokenWalker.Current.SyntaxKind)
        {
            case SyntaxKind.CloseParenthesisToken:
                if (parenthesizedExpressionNode.InnerExpression.SyntaxKind == SyntaxKind.TypeClauseNode)
                {
                    var typeClauseNode = (TypeClauseNode)parenthesizedExpressionNode.InnerExpression;
                    var explicitCastNode = new ExplicitCastNode(parenthesizedExpressionNode.OpenParenthesisToken, new TypeReference(typeClauseNode));
                    parserModel.Return_TypeClauseNode(typeClauseNode);
                    return ExplicitCastMergeToken(explicitCastNode, ref parserModel);
                }
                
                parenthesizedExpressionNode.CloseParenthesisToken = parserModel.TokenWalker.Current;
                return parenthesizedExpressionNode;
            case SyntaxKind.EqualsCloseAngleBracketToken:
                // TODO: I think this switch case needs to be removed. With the addition of the AmbiguousParenthesizedExpressionNode code...
                // ...(what is about to be said needs confirmation) the parser now only creates the parenthesized expression in the
                // absence of the 'EqualsCloseAngleBracketToken'?
                var lambdaExpressionNode = new LambdaExpressionNode(CSharpFacts.Types.Void.ToTypeReference());
                SetLambdaExpressionNodeVariableDeclarationNodeList(lambdaExpressionNode, parenthesizedExpressionNode.InnerExpression, ref parserModel);
                return lambdaExpressionNode;
            default:
                return parserModel.Binder.Shared_BadExpressionNode;
        }
    }
    
    public static IExpressionNode ParenthesizedMergeExpression(
        IExpressionNode expressionSecondary, ref CSharpParserModel parserModel)
    {
        var parenthesizedExpressionNode = (ParenthesizedExpressionNode)parserModel.ExpressionPrimary;
    
        if (parserModel.TokenWalker.Next.SyntaxKind == SyntaxKind.EqualsCloseAngleBracketToken)
        {
            // TODO: I think this conditional branch needs to be removed. With the addition of the AmbiguousParenthesizedExpressionNode code...
            // ...(what is about to be said needs confirmation) the parser now only creates the parenthesized expression in the
            // absence of the 'EqualsCloseAngleBracketToken'?
            var lambdaExpressionNode = new LambdaExpressionNode(CSharpFacts.Types.Void.ToTypeReference());
            return SetLambdaExpressionNodeVariableDeclarationNodeList(lambdaExpressionNode, expressionSecondary, ref parserModel);
        }
    
        if (expressionSecondary.SyntaxKind == SyntaxKind.AmbiguousIdentifierExpressionNode)
            expressionSecondary = ForceDecisionAmbiguousIdentifier(parenthesizedExpressionNode, (AmbiguousIdentifierExpressionNode)expressionSecondary, ref parserModel);
    
        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CommaToken)
        {
            parserModel.NoLongerRelevantExpressionNode = parenthesizedExpressionNode;
            var tupleExpressionNode = new TupleExpressionNode();
            // tupleExpressionNode.InnerExpressionList.Add(expressionSecondary);
            // tupleExpressionNode never saw the 'OpenParenthesisToken' so the 'ParenthesizedExpressionNode'
            // has to create the ExpressionList entry on behalf of the 'TupleExpressionNode'.
            parserModel.ExpressionList.Add((SyntaxKind.CloseParenthesisToken, tupleExpressionNode));
            return tupleExpressionNode;
        }
    
        if (parenthesizedExpressionNode.InnerExpression.SyntaxKind != SyntaxKind.EmptyExpressionNode)
            return parserModel.Binder.Shared_BadExpressionNode;
        
        // TODO: This seems like a bad idea?
        if (expressionSecondary.SyntaxKind == SyntaxKind.VariableReferenceNode)
        {
             var variableReferenceNode = (VariableReferenceNode)expressionSecondary;
             
             if (variableReferenceNode.IsFabricated)
             {
                 var typeClauseNode = parserModel.Rent_TypeClauseNode();
                 typeClauseNode.TypeIdentifierToken = variableReferenceNode.VariableIdentifierToken;
                parserModel.BindTypeClauseNode(typeClauseNode);
                
                var typeReference = new TypeReference(typeClauseNode);
                parserModel.Return_TypeClauseNode(typeClauseNode);
                return new ExplicitCastNode(parenthesizedExpressionNode.OpenParenthesisToken, typeReference);
             }
        }

        parenthesizedExpressionNode.InnerExpression = expressionSecondary;
        return parenthesizedExpressionNode;
    }
    
    public static IExpressionNode TupleMergeToken(
        TupleExpressionNode tupleExpressionNode, ref CSharpParserModel parserModel)
    {
        var token = parserModel.TokenWalker.Current;
    
        switch (token.SyntaxKind)
        {
            case SyntaxKind.CloseParenthesisToken:
                return tupleExpressionNode;
            case SyntaxKind.CommaToken:
                // TODO: Track the CloseParenthesisToken and ensure it isn't 'ConstructorWasInvoked'.
                parserModel.ExpressionList.Add((SyntaxKind.CommaToken, tupleExpressionNode));
                return EmptyExpressionNode.Empty;
            default:
                return parserModel.Binder.Shared_BadExpressionNode;
        }
    }
    
    public static IExpressionNode TupleMergeExpression(
        IExpressionNode expressionSecondary, ref CSharpParserModel parserModel)
    {
        var tupleExpressionNode = (TupleExpressionNode)parserModel.ExpressionPrimary;
        
        switch (expressionSecondary.SyntaxKind)
        {
            case SyntaxKind.TupleExpressionNode:
                return tupleExpressionNode;
            default:
                if (expressionSecondary.SyntaxKind == SyntaxKind.AmbiguousIdentifierExpressionNode)
                {
                    expressionSecondary = ForceDecisionAmbiguousIdentifier(
                        EmptyExpressionNode.Empty,
                        (AmbiguousIdentifierExpressionNode)expressionSecondary,
                        ref parserModel);
                }
                
                if (expressionSecondary.SyntaxKind == SyntaxKind.VariableReferenceNode)
                {
                    parserModel.Return_VariableReferenceNode((VariableReferenceNode)expressionSecondary);
                }
                else if (expressionSecondary.SyntaxKind == SyntaxKind.FunctionInvocationNode)
                {
                    parserModel.Return_FunctionInvocationNode((FunctionInvocationNode)expressionSecondary);
                }
                
                if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CommaToken || parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CloseParenthesisToken)
                {
                    return tupleExpressionNode;
                }
            
                return parserModel.Binder.Shared_BadExpressionNode;
        }
    }
    
    public static IExpressionNode TypeClauseMergeToken(
        TypeClauseNode typeClauseNode, ref CSharpParserModel parserModel)
    {
        if (typeClauseNode.IsParsingGenericParameters)
            return GenericParametersListingMergeToken(typeClauseNode, ref parserModel);
        
        SyntaxToken token;
    
        switch (parserModel.TokenWalker.Current.SyntaxKind)
        {
            case SyntaxKind.OpenAngleBracketToken:
                token = parserModel.TokenWalker.Current;
                return ParseGenericParameterNode_Start(typeClauseNode, ref token, ref parserModel);
            case SyntaxKind.CloseAngleBracketToken:
                if (typeClauseNode.OpenAngleBracketToken.ConstructorWasInvoked)
                {
                    typeClauseNode.CloseAngleBracketToken = parserModel.TokenWalker.Current;
                    return typeClauseNode;
                }
                
                goto default;
            case SyntaxKind.QuestionMarkToken:
                if (!typeClauseNode.HasQuestionMark)
                {
                    typeClauseNode.HasQuestionMark = true;
                    return typeClauseNode;
                }
                
                goto default;
            case SyntaxKind.OpenParenthesisToken:
                if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenParenthesisToken &&
                    UtilityApi.IsConvertibleToIdentifierToken(typeClauseNode.TypeIdentifierToken.SyntaxKind))
                {
                    var typeClauseToken = typeClauseNode.TypeIdentifierToken;
                    
                    var functionInvocationNode = parserModel.Rent_FunctionInvocationNode();
                    functionInvocationNode.FunctionInvocationIdentifierToken = UtilityApi.ConvertToIdentifierToken(ref typeClauseToken, ref parserModel);
                    functionInvocationNode.OpenAngleBracketToken = typeClauseNode.OpenAngleBracketToken;
                    functionInvocationNode.IndexGenericParameterEntryList = typeClauseNode.IndexGenericParameterEntryList;
                    functionInvocationNode.CountGenericParameterEntryList = typeClauseNode.CountGenericParameterEntryList;
                    functionInvocationNode.CloseAngleBracketToken = typeClauseNode.CloseAngleBracketToken;
                    functionInvocationNode.OpenParenthesisToken = parserModel.TokenWalker.Current;
                    functionInvocationNode.IndexFunctionParameterEntryList = parserModel.Binder.FunctionParameterEntryList.Count;
                        
                    parserModel.BindFunctionInvocationNode(functionInvocationNode);
        
                    return ParseFunctionParameterListing_Start(functionInvocationNode, ref parserModel);
                }
                
                goto default;
            case SyntaxKind.CommaToken:
                if (typeClauseNode.ArrayRank == 1)
                {
                    typeClauseNode.TypeKind = TypeKind.ArrayMultiDimensional;
                }
                
                if (typeClauseNode.TypeKind == TypeKind.ArrayMultiDimensional)
                {
                    ++typeClauseNode.ArrayRank;
                    parserModel.ExpressionList.Add((SyntaxKind.CommaToken, typeClauseNode));
                    return typeClauseNode;
                }
                else
                {
                    goto default;
                }
            case SyntaxKind.OpenSquareBracketToken:
                if (typeClauseNode.ArrayRank == 0)
                {
                    typeClauseNode.TypeKind = TypeKind.ArrayJagged;
                    
                    if (parserModel.TokenWalker.Next.SyntaxKind != SyntaxKind.CloseSquareBracketToken)
                    {
                        parserModel.ExpressionList.Add((SyntaxKind.CloseSquareBracketToken, typeClauseNode));
                        parserModel.ExpressionList.Add((SyntaxKind.CommaToken, typeClauseNode));
                    }
                }
            
                if (typeClauseNode.TypeKind != TypeKind.ArrayMultiDimensional)
                {
                    ++typeClauseNode.ArrayRank;
                }
                
                return typeClauseNode;
            case SyntaxKind.CloseSquareBracketToken:
                return typeClauseNode;
            default:
                if (UtilityApi.IsConvertibleToIdentifierToken(parserModel.TokenWalker.Current.SyntaxKind))
                {
                    token = parserModel.TokenWalker.Current;
                    var identifierToken = UtilityApi.ConvertToIdentifierToken(ref token, ref parserModel);
                    var isRootExpression = true;
                    var hasAmbiguousParenthesizedExpressionNode = false;
                    
                    foreach (var tuple in parserModel.ExpressionList)
                    {
                        if (tuple.ExpressionNode is null)
                            continue;
                        
                        isRootExpression = false;
                        
                        if (tuple.ExpressionNode.SyntaxKind == SyntaxKind.AmbiguousParenthesizedExpressionNode)
                        {
                            hasAmbiguousParenthesizedExpressionNode = true;
                            break;
                        }
                    }
                    
                    VariableDeclarationNode variableDeclarationNode;
                    
                    if (isRootExpression)
                    {
                        // If isRootExpression do not bind the VariableDeclarationNode
                        // because it could in reality be a FunctionDefinitionNode.
                        //
                        // So, manually create the node, and then eventually return back to the
                        // statement code so it can check for a FunctionDefinitionNode.
                        //
                        // If it truly is a VariableDeclarationNode,
                        // then it is the responsibility of the statement code
                        // to bind the VariableDeclarationNode, and add it to the current code block builder.
                        
                        var nameToken = identifierToken;
                        
                        if (parserModel.ScopeCurrent.OwnerSyntaxKind == SyntaxKind.TypeDefinitionNode &&
                            parserModel.TokenWalker.Next.SyntaxKind == SyntaxKind.MemberAccessToken)
                        {
                            parserModel.ParserContextKind = CSharpParserContextKind.None;
                            var ambiguousExpressionNode = new AmbiguousIdentifierExpressionNode(
                                parserModel.TokenWalker.Current,
                                openAngleBracketToken: default,
                                indexGenericParameterEntryList: -1,
                                countGenericParameterEntryList: 0,
                                closeAngleBracketToken: default,
                                resultTypeReference: default);
                            var expressionNode = ForceDecisionAmbiguousIdentifier(typeClauseNode, ambiguousExpressionNode, ref parserModel);
                            nameToken = parserModel.Binder.GetNameToken(expressionNode);
                        }
                        
                        if (parserModel.Compilation.CompilationUnitKind == CompilationUnitKind.SolutionWide_DefinitionsOnly ||
                            parserModel.Compilation.CompilationUnitKind == CompilationUnitKind.SolutionWide_MinimumLocalsData)
                        {
                            variableDeclarationNode = parserModel.Rent_TemporaryLocalVariableDeclarationNode();
                            variableDeclarationNode.TypeReference = new TypeReference(typeClauseNode);
                            variableDeclarationNode.IdentifierToken = nameToken;
                            variableDeclarationNode.VariableKind = VariableKind.Local;
                            variableDeclarationNode.IsInitialized = false;
                            variableDeclarationNode.ResourceUri = parserModel.ResourceUri;
                            variableDeclarationNode._isFabricated = false;
                        }
                        else
                        {
                            variableDeclarationNode = new VariableDeclarationNode(
                                new TypeReference(typeClauseNode),
                                nameToken,
                                VariableKind.Local,
                                false,
                                parserModel.ResourceUri);
                        }
                        parserModel.Return_TypeClauseNode(typeClauseNode);
                    }
                    else
                    {
                        if (hasAmbiguousParenthesizedExpressionNode)
                        {
                            variableDeclarationNode = new VariableDeclarationNode(
                                new TypeReference(typeClauseNode),
                                identifierToken,
                                VariableKind.Local,
                                false,
                                parserModel.ResourceUri);
                            parserModel.Return_TypeClauseNode(typeClauseNode);
                            parserModel.CreateVariableSymbol(variableDeclarationNode.IdentifierToken, variableDeclarationNode.VariableKind);
                        }
                        else
                        {
                            // TODO: Why is typeClauseNode not returned here, but it is in the other conditional branches?
                            variableDeclarationNode = ParseVariables.HandleVariableDeclarationExpression(
                                typeClauseNode,
                                identifierToken,
                                VariableKind.Local,
                                ref parserModel);
                        }
                    }
                        
                    return variableDeclarationNode;
                }
                
                return parserModel.Binder.Shared_BadExpressionNode;
        }
    }
    
    public static IExpressionNode TypeClauseMergeExpression(
        IExpressionNode expressionSecondary, ref CSharpParserModel parserModel)
    {
        var typeClauseNode = (TypeClauseNode)parserModel.ExpressionPrimary;
        
        if (typeClauseNode.IsParsingGenericParameters)
        {
            return GenericParametersListingMergeExpression(
                typeClauseNode, expressionSecondary, ref parserModel);
        }
    
        switch (expressionSecondary.SyntaxKind)
        {
            case SyntaxKind.GenericParametersListingNode:
                if (typeClauseNode.OpenAngleBracketToken.ConstructorWasInvoked &&
                    !typeClauseNode.CloseAngleBracketToken.ConstructorWasInvoked)
                {
                    return typeClauseNode;
                }
                
                goto default;
            case SyntaxKind.TypeClauseNode:
                if (typeClauseNode == expressionSecondary)
                {
                    // When parsing multi-dimensional arrays.
                    return typeClauseNode;
                }
                
                goto default;
            default:
                return parserModel.Binder.Shared_BadExpressionNode;
        }
    }
    
    public static IExpressionNode FunctionInvocationMergeToken(
        FunctionInvocationNode functionInvocationNode, ref CSharpParserModel parserModel)
    {
        SyntaxToken token;
    
        if (functionInvocationNode.IsParsingFunctionParameters)
        {
            token = parserModel.TokenWalker.Current;
            return ParseFunctionParameterListing_Token(functionInvocationNode, ref token, ref parserModel);
        }
        else if (functionInvocationNode.IsParsingGenericParameters)
        {
            return GenericParametersListingMergeToken(functionInvocationNode, ref parserModel);
        }

        switch (parserModel.TokenWalker.Current.SyntaxKind)
        {
            case SyntaxKind.OpenAngleBracketToken:
                if (!functionInvocationNode.OpenParenthesisToken.ConstructorWasInvoked)
                {
                    // Note: non member access function invocation takes the path:
                    //       AmbiguousIdentifierExpressionNode -> FunctionInvocationNode
                    //
                    // ('AmbiguousIdentifierExpressionNode' converts when it sees 'OpenParenthesisToken')
                    //
                    //
                    // But, member access will determine that an identifier is a function
                    // prior to seeing the 'OpenAngleBracketToken' or the 'OpenParenthesisToken'.
                    //
                    // These paths would preferably be combined into a less "hacky" two way path.
                    // Until then these 'if (functionInvocationNode.FunctionParametersListingNode is null)'
                    // statements will be here.
                    
                    if (functionInvocationNode.OpenAngleBracketToken.ConstructorWasInvoked)
                        goto default;
                    
                    token = parserModel.TokenWalker.Current;
                    return ParseGenericParameterNode_Start(functionInvocationNode, ref token, ref parserModel);
                }
                
                goto default;
            case SyntaxKind.CloseAngleBracketToken:
                return functionInvocationNode;
            case SyntaxKind.OpenParenthesisToken:
                if (!functionInvocationNode.OpenParenthesisToken.ConstructorWasInvoked)
                {
                    // Note: non member access function invocation takes the path:
                    //       AmbiguousIdentifierExpressionNode -> FunctionInvocationNode
                    //
                    // ('AmbiguousIdentifierExpressionNode' converts when it sees 'OpenParenthesisToken')
                    //
                    //
                    // But, member access will determine that an identifier is a function
                    // prior to seeing the 'OpenAngleBracketToken' or the 'OpenParenthesisToken'.
                    //
                    // These paths would preferably be combined into a less "hacky" two way path.
                    // Until then these 'if (functionInvocationNode.FunctionParametersListingNode is null)'
                    // statements will be here.
                    
                    functionInvocationNode.OpenParenthesisToken = parserModel.TokenWalker.Current;
                    functionInvocationNode.IndexFunctionParameterEntryList = parserModel.Binder.FunctionParameterEntryList.Count;
            		functionInvocationNode.CountFunctionParameterEntryList = 0;
                    functionInvocationNode.CloseParenthesisToken = default;
                    
                    return ParseFunctionParameterListing_Start(
                        functionInvocationNode, ref parserModel);
                }

                goto default;
            case SyntaxKind.CloseParenthesisToken:
                functionInvocationNode.CloseParenthesisToken = parserModel.TokenWalker.Current;
                return functionInvocationNode;
            default:
                parserModel.Return_FunctionInvocationNode(functionInvocationNode);
                return parserModel.Binder.Shared_BadExpressionNode;
        }
    }
    
    public static IExpressionNode FunctionInvocationMergeExpression(
        IExpressionNode expressionSecondary, ref CSharpParserModel parserModel)
    {
        var functionInvocationNode = (FunctionInvocationNode)parserModel.ExpressionPrimary;
    
        if (functionInvocationNode.IsParsingFunctionParameters)
        {
            return ParseFunctionParameterListing_Expression(
                functionInvocationNode, expressionSecondary, ref parserModel);
        }
        else if (functionInvocationNode.IsParsingGenericParameters)
        {
            return GenericParametersListingMergeExpression(
                functionInvocationNode, expressionSecondary, ref parserModel);
        }
    
        if (expressionSecondary.SyntaxKind == SyntaxKind.AmbiguousIdentifierExpressionNode)
        {
            expressionSecondary = ForceDecisionAmbiguousIdentifier(functionInvocationNode, (AmbiguousIdentifierExpressionNode)expressionSecondary, ref parserModel);
        }
    
        switch (expressionSecondary.SyntaxKind)
        {
            case SyntaxKind.EmptyExpressionNode:
                return functionInvocationNode;
            case SyntaxKind.FunctionInvocationNode:
                return functionInvocationNode;
            default:
                parserModel.Return_FunctionInvocationNode(functionInvocationNode);
                return parserModel.Binder.Shared_BadExpressionNode;
        }
    }
    
    public static IExpressionNode SetLambdaExpressionNodeVariableDeclarationNodeList(
        LambdaExpressionNode lambdaExpressionNode, IExpressionNode expressionNode, ref CSharpParserModel parserModel)
    {
        if (expressionNode.SyntaxKind == SyntaxKind.AmbiguousIdentifierExpressionNode)
        {
            var token = ((AmbiguousIdentifierExpressionNode)expressionNode).Token;
            
            if (token.SyntaxKind != SyntaxKind.IdentifierToken)
                return lambdaExpressionNode;
        
            var variableDeclarationNode = new VariableDeclarationNode(
                TypeFacts.Empty.ToTypeReference(),
                token,
                VariableKind.Local,
                isInitialized: false,
                parserModel.ResourceUri);
                
            parserModel.Binder.LambdaExpressionNodeChildList.Insert(
                lambdaExpressionNode.IndexLambdaExpressionNodeChildList + lambdaExpressionNode.CountLambdaExpressionNodeChildList,
                variableDeclarationNode);
            ++lambdaExpressionNode.CountLambdaExpressionNodeChildList;
        }
        
        return lambdaExpressionNode;
    }
    
    /// <summary>
    /// A ParenthesizedExpressionNode expression will "become" a CommaSeparatedExpressionNode
    /// upon encounter a CommaToken within its parentheses.
    ///
    /// An issue arises however, because the parserModel.ExpressionList still says to
    /// "short circuit" when the CloseParenthesisToken is encountered,
    /// and to at this point make the ParenthesizedExpressionNode the primary expression.
    ///
    /// Well, the ParenthesizedExpressionNode should no longer exist, it was deemed
    /// to be more accurately described by a CommaSeparatedExpressionNode.
    ///
    /// So, this method will remove any entries in the parserModel.ExpressionList
    /// that have the 'ParenthesizedExpressionNode' as the to-be primary expression.
    /// </summary>
    public static void ClearFromExpressionList(IExpressionNode expressionNode, ref CSharpParserModel parserModel)
    {
        for (int i = parserModel.ExpressionList.Count - 1; i > -1; i--)
        {
            if (expressionNode == parserModel.ExpressionList[i].ExpressionNode)
                parserModel.ExpressionList.RemoveAt(i);
        }
    }
    
    /// <summary>
    /// 'bool foundChild' usage:
    /// If the child is NOT in the ExpressionList then this is true,
    ///
    /// But, if the child is in the ExpressionList, and is not the final entry in the ExpressionList,
    /// then this needs to be set to 'false', otherwise a descendent node of 'childExpressionNode'
    /// will be thought to be the parent node due to the list being traversed end to front order.
    /// </summary>
    public static IExpressionNode GetParentNode(
        IExpressionNode childExpressionNode, ref CSharpParserModel parserModel, bool foundChild = true)
    {
        for (int i = parserModel.ExpressionList.Count - 1; i > -1; i--)
        {
            var delimiterExpressionTuple = parserModel.ExpressionList[i];
            
            if (foundChild)
            {
                if (delimiterExpressionTuple.ExpressionNode is null)
                    break;
                    
                if (childExpressionNode != delimiterExpressionTuple.ExpressionNode)
                    return delimiterExpressionTuple.ExpressionNode;
            }
            else
            {
                if (childExpressionNode == delimiterExpressionTuple.ExpressionNode)
                    foundChild = true;
            }
        }
        
        return EmptyExpressionNode.Empty;
    }
    
    public static IExpressionNode ParseLambdaExpressionNode(LambdaExpressionNode lambdaExpressionNode, ref SyntaxToken openBraceToken, ref CSharpParserModel parserModel)
    {
        // If the lambda expression's code block is a single expression then there is no end delimiter.
        // Instead, it is the parent expression's delimiter that causes the lambda expression's code block to short circuit.
        // At this moment, the lambda expression is given whatever expression was able to be parsed and can take it as its "code block".
        // And then restore the parent expression as the expressionPrimary.
        //
        // -----------------------------------------------------------------------------------------------------------------------------
        //
        // If the lambda expression's code block is deliminated by braces
        // then the end delimiter is the CloseBraceToken.
        // But, we can only add a "short circuit" for 'CloseBraceToken and lambdaExpressionNode'
        // if we have seen the 'OpenBraceToken'.
        
        if (parserModel.TokenWalker.Next.SyntaxKind == SyntaxKind.OpenBraceToken)
        {
            OpenLambdaExpressionScope(lambdaExpressionNode, ref openBraceToken, ref parserModel);
            return SkipLambdaExpressionStatements(lambdaExpressionNode, ref parserModel);
        }
        else
        {
            parserModel.ExpressionList.Add((SyntaxKind.EndOfFileToken, lambdaExpressionNode));
            OpenLambdaExpressionScope(lambdaExpressionNode, ref openBraceToken, ref parserModel);
            if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.EqualsCloseAngleBracketToken)
            {
                _ = parserModel.TokenWalker.Consume(); // EqualsCloseAngleBracketToken
            }
            return EmptyExpressionNode.Empty;
        }
    }
    
    public static void OpenLambdaExpressionScope(LambdaExpressionNode lambdaExpressionNode, ref SyntaxToken openBraceToken, ref CSharpParserModel parserModel)
    {
        lambdaExpressionNode.ParentScopeSubIndex = parserModel.ScopeCurrentSubIndex;
        lambdaExpressionNode.SelfScopeSubIndex = parserModel.Compilation.ScopeLength;
        parserModel.RegisterScope(
        	new Scope(
        		ScopeDirectionKind.Down,
        		scope_StartInclusiveIndex: openBraceToken.TextSpan.StartInclusiveIndex,
        		scope_EndExclusiveIndex: -1,
        		codeBlock_StartInclusiveIndex: -1,
        		codeBlock_EndExclusiveIndex: -1,
        		parentScopeSubIndex: parserModel.ScopeCurrentSubIndex,
        		selfScopeSubIndex: parserModel.Compilation.ScopeLength,
        		nodeSubIndex: 0,
        		permitCodeBlockParsing: true,
        		isImplicitOpenCodeBlockTextSpan: false,
        		ownerSyntaxKind: lambdaExpressionNode.SyntaxKind),
            codeBlockOwner: null);
        /*
        TODO: Optimize this by clearing the parserModel.Binder.LambdaExpressionNodeChildList after a file is parsed?...
        ...you can't clear them during a parse due to the chance of recursive lambdas that each have their own variables.
        But once you've finished parsing a file, you can clear it because this data is being copied to just be a "variable".
        */
        for (int i = lambdaExpressionNode.IndexLambdaExpressionNodeChildList; i < lambdaExpressionNode.IndexLambdaExpressionNodeChildList + lambdaExpressionNode.CountLambdaExpressionNodeChildList; i++)
        {
            parserModel.BindVariableDeclarationNode(parserModel.Binder.LambdaExpressionNodeChildList[i]);
        }
    }
    
    public static void CloseLambdaExpressionScope(LambdaExpressionNode lambdaExpressionNode, ref CSharpParserModel parserModel)
    {
        var closeBraceToken = new SyntaxToken(SyntaxKind.CloseBraceToken, parserModel.TokenWalker.Current.TextSpan);        
        parserModel.CloseScope(closeBraceToken.TextSpan, isStatementLoop: false);
    }
    
    /// <summary>
    /// TODO: Parse the lambda expression's statements...
    ///       ...This sounds quite complicated because we went
    ///       from the statement-loop to the expression-loop
    ///       and now have to run the statement-loop again
    ///       but not lose the state of any active loops.
    ///       For now skip tokens until the close brace token is matched in order to
    ///       preserve the other features of the text editor.
    ///       (rather than lambda expression statements clobbering the entire syntax highlighting of the file).
    /// </summary>
    public static IExpressionNode SkipLambdaExpressionStatements(LambdaExpressionNode lambdaExpressionNode, ref CSharpParserModel parserModel)
    {
        parserModel.TokenWalker.Consume(); // Skip the EqualsCloseAngleBracketToken
        
        var openTokenIndex = parserModel.TokenWalker.Index;
        _ = parserModel.TokenWalker.Consume(); // openBraceToken

        var openBraceCounter = 1;
        
        while (true)
        {
            if (parserModel.TokenWalker.IsEof)
                break;

            if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenBraceToken)
            {
                ++openBraceCounter;
            }
            else if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CloseBraceToken)
            {
                if (--openBraceCounter <= 0)
                    break;
            }

            _ = parserModel.TokenWalker.Consume();
        }
        
        var lambdaScope = parserModel.ScopeCurrent;
        CloseLambdaExpressionScope(lambdaExpressionNode, ref parserModel);
    
        var closeTokenIndex = parserModel.TokenWalker.Index;
        _ = parserModel.TokenWalker.Match(SyntaxKind.CloseBraceToken);

        var nonLambdaScopeParentSubIndex = parserModel.ScopeCurrentSubIndex;
        while (parserModel.Binder.ScopeList[parserModel.Compilation.ScopeOffset + nonLambdaScopeParentSubIndex].OwnerSyntaxKind == SyntaxKind.LambdaExpressionNode)
        {
            nonLambdaScopeParentSubIndex = parserModel.Binder.ScopeList[parserModel.Compilation.ScopeOffset + nonLambdaScopeParentSubIndex].ParentScopeSubIndex;
        }

        parserModel.StatementBuilder.ParseLambdaStatementScopeStack.Push(
            (
                nonLambdaScopeParentSubIndex,
                new CSharpDeferredChildScope(
                    openTokenIndex,
                    closeTokenIndex,
                    lambdaScope.SelfScopeSubIndex)
            ));
            
        return lambdaExpressionNode;
    }

    public static IExpressionNode ParseMemberAccessToken(
        IExpressionNode expressionPrimary, ref SyntaxToken tokenIn, ref CSharpParserModel parserModel)
    {
        return expressionPrimary;
        /*var token = tokenIn;
        var loopIteration = 0;
        
        while (!parserModel.TokenWalker.IsEof)
        {
            if (loopIteration++ >= 1)
            {
                // The object initialization / record 'with' keyword
                // provide a fabricated initial token.
                //
                // So the 0th iteration needs to use this function's SyntaxToken parameter.
                token = parserModel.TokenWalker.Current;
                
                if (token.SyntaxKind != SyntaxKind.MemberAccessToken)
                    break;
            }
        
            if (!token.IsFabricated && !UtilityApi.IsConvertibleToIdentifierToken(parserModel.TokenWalker.Next.SyntaxKind))
                break; // TODO: Consume and return the MemberAccessToken here?
    
            if (!token.IsFabricated)
                _ = parserModel.TokenWalker.Consume(); // Consume the 'MemberAccessToken'
            
            // Consume the 'NameableToken'
            var nameableToken = parserModel.TokenWalker.Consume();
            var memberIdentifierToken = UtilityApi.ConvertToIdentifierToken(
                ref nameableToken,
                ref parserModel);
                
            if (!memberIdentifierToken.ConstructorWasInvoked || expressionPrimary is null)
                break;
            
            if (expressionPrimary.SyntaxKind == SyntaxKind.AmbiguousIdentifierExpressionNode)
            {
                var ambiguousIdentifierExpressionNode = (AmbiguousIdentifierExpressionNode)expressionPrimary;
                if (!ambiguousIdentifierExpressionNode.FollowsMemberAccessToken)
                {
                    expressionPrimary = ForceDecisionAmbiguousIdentifier(
                        EmptyExpressionNode.Empty,
                        ambiguousIdentifierExpressionNode,
                        ref parserModel);
                }
            }
        
            TypeReference typeReference = default;
            // TextEditorTextSpan explicitDefinitionTextSpan = default;
            // ResourceUri explicitDefinitionResourceUri = default;
        
            if (expressionPrimary.SyntaxKind == SyntaxKind.VariableReferenceNode)
            {
                var variableReferenceNode = (VariableReferenceNode)expressionPrimary;
                if (variableReferenceNode.VariableDeclarationNode is not null)
                    typeReference = variableReferenceNode.VariableDeclarationNode.TypeReference;
            }
            else if (expressionPrimary.SyntaxKind == SyntaxKind.FunctionInvocationNode)
            {
                typeReference = ((FunctionInvocationNode)expressionPrimary).ResultTypeReference;
            }
            else if (expressionPrimary.SyntaxKind == SyntaxKind.TypeClauseNode)
            {
                var typeClauseNode = (TypeClauseNode)expressionPrimary;
                // explicitDefinitionTextSpan = typeClauseNode.ExplicitDefinitionTextSpan;
                // explicitDefinitionResourceUri = typeClauseNode.ExplicitDefinitionResourceUri;
                typeReference = new TypeReference(typeClauseNode);
                parserModel.Return_TypeClauseNode(typeClauseNode);
            }
            else if (expressionPrimary.SyntaxKind == SyntaxKind.TypeDefinitionNode)
            {
                typeReference = ((TypeDefinitionNode)expressionPrimary).ToTypeReference();
            }
                
            if (typeReference.IsDefault())
            {
                if (expressionPrimary.SyntaxKind == SyntaxKind.VariableReferenceNode)
                {
                    parserModel.Return_VariableReferenceNode((VariableReferenceNode)expressionPrimary);
                }
                else if (expressionPrimary.SyntaxKind == SyntaxKind.FunctionInvocationNode)
                {
                    parserModel.Return_FunctionInvocationNode((FunctionInvocationNode)expressionPrimary);
                }
                
                expressionPrimary = ParseMemberAccessToken_UndefinedNode(expressionPrimary, memberIdentifierToken, ref parserModel);
                continue;
            }
            
            TypeDefinitionNode? typeDefinitionNode;
            
            CSharpCompilationUnit innerCompilationUnit;
            ResourceUri innerResourceUri;
            
            if (typeReference.ExplicitDefinitionTextSpan.ConstructorWasInvoked && typeReference.ExplicitDefinitionResourceUri.Value is not null)
            {
                if (parserModel.Binder.__CompilationUnitMap.TryGetValue(typeReference.ExplicitDefinitionResourceUri, out innerCompilationUnit))
                {
                    innerResourceUri = typeReference.ExplicitDefinitionResourceUri;
                    var scope = parserModel.Binder.GetScope(innerCompilationUnit, typeReference.ExplicitDefinitionTextSpan);

                    if (!scope.IsDefault())
                    {
                        if (parserModel.TryGetTypeDefinitionHierarchically(
                                innerResourceUri,
                                innerCompilationUnit,
                                scope.SelfScopeSubIndex,
                                innerResourceUri,
                                typeReference.ExplicitDefinitionTextSpan,
                                out var innerTypeDefinitionNode) &&
                            innerTypeDefinitionNode is not null)
                        {
                            typeDefinitionNode = innerTypeDefinitionNode;
                            
                            // This assignment does nothing but it is commented out here to follow the pattern.
                            // innerCompilationUnit = innerCompilationUnit;
                            // innerResourceUri = innerResourceUri;
                        }
                        else
                        {
                            typeDefinitionNode = null;
                            innerCompilationUnit = parserModel.Compilation;
                            innerResourceUri = parserModel.ResourceUri;
                        }
                    }
                    else
                    {
                        typeDefinitionNode = null;
                        innerCompilationUnit = parserModel.Compilation;
                        innerResourceUri = parserModel.ResourceUri;
                    }
                }
                else
                {
                    typeDefinitionNode = null;
                    innerCompilationUnit = parserModel.Compilation;
                    innerResourceUri = parserModel.ResourceUri;
                }
            }
            else
            {
                var scope = parserModel.Binder.GetScope(parserModel.Compilation, typeReference.TypeIdentifierToken.TextSpan);

                if (!scope.IsDefault())
                {
                    if (parserModel.TryGetTypeDefinitionHierarchically(
                            parserModel.ResourceUri,
                            parserModel.Compilation,
                            scope.SelfScopeSubIndex,
                            parserModel.ResourceUri,
                            typeReference.TypeIdentifierToken.TextSpan,
                            out var innerTypeDefinitionNode) &&
                        innerTypeDefinitionNode is not null)
                    {
                        typeDefinitionNode = innerTypeDefinitionNode;
                        innerCompilationUnit = parserModel.Compilation;
                        innerResourceUri = parserModel.ResourceUri;
                    }
                    else
                    {
                        typeDefinitionNode = null;
                        innerCompilationUnit = parserModel.Compilation;
                        innerResourceUri = parserModel.ResourceUri;
                    }
                }
                else
                {
                    typeDefinitionNode = null;
                    innerCompilationUnit = parserModel.Compilation;
                    innerResourceUri = parserModel.ResourceUri;
                }
            }
            
            if (typeDefinitionNode is null)
            {
                if (expressionPrimary.SyntaxKind == SyntaxKind.VariableReferenceNode)
                {
                    parserModel.Return_VariableReferenceNode((VariableReferenceNode)expressionPrimary);
                }
                else if (expressionPrimary.SyntaxKind == SyntaxKind.FunctionInvocationNode)
                {
                    parserModel.Return_FunctionInvocationNode((FunctionInvocationNode)expressionPrimary);
                }
                expressionPrimary = ParseMemberAccessToken_UndefinedNode(expressionPrimary, memberIdentifierToken, ref parserModel);
                continue;
            }

            var memberList = parserModel.Binder.Internal_GetMemberList_TypeDefinitionNode(typeDefinitionNode);
            ISyntaxNode? foundDefinitionNode = null;
            
            foreach (var node in memberList)
            {
                if (node.SyntaxKind == SyntaxKind.VariableDeclarationNode)
                {
                    var variableDeclarationNode = (VariableDeclarationNode)node;
                    if (!variableDeclarationNode.IdentifierToken.ConstructorWasInvoked)
                        continue;
                    
                    string resourceUriValue;
                    
                    if (variableDeclarationNode.ResourceUri != parserModel.ResourceUri)
                    {
                        if (parserModel.Binder.__CompilationUnitMap.TryGetValue(variableDeclarationNode.ResourceUri, out var variableDeclarationCompilationUnit))
                            resourceUriValue = variableDeclarationNode.ResourceUri.Value;
                        else
                            resourceUriValue = innerResourceUri.Value;
                    }
                    else
                    {
                        resourceUriValue = innerResourceUri.Value;
                    }
                    
                    if (parserModel.Binder.CSharpCompilerService.SafeCompareTextSpans(parserModel.ResourceUri.Value, memberIdentifierToken.TextSpan, resourceUriValue, variableDeclarationNode.IdentifierToken.TextSpan))
                    {
                        foundDefinitionNode = variableDeclarationNode;
                        break;
                    }
                }
                else if (node.SyntaxKind == SyntaxKind.FunctionDefinitionNode)
                {
                    // TODO: Create a Binder.Main method that takes a node and returns its identifier?
                    var functionDefinitionNode = (FunctionDefinitionNode)node;
                    if (!functionDefinitionNode.FunctionIdentifierToken.ConstructorWasInvoked)
                        continue;

                    string resourceUriValue;
                    
                    if (functionDefinitionNode.ResourceUri != parserModel.ResourceUri)
                    {
                        if (parserModel.Binder.__CompilationUnitMap.TryGetValue(functionDefinitionNode.ResourceUri, out var functionDefinitionCompilationUnit))
                            resourceUriValue = functionDefinitionNode.ResourceUri.Value;
                        else
                            resourceUriValue = innerResourceUri.Value;
                    }
                    else
                    {
                        resourceUriValue = innerResourceUri.Value;
                    }
                    
                    if (parserModel.Binder.CSharpCompilerService.SafeCompareTextSpans(parserModel.ResourceUri.Value, memberIdentifierToken.TextSpan, resourceUriValue, functionDefinitionNode.FunctionIdentifierToken.TextSpan))
                    {
                        foundDefinitionNode = functionDefinitionNode;
                        break;
                    }
                }
                // TODO: Nested type definitions needs to be added here.
            }
            
            if (foundDefinitionNode is null)
            {
                if (expressionPrimary.SyntaxKind == SyntaxKind.VariableReferenceNode)
                {
                    parserModel.Return_VariableReferenceNode((VariableReferenceNode)expressionPrimary);
                }
                else if (expressionPrimary.SyntaxKind == SyntaxKind.FunctionInvocationNode)
                {
                    parserModel.Return_FunctionInvocationNode((FunctionInvocationNode)expressionPrimary);
                }
                expressionPrimary = ParseMemberAccessToken_UndefinedNode(expressionPrimary, memberIdentifierToken, ref parserModel);
                continue;
            }

            if (foundDefinitionNode.SyntaxKind == SyntaxKind.VariableDeclarationNode)
            {
                var variableDeclarationNode = (VariableDeclarationNode)foundDefinitionNode;
                
                var variableReferenceNode = parserModel.Rent_VariableReferenceNode();
                variableReferenceNode.VariableIdentifierToken = memberIdentifierToken;
                variableReferenceNode.VariableDeclarationNode = variableDeclarationNode;
                var symbolId = parserModel.CreateVariableSymbol(variableReferenceNode.VariableIdentifierToken, variableDeclarationNode.VariableKind);
                
                if (parserModel.Binder.SymbolIdToExternalTextSpanMap.TryGetValue(parserModel.ResourceUri.Value, out var symbolIdToExternalTextSpanMap))
                {
                    symbolIdToExternalTextSpanMap.TryAdd(
                        symbolId,
                        (variableDeclarationNode.ResourceUri, variableDeclarationNode.IdentifierToken.TextSpan.StartInclusiveIndex));
                }
                
                if (expressionPrimary.SyntaxKind == SyntaxKind.VariableReferenceNode)
                {
                    parserModel.Return_VariableReferenceNode((VariableReferenceNode)expressionPrimary);
                }
                else if (expressionPrimary.SyntaxKind == SyntaxKind.FunctionInvocationNode)
                {
                    parserModel.Return_FunctionInvocationNode((FunctionInvocationNode)expressionPrimary);
                }
                
                expressionPrimary = variableReferenceNode;
            }
            else if (foundDefinitionNode.SyntaxKind == SyntaxKind.FunctionDefinitionNode)
            {
                var functionDefinitionNode = (FunctionDefinitionNode)foundDefinitionNode;
                
                // TODO: Method group node?
                // TODO: Don't store a reference to definitons.
                // TODO: Type -> "<...>" -> "(" -> FunctionInvocationNode, but will FunctionInvocationNode -> "<...>"?
                // TODO: Bind the named arguments to their declaration within the definition.
                
                var functionInvocationNode = parserModel.Rent_FunctionInvocationNode();
                functionInvocationNode.FunctionInvocationIdentifierToken = memberIdentifierToken;
                functionInvocationNode.ResultTypeReference = functionDefinitionNode.ReturnTypeReference;
                
                var symbolId = parserModel.GetNextSymbolId();
                
                parserModel.Binder.SymbolList.Insert(
                    parserModel.Compilation.SymbolOffset + parserModel.Compilation.SymbolLength,
                    new Symbol(
                        SyntaxKind.FunctionSymbol,
                        symbolId,
                        functionInvocationNode.FunctionInvocationIdentifierToken.TextSpan with
                        {
                            DecorationByte = (byte)GenericDecorationKind.Function
                        }));
                ++parserModel.Compilation.SymbolLength;
                
                if (parserModel.Binder.SymbolIdToExternalTextSpanMap.TryGetValue(parserModel.ResourceUri.Value, out var symbolIdToExternalTextSpanMap))
                {
                    symbolIdToExternalTextSpanMap.TryAdd(
                        symbolId,
                        (functionDefinitionNode.ResourceUri, functionDefinitionNode.FunctionIdentifierToken.TextSpan.StartInclusiveIndex));
                }
                
                functionInvocationNode.ExplicitDefinitionTextSpan = functionDefinitionNode.FunctionIdentifierToken.TextSpan;
                
                // TODO: Transition from 'FunctionInvocationNode' to GenericParameters / FunctionParameters
                // TODO: Method group if next token is not '<' or '('
                if (expressionPrimary.SyntaxKind == SyntaxKind.VariableReferenceNode)
                {
                    parserModel.Return_VariableReferenceNode((VariableReferenceNode)expressionPrimary);
                }
                else if (expressionPrimary.SyntaxKind == SyntaxKind.FunctionInvocationNode)
                {
                    parserModel.Return_FunctionInvocationNode((FunctionInvocationNode)expressionPrimary);
                }
                expressionPrimary = functionInvocationNode;
            }
        }
        
        // TODO: Transition from 'FunctionInvocationNode' to GenericParameters / FunctionParameters
        // TODO: Transition from 'ConstructorInvocationNode' to GenericParameters / FunctionParameters
        // TODO: Method group if next token is not '<' or '('
        // TODO: return new Aaa.Bbb(); // is a very good test case.
        
        return expressionPrimary;*/
    }
    
    private static IExpressionNode ParseMemberAccessToken_UndefinedNode(
        IExpressionNode expressionPrimary, SyntaxToken memberIdentifierToken, ref CSharpParserModel parserModel)
    {
        return expressionPrimary;
        /*if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenParenthesisToken ||
            parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenAngleBracketToken)
        {
            var ambiguousIdentifier = parserModel.Rent_AmbiguousIdentifierExpressionNode();
            ambiguousIdentifier.Token = memberIdentifierToken;
            return ambiguousIdentifier;
        }
        else
        {
            if (expressionPrimary.SyntaxKind == SyntaxKind.NamespaceClauseNode)
            {
                var firstNamespaceClauseNode = (NamespaceClauseNode)expressionPrimary;
                NamespacePrefixNode? firstNamespacePrefixNode = firstNamespaceClauseNode.NamespacePrefixNode;
                
                if (firstNamespacePrefixNode is null)
                {
                    firstNamespacePrefixNode = parserModel.Binder.FindPrefix(
                        parserModel.Binder.NamespacePrefixTree.__Root,
                        firstNamespaceClauseNode.IdentifierToken.TextSpan,
                        parserModel.ResourceUri.Value);
                    if (firstNamespacePrefixNode is not null)
                    {
                        firstNamespaceClauseNode.NamespacePrefixNode = firstNamespacePrefixNode;
                        firstNamespaceClauseNode.StartOfMemberAccessChainPositionIndex = firstNamespaceClauseNode.IdentifierToken.TextSpan.StartInclusiveIndex;
                    }
                }
                
                if (firstNamespacePrefixNode is not null)
                {
                    var secondNamespacePrefixNode = parserModel.Binder.FindPrefix(
                        firstNamespacePrefixNode,
                        memberIdentifierToken.TextSpan,
                        parserModel.ResourceUri.Value);
                
                    if (secondNamespacePrefixNode is not null)
                    {
                        memberIdentifierToken.TextSpan = memberIdentifierToken.TextSpan with
                        {
                            StartInclusiveIndex = firstNamespaceClauseNode.StartOfMemberAccessChainPositionIndex
                        };
                        
                        parserModel.Binder.SymbolList.Insert(
                            parserModel.Compilation.SymbolOffset + parserModel.Compilation.SymbolLength,
                            new Symbol(
                                SyntaxKind.NamespaceSymbol,
                                parserModel.GetNextSymbolId(),
                                memberIdentifierToken.TextSpan));
                        ++parserModel.Compilation.SymbolLength;
                        
                        var namespaceClauseNode = parserModel.Rent_NamespaceClauseNode();
                        namespaceClauseNode.IdentifierToken = memberIdentifierToken;
                        namespaceClauseNode.NamespacePrefixNode = secondNamespacePrefixNode;
                        namespaceClauseNode.PreviousNamespaceClauseNode = firstNamespaceClauseNode;
                        namespaceClauseNode.StartOfMemberAccessChainPositionIndex = firstNamespaceClauseNode.StartOfMemberAccessChainPositionIndex;
                        return namespaceClauseNode;
                    }
                }

                var tuple = parserModel.Binder.FindNamespaceGroup_Reversed_WithMatchedIndex(
                    parserModel.ResourceUri,
                    firstNamespaceClauseNode.IdentifierToken.TextSpan);

                if (tuple.TargetGroup.ConstructorWasInvoked)
                {
                    var innerCompilationUnit = parserModel.Compilation;
                    var innerResourceUri = parserModel.ResourceUri;
                
                    foreach (var typeDefinitionNode in parserModel.Binder.Internal_GetTopLevelTypeDefinitionNodes_NamespaceGroup(tuple.TargetGroup))
                    {
                        if (innerResourceUri != typeDefinitionNode.ResourceUri)
                        {
                            if (!parserModel.Binder.__CompilationUnitMap.TryGetValue(typeDefinitionNode.ResourceUri, out innerCompilationUnit))
                                continue;
                        }

                        if (parserModel.Binder.CSharpCompilerService.SafeCompareTextSpans(parserModel.ResourceUri.Value, memberIdentifierToken.TextSpan, typeDefinitionNode.ResourceUri.Value, typeDefinitionNode.IdentifierToken.TextSpan))
                        {
                            var typeClauseNode = parserModel.Rent_TypeClauseNode();
                            typeClauseNode.TypeIdentifierToken = memberIdentifierToken;
                            
                            var symbolId = parserModel.GetNextSymbolId();
                            
                            parserModel.Binder.SymbolList.Insert(
                                parserModel.Compilation.SymbolOffset + parserModel.Compilation.SymbolLength,
                                new Symbol(
                                    SyntaxKind.TypeSymbol,
                                    symbolId,
                                    typeClauseNode.TypeIdentifierToken.TextSpan with
                                    {
                                        DecorationByte = (byte)GenericDecorationKind.Type
                                    }));
                            ++parserModel.Compilation.SymbolLength;
                            
                            if (parserModel.Binder.SymbolIdToExternalTextSpanMap.TryGetValue(parserModel.ResourceUri.Value, out var symbolIdToExternalTextSpanMap))
                            {
                                symbolIdToExternalTextSpanMap.TryAdd(
                                    symbolId,
                                    (typeDefinitionNode.ResourceUri, typeDefinitionNode.IdentifierToken.TextSpan.StartInclusiveIndex));
                            }
                            
                            typeClauseNode.ExplicitDefinitionTextSpan = typeDefinitionNode.IdentifierToken.TextSpan;
                            typeClauseNode.ExplicitDefinitionResourceUri = typeDefinitionNode.ResourceUri;
                               
                            expressionPrimary = typeClauseNode;
                            
                            // Variable name collision issues, thus 'A'
                            var targetNodeA = firstNamespaceClauseNode;
                            while (targetNodeA is not null)
                            {
                                var temporaryNode = targetNodeA.PreviousNamespaceClauseNode;
                                parserModel.Return_NamespaceClauseNode(targetNodeA);
                                targetNodeA = temporaryNode;
                            }
                            
                            return typeClauseNode;
                        }
                    }
                }
                
                // Variable name collision issues, thus 'B'
                var targetNodeB = firstNamespaceClauseNode;
                while (targetNodeB is not null)
                {
                    var temporaryNode = targetNodeB.PreviousNamespaceClauseNode;
                    parserModel.Return_NamespaceClauseNode(targetNodeB);
                    targetNodeB = temporaryNode;
                }
            }
            
            if (UtilityApi.IsConvertibleToIdentifierToken(parserModel.TokenWalker.Current.SyntaxKind) &&
                parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.WithTokenContextualKeyword)
            {
                var ambiguousIdentifier = parserModel.Rent_AmbiguousIdentifierExpressionNode();
                ambiguousIdentifier.Token = memberIdentifierToken;
                return ambiguousIdentifier;
            }
            else
            {
                var variableReferenceNode = parserModel.Rent_VariableReferenceNode();
                variableReferenceNode.VariableIdentifierToken = memberIdentifierToken;
                variableReferenceNode.VariableDeclarationNode = null;
                _ = parserModel.CreateVariableSymbol(variableReferenceNode.VariableIdentifierToken, VariableKind.Property);
                return variableReferenceNode;
            }
        }*/
    }
    
    private static IExpressionNode AmbiguousParenthesizedExpressionTransformTo_ParenthesizedExpressionNode(
        AmbiguousParenthesizedExpressionNode ambiguousParenthesizedExpressionNode, IExpressionNode expressionSecondary, ref CSharpParserModel parserModel)
    {
        var parenthesizedExpressionNode = new ParenthesizedExpressionNode(
            ambiguousParenthesizedExpressionNode.OpenParenthesisToken,
            CSharpFacts.Types.Void.ToTypeReference());
            
        parenthesizedExpressionNode.InnerExpression = expressionSecondary;
            
        parserModel.NoLongerRelevantExpressionNode = ambiguousParenthesizedExpressionNode;
        
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.CloseParenthesisToken)
            parserModel.ExpressionList.Add((SyntaxKind.CloseParenthesisToken, parenthesizedExpressionNode));
            
        return parenthesizedExpressionNode;
    }
    
    private static IExpressionNode AmbiguousParenthesizedExpressionTransformTo_TupleExpressionNode(
        AmbiguousParenthesizedExpressionNode ambiguousParenthesizedExpressionNode, IExpressionNode? expressionSecondary, ref CSharpParserModel parserModel)
    {
        var tupleExpressionNode = new TupleExpressionNode();
            
        for (int i = ambiguousParenthesizedExpressionNode.IndexAmbiguousParenthesizedExpressionNodeChildList; i < ambiguousParenthesizedExpressionNode.IndexAmbiguousParenthesizedExpressionNodeChildList + ambiguousParenthesizedExpressionNode.CountAmbiguousParenthesizedExpressionNodeChildList; i++)
        {
            var node = parserModel.Binder.AmbiguousParenthesizedExpressionNodeChildList[i];
            if (node is IExpressionNode expressionNode)
            {
                // (x, y) => 3; # Lambda expression node
                // (x, 2);      # At first appeared to be Lambda expression node, but is actually tuple expression node.
                if (expressionNode.SyntaxKind == SyntaxKind.AmbiguousIdentifierExpressionNode)
                {
                    expressionNode = ForceDecisionAmbiguousIdentifier(
                        EmptyExpressionNode.Empty,
                        (AmbiguousIdentifierExpressionNode)expressionNode,
                        ref parserModel);
                }
                
                // tupleExpressionNode.InnerExpressionList.Add(expressionNode);
            }
        }
        
        // if (expressionSecondary is not null)
        //    tupleExpressionNode.InnerExpressionList.Add(expressionSecondary);
        
        parserModel.NoLongerRelevantExpressionNode = ambiguousParenthesizedExpressionNode;
        
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.CloseParenthesisToken)
        {
            parserModel.ExpressionList.Add((SyntaxKind.CloseParenthesisToken, tupleExpressionNode));
            parserModel.ExpressionList.Add((SyntaxKind.CommaToken, tupleExpressionNode));
        }
            
        return tupleExpressionNode;
    }
    
    private static IExpressionNode AmbiguousParenthesizedExpressionTransformTo_ExplicitCastNode(
        AmbiguousParenthesizedExpressionNode ambiguousParenthesizedExpressionNode, IExpressionNode expressionNode, ref SyntaxToken closeParenthesisToken, ref CSharpParserModel parserModel)
    {
        TypeClauseNode typeClauseNode;
    
        if (expressionNode.SyntaxKind == SyntaxKind.AmbiguousIdentifierExpressionNode)
        {
            var token = ((AmbiguousIdentifierExpressionNode)expressionNode).Token;
            
            typeClauseNode = ExplicitCastAndGenericParametersForceType(
                ref token,
                ref parserModel);
        }
        else if (expressionNode.SyntaxKind == SyntaxKind.TypeClauseNode)
        {
            typeClauseNode = (TypeClauseNode)expressionNode;
            parserModel.BindTypeClauseNode(typeClauseNode);
        }
        else if (expressionNode.SyntaxKind == SyntaxKind.VariableReferenceNode)
        {
            var token = ((VariableReferenceNode)expressionNode).VariableIdentifierToken;
            typeClauseNode = ExplicitCastAndGenericParametersForceType(
                ref token,
                ref parserModel);
        }
        else
        {
            return parserModel.Binder.Shared_BadExpressionNode;
        }
        
        var explicitCastNode = new ExplicitCastNode(ambiguousParenthesizedExpressionNode.OpenParenthesisToken, new TypeReference(typeClauseNode), closeParenthesisToken);
        parserModel.Return_TypeClauseNode(typeClauseNode);
        return explicitCastNode;
    }
    
    private static IExpressionNode AmbiguousParenthesizedExpressionTransformTo_TypeClauseNode(
        AmbiguousParenthesizedExpressionNode ambiguousParenthesizedExpressionNode, ref SyntaxToken token, ref CSharpParserModel parserModel)
    {        
        var typeClauseNode = parserModel.Rent_TypeClauseNode();
        
        typeClauseNode.TypeIdentifierToken = new SyntaxToken(
            SyntaxKind.IdentifierToken,
            new TextEditorTextSpan(
                ambiguousParenthesizedExpressionNode.OpenParenthesisToken.TextSpan.StartInclusiveIndex,
                token.TextSpan.EndExclusiveIndex,
                default(byte)));
        
        if (typeClauseNode.ExplicitDefinitionResourceUri.Value is null &&
            typeClauseNode.ExplicitDefinitionTextSpan == default)
        {
            typeClauseNode.ExplicitDefinitionResourceUri = parserModel.ResourceUri;
            typeClauseNode.ExplicitDefinitionTextSpan = typeClauseNode.TypeIdentifierToken.TextSpan;
        }
        
        return typeClauseNode;
    }
    
    private static IExpressionNode AmbiguousParenthesizedExpressionTransformTo_LambdaExpressionNode(
        AmbiguousParenthesizedExpressionNode ambiguousParenthesizedExpressionNode, ref CSharpParserModel parserModel)
    {
        var lambdaExpressionNode = new LambdaExpressionNode(CSharpFacts.Types.Void.ToTypeReference());
                    
        if (ambiguousParenthesizedExpressionNode.CountAmbiguousParenthesizedExpressionNodeChildList >= 1)
        {
            for (int i = ambiguousParenthesizedExpressionNode.IndexAmbiguousParenthesizedExpressionNodeChildList; i < ambiguousParenthesizedExpressionNode.IndexAmbiguousParenthesizedExpressionNodeChildList + ambiguousParenthesizedExpressionNode.CountAmbiguousParenthesizedExpressionNodeChildList; i++)
            {
                var node = parserModel.Binder.AmbiguousParenthesizedExpressionNodeChildList[i];
                if (node.SyntaxKind == SyntaxKind.VariableDeclarationNode)
                {
                    parserModel.Binder.LambdaExpressionNodeChildList.Insert(
                        lambdaExpressionNode.IndexLambdaExpressionNodeChildList + lambdaExpressionNode.CountLambdaExpressionNodeChildList,
                        (VariableDeclarationNode)node);
                    ++lambdaExpressionNode.CountLambdaExpressionNodeChildList;
                }
                else
                {
                    SyntaxToken identifierToken;
                
                    if (node.SyntaxKind == SyntaxKind.AmbiguousIdentifierExpressionNode)
                    {
                        var token = ((AmbiguousIdentifierExpressionNode)node).Token;
                        identifierToken = UtilityApi.ConvertToIdentifierToken(ref token, ref parserModel);
                    }
                    else if (node.SyntaxKind == SyntaxKind.TypeClauseNode)
                    {
                        var token = ((TypeClauseNode)node).TypeIdentifierToken;
                        identifierToken = UtilityApi.ConvertToIdentifierToken(ref token, ref parserModel);
                    }
                    else if (node.SyntaxKind == SyntaxKind.VariableReferenceNode)
                    {
                        var token = ((VariableReferenceNode)node).VariableIdentifierToken;
                        identifierToken = UtilityApi.ConvertToIdentifierToken(ref token, ref parserModel);
                    }
                    else
                    {
                        return parserModel.Binder.Shared_BadExpressionNode;
                    }
                
                    var variableDeclarationNode = new VariableDeclarationNode(
                        TypeFacts.Empty.ToTypeReference(),
                        identifierToken,
                        VariableKind.Local,
                        false,
                        parserModel.ResourceUri);
                        
                    parserModel.Binder.LambdaExpressionNodeChildList.Insert(
                        lambdaExpressionNode.IndexLambdaExpressionNodeChildList + lambdaExpressionNode.CountLambdaExpressionNodeChildList,
                        variableDeclarationNode);
                    ++lambdaExpressionNode.CountLambdaExpressionNodeChildList;
                }
            }
        }
        
        // CONFUSING: the 'AmbiguousIdentifierExpressionNode' when merging with 'EqualsCloseAngleBracketToken'...
        // ...will invoke the 'ParseLambdaExpressionNode(...)' method.
        //
        // But, the loop entered in on 'EqualsCloseAngleBracketToken' for the 'AmbiguousIdentifierExpressionNode'.
        // Whereas this code block's loop entered in on 'CloseParenthesisToken'.
        //
        // This "desync" means you cannot synchronize them, while sharing the code for 'ParseLambdaExpressionNode(...)'
        // in its current state.
        // 
        // So, this code block needs to do some odd 'parserModel.TokenWalker.Consume();' before and after
        // the 'ParseLambdaExpressionNode(...)' invocation in order to "sync" the token walker
        // with the 'AmbiguousIdentifierExpressionNode' path.
        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CloseParenthesisToken &&
            parserModel.TokenWalker.Next.SyntaxKind == SyntaxKind.EqualsCloseAngleBracketToken)
        {
            _ = parserModel.TokenWalker.Consume(); // CloseParenthesisToken
        }
        
        SyntaxToken openBraceToken;
        
        if (parserModel.TokenWalker.Next.SyntaxKind == SyntaxKind.OpenBraceToken)
            openBraceToken = parserModel.TokenWalker.Next;
        else
            openBraceToken = new SyntaxToken(SyntaxKind.OpenBraceToken, parserModel.TokenWalker.Current.TextSpan);
        
        var resultExpression = ParseLambdaExpressionNode(lambdaExpressionNode, ref openBraceToken, ref parserModel);
        
        return resultExpression;
    }
    
    /// <summary>
    /// Am working on the first implementation of parsing interpolated strings.
    /// Need a way for a 'StringInterpolatedToken' to trigger the new code.
    ///
    /// Currently there are 2 'LiteralExpressionNode' constructor invocations,
    /// so under each of them I've invoked this method.
    ///
    /// Will see where things go from here, TODO: don't do this long term.
    ///
    /// --------------------------------
    ///
    /// Interpolated strings might not actually be "literal expressions"
    /// but I think this is a good path to investigate that will lead to understanding the correct answer.
    /// </summary>
    public static IExpressionNode ParseInterpolatedStringNode(
        InterpolatedStringNode interpolatedStringNode,
        ref CSharpParserModel parserModel)
    {
        parserModel.ExpressionList.Add((SyntaxKind.StringInterpolatedEndToken, interpolatedStringNode));
        parserModel.ExpressionList.Add((SyntaxKind.StringInterpolatedContinueToken, interpolatedStringNode));
        return EmptyExpressionNode.Empty;
    }
    
    public static IExpressionNode ParseFunctionParameterListing_Token(
        IInvocationNode invocationNode, ref SyntaxToken token, ref CSharpParserModel parserModel)
    {
        switch (token.SyntaxKind)
        {
            case SyntaxKind.CommaToken:
                parserModel.ExpressionList.Add((SyntaxKind.CommaToken, invocationNode));
                parserModel.ExpressionList.Add((SyntaxKind.ColonToken, invocationNode));
                return ParseNamedParameterSyntaxAndReturnEmptyExpressionNode(ref parserModel);
            case SyntaxKind.ColonToken:
                parserModel.ExpressionList.Add((SyntaxKind.ColonToken, invocationNode));
                return EmptyExpressionNode.Empty;
            default:
                return parserModel.Binder.Shared_BadExpressionNode;
        }
    }
    
    public static IExpressionNode ParseFunctionParameterListing_Expression(
        IInvocationNode invocationNode, IExpressionNode expressionSecondary, ref CSharpParserModel parserModel)
    {
        return invocationNode;
        /*
        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CloseParenthesisToken)
            invocationNode.IsParsingFunctionParameters = false;
    
        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.ColonToken)
            return invocationNode;
        
        if (expressionSecondary.SyntaxKind == SyntaxKind.EmptyExpressionNode)
            return invocationNode;
            
        if (expressionSecondary.SyntaxKind == SyntaxKind.AmbiguousIdentifierExpressionNode)
        {
            expressionSecondary = ForceDecisionAmbiguousIdentifier(
                EmptyExpressionNode.Empty,
                (AmbiguousIdentifierExpressionNode)expressionSecondary,
                ref parserModel);
        }
        
        if (expressionSecondary.SyntaxKind == SyntaxKind.VariableDeclarationNode &&
            parserModel.ParameterModifierKind == ParameterModifierKind.Out)
        {
            var variableDeclarationNode = (VariableDeclarationNode)expressionSecondary;
            
            if (parserModel.Binder.CSharpCompilerService.SafeCompareText(parserModel.ResourceUri.Value, "var", variableDeclarationNode.TypeReference.TypeIdentifierToken.TextSpan))
            {
                if (invocationNode.SyntaxKind == SyntaxKind.FunctionInvocationNode)
                {
                    var functionInvocationNode = (FunctionInvocationNode)invocationNode;
                    
                    ISyntaxNode? maybeFunctionDefinitionNode;
                    
                    if (functionInvocationNode.ExplicitDefinitionTextSpan.ConstructorWasInvoked)
                    {
                        if (parserModel.Binder.__CompilationUnitMap.TryGetValue(parserModel.ResourceUri, out var innerCompilationUnit))
                        {
                            maybeFunctionDefinitionNode = parserModel.Binder.GetDefinitionNodeValue(
                                parserModel.ResourceUri,
                                innerCompilationUnit,
                                functionInvocationNode.ExplicitDefinitionTextSpan,
                                SyntaxKind.FunctionInvocationNode);
                        }
                        else
                        {
                            maybeFunctionDefinitionNode = null;
                        }
                    }
                    else
                    {
                        maybeFunctionDefinitionNode = parserModel.Binder.GetDefinitionNodeValue(
                            parserModel.ResourceUri,
                            parserModel.Compilation,
                            functionInvocationNode.FunctionInvocationIdentifierToken.TextSpan,
                            SyntaxKind.FunctionInvocationNode);
                    }
                    
                    if (maybeFunctionDefinitionNode is not null &&
                        maybeFunctionDefinitionNode.SyntaxKind == SyntaxKind.FunctionDefinitionNode)
                    {
                        var functionDefinitionNode = (FunctionDefinitionNode)maybeFunctionDefinitionNode;
                    
                        if (functionDefinitionNode.CountFunctionArgumentEntryList > invocationNode.CountFunctionParameterEntryList)
                        {
                            var matchingArgument = parserModel.Binder.FunctionArgumentEntryList[functionDefinitionNode.IndexFunctionArgumentEntryList + invocationNode.CountFunctionParameterEntryList];
                            
                            variableDeclarationNode.SetImplicitTypeReference(matchingArgument.TypeReference);
                        }
                    }
                }
            }
        }
        
        /*parserModel.Binder.FunctionParameterEntryList.Add(
            invocationNode.IndexFunctionParameterEntryList + invocationNode.CountFunctionParameterEntryList,
            new FunctionParameterEntry(parserModel.ParameterModifierKind));*//*
        invocationNode.CountFunctionParameterEntryList++;
        
        /*if (parserModel.Compilation.CompilationUnitKind == CompilationUnitKind.IndividualFile_AllData)
        {
            if (parserModel.Compilation.FunctionInvocationParameterMetadataOffset == -1)
                parserModel.Compilation.FunctionInvocationParameterMetadataOffset = parserModel.Binder.FunctionInvocationParameterMetadataList.Count;
            
            parserModel.Binder.FunctionInvocationParameterMetadataList.Insert(
                parserModel.Compilation.FunctionInvocationParameterMetadataOffset + parserModel.Compilation.FunctionInvocationParameterMetadataLength,
                new FunctionInvocationParameterMetadata(
                    invocationNode.IdentifierStartInclusiveIndex,
                    expressionSecondary.ResultTypeReference,
                    parserModel.ParameterModifierKind));
            ++parserModel.Compilation.FunctionInvocationParameterMetadataLength;
        }*//*
        
        if (expressionSecondary.SyntaxKind == SyntaxKind.VariableReferenceNode)
        {
            parserModel.Return_VariableReferenceNode((VariableReferenceNode)expressionSecondary);
        }
        else if (expressionSecondary.SyntaxKind == SyntaxKind.FunctionInvocationNode)
        {
            parserModel.Return_FunctionInvocationNode((FunctionInvocationNode)expressionSecondary);
        }
        else if (expressionSecondary.SyntaxKind == SyntaxKind.ConstructorInvocationExpressionNode)
        {
            parserModel.Return_ConstructorInvocationExpressionNode((ConstructorInvocationExpressionNode)expressionSecondary);
        }
        else if (expressionSecondary.SyntaxKind == SyntaxKind.BinaryExpressionNode)
        {
            parserModel.Return_BinaryExpressionNode((BinaryExpressionNode)expressionSecondary);
        }
        
        // Just needs to be set to anything other than out, in, ref.
        parserModel.ParameterModifierKind = ParameterModifierKind.None;
        return invocationNode;
        */
    }
    
    /// <summary>
    /// Careful if changing this method:
    /// ================================
    /// Constructor secondary syntax with 'base' or 'this':
    /// ````public MyClass() : base() { }
    ///
    /// 'ParseFunctions.HandleConstructorDefinition(...)' has this logic repeated
    /// because it exists outside the expression loop.
    ///
    /// You may want to change both locations.
    /// </summary>
    public static IExpressionNode ParseFunctionParameterListing_Start(IInvocationNode invocationNode, ref CSharpParserModel parserModel)
    {
        invocationNode.IsParsingFunctionParameters = true;
    
        parserModel.ExpressionList.Add((SyntaxKind.CloseParenthesisToken, invocationNode));
        parserModel.ExpressionList.Add((SyntaxKind.CommaToken, invocationNode));
        parserModel.ExpressionList.Add((SyntaxKind.ColonToken, invocationNode));
        return ParseNamedParameterSyntaxAndReturnEmptyExpressionNode(ref parserModel);
    }
    
    /// <summary>
    /// Careful if changing this method:
    /// ================================
    /// Constructor secondary syntax with 'base' or 'this':
    /// ````public MyClass() : base() { }
    ///
    /// 'ParseFunctions.HandleConstructorDefinition(...)' 
    /// will invoke this method from outside of the expression loop.
    ///
    /// You may want to change both locations.
    ///
    /// 'guaranteeOpenParenthesisConsume' is used for the constructor definitions
    /// because it doesn't have the expression loop to guarantee the consumption (with proper timing).
    /// </summary>
    public static IExpressionNode ParseNamedParameterSyntaxAndReturnEmptyExpressionNode(
        ref CSharpParserModel parserModel, bool guaranteeConsume = false)
    {
        if (UtilityApi.IsConvertibleToIdentifierToken(parserModel.TokenWalker.Peek(1).SyntaxKind) &&
            parserModel.TokenWalker.Peek(2).SyntaxKind == SyntaxKind.ColonToken)
        {
            // Consume the 'open parenthesis' / 'comma'
            _ = parserModel.TokenWalker.Consume();
            
            // Consume the identifierToken
            var token = parserModel.TokenWalker.Consume();
            parserModel.CreateVariableSymbol(
                UtilityApi.ConvertToIdentifierToken(ref token, ref parserModel),
                VariableKind.Local);
            
            // Consume the ColonToken
            _ = parserModel.TokenWalker.Consume();
        }
        else
        {
            if (guaranteeConsume)
            {
                // Consume the 'open parenthesis'
                // (this is a hack for constructor definition secondary syntax)
                _ = parserModel.TokenWalker.Consume();
            }
        }
    
        return EmptyExpressionNode.Empty;
    }
    
    public static IExpressionNode FunctionDefinitionMergeToken(
        IFunctionDefinitionNode functionDefinitionNode, ref CSharpParserModel parserModel)
    {
        switch (parserModel.TokenWalker.Current.SyntaxKind)
        {
            case SyntaxKind.OpenAngleBracketToken:
                if (functionDefinitionNode.SyntaxKind == SyntaxKind.FunctionDefinitionNode && ((FunctionDefinitionNode)functionDefinitionNode).IsParsingGenericParameters)
                    return GenericParametersListingMergeToken((FunctionDefinitionNode)functionDefinitionNode, ref parserModel);
                goto default;
            case SyntaxKind.CloseAngleBracketToken:
                if (functionDefinitionNode.SyntaxKind == SyntaxKind.FunctionDefinitionNode && ((FunctionDefinitionNode)functionDefinitionNode).IsParsingGenericParameters)
                    return functionDefinitionNode;
                goto default;
            case SyntaxKind.CommaToken:
                parserModel.ExpressionList.Add((SyntaxKind.CommaToken, functionDefinitionNode));
                return EmptyExpressionNode.Empty;
            default:
                return parserModel.Binder.Shared_BadExpressionNode;
        }
    }
    
    public static IExpressionNode FunctionDefinitionMergeExpression(
        IFunctionDefinitionNode functionDefinitionNode, IExpressionNode expressionSecondary, ref CSharpParserModel parserModel)
    {
        if (expressionSecondary.SyntaxKind == SyntaxKind.EmptyExpressionNode)
            return functionDefinitionNode;
            
        if (expressionSecondary.SyntaxKind == SyntaxKind.AmbiguousIdentifierExpressionNode)
        {
            expressionSecondary = ForceDecisionAmbiguousIdentifier(
                functionDefinitionNode,
                (AmbiguousIdentifierExpressionNode)expressionSecondary,
                ref parserModel);
        }
        
        // TODO: Where is this containing-method invoked from?
        parserModel.Binder.FunctionArgumentEntryList.Insert(
            functionDefinitionNode.IndexFunctionArgumentEntryList + functionDefinitionNode.CountFunctionArgumentEntryList,
            new FunctionArgumentEntry(
                variableDeclarationNode: null,
                optionalCompileTimeConstantToken: new SyntaxToken(SyntaxKind.NotApplicable, textSpan: default),
                ArgumentModifierKind.None));
        
        return functionDefinitionNode;
    }
    
    public static IExpressionNode GenericParametersListingMergeToken(
        IGenericParameterNode genericParameterNode, ref CSharpParserModel parserModel)
    {
        var token = parserModel.TokenWalker.Current;
    
        if (UtilityApi.IsConvertibleToTypeClauseNode(token.SyntaxKind))
        {
            var nameToken = token;
                        
            TypeClauseNode? typeClauseNode = null;
            
            if (parserModel.TokenWalker.Next.SyntaxKind == SyntaxKind.MemberAccessToken)
            {
                parserModel.ParserContextKind = CSharpParserContextKind.None;
                var ambiguousExpressionNode = new AmbiguousIdentifierExpressionNode(
                    parserModel.TokenWalker.Current,
                    openAngleBracketToken: default,
                    indexGenericParameterEntryList: -1,
                    countGenericParameterEntryList: 0,
                    closeAngleBracketToken: default,
                    resultTypeReference: default);
                var expressionNode = ForceDecisionAmbiguousIdentifier(genericParameterNode, ambiguousExpressionNode, ref parserModel);
                nameToken = parserModel.Binder.GetNameToken(expressionNode);
                
                if (expressionNode.SyntaxKind == SyntaxKind.TypeClauseNode)
                {
                    typeClauseNode = (TypeClauseNode)expressionNode;
                }
                else
                {
                    nameToken = parserModel.Binder.GetNameToken(expressionNode);
                }
            }
            
            // TODO: Does typeClauseNode -> Generic params?
            typeClauseNode ??= ExplicitCastAndGenericParametersForceType(
                ref nameToken,
                ref parserModel);
            return typeClauseNode;
        }
    
        switch (token.SyntaxKind)
        {
            case SyntaxKind.CommaToken:
                parserModel.ExpressionList.Add((SyntaxKind.CommaToken, genericParameterNode));
                return EmptyExpressionNode.Empty;
            case SyntaxKind.CloseAngleBracketToken:
                // This case only occurs when the text won't compile.
                // i.e.: "<int>" rather than "MyClass<int>".
                // The case is for when the user types just the generic parameter listing text without an identifier before it.
                //
                // In the case of "SomeMethod<int>()", the FunctionInvocationNode
                // is expected to have ran 'parserModel.ExpressionList.Add((SyntaxKind.CloseAngleBracketToken, functionInvocationNode));'
                // to receive the genericParametersListingNode.
                return genericParameterNode;
            case SyntaxKind.OpenParenthesisToken:
                return ShareEmptyExpressionNodeIntoOpenParenthesisTokenCase(ref token, ref parserModel);
            default:
                return parserModel.Binder.Shared_BadExpressionNode;
        }
    }
    
    public static IExpressionNode GenericParametersListingMergeExpression(
        IGenericParameterNode genericParameterNode, IExpressionNode expressionSecondary, ref CSharpParserModel parserModel)
    {
        if (expressionSecondary.SyntaxKind == SyntaxKind.EmptyExpressionNode)
            return genericParameterNode;
            
        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CloseAngleBracketToken)
        {
            genericParameterNode.IsParsingGenericParameters = false;
            // Anything after this point parses as TypeClauseNode(s) without this.
            parserModel.ParserContextKind = CSharpParserContextKind.None;
        }
    
        if (genericParameterNode == expressionSecondary)
        {
            // If the generic parameters are empty: "List<>" then this is an infinite loop
            // if you ever show on the UI the tooltip, without this case.
            return genericParameterNode;
        }
        else if (expressionSecondary.SyntaxKind == SyntaxKind.AmbiguousIdentifierExpressionNode)
        {
            var expressionSecondaryTyped = (AmbiguousIdentifierExpressionNode)expressionSecondary;
            
            var token = expressionSecondaryTyped.Token;
            var typeClauseNode = UtilityApi.ConvertTokenToTypeClauseNode(ref token, ref parserModel);
            
            // TODO: Is this running everytime a parameter is added???...
            // ...only do this at the end?
            parserModel.BindTypeClauseNode(typeClauseNode);
            
            parserModel.Binder.GenericParameterEntryList.Insert(
                genericParameterNode.IndexGenericParameterEntryList + genericParameterNode.CountGenericParameterEntryList,
                new GenericParameterEntry(new TypeReference(typeClauseNode)));
            parserModel.Return_TypeClauseNode(typeClauseNode);
            genericParameterNode.CountGenericParameterEntryList++;
            
            return genericParameterNode;
        }
        else if (expressionSecondary.SyntaxKind == SyntaxKind.TypeClauseNode)
        {
            var typeClauseNode = (TypeClauseNode)expressionSecondary;
        
            parserModel.Binder.GenericParameterEntryList.Insert(
                genericParameterNode.IndexGenericParameterEntryList + genericParameterNode.CountGenericParameterEntryList,
                new GenericParameterEntry(new TypeReference(typeClauseNode)));
            parserModel.Return_TypeClauseNode(typeClauseNode);
            genericParameterNode.CountGenericParameterEntryList++;
            
            return genericParameterNode;
        }
        
        return parserModel.Binder.Shared_BadExpressionNode;
    }
    
    public static IExpressionNode ParseGenericParameterNode_Start(
        IGenericParameterNode genericParameterNode, ref SyntaxToken openAngleBracketToken, ref CSharpParserModel parserModel, IExpressionNode nodeToRestoreAtCloseAngleBracketToken = null)
    {
        nodeToRestoreAtCloseAngleBracketToken ??= genericParameterNode;
    
        if (!genericParameterNode.OpenAngleBracketToken.ConstructorWasInvoked)
        {
            // Idea: 1 listing for the entire file and store the indices at which your parameters lie?
            genericParameterNode.OpenAngleBracketToken = openAngleBracketToken;
            genericParameterNode.IndexGenericParameterEntryList = parserModel.Binder.GenericParameterEntryList.Count;
            genericParameterNode.CountGenericParameterEntryList = 0;
            genericParameterNode.CloseAngleBracketToken = default;
            genericParameterNode.IsParsingGenericParameters = true;
        }
        
        parserModel.ExpressionList.Add((SyntaxKind.CloseAngleBracketToken, nodeToRestoreAtCloseAngleBracketToken));
        parserModel.ExpressionList.Add((SyntaxKind.CommaToken, genericParameterNode));
        return genericParameterNode;
    }
    
    /// <summary>
    /// WARNING: If this parses a TypeClauseNode, it will return false, but not revert the TokenWalker's TokenIndex...
    /// ...in all other cases where this returns false, the TokenWalker's TokenIndex is reverted.
    /// This is done to preserve the way VariableDeclarationNode(s) were being parsed prior to this method's creation.
    /// Whether it would make more sense to revert in the case of a TypeClauseNode needs to be investigated.
    /// 
    /// This method handles all VariableDeclarationNode cases including ValueTupleType(s).
    ///
    /// This method does NOT invoke parserModel.Binder.BindVariableDeclarationNode(...), and does NOT create a Symbol.
    /// </summary>
    public static bool TryParseVariableDeclarationNode(ref CSharpParserModel parserModel, out VariableDeclarationNode? variableDeclarationNode)
    {
        var originalTokenIndex = parserModel.TokenWalker.Index;
        
        parserModel.TryParseExpressionSyntaxKindList.Add(SyntaxKind.TypeClauseNode);
        parserModel.TryParseExpressionSyntaxKindList.Add(SyntaxKind.VariableDeclarationNode);
        parserModel.TryParseExpressionSyntaxKindList.Add(SyntaxKind.AmbiguousParenthesizedExpressionNode);
        parserModel.TryParseExpressionSyntaxKindList.Add(SyntaxKind.AmbiguousIdentifierExpressionNode);
        parserModel.ParserContextKind = CSharpParserContextKind.ForceStatementExpression;
        _ = TryParseExpression(ref parserModel, out var expressionNode);

        if (expressionNode.SyntaxKind == SyntaxKind.VariableDeclarationNode)
        {
            variableDeclarationNode = (VariableDeclarationNode)expressionNode;
            return true;
        }
        else
        {
            if (expressionNode.SyntaxKind == SyntaxKind.AmbiguousParenthesizedExpressionNode)
            {
                var distance = parserModel.TokenWalker.Index - originalTokenIndex;
                    
                for (int i = 0; i < distance; i++)
                {
                    parserModel.TokenWalker.BacktrackNoReturnValue();
                }
            }
        
            variableDeclarationNode = null;
            return false;
        }
    }
    
    /// <summary>
    /// ParseExpression while expressionPrimary.SyntaxKind == syntaxKind
    /// 
    /// if (expressionPrimary.SyntaxKind != syntaxKind)
    ///     parserModel.TokenWalker.Backtrack() to either the previous loops tokenIndex where
    ///         the syntax kinds did match.
    /// 
    ///     Or, if they never matched then parserModel.TokenWalker.Backtrack()
    ///         to the tokenIndex that was had when this function was invoked.
    ///
    /// Return true if a match was found, return false if NO match was found.
    ///
    /// TypeClauseNode code exists in the expression code.
    /// As a result, some statements need to read a TypeClauseNode by invoking 'ParseExpression(...)'.
    ///
    /// In order to "short circut" or "force exit" from the expression code back to the statement code,
    /// if the root primary expression is not equal to the parserModel.ForceParseExpressionSyntaxKind
    /// then stop.
    ///
    /// ------------------------------
    /// Retrospective comment (2024-12-16):
    /// It appears that the 'SyntaxKind? syntaxKind'
    /// argument is nullable in order to permit
    /// usage of 'parserModel.ForceParseExpressionInitialPrimaryExpression'
    /// without specifying a specific syntax kind?
    ///
    /// The use case:
    /// FunctionInvocationNode as a statement
    /// will currently erroneously parse as a TypeClauseNode.
    ///
    /// But, once the statement code receives the 'TypeClauseNode' result
    /// from 'TryParseExpression', the next SyntaxToken
    /// is OpenParenthesisToken.
    ///
    /// Therefore, it is obvious at this point that we really wanted
    /// to parse a function invocation node.
    ///
    /// But, if there is any code that comes after the function invocation,
    /// and prior to the statement delimiter.
    ///
    /// Then a FunctionInvocationNode would not sufficiently represent the statement-expression.
    /// 
    /// i.e.: MyMethod() + 2;
    ///
    /// So, I cannot 'TryParseExpression' for a SyntaxKind.FunctionInvocationNode for this reason.
    ///
    /// But, I need to initialize the 'ParseExpression' method with the 'TypeClauseNode'
    /// (the 'TypeClauseNode' is in reality the function identifier / generic arguments to the function if there are any).
    ///
    /// Then, the 'ParseExpression(...)' code can see that there is a 'TypeClauseNode' merging with an OpenParenthesisToken,
    /// and that the only meaning this can have is function invocation.
    ///
    /// At that point, go on to move the 'TypeClauseNode' to be a function identifier, and the
    /// generic arguments for the function invocation, and go on from there.
    /// </summary>
    public static bool TryParseExpression(ref CSharpParserModel parserModel, out IExpressionNode expressionNode)
    {
        expressionNode = ParseExpression(ref parserModel);
        
        var success = parserModel.TryParseExpressionSyntaxKindList.Contains(expressionNode.SyntaxKind);
        
        parserModel.TryParseExpressionSyntaxKindList.Clear();
        parserModel.ForceParseExpressionInitialPrimaryExpression = EmptyExpressionNode.Empty;
        parserModel.ParserContextKind = CSharpParserContextKind.None;
        
        return success;
    }
}
