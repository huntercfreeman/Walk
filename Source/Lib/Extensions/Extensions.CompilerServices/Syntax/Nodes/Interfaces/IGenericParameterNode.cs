namespace Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

public interface IGenericParameterNode : IExpressionNode
{
    public SyntaxToken OpenAngleBracketToken { get; set; }
    public int IndexGenericParameterEntryList { get; set; }
    public int CountGenericParameterEntryList { get; set; }
    public SyntaxToken CloseAngleBracketToken { get; set; }
    
    public bool IsParsingGenericParameters { get; set; }
}
