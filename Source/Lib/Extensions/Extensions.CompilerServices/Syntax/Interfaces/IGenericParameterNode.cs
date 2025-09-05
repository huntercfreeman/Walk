namespace Walk.Extensions.CompilerServices.Syntax.Interfaces;

public interface IGenericParameterNode : IExpressionNode
{
    public SyntaxToken OpenAngleBracketToken { get; set; }
    /// <summary>The default value for this needs to be -1 to indicate that there are no entries in the pooled list.</summary>
    public int IndexGenericParameterEntryList { get; set; }
    public int CountGenericParameterEntryList { get; set; }
    public SyntaxToken CloseAngleBracketToken { get; set; }
    
    public bool IsParsingGenericParameters { get; set; }
}
