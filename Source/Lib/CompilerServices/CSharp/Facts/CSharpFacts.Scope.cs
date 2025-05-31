using Walk.Extensions.CompilerServices;
using Walk.Extensions.CompilerServices.Syntax.Nodes;

namespace Walk.CompilerServices.CSharp.Facts;

public partial class CSharpFacts
{
    public class ScopeFacts
    {
        public static Scope GetInitialGlobalScope()
        {
            var typeDefinitionMap = new Dictionary<int, TypeDefinitionNode>
	        {
	            {
	                Types.Void.TypeIdentifierToken.TextSpan.ToHash(),
	                Types.Void
	            },
	            {
	                Types.Var.TypeIdentifierToken.TextSpan.ToHash(),
	                Types.Var
	            },
	            {
	                Types.Bool.TypeIdentifierToken.TextSpan.ToHash(),
	                Types.Bool
	            },
	            {
	                Types.Int.TypeIdentifierToken.TextSpan.ToHash(),
	                Types.Int
	            },
	            {
	                Types.String.TypeIdentifierToken.TextSpan.ToHash(),
	                Types.String
	            },
	        };
	        
			return new Scope(
				codeBlockOwner: null,
				indexKey: 0,
			    parentIndexKey: -1,
			    0,
			    endExclusiveIndex: -1);
        }
    }
}