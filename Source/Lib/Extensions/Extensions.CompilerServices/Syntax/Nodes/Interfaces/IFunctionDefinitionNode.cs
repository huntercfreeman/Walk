namespace Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

public interface IFunctionDefinitionNode : IExpressionNode
{
    /// <summary>
    /// TODO: does this having a setter bug TypeDefinitionNode '_memberList'.
    /// </summary>
    public SyntaxToken OpenParenthesisToken { get; set; }
    public List<FunctionArgumentEntry> FunctionArgumentEntryList { get; set; }
    public SyntaxToken CloseParenthesisToken { get; set; }
}
