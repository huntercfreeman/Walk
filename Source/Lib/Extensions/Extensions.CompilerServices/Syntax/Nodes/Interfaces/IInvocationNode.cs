namespace Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

public interface IInvocationNode : IExpressionNode
{
    public FunctionParameterListing FunctionParameterListing { get; }
    public bool IsParsingFunctionParameters { get; set; }
    public int IdentifierStartInclusiveIndex { get; }
}
