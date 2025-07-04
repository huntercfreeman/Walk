using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes;

namespace Walk.CompilerServices.CSharp.Facts;

public partial class CSharpFacts
{
    public class Namespaces
    {
        private const string SystemNamespaceIdentifier = "System";

        public const string TopLevelNamespaceIdentifier = "GetTopLevelNamespaceStatementNode";

        /// <summary>TODO: Implement the initial bound namespaces correctly. Until then just add the "System" namespace as a nonsensical node.</summary>
        public static Dictionary<string, NamespaceGroup> GetInitialBoundNamespaceStatementNodes()
        {
            return new Dictionary<string, NamespaceGroup>
            {
                {
                    SystemNamespaceIdentifier,
                    new NamespaceGroup(SystemNamespaceIdentifier,
                        new List<NamespaceStatementNode>
                        {
                            new NamespaceStatementNode(
                                new(SyntaxKind.UnrecognizedTokenKeyword, new(0, 0, 0)),
                                new(SyntaxKind.IdentifierToken, new(0, SystemNamespaceIdentifier.Length, 0)),
                                new CodeBlock(Array.Empty<ISyntax>()))
                        })
                }
            };
        }
        
        public static NamespaceStatementNode GetTopLevelNamespaceStatementNode()
        {
            return new NamespaceStatementNode(
                new(SyntaxKind.UnrecognizedTokenKeyword, new(0, 0, 0)),
                new(SyntaxKind.IdentifierToken, new(0, TopLevelNamespaceIdentifier.Length, 0)),
                new CodeBlock(Array.Empty<ISyntax>()));
        }
    }
}