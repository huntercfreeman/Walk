namespace Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

public interface IInvocationNode : IExpressionNode
{
    public SyntaxToken OpenParenthesisToken { get; set; }
    public List<FunctionParameterEntry> FunctionParameterEntryList { get; set; }
    public SyntaxToken CloseParenthesisToken { get; set; }
    
    public bool IsParsingFunctionParameters { get; set; }
    public int IdentifierStartInclusiveIndex { get; }
}
