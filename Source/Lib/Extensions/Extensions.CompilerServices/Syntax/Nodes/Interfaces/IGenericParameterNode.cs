namespace Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

public interface IGenericParameterNode : IExpressionNode
{
    public SyntaxToken OpenAngleBracketToken { get; set; }
    public List<GenericParameterEntry> GenericParameterEntryList { get; set; }
    public SyntaxToken CloseAngleBracketToken { get; set; }
    
    public bool IsParsingGenericParameters { get; set; }
}
