using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes;

namespace Walk.CompilerServices.CSharp.ParserCase.Internals;

public static class ParseOthers
{
	/// <summary>
	/// TODO: Delete this method, to parse a namespace identifier one should be able to just invoke 'ParseExpression(...)'
	///
	/// 'isNamespaceStatement' refers to 'namespace Walk.CompilerServices;'
	/// </summary>
	public static SyntaxToken HandleNamespaceIdentifier(ref CSharpParserModel parserModel, bool isNamespaceStatement)
    {
        TextEditorTextSpan textSpan = default;
        int count = 0;

        while (!parserModel.TokenWalker.IsEof)
        {
            if (count % 2 == 0)
            {
                var matchedToken = parserModel.TokenWalker.Match(SyntaxKind.IdentifierToken);
                count++;
                
                if (textSpan == default)
                {
                	textSpan = matchedToken.TextSpan;
                }
                else
                {
                	textSpan = textSpan with
			        {
			            EndExclusiveIndex = matchedToken.TextSpan.EndExclusiveIndex
			        };
                }
                
                // NamespaceStatements will add the final symbol themselves.
                
                if (isNamespaceStatement && (parserModel.TokenWalker.Next.SyntaxKind != SyntaxKind.StatementDelimiterToken))
                {
                    // !StatementDelimiterToken because presumably the final namespace is already being handled.
    		        //
    		        // (I don't think the above statement is true... the final namespace gets handled only after the codeblock is parsed.
    		        //  so you should probably bring the other contributors of the namespace into scope immediately).
    		        // 
		        	parserModel.AddNamespaceToCurrentScope(textSpan.GetText(parserModel.Compilation.SourceText, parserModel.Binder.TextEditorService));
		        }

                if (matchedToken.IsFabricated)
                    break;
            }
            else
            {
                if (SyntaxKind.MemberAccessToken == parserModel.TokenWalker.Current.SyntaxKind)
                {
                	_ = parserModel.TokenWalker.Consume();
                    count++;
                }
                else
                {
                    break;
                }
            }
        }

        if (count == 0)
            return default;
            
        parserModel.Compilation.__SymbolList.Add(
        	new Symbol(
        		SyntaxKind.NamespaceSymbol,
        		parserModel.GetNextSymbolId(),
        		textSpan));

        return new SyntaxToken(SyntaxKind.IdentifierToken, textSpan);
    }
    
    /// <summary>
    /// parserModel.TokenWalker.Current is a NameableToken
    /// parserModel.TokenWalker.Next is a ColonToken
    /// </summary>
    public static void HandleLabelDeclaration(ref CSharpParserModel parserModel)
    {
        var labelDeclarationNode = new LabelDeclarationNode(parserModel.TokenWalker.Current);
		
	    parserModel.BindLabelDeclarationNode(labelDeclarationNode);
            
        var labelReferenceNode = new LabelReferenceNode(labelDeclarationNode.IdentifierToken);
        
        parserModel.TokenWalker.Consume(); // Consume 'NameableToken'
        parserModel.TokenWalker.Consume(); // Consume 'ColonToken'
		return;
    }
}
