namespace Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

public interface IFunctionDefinitionNode : IExpressionNode
{
    /// <summary>
    /// TODO: does this having a setter bug TypeDefinitionNode '_memberList'.
    /// </summary>
    public SyntaxToken OpenParenthesisToken { get; set; }
    /// <summary>The default value for this needs to be -1 to indicate that there are no entries in the pooled list.</summary>
    public int IndexFunctionArgumentEntryList { get; set; }
    public int CountFunctionArgumentEntryList { get; set; }
    public SyntaxToken CloseParenthesisToken { get; set; }
}
