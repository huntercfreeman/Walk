using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Enums;
using Walk.Extensions.CompilerServices.Utility;

namespace Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

public interface ICodeBlockOwner : ISyntaxNode
{
    /// <summary>
    /// This should be initialized to -1 as that will imply "null" / that it wasn't set yet.
    ///
    /// This indicates the index that this 'ICodeBlockOwner' is at in the 'CSharpCompilationUnit.DefinitionTupleList'.
    ///
    /// This is unsafe, because you must be certain that all data you're interacting with is coming from the same 'CSharpCompilationUnit'.
    /// </summary>
    public int SelfScopeSubIndex { get; set; }
    
    public static bool ImplementsICodeBlockOwner(SyntaxKind syntaxKind)
    {
        switch (syntaxKind)
        {
            case SyntaxKind.DoWhileStatementNode:
            case SyntaxKind.ForeachStatementNode:
            case SyntaxKind.ArbitraryCodeBlockNode:
            case SyntaxKind.ConstructorDefinitionNode:
            case SyntaxKind.ForStatementNode:
            case SyntaxKind.FunctionDefinitionNode:
            case SyntaxKind.IfStatementNode:
            case SyntaxKind.GetterOrSetterNode:
            case SyntaxKind.GlobalCodeBlockNode:
            case SyntaxKind.NamespaceStatementNode:
            case SyntaxKind.LambdaExpressionNode:
            case SyntaxKind.LockStatementNode:
            case SyntaxKind.TryStatementFinallyNode:
            case SyntaxKind.SwitchStatementNode:
            case SyntaxKind.TryStatementTryNode:
            case SyntaxKind.WhileStatementNode:
            case SyntaxKind.TryStatementCatchNode:
            case SyntaxKind.TypeDefinitionNode:
                return true;
            default:
                return false;
        }
    }
}
