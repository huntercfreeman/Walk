namespace Walk.Extensions.CompilerServices.Syntax.NodeValues;

public struct FunctionDefinitionTraits
{
    public FunctionDefinitionTraits(
        TypeReferenceValue returnTypeReference,
        SyntaxToken openAngleBracketToken,
        int indexFunctionArgumentEntryList,
        int countFunctionArgumentEntryList)
    {
        ReturnTypeReference = returnTypeReference;
        OpenAngleBracketToken = openAngleBracketToken;
        IndexFunctionArgumentEntryList = indexFunctionArgumentEntryList;
        CountFunctionArgumentEntryList = countFunctionArgumentEntryList;
    }
    
    public TypeReferenceValue ReturnTypeReference { get; set; }
    public SyntaxToken OpenAngleBracketToken { get; set; }
    public int IndexFunctionArgumentEntryList { get; set; }
    public int CountFunctionArgumentEntryList { get; set; }
}
