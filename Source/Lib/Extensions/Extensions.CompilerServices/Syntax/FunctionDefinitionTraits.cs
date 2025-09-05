namespace Walk.Extensions.CompilerServices.Syntax;

public class FunctionDefinitionTraits
{
    public FunctionDefinitionTraits(
        TypeReference returnTypeReference,
        SyntaxToken openAngleBracketToken,
        int indexFunctionArgumentEntryList,
        int countFunctionArgumentEntryList)
    {
        ReturnTypeReference = returnTypeReference;
        OpenAngleBracketToken = openAngleBracketToken;
        IndexFunctionArgumentEntryList = indexFunctionArgumentEntryList;
        CountFunctionArgumentEntryList = countFunctionArgumentEntryList;
    }
    
    public TypeReference ReturnTypeReference { get; set; }
    public SyntaxToken OpenAngleBracketToken { get; set; }
    public int IndexFunctionArgumentEntryList { get; set; }
    public int CountFunctionArgumentEntryList { get; set; }
}
