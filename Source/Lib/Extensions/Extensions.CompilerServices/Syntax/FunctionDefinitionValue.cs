namespace Walk.Extensions.CompilerServices.Syntax;

public class FunctionDefinitionValue
{
    public FunctionDefinitionValue(TypeReference returnTypeReference)
    {
        ReturnTypeReference = returnTypeReference;
    }
    
    public TypeReference ReturnTypeReference { get; set; }
}
