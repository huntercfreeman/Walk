using Walk.Extensions.CompilerServices;
using Walk.Extensions.CompilerServices.Syntax.Enums;
using Walk.Extensions.CompilerServices.Syntax.NodeReferences;
using Walk.Extensions.CompilerServices.Syntax.NodeValues;

namespace Walk.CompilerServices.CSharp.ParserCase;

public ref partial struct CSharpParserState
{
    /// <summary>
    /// It is expected that any invoker of this method will immediately set the returned BinaryExpressionNode instance's:
    /// - TODO
    /// Thus, the Return_BinaryExpressionNode(...) method will NOT clear that property's state.
    /// </summary>
    public readonly BinaryExpressionNode Rent_BinaryExpressionNode()
    {
        if (Binder.Pool_BinaryExpressionNode_Queue.TryDequeue(out var binaryExpressionNode))
        {
            return binaryExpressionNode;
        }

        return new BinaryExpressionNode(
            leftOperandTypeReference: default,
            operatorToken: default,
            rightOperandTypeReference: default,
            resultTypeReference: default);
    }

    /// <summary>
    /// It is expected that any invoker of this method will immediately set the returned BinaryExpressionNode instance's:
    /// - TODO
    /// Thus, the Return_BinaryExpressionNode(...) method will NOT clear that property's state.
    /// </summary>
    public readonly void Return_BinaryExpressionNode(BinaryExpressionNode binaryExpressionNode)
    {
        binaryExpressionNode._isFabricated = false;

        binaryExpressionNode.LeftOperandTypeReference = default;
        binaryExpressionNode.OperatorToken = default;
        binaryExpressionNode.RightOperandTypeReference = default;
        binaryExpressionNode.ResultTypeReference = default;

        binaryExpressionNode._rightExpressionResultTypeReference = default;
        binaryExpressionNode.RightExpressionNodeWasSet = false;
        binaryExpressionNode.ParentScopeSubIndex = 0;

        Binder.Pool_BinaryExpressionNode_Queue.Enqueue(binaryExpressionNode);
    }

    /// <summary>
    /// It is expected that any invoker of this method will immediately set the returned TypeClauseNode instance's:
    /// - TypeIdentifierToken
    /// Thus, the Return_TypeClauseNode(...) method will NOT clear that property's state.
    /// </summary>
    public readonly TypeClauseNode Rent_TypeClauseNode()
    {
        if (Binder.Pool_TypeClauseNode_Queue.TryDequeue(out var typeClauseNode))
        {
            return typeClauseNode;
        }

        return new TypeClauseNode(
            typeIdentifier: default,
            openAngleBracketToken: default,
            offsetGenericParameterEntryList: -1,
            lengthGenericParameterEntryList: 0,
            closeAngleBracketToken: default,
            isKeywordType: false);
    }

    /// <summary>
    /// It is expected that any invoker of this method will immediately set the returned TypeClauseNode instance's:
    /// - TypeIdentifierToken
    /// Thus, the Return_TypeClauseNode(...) method will NOT clear that property's state.
    /// </summary>
    public readonly void Return_TypeClauseNode(TypeClauseNode typeClauseNode)
    {
        typeClauseNode.OpenAngleBracketToken = default;
        typeClauseNode.OffsetGenericParameterEntryList = -1;
        typeClauseNode.LengthGenericParameterEntryList = 0;
        typeClauseNode.CloseAngleBracketToken = default;
        typeClauseNode.IsKeywordType = false;
        typeClauseNode.TypeKind = TypeKind.None;
        typeClauseNode.HasQuestionMark = false;
        typeClauseNode.ArrayRank = 0;
        // IsFabricated is not currently being used for this type, so the pooling logic doesn't need to reset it.
        //typeClauseNode._isFabricated = false;
        typeClauseNode.IsParsingGenericParameters = false;
        typeClauseNode.ExplicitDefinitionTextSpan = default;
        typeClauseNode.ExplicitDefinitionResourceUri = default;

        Binder.Pool_TypeClauseNode_Queue.Enqueue(typeClauseNode);
    }

    /// <summary>
    /// It is expected that any invoker of this method will immediately set the returned TemporaryLocalVariableDeclarationNode instance's:
    /// - TODO
    /// Thus, the Return_TemporaryLocalVariableDeclarationNode(...) method will NOT clear that property's state.
    /// </summary>
    public readonly VariableDeclarationNode Rent_TemporaryLocalVariableDeclarationNode()
    {
        if (Binder.Pool_TemporaryLocalVariableDeclarationNode_Queue.TryDequeue(out var variableDeclarationNode))
        {
            return variableDeclarationNode;
        }

        return new VariableDeclarationNode(
            typeReference: Facts.CSharpFacts.Types.Var.ToTypeReference(),
            identifierToken: default,
            VariableKind.Local,
            isInitialized: false,
            ResourceUri);
    }

    /// <summary>
    /// It is expected that any invoker of this method will immediately set the returned TemporaryLocalVariableDeclarationNode instance's:
    /// - TODO
    /// Thus, the Return_TemporaryLocalVariableDeclarationNode(...) method will NOT clear that property's state.
    /// </summary>
    public readonly void Return_TemporaryLocalVariableDeclarationNode(VariableDeclarationNode variableDeclarationNode)
    {
        variableDeclarationNode.TypeReference = default;
        variableDeclarationNode.IdentifierToken = default;
        variableDeclarationNode.VariableKind = VariableKind.Local;
        variableDeclarationNode.IsInitialized = false;
        variableDeclarationNode.ResourceUri = default;
        variableDeclarationNode.HasGetter = default;
        variableDeclarationNode.GetterIsAutoImplemented = default;
        variableDeclarationNode.HasSetter = default;
        variableDeclarationNode.SetterIsAutoImplemented = default;
        variableDeclarationNode.ParentScopeSubIndex = default;
        variableDeclarationNode._isFabricated = default;

        Binder.Pool_TemporaryLocalVariableDeclarationNode_Queue.Enqueue(variableDeclarationNode);
    }

    /// <summary>
    /// It is expected that any invoker of this method will immediately set the returned VariableReferenceNode instance's:
    /// - VariableIdentifierToken
    /// Thus, the Return_VariableReferenceNode(...) method will NOT clear that property's state.
    /// </summary>
    public readonly VariableReferenceNode Rent_VariableReferenceNode()
    {
        if (Binder.Pool_VariableReferenceNode_Queue.TryDequeue(out var variableReferenceNode))
        {
            return variableReferenceNode;
        }

        return new VariableReferenceNode(
            variableIdentifierToken: default,
            variableDeclarationNode: default);
    }

    /// <summary>
    /// It is expected that any invoker of this method will immediately set the returned VariableReferenceNode instance's:
    /// - VariableIdentifierToken
    /// Thus, the Return_VariableReferenceNode(...) method will NOT clear that property's state.
    /// </summary>
    public readonly void Return_VariableReferenceNode(VariableReferenceNode variableReferenceNode)
    {
        variableReferenceNode.ResultTypeReference = TypeFacts.Empty.ToTypeReference();
        variableReferenceNode._isFabricated = false;

        Binder.Pool_VariableReferenceNode_Queue.Enqueue(variableReferenceNode);
    }

    /// <summary>
    /// It is expected that any invoker of this method will immediately set the returned VariableReferenceNode instance's:
    /// - VariableIdentifierToken
    /// Thus, the Return_VariableReferenceNode(...) method will NOT clear that property's state.
    /// </summary>
    public readonly VariableReferenceValue Return_VariableReferenceNode_ToStruct(VariableReferenceNode variableReferenceNode)
    {
        var variableReference = new VariableReferenceValue(variableReferenceNode);
        Return_VariableReferenceNode(variableReferenceNode);
        return variableReference;
    }

    /// <summary>
    /// It is expected that any invoker of this method will immediately set the returned NamespaceClauseNode instance's:
    /// - IdentifierToken
    /// Thus, the Return_NamespaceClauseNode(...) method will NOT clear that property's state.
    /// </summary>
    public readonly NamespaceClauseNode Rent_NamespaceClauseNode()
    {
        if (Binder.Pool_NamespaceClauseNode_Queue.TryDequeue(out var namespaceClauseNode))
        {
            return namespaceClauseNode;
        }

        return new NamespaceClauseNode(
            identifierToken: default);
    }

    /// <summary>
    /// It is expected that any invoker of this method will immediately set the returned NamespaceClauseNode instance's:
    /// - IdentifierToken
    /// Thus, the Return_NamespaceClauseNode(...) method will NOT clear that property's state.
    /// </summary>
    public readonly void Return_NamespaceClauseNode(NamespaceClauseNode namespaceClauseNode)
    {
        // IsFabricated is not currently being used for this type, so the pooling logic doesn't need to reset it.
        //namespaceClauseNode._isFabricated = false;

        // namespaceClauseNode.NamespacePrefixNode = null;
        namespaceClauseNode.PreviousNamespaceClauseNode = null;
        namespaceClauseNode.StartOfMemberAccessChainPositionIndex = default;

        Binder.Pool_NamespaceClauseNode_Queue.Enqueue(namespaceClauseNode);
    }

    /// <summary>
    /// It is expected that any invoker of this method will immediately set the returned AmbiguousIdentifierExpressionNode instance's:
    /// - Token
    /// - FollowsMemberAccessToken
    /// Thus, the Return_AmbiguousIdentifierExpressionNode(...) method will NOT clear that property's state.
    /// </summary>
    public readonly AmbiguousIdentifierNode Rent_AmbiguousIdentifierExpressionNode()
    {
        if (Binder.Pool_AmbiguousIdentifierExpressionNode_Queue.TryDequeue(out var ambiguousIdentifierExpressionNode))
        {
            return ambiguousIdentifierExpressionNode;
        }

        return new AmbiguousIdentifierNode(
            token: default,
            openAngleBracketToken: default,
            offsetGenericParameterEntryList: -1,
            lengthGenericParameterEntryList: 0,
            closeAngleBracketToken: default,
            resultTypeReference: Facts.CSharpFacts.Types.Void.ToTypeReference());
    }

    /// <summary>
    /// It is expected that any invoker of this method will immediately set the returned AmbiguousIdentifierExpressionNode instance's:
    /// - Token
    /// - FollowsMemberAccessToken
    /// Thus, the Return_AmbiguousIdentifierExpressionNode(...) method will NOT clear that property's state.
    /// </summary>
    public readonly void Return_AmbiguousIdentifierExpressionNode(AmbiguousIdentifierNode ambiguousIdentifierExpressionNode)
    {
        ambiguousIdentifierExpressionNode.OpenAngleBracketToken = default;
        ambiguousIdentifierExpressionNode.OffsetGenericParameterEntryList = -1;
        ambiguousIdentifierExpressionNode.LengthGenericParameterEntryList = 0;
        ambiguousIdentifierExpressionNode.CloseAngleBracketToken = default;

        ambiguousIdentifierExpressionNode.ResultTypeReference = Facts.CSharpFacts.Types.Void.ToTypeReference();
        ambiguousIdentifierExpressionNode.HasQuestionMark = false;

        Binder.Pool_AmbiguousIdentifierExpressionNode_Queue.Enqueue(ambiguousIdentifierExpressionNode);
    }

    /// <summary>
    /// It is expected that any invoker of this method will immediately set the returned FunctionInvocationNode instance's:
    /// - FunctionInvocationIdentifierToken
    /// Thus, the Return_FunctionInvocationNode(...) method will NOT clear that property's state.
    /// </summary>
    public readonly FunctionInvocationNode Rent_FunctionInvocationNode()
    {
        if (Binder.Pool_FunctionInvocationNode_Queue.TryDequeue(out var functionInvocationNode))
        {
            return functionInvocationNode;
        }

        return new FunctionInvocationNode(
            functionInvocationIdentifierToken: default,
            openAngleBracketToken: default,
            offsetGenericParameterEntryList: -1,
            lengthGenericParameterEntryList: 0,
            closeAngleBracketToken: default,
            openParenthesisToken: default,
            offsetFunctionParameterEntryList: -1,
            lengthFunctionParameterEntryList: 0,
            closeParenthesisToken: default,
            resultTypeReference: Facts.CSharpFacts.Types.Void.ToTypeReference());
    }

    /// <summary>
    /// It is expected that any invoker of this method will immediately set the returned FunctionInvocationNode instance's:
    /// - FunctionInvocationIdentifierToken
    /// Thus, the Return_FunctionInvocationNode(...) method will NOT clear that property's state.
    /// </summary>
    public readonly void Return_FunctionInvocationNode(FunctionInvocationNode functionInvocationNode)
    {
        functionInvocationNode.OpenAngleBracketToken = default;
        functionInvocationNode.OffsetGenericParameterEntryList = -1;
        functionInvocationNode.LengthGenericParameterEntryList = 0;
        functionInvocationNode.CloseAngleBracketToken = default;

        functionInvocationNode.OpenParenthesisToken = default;
        functionInvocationNode.OffsetFunctionParameterEntryList = -1;
        functionInvocationNode.LengthFunctionParameterEntryList = 0;
        functionInvocationNode.CloseParenthesisToken = default;

        functionInvocationNode.ResultTypeReference = Facts.CSharpFacts.Types.Void.ToTypeReference();

        functionInvocationNode.ResourceUri = default;
        functionInvocationNode.ExplicitDefinitionTextSpan = default;

        // IsFabricated is not currently being used for this type, so the pooling logic doesn't need to reset it.
        //functionInvocationNode._isFabricated = false;

        functionInvocationNode.IsParsingFunctionParameters = false;
        functionInvocationNode.IsParsingGenericParameters = false;

        Binder.Pool_FunctionInvocationNode_Queue.Enqueue(functionInvocationNode);
    }

    /// <summary>
    /// It is expected that any invoker of this method will immediately set the returned ConstructorInvocationExpressionNode instance's:
    /// - NewKeywordToken
    /// Thus, the Return_ConstructorInvocationExpressionNode(...) method will NOT clear that property's state.
    /// </summary>
    public readonly ConstructorInvocationNode Rent_ConstructorInvocationExpressionNode()
    {
        if (Binder.Pool_ConstructorInvocationExpressionNode_Queue.TryDequeue(out var constructorInvocationExpressionNode))
        {
            return constructorInvocationExpressionNode;
        }

        return new ConstructorInvocationNode(
            newKeywordToken: default,
            typeReference: default,
            openParenthesisToken: default,
            offsetFunctionParameterEntryList: -1,
            lengthFunctionParameterEntryList: 0,
            closeParenthesisToken: default);
    }

    /// <summary>
    /// It is expected that any invoker of this method will immediately set the returned ConstructorInvocationExpressionNode instance's:
    /// - NewKeywordToken
    /// Thus, the Return_ConstructorInvocationExpressionNode(...) method will NOT clear that property's state.
    /// </summary>
    public readonly void Return_ConstructorInvocationExpressionNode(ConstructorInvocationNode constructorInvocationExpressionNode)
    {
        constructorInvocationExpressionNode.ResultTypeReference = default;

        constructorInvocationExpressionNode.OpenParenthesisToken = default;
        constructorInvocationExpressionNode.OffsetFunctionParameterEntryList = -1;
        constructorInvocationExpressionNode.LengthFunctionParameterEntryList = 0;
        constructorInvocationExpressionNode.CloseParenthesisToken = default;

        constructorInvocationExpressionNode.ConstructorInvocationStageKind = ConstructorInvocationStageKind.Unset;

        // IsFabricated is not currently being used for this type, so the pooling logic doesn't need to reset it.
        //constructorInvocationExpressionNode._isFabricated = false;

        constructorInvocationExpressionNode.IsParsingFunctionParameters = false;

        Binder.Pool_ConstructorInvocationExpressionNode_Queue.Enqueue(constructorInvocationExpressionNode);
    }
}
