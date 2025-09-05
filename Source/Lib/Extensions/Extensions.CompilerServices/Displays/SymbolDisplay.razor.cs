using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.Extensions.CompilerServices.Syntax;

namespace Walk.Extensions.CompilerServices.Displays;

public partial class SymbolDisplay : ComponentBase
{
    [Inject]
    private TextEditorService TextEditorService { get; set; } = null!;

    [Parameter, EditorRequired]
    public Symbol Symbol { get; set; }
    [Parameter, EditorRequired]
    public ResourceUri ResourceUri { get; set; }
    
    private int _shouldRenderHashCode = 0;
    
    protected override bool ShouldRender()
    {
        // When reading about 'HashCode.Combine'
        // it could not be determined whether it could throw an exception
        // (specifically thinking of integer overflow).
        //
        // The UI under no circumstance should cause a fatal exception,
        // especially a tooltip.
        //
        // Not much time was spent looking into this because far higher priority
        // work needs to be done.
        //
        // Therefore a try catch is being put around this to be safe.
    
        try
        {
            var outShouldRenderHashCode = HashCode.Combine(
                Symbol.TextSpan.StartInclusiveIndex,
                Symbol.TextSpan.EndExclusiveIndex,
                Symbol.TextSpan.DecorationByte,
                ResourceUri.Value);
                
            if (outShouldRenderHashCode != _shouldRenderHashCode)
            {
                _shouldRenderHashCode = outShouldRenderHashCode;
                return true;
            }
            
            return false;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
    
    private Task OpenInEditorOnClick(string filePath)
    {
        TextEditorService.WorkerArbitrary.PostUnique(async editContext =>
        {
            await TextEditorService.OpenInEditorAsync(
                editContext,
                filePath,
                true,
                null,
                new Category("main"),
                Key<TextEditorViewModel>.NewKey());
        });
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// A TypeSymbol is used for both a TypeClauseNode and a TypeDefinitionNode.
    ///
    /// Therefore, if one hovers a TypeSymbol that maps to a TypeClauseNode.
    /// An additional step is needed to get the actual TypeDefinitionNode that the TypeClauseNode is referencing.
    ///
    /// The 'targetNode' is whichever node the ISymbol directly mapped to.
    /// </summary>
    public static ISyntaxNode? GetTargetNode(TextEditorService textEditorService, Symbol symbolLocal, ResourceUri resourceUri)
    {
        try
        {
            var textEditorModel = textEditorService.Model_GetOrDefault(resourceUri);
            if (textEditorModel is null)
                return null;
            
            var extendedCompilerService = (IExtendedCompilerService)textEditorModel.PersistentState.CompilerService;
            
            var compilerServiceResource = extendedCompilerService.GetResource(textEditorModel.PersistentState.ResourceUri);
            if (compilerServiceResource is null)
                return null;
    
            return extendedCompilerService.GetSyntaxNode(
                symbolLocal.TextSpan.StartInclusiveIndex,
                textEditorModel.PersistentState.ResourceUri,
                compilerServiceResource);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }
    
    /// <summary>
    /// If the 'targetNode' itself is a definition, then return the 'targetNode'.
    ///
    /// Otherwise, ask the IBinder for the definition node:
    /// </summary>
    public static SyntaxNodeValue GetDefinitionNode(TextEditorService textEditorService, Symbol symbolLocal, SyntaxNodeValue targetNode, ResourceUri resourceUri)
    {
        try
        {
            if (!targetNode.IsDefault())
            {
                switch (targetNode.SyntaxKind)
                {
                    case SyntaxKind.ConstructorDefinitionNode:
                    case SyntaxKind.FunctionDefinitionNode:
                    case SyntaxKind.NamespaceStatementNode:
                    case SyntaxKind.TypeDefinitionNode:
                    case SyntaxKind.VariableDeclarationNode:
                        return targetNode;
                }
            }
        
            var textEditorModel = textEditorService.Model_GetOrDefault(resourceUri);
            var extendedCompilerService = (IExtendedCompilerService)textEditorModel.PersistentState.CompilerService;
            var compilerServiceResource = extendedCompilerService.GetResource(textEditorModel.PersistentState.ResourceUri);
    
            return extendedCompilerService.GetDefinitionNodeValue(symbolLocal.TextSpan, textEditorModel.PersistentState.ResourceUri, compilerServiceResource, symbolLocal);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return default;
        }
    }
}
