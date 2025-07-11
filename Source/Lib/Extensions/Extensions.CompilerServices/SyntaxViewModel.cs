using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes;
using Walk.TextEditor.RazorLib.CompilerServices;

namespace Walk.Extensions.CompilerServices;

/// <summary>
/// All the properties are null because the symbol UI is used recursively.
/// 
/// i.e.:
/// TypeDefinitionNode renders TypeDefinitionNodeDisplay
/// 	TypeSyntaxDisplay
/// 	GenericArgumentsListingNode
/// 		TypeSyntaxDisplay
/// 		...
///
/// So, the symbol tooltip starts with 'symbol' not being null.
///
/// But, if the UI that represents the symbol requires
/// multiple nodes to be shown, symbol goes on to be null for the inner UI components.
///
/// The 'SymbolDisplay' is expected to prevent itself from re-rendering unnecessarily.
/// So, the optimization of the tooltip is not as crucial because
/// of that, and furthermore because it is a throttled user gesture
/// so many many loops of rendering this within a short period of time are expected to never occur.
/// </summary>
public struct SyntaxViewModel
{
	public SyntaxViewModel(
	    ICompilerService compilerService,
	    TextEditorService textEditorService,
	    ResourceUri resourceUri,
		Symbol? targetSymbol,
		ISyntaxNode? targetNode,
		ISyntaxNode? definitionNode,
		int depth)
	{
	    CompilerService = compilerService;
	    TextEditorService = textEditorService;
	    ResourceUri = resourceUri;
		TargetSymbol = targetSymbol;
		TargetNode = targetNode;
		DefinitionNode = definitionNode;
		Depth = depth;
	}
	
	public ICompilerService CompilerService { get; }
	
	public TextEditorService TextEditorService { get; }
	
	public ResourceUri ResourceUri { get; set; }
	
	/// <summary>
	/// The user interface gesture is to hover a symbol in the text editor.
	/// Other than a UI event, it is unlikely this property is non-null.
	/// </summary>
	public Symbol? TargetSymbol { get; }
	
	/// <summary>
	/// The user interface provided an 'ITextEditorSymbol',
	/// but there is little data available on a symbol relative
	/// to the an ISyntaxNode it represents.
	///
	/// Therefore, the symbol is mapped to its corresponding node,
	/// so that more information can be displayed on the UI.
	/// </summary>
	public ISyntaxNode? TargetNode { get; }
	
	/// <summary>
	/// The user interface goes from an ISymbol to an ISyntaxNode,
	/// but that ISyntaxNode might be a reference to a definition node, declaration node, or etc...
	///
	/// So, since there is other data that exists on the definition node, that the reference node
	/// does not have access to, then the definition node is retrieved
	/// in order to display more information on the UI.
	///
	/// As well, if there is a definition node, then when
	/// the text is hovered, it should show an underline and be clickable.
	/// This click will then 'goto definition' to that node.
	/// </summary>
	public ISyntaxNode? DefinitionNode { get; }
	
	/// <summary>
	/// The 'TypeSyntaxDisplay' is used recursively because a 'TypeClauseNode'
	/// can have a 'GenericParametersListingNode' which goes on to contains other 'TypeClauseNode'
	/// (which as well may or may not have their own 'GenericParametersListingNode').
	///
	/// But, it is only for a 'Depth == 0' that extra information about the
	/// node should be added to the UI. (is it public..., partial, class or struct..., etc...)
	/// </summary>
	public int Depth { get; }
	
	public bool ShowPrefixText { get; } = false;
	public bool ShouldTryResolveNullDefinitionNode { get; } = false;
	
	public Task HandleOnClick(TextEditorService textEditorService, SyntaxKind syntaxKindExpected)
	{
		if (DefinitionNode is null || DefinitionNode.SyntaxKind != syntaxKindExpected)
			return Task.CompletedTask;
		
		var resourceUri = ResourceUri;
		var compilerService = CompilerService;
		
		textEditorService.WorkerArbitrary.PostUnique(editContext =>
		{
		    var model = editContext.GetModelModifier(resourceUri);
		    
		    var tooltipState = textEditorService.CommonUtilityService.GetTooltipState();
		    
		    if (tooltipState.TooltipModel.ItemUntyped is not ValueTuple<TextEditorService, Key<TextEditorViewModel>> tuple)
                return ValueTask.CompletedTask;
            
            var viewModel = editContext.GetViewModelModifier(tuple.Item2);
		    
		    return compilerService.GoToDefinition(
                editContext,
                model,
                viewModel,
                new Category("main"),
        		model.GetPositionIndex(viewModel));
		});
		
		return Task.CompletedTask;
	}
	
	public string GetIdentifierText(ISyntaxNode node)
	{
	    if (CompilerService is IExtendedCompilerService extendedCompilerService)
	    {
	        var compilationUnit = CompilerService.GetResource(ResourceUri);
	        return extendedCompilerService.GetIdentifierText(node, ResourceUri);
        }
	    
	    return "null";
	}
	
	public string GetTextFromTextSpan(TextEditorTextSpan textSpan)
	{
	    if (CompilerService is IExtendedCompilerService extendedCompilerService)
	    {
	        var compilationUnit = CompilerService.GetResource(ResourceUri)?.CompilationUnit;
	        
	        if (compilationUnit is IExtendedCompilationUnit extendedCompilationUnit)
	            return textSpan.GetText(extendedCompilationUnit.SourceText, TextEditorService);
        }
	    
        return "null";
	}
}
