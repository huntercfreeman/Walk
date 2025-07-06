using Walk.Extensions.CompilerServices;
using Walk.Extensions.CompilerServices.Syntax.Nodes;

namespace Walk.CompilerServices.CSharp.Facts;

public partial class CSharpFacts
{
    public class ScopeFacts
    {
        public static Scope GetInitialGlobalScope()
        {
            var typeDefinitionMap = new Dictionary<string, TypeDefinitionNode>
	        {
	            {
	                Types.Void.TypeIdentifierToken.TextSpan.Text,
	                Types.Void
	            },
	            {
	                Types.Var.TypeIdentifierToken.TextSpan.Text,
	                Types.Var
	            },
	            {
	                Types.Bool.TypeIdentifierToken.TextSpan.Text,
	                Types.Bool
	            },
	            {
	                Types.Int.TypeIdentifierToken.TextSpan.Text,
	                Types.Int
	            },
	            {
	                Types.String.TypeIdentifierToken.TextSpan.Text,
	                Types.String
	            },
	        };
	        
			return new Scope(
				indexCodeBlockOwner: 0,
				indexKey: 0,
			    parentIndexKey: -1,
			    0,
			    endExclusiveIndex: -1);
        }
    }
}