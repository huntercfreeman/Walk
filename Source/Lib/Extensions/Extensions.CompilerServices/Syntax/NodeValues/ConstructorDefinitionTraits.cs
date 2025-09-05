namespace Walk.Extensions.CompilerServices.Syntax.NodeValues;

public struct ConstructorDefinitionTraits
{
    public ConstructorDefinitionTraits(
        TypeReferenceValue returnTypeReference,
        int indexFunctionArgumentEntryList,
        int countFunctionArgumentEntryList)
    {
        ReturnTypeReference = returnTypeReference;
        IndexFunctionArgumentEntryList = indexFunctionArgumentEntryList;
        CountFunctionArgumentEntryList = countFunctionArgumentEntryList;
    }
    
    public TypeReferenceValue ReturnTypeReference { get; set; }
    public int IndexFunctionArgumentEntryList { get; set; }
    public int CountFunctionArgumentEntryList { get; set; }
}
