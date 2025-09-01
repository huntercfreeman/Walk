using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Lexers.Models;

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
        NamespacePrefixNode? namespacePrefixNode = parserModel.Binder.NamespacePrefixTree.__Root;

        TextEditorTextSpan textSpan = default;
        int count = 0;

        var charIntSum = 0;

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

                charIntSum += matchedToken.TextSpan.CharIntSum;
                
                if (isNamespaceStatement)
                {
                    var tuple = parserModel.Binder.FindPrefix_WithInsertionIndex(
                        namespacePrefixNode,
                        matchedToken.TextSpan,
                        parserModel.ResourceUri.Value);
                    
                    bool nodeWasFound;
                    
                    if (tuple.Node is null)
                    {
                        nodeWasFound = false;
                    }
                    else
                    {
                        nodeWasFound = true;
                        namespacePrefixNode = tuple.Node;
                    }
                    
                    if (!nodeWasFound)
                    {
                        var newNode = new NamespacePrefixNode(parserModel.ResourceUri, matchedToken.TextSpan);
                        namespacePrefixNode.Children.Insert(tuple.InsertionIndex, newNode);
                        namespacePrefixNode = newNode;
                    }
                }

                if (parserModel.TokenWalker.Next.SyntaxKind != SyntaxKind.StatementDelimiterToken)
                {
                    if (parserModel.Compilation.CompilationUnitKind == CompilationUnitKind.IndividualFile_AllData)
                    {
                        parserModel.Binder.SymbolList.Insert(
                            parserModel.Compilation.SymbolOffset + parserModel.Compilation.SymbolLength,
                            new Symbol(
                                SyntaxKind.NamespaceSymbol,
                                parserModel.GetNextSymbolId(),
                                textSpan));
                        ++parserModel.Compilation.SymbolLength;
                    }
                
                    if (isNamespaceStatement)
                    {
                        parserModel.AddNamespaceToCurrentScope(textSpan);
                    }
                }
                
                if (matchedToken.IsFabricated)
                    break;
            }
            else
            {
                if (SyntaxKind.MemberAccessToken == parserModel.TokenWalker.Current.SyntaxKind)
                {
                    charIntSum += (int)'.';
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
            
        parserModel.Binder.SymbolList.Insert(
            parserModel.Compilation.SymbolOffset + parserModel.Compilation.SymbolLength,
            new Symbol(
                SyntaxKind.NamespaceSymbol,
                parserModel.GetNextSymbolId(),
                textSpan));
        ++parserModel.Compilation.SymbolLength;

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
