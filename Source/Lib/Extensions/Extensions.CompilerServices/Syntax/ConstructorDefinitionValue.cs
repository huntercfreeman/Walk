namespace Walk.Extensions.CompilerServices.Syntax;

public struct ConstructorDefinitionValue
{
    public ConstructorDefinitionValue(
        TypeReference returnTypeReference,
        int indexFunctionArgumentEntryList,
        int countFunctionArgumentEntryList)
    {
        ReturnTypeReference = returnTypeReference;
        IndexFunctionArgumentEntryList = indexFunctionArgumentEntryList;
        CountFunctionArgumentEntryList = countFunctionArgumentEntryList;
    }
    
    public TypeReference ReturnTypeReference { get; set; }
    public int IndexFunctionArgumentEntryList { get; set; }
    public int CountFunctionArgumentEntryList { get; set; }
}
