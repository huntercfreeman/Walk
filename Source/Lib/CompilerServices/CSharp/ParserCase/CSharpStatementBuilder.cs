using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;
using Walk.CompilerServices.CSharp.CompilerServiceCase;
using Walk.CompilerServices.CSharp.BinderCase;

namespace Walk.CompilerServices.CSharp.ParserCase;

public struct CSharpStatementBuilder
{
	public CSharpStatementBuilder(CSharpBinder binder)
	{
		ChildList = binder.CSharpStatementBuilder_ChildList;
		ChildList.Clear();
		
		ParseLambdaStatementScopeStack = binder.CSharpStatementBuilder_ParseLambdaStatementScopeStack;
		ParseLambdaStatementScopeStack.Clear();
	}

	public List<ISyntax> ChildList { get; }
	
	/// <summary>
    /// Prior to finishing a statement, you must check whether ParseLambdaStatementScopeStack has a child that needs to be parsed.
    /// All currently known cases of finishing a statement will do so by invoking FinishStatement(...),
    /// this method will perform this check internally.
	/// </summary>
	public Stack<(ICodeBlockOwner CodeBlockOwner, CSharpDeferredChildScope DeferredChildScope)> ParseLambdaStatementScopeStack { get; }
	
	/// <summary>Invokes the other overload with index: ^1</summary>
	public bool TryPeek(out ISyntax syntax)
	{
		return TryPeek(^1, out syntax);
	}
	
	/// <summary>^1 gives the last entry</summary>
	public bool TryPeek(Index index, out ISyntax syntax)
	{
		if (ChildList.Count - index.Value > -1)
		{
			syntax = ChildList[index];
			return true;
		}
		
		syntax = null;
		return false;
	}
	
	public ISyntax Pop()
	{
		var syntax = ChildList[^1];
		ChildList.RemoveAt(ChildList.Count - 1);
		return syntax;
	}

	/// <summary>
	/// If 'StatementDelimiterToken', 'OpenBraceToken', or 'CloseBraceToken'
	/// are parsed by the main loop,
	///
	/// Then check that the last item in the StatementBuilder.ChildList
	/// has been added to the parserModel.CurrentCodeBlockBuilder.ChildList.
	///
	/// If it was not yet added, then add it.
	///
	/// Lastly, clear the StatementBuilder.ChildList.
	///
	/// Returns the result of 'ParseLambdaStatementScopeStack.TryPop(out var deferredChildScope)'.
	/// </summary>
	public bool FinishStatement(int finishTokenIndex, ref CSharpParserModel parserModel)
	{
		// TODO: This is bad. Only do this when constructing the struct version.
		parserModel.TypeClauseNode.IsBeingUsed = false;
		parserModel.VariableReferenceNode.IsBeingUsed = false;
		
		ChildList.Clear();
		/*if (ChildList.Count != 0)
		{
			var statementSyntax = ChildList[^1];
			
			ISyntax codeBlockBuilderSyntax;
			
			if (parserModel.CurrentCodeBlockBuilder.ChildList.Count == 0)
				codeBlockBuilderSyntax = EmptyExpressionNode.Empty;
			else
				codeBlockBuilderSyntax = parserModel.CurrentCodeBlockBuilder.ChildList[^1];

			if (!Object.ReferenceEquals(statementSyntax, codeBlockBuilderSyntax) &&
				!Object.ReferenceEquals(statementSyntax, parserModel.CurrentCodeBlockBuilder.CodeBlockOwner))
			{
				parserModel.CurrentCodeBlockBuilder.AddChild(statementSyntax);
			}
			
			ChildList.Clear();
		}*/
		
		if (ParseLambdaStatementScopeStack.Count > 0)
		{
			var tuple = ParseLambdaStatementScopeStack.Peek();
			
			if (Object.ReferenceEquals(tuple.CodeBlockOwner, parserModel.CurrentCodeBlockOwner))
			{
				tuple = ParseLambdaStatementScopeStack.Pop();
				tuple.DeferredChildScope.PrepareMainParserLoop(finishTokenIndex, ref parserModel);
				return true;
			}
		}
		
		return false;
	}
}

