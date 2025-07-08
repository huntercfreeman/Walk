using Walk.TextEditor.RazorLib;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes;

public sealed class CollectionInitializationNode : IExpressionNode
{
	public CollectionInitializationNode()
	{
		#if DEBUG
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.CollectionInitializationNode++;
		#endif
	}

	public int Unsafe_ParentIndexKey { get; set; }
	public bool IsFabricated { get; init; }
	public SyntaxKind SyntaxKind => SyntaxKind.CollectionInitializationNode;
	
	public TypeReference ResultTypeReference { get; }
	
	public bool IsClosed { get; set; }

	public string IdentifierText(string sourceText, TextEditorService textEditorService) => nameof(CollectionInitializationNode);

#if DEBUG
	~CollectionInitializationNode()
	{
		Walk.Common.RazorLib.Installations.Models.WalkDebugSomething.CollectionInitializationNode--;
	}
	#endif
}