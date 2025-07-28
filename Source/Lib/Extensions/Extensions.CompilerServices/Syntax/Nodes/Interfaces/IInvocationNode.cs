namespace Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

public interface IInvocationNode : IExpressionNode
{
    public SyntaxToken OpenParenthesisToken { get; set; }
    /// <summary>The default value for this needs to be -1 to indicate that there are no entries in the pooled list.</summary>
    public int IndexFunctionParameterEntryList { get; set; }
    public int CountFunctionParameterEntryList { get; set; }
    public SyntaxToken CloseParenthesisToken { get; set; }
    
    public bool IsParsingFunctionParameters { get; set; }
    public int IdentifierStartInclusiveIndex { get; }
}
