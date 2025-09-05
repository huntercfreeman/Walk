using Walk.Extensions.CompilerServices.Syntax.NodeReferences;

namespace Walk.Extensions.CompilerServices.Syntax.NodeValues;

public record struct NamespaceGroup
{
    public NamespaceGroup(
        int charIntSum,
        List<NamespaceStatementNode> namespaceStatementNodeList)
    {
        CharIntSum = charIntSum;
        NamespaceStatementNodeList = namespaceStatementNodeList;
    }

    public int CharIntSum { get; }
    public List<NamespaceStatementNode> NamespaceStatementNodeList { get; }

    public bool ConstructorWasInvoked => NamespaceStatementNodeList is not null;
}
