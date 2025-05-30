using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Utility;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

public interface ICodeBlockOwner : ISyntaxNode
{
	// ICodeBlockOwner properties.
	public ScopeDirectionKind ScopeDirectionKind { get; }
	public TextEditorTextSpan OpenCodeBlockTextSpan { get; set; }
	public CodeBlock CodeBlock { get; set; }
	public TextEditorTextSpan CloseCodeBlockTextSpan { get; set; }
	
	/// <summary>
	/// The initializer for this should set it to '-1'
	/// to signify that the scope was not yet assigned.
	///
	/// This is unfortunate, since the default value of 'int' is '0',
	/// then this is sort of "omnipotent information" that absolutely would
	/// cause a bug if someone were to miss this step when
	/// creating a new implementation of the 'ICodeBlockOwner'.
	/// 
	/// Initially this was a nullable int, in order to ensure the implementations
	/// of 'ICodeBlockOwner', didn't have to initialize the int to '-1'.
	///
	/// The issue with the nullable approach, is that whenever you are parsing
	/// and want to change scope, you're then reading a nullable int,
	/// which results in the int being boxed.
	/// </summary>
	public int ScopeIndexKey { get; set; }

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
