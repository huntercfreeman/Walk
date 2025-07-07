using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Utility;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

public interface ICodeBlockOwner : ISyntaxNode
{
	public ScopeDirectionKind ScopeDirectionKind { get; }
	/// <summary>
	/// This should be initialized to -1 as that will imply "null" / that it wasn't set yet.
	/// </summary>
	public int Scope_StartInclusiveIndex { get; set; }
	/// <summary>
	/// This should be initialized to -1 as that will imply "null" / that it wasn't set yet.
	/// </summary>
	public int Scope_EndExclusiveIndex { get; set; }
	/// <summary>
	/// This should be initialized to -1 as that will imply "null" / that it wasn't set yet.
	/// </summary>
	public int CodeBlock_StartInclusiveIndex { get; set; }
	/// <summary>
	/// This should be initialized to -1 as that will imply "null" / that it wasn't set yet.
	/// </summary>
	public int CodeBlock_EndExclusiveIndex { get; set; }
	/// <summary>
	/// This should be initialized to -1 as that will imply "null" / that it wasn't set yet.
	///
	/// This indicates the index that the parent 'ICodeBlockOwner' is at in the 'CSharpCompilationUnit.DefinitionTupleList'.
	///
	/// This is unsafe, because you must be certain that all data you're interacting with is coming from the same 'CSharpCompilationUnit'.
	/// </summary>
	public int Unsafe_ParentIndexKey { get; set; }
	/// <summary>
	/// This should be initialized to -1 as that will imply "null" / that it wasn't set yet.
	///
	/// This indicates the index that this 'ICodeBlockOwner' is at in the 'CSharpCompilationUnit.DefinitionTupleList'.
	///
	/// This is unsafe, because you must be certain that all data you're interacting with is coming from the same 'CSharpCompilationUnit'.
	/// </summary>
	public int Unsafe_SelfIndexKey { get; set; }
	
	public bool PermitCodeBlockParsing { get; set; }
	
	/// <summary>
	/// This belongs on the 'CSharpCodeBlockBuilder', not the 'ICodeBlockOwner'.
	///
	/// This is computational state to know whether to search
	/// for 'StatementDelimiterToken' (if this is true) as the terminator or a 'CloseBraceToken' (if this is false).
	///
	/// This is not necessary to disambiguate the SyntaxKind of the text spans that mark
	/// the start and end of the code block.
	///
	/// This is mentioned because that might be an argument for this being moved to 'ICodeBlockOwner'.
	///
	/// But, .... interrupting my thought I think I'm wrong hang on....
	///
	/// ````public void SomeFunction() => }
	/// 
	/// What should the above code snippet parse as?
	/// Should the '}' be consumed as the closing delimiter token for 'SomeFunction()'?
	///
	/// Is it the case that the closing text span of a scope is only
	/// a 'CloseBracetoken' if the start text span is from an 'OpenBraceToken'?
	///
	/// Furthermore, is it true that the start text span is only non-null
	/// if it is an 'OpenBraceToken' that started the code block?
	///
	/// An implicitly opened code block can have its start text span
	/// retrieved on a 'per ICodeBlockOwner' basis.
	///
	/// I am going to decide that:
	/// ````public void SomeFunction() => }
	///
	/// will not consume the 'CloseBraceToken' as its delimiter.
	/// This matter is open to be changed though,
	/// this decision is only being made to create consistency.
	/// </summary>
	public bool IsImplicitOpenCodeBlockTextSpan { get; set; }

	public TypeReference GetReturnTypeReference();
	
	public static void ThrowMultipleScopeDelimiterException(List<TextEditorDiagnostic> diagnosticList, TokenWalker tokenWalker)
	{
		// 'model.TokenWalker.Current.TextSpan' isn't necessarily the syntax passed to this method.
		// TODO: But getting a TextSpan from a general type such as 'ISyntax' is a pain.
		//
		/*diagnosticBag.ReportTodoException(
    		tokenWalker.Current.TextSpan,
    		"Scope must be set by either OpenBraceToken and CloseBraceToken; or by StatementDelimiterToken, but not both.");*/
	}

	public static void ThrowAlreadyAssignedCodeBlockNodeException(List<TextEditorDiagnostic> diagnosticList, TokenWalker tokenWalker)
	{
		// 'model.TokenWalker.Current.TextSpan' isn't necessarily the syntax passed to this method.
		// TODO: But getting a TextSpan from a general type such as 'ISyntax' is a pain.
		//
		/*diagnosticBag.ReportTodoException(
    		tokenWalker.Current.TextSpan,
    		$"The {nameof(CodeBlockNode)} was already assigned.");*/
	}
}
