using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class NamespaceStatementNode : ICodeBlockOwner
{
	public NamespaceStatementNode(
		SyntaxToken keywordToken,
		SyntaxToken identifierToken,
		CodeBlock codeBlock)
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.NamespaceStatementNode++;
		#endif
	
		KeywordToken = keywordToken;
		IdentifierToken = identifierToken;
		CodeBlock = codeBlock;
	}
	
	private bool _isDirtytypeDefinitionNodeList = true;
	private List<TypeDefinitionNode> _typeDefinitionNodeList;
	
	private CodeBlock _codeBlock;

	public SyntaxToken KeywordToken { get; }
	public SyntaxToken IdentifierToken { get; }
	
	public List<TypeDefinitionNode> TypeDefinitionNodeList
	{
		get
		{
			if (_isDirtytypeDefinitionNodeList)
			{
				_typeDefinitionNodeList = CodeBlock.ChildList
					.Where(innerC => innerC.SyntaxKind == SyntaxKind.TypeDefinitionNode)
					.Select(td => (TypeDefinitionNode)td)
					.ToList();
				_isDirtytypeDefinitionNodeList = false;
			}
			
			return _typeDefinitionNodeList;
		}
	}

	// ICodeBlockOwner properties.
	public ScopeDirectionKind ScopeDirectionKind => ScopeDirectionKind.Both;
	public TextEditorTextSpan OpenCodeBlockTextSpan { get; set; }
	
	public CodeBlock CodeBlock
	{
		get => _codeBlock;
		set
		{
			_codeBlock = value;
			_isDirtytypeDefinitionNodeList = true;
		}
	}
	
	public TextEditorTextSpan CloseCodeBlockTextSpan { get; set; }
	public int ScopeIndexKey { get; set; } = -1;

	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.NamespaceStatementNode;

	#region ICodeBlockOwner_Methods
	public TypeReference GetReturnTypeReference()
	{
		return TypeFacts.Empty.ToTypeReference();
	}
	#endregion
	
	#if DEBUG	
	~NamespaceStatementNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.NamespaceStatementNode--;
	}
	#endif
}
