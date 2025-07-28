namespace Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

public interface IInvocationNode : IExpressionNode
{
    public SyntaxToken OpenParenthesisToken { get; set; }
    public int IndexFunctionParameterEntryList { get; set; }
    public int CountFunctionParameterEntryList { get; set; }
    public SyntaxToken CloseParenthesisToken { get; set; }
    
    public bool IsParsingFunctionParameters { get; set; }
    public int IdentifierStartInclusiveIndex { get; }
}
