using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.CompilerServices.CSharp.Facts;
using Walk.CompilerServices.CSharp.CompilerServiceCase;

namespace Walk.CompilerServices.CSharp.ParserCase.Internals;

public class ParseFunctions
{
    public static void HandleFunctionDefinition(
        SyntaxToken consumedIdentifierToken,
        TypeReference consumedTypeReference,
        CSharpCompilationUnit compilationUnit,
        ref CSharpParserModel parserModel)
    {
    	var functionDefinitionNode = new FunctionDefinitionNode(
            AccessModifierKind.Public,
            consumedTypeReference,
            consumedIdentifierToken,
            genericParameterListing: default,
            functionArgumentListing: default,
            default);
            
        parserModel.Binder.BindFunctionDefinitionNode(functionDefinitionNode, compilationUnit, ref parserModel);
        
        parserModel.Binder.NewScopeAndBuilderFromOwner(
        	functionDefinitionNode,
	        parserModel.TokenWalker.Current.TextSpan,
	        compilationUnit,
	        ref parserModel);
    
    	if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenAngleBracketToken)
    	{
			parserModel.ForceParseExpressionInitialPrimaryExpression = functionDefinitionNode;

			var openAngleBracketToken = parserModel.TokenWalker.Consume();
    		
    		parserModel.Binder.ParseGenericParameterNode_Start(
    			functionDefinitionNode, ref openAngleBracketToken, compilationUnit, ref parserModel);
    		
    		parserModel.TryParseExpressionSyntaxKindList.Add(SyntaxKind.FunctionDefinitionNode);
    		var successGenericParametersListingNode = ParseOthers.TryParseExpression(
    			compilationUnit,
    			ref parserModel,
    			out var expressionNode);
    		
    		if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.CloseAngleBracketToken &&
    			parserModel.TokenWalker.Next.SyntaxKind == SyntaxKind.CloseAngleBracketToken)
    		{
    			_ = parserModel.TokenWalker.Consume();
    			_ = parserModel.TokenWalker.Consume();
    		}
    	}
    
        if (parserModel.TokenWalker.Current.SyntaxKind != SyntaxKind.OpenParenthesisToken)
            return;

        HandleFunctionArguments(functionDefinitionNode, compilationUnit, ref parserModel);
        
        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.StatementDelimiterToken)
        	parserModel.CurrentCodeBlockBuilder.IsImplicitOpenCodeBlockTextSpan = true;
        else if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.EqualsCloseAngleBracketToken)
        	ParseTokens.MoveToExpressionBody(compilationUnit, ref parserModel);
    }

    public static void HandleConstructorDefinition(
    	TypeDefinitionNode typeDefinitionNodeCodeBlockOwner,
        SyntaxToken consumedIdentifierToken,
        CSharpCompilationUnit compilationUnit,
        ref CSharpParserModel parserModel)
    {
    	var typeClauseNode = parserModel.ConstructOrRecycleTypeClauseNode(
            typeDefinitionNodeCodeBlockOwner.TypeIdentifierToken,
            valueType: null,
            genericParameterListing: default,
            isKeywordType: false);

        var constructorDefinitionNode = new ConstructorDefinitionNode(
            new TypeReference(typeClauseNode),
            consumedIdentifierToken,
            default,
            functionArgumentListing: default,
            default);
    
    	parserModel.Binder.BindConstructorDefinitionIdentifierToken(consumedIdentifierToken, compilationUnit, ref parserModel);
    	
    	parserModel.Binder.NewScopeAndBuilderFromOwner(
        	constructorDefinitionNode,
	        parserModel.TokenWalker.Current.TextSpan,
	        compilationUnit,
	        ref parserModel);
    	
    	HandleFunctionArguments(constructorDefinitionNode, compilationUnit, ref parserModel);

        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.ColonToken)
        {
        	_ = parserModel.TokenWalker.Consume();
            // Constructor invokes some other constructor as well
        	// 'this(...)' or 'base(...)'
        	
        	SyntaxToken keywordToken;
        	
        	if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.ThisTokenKeyword)
        		keywordToken = parserModel.TokenWalker.Match(SyntaxKind.ThisTokenKeyword);
        	else if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.BaseTokenKeyword)
        		keywordToken = parserModel.TokenWalker.Match(SyntaxKind.BaseTokenKeyword);
        	else
        		keywordToken = default;
        	
        	while (!parserModel.TokenWalker.IsEof)
            {
            	// "short circuit"
            	if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenBraceToken ||
                    parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CloseBraceToken ||
                    parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CloseAngleBracketEqualsToken ||
                    parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.StatementDelimiterToken)
                {
                    break;
                }
                
                // Good case
                if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenParenthesisToken)
                {
                	break;
                }

                _ = parserModel.TokenWalker.Consume();
            }
            
            // Parse secondary syntax ': base(myVariable, 7)'
            if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenParenthesisToken)
            {
            	var openParenthesisToken = parserModel.TokenWalker.Current;
            
				var functionInvocationNode = new FunctionInvocationNode(
					consumedIdentifierToken,
			        genericParameterListing: default,
			        new FunctionParameterListing(
						openParenthesisToken,
				        new List<FunctionParameterEntry>(),
				        closeParenthesisToken: default),
			        CSharpFacts.Types.Void.ToTypeReference());
			        
			    functionInvocationNode.IsParsingFunctionParameters = true;
			        
			    parserModel.ExpressionList.Add((SyntaxKind.CloseParenthesisToken, null));
				parserModel.ExpressionList.Add((SyntaxKind.CommaToken, functionInvocationNode));
				parserModel.ExpressionList.Add((SyntaxKind.ColonToken, functionInvocationNode));
				
				// TODO: The 'ParseNamedParameterSyntaxAndReturnEmptyExpressionNode(...)' code needs to be invoked...
				// ...from within the expression loop.
				// But, as of this comment a way to do so does not exist.
				//
				// Therefore, if the secondary constructor invocation were ': base(person: new Person())'
				// then the first named parameter would not parse correctly.
				//
				// If the second or onwards parameters were named they would be parsed correctly.
				//
				// So, explicitly adding this invocation so that the first named parameter parses correctly.
				//
				_ = parserModel.Binder.ParseNamedParameterSyntaxAndReturnEmptyExpressionNode(compilationUnit, ref parserModel, guaranteeConsume: true);
				
				// This invocation will parse all of the parameters because the 'parserModel.ExpressionList'
				// contains (SyntaxKind.CommaToken, functionParametersListingNode).
				//
				// Upon encountering a CommaToken the expression loop will set 'functionParametersListingNode'
				// to the primary expression, then return an EmptyExpressionNode in order to parse the next parameter.
				_ = ParseOthers.ParseExpression(compilationUnit, ref parserModel);
            }
        }
        
        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.EqualsCloseAngleBracketToken)
        {
        	ParseTokens.MoveToExpressionBody(compilationUnit, ref parserModel);
        }
    }

    /// <summary>Use this method for function definition, whereas <see cref="HandleFunctionParameters"/> should be used for function invocation.</summary>
    public static void HandleFunctionArguments(
    	IFunctionDefinitionNode functionDefinitionNode,
    	CSharpCompilationUnit compilationUnit,
    	ref CSharpParserModel parserModel,
    	VariableKind variableKind = VariableKind.Local)
    {
    	var openParenthesisToken = parserModel.TokenWalker.Consume();
    	var functionArgumentEntryList = new List<FunctionArgumentEntry>();
    	var openParenthesisCount = 1;
    	var corruptState = false;
    	
    	while (!parserModel.TokenWalker.IsEof)
        {
        	if (corruptState)
        	{
        	    if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenParenthesisToken)
        		    openParenthesisCount++;
        	}
        	else if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CloseParenthesisToken)
        	{
        		openParenthesisCount--;
        		
        		if (openParenthesisCount == 0)
        		{
        			break;
        		}
        	}
            else if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.OpenBraceToken)
            {
                break;
            }
            else if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.EqualsCloseAngleBracketToken)
            {
            	break;
            }
            else if (!corruptState)
            {
                parserModel.ArgumentModifierKind = ArgumentModifierKind.None;
                
                checkModifier:
                switch (parserModel.TokenWalker.Current.SyntaxKind)
                {
                    case SyntaxKind.OutTokenKeyword:
            		    _ = parserModel.TokenWalker.Consume();
            		    parserModel.ArgumentModifierKind = ArgumentModifierKind.Out;
            		    break;
            		case SyntaxKind.InTokenKeyword:
            		    _ = parserModel.TokenWalker.Consume();
            		    parserModel.ArgumentModifierKind = ArgumentModifierKind.In;
            		    break;
            		case SyntaxKind.RefTokenKeyword:
            		    _ = parserModel.TokenWalker.Consume();
            		    if (parserModel.ArgumentModifierKind == ArgumentModifierKind.This)
            		    {
            		        parserModel.ArgumentModifierKind = ArgumentModifierKind.ThisRef;
            		        break;
            		    }
            		    else if (parserModel.ArgumentModifierKind == ArgumentModifierKind.Readonly)
            		    {
            		        parserModel.ArgumentModifierKind = ArgumentModifierKind.ReadonlyRef;
            		        break;
            		    }
            		    else
            		    {
            		        parserModel.ArgumentModifierKind = ArgumentModifierKind.Ref;
            		        goto checkModifier;
            		    }
            		case SyntaxKind.ThisTokenKeyword:
            		    _ = parserModel.TokenWalker.Consume();
            		    if (parserModel.ArgumentModifierKind == ArgumentModifierKind.Ref)
            		    {
            		        parserModel.ArgumentModifierKind = ArgumentModifierKind.RefThis;
            		        break;
            		    }
            		    else
            		    {
            		        parserModel.ArgumentModifierKind = ArgumentModifierKind.This;
            		        goto checkModifier;
            		    }
        		    case SyntaxKind.ReadonlyTokenKeyword:
            		    _ = parserModel.TokenWalker.Consume();
            		    if (parserModel.ArgumentModifierKind == ArgumentModifierKind.Ref)
            		    {
            		        parserModel.ArgumentModifierKind = ArgumentModifierKind.RefReadonly;
            		        break;
            		    }
            		    else
            		    {
            		        parserModel.ArgumentModifierKind = ArgumentModifierKind.Readonly;
            		        goto checkModifier;
            		    }
        		    case SyntaxKind.ParamsTokenKeyword:
            		    _ = parserModel.TokenWalker.Consume();
            		    parserModel.ArgumentModifierKind = ArgumentModifierKind.Params;
            		    break;
                }
            
            	var tokenIndexOriginal = parserModel.TokenWalker.Index;
            	
            	var successParse = ParseOthers.TryParseVariableDeclarationNode(compilationUnit, ref parserModel, out var variableDeclarationNode);
            	
            	if (successParse)
            	{
                    parserModel.Binder.CreateVariableSymbol(variableDeclarationNode.IdentifierToken, variableDeclarationNode.VariableKind, compilationUnit, ref parserModel);
    	    		variableDeclarationNode.VariableKind = variableKind;
    	    		parserModel.Binder.BindVariableDeclarationNode(variableDeclarationNode, compilationUnit, ref parserModel, shouldCreateVariableSymbol: false);
                    
                    SyntaxToken optionalCompileTimeConstantToken;
                    
                    if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.EqualsToken)
        			{
        				_ = parserModel.TokenWalker.Consume();
        				optionalCompileTimeConstantToken = parserModel.TokenWalker.Current;
        				
        				parserModel.ExpressionList.Add((SyntaxKind.CloseParenthesisToken, null));
        				parserModel.ExpressionList.Add((SyntaxKind.CommaToken, null));
        				_ = ParseOthers.ParseExpression(compilationUnit, ref parserModel);
        			}
        			else
        			{
        			    optionalCompileTimeConstantToken = new SyntaxToken(SyntaxKind.NotApplicable, textSpan: default);
        			}
        			
        			functionArgumentEntryList.Add(
        				new FunctionArgumentEntry(
    				        variableDeclarationNode,
    				        optionalCompileTimeConstantToken,
    				        parserModel.ArgumentModifierKind));
        			
        			if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CommaToken)
        				_ = parserModel.TokenWalker.Consume();
            	}
            	else
            	{
            	    variableDeclarationNode = null;
            	    corruptState = true;
            	}
            	
            	if (tokenIndexOriginal < parserModel.TokenWalker.Index)
    				continue; // Already consumed so avoid the one at the end of the while loop
            }

            _ = parserModel.TokenWalker.Consume();
        }
        
        var closeParenthesisToken = default(SyntaxToken);
        
        if (parserModel.TokenWalker.Current.SyntaxKind == SyntaxKind.CloseParenthesisToken)
        	closeParenthesisToken = parserModel.TokenWalker.Consume();
        
        functionDefinitionNode.FunctionArgumentListing =
        	new FunctionArgumentListing(
	        	openParenthesisToken,
		        functionArgumentEntryList,
		        closeParenthesisToken);
    }
}
