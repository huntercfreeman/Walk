using System.Text;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;
using Walk.CompilerServices.CSharp.LexerCase;
using Walk.CompilerServices.CSharp.ParserCase;
using Walk.CompilerServices.CSharp.BinderCase;
using Walk.CompilerServices.CSharp.CompilerServiceCase;

namespace Walk.Tests.csproj;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
    	var binder = new CSharpBinder(new TextEditorService(
	        findAllService: null,
	        dirtyResourceUriService: null,
	        themeService: null,
	        backgroundTaskService: null,
	        textEditorConfig: null,
	        textEditorRegistryWrap: null,
	        storageService: null,
	        jsRuntime: null,
	        commonBackgroundTaskApi: null,
	        panelService: null,
	        dialogService: null,
	        contextService: null,
	        keymapService: null,
	        environmentProvider: null,
			autocompleteIndexer: null,
			autocompleteService: null,
			appDimensionService: null,
			serviceProvider: null));
    	var resourceUri = new ResourceUri("/luthetusUnitTest1.cs");
    	
    	// var content = @"$"""";";
    	// "";
    	
    	// var content = @"$""aaa"";";
    	// $"aaa";
    	
    	// var content = @"$""a{bbb}a"";";
    	// $"a{bbb}a;"
    	
    	var content =
    	""""
    	$$"""{The point {s{{X * X}}s, s{{Y}}s} is s{{sMath.Sqrt(X * X + Y * Y):F3s}}s from the origin}"""
    	"""";
	
		var cSharpCompilationUnit = new CSharpCompilationUnit(resourceUri, content);
		var lexerOutput = CSharpLexer.Lex(binder, resourceUri, content, shouldUseSharedStringWalker: true);
		
		Console.WriteLine("lexerOutput.SyntaxTokenList:");
		foreach (var syntaxToken in lexerOutput.SyntaxTokenList)
		{
			Console.WriteLine($"\t{syntaxToken.SyntaxKind}");
			Console.WriteLine($"\t\t{syntaxToken.TextSpan.StartInclusiveIndex}");
			Console.WriteLine($"\t\t{syntaxToken.TextSpan.EndExclusiveIndex}");
			Console.WriteLine($"\t\t{(GenericDecorationKind)syntaxToken.TextSpan.DecorationByte}");
		}
		
		Console.WriteLine("lexerOutput.MiscTextSpanList:");
		foreach (var textSpan in lexerOutput.MiscTextSpanList)
		{
			Console.WriteLine($"\t{textSpan.StartInclusiveIndex}");
			Console.WriteLine($"\t{textSpan.EndExclusiveIndex}");
			Console.WriteLine($"\t{(GenericDecorationKind)textSpan.DecorationByte}");
		}
		
		cSharpCompilationUnit.TokenList = lexerOutput.SyntaxTokenList;
		cSharpCompilationUnit.MiscTextSpanList = lexerOutput.MiscTextSpanList;
		binder.StartCompilationUnit(resourceUri);
		CSharpParser.Parse(cSharpCompilationUnit, binder, ref lexerOutput);
		
		WriteChildrenIndentedRecursive(cSharpCompilationUnit.RootCodeBlockNode);
		
		Console.WriteLine(((ICodeBlockOwner)cSharpCompilationUnit.RootCodeBlockNode).CodeBlock.ChildList.Count);
    }
    
    public void WriteChildrenIndented(ISyntaxNode node, string name = "node")
    {
    	Console.WriteLine($"foreach (var child in {name}.GetChildList())");
		foreach (var child in GetChildList(node))
		{
			Console.WriteLine("\t" + child.SyntaxKind);
		}
		Console.WriteLine();
    }
    
    public void WriteChildrenIndentedRecursive(ISyntaxNode node, string name = "node", int indentation = 0)
    {
    	var indentationStringBuilder = new StringBuilder();
    	for (int i = 0; i < indentation; i++)
    		indentationStringBuilder.Append('\t');
    	
    	WriteNodeName(node, indentationStringBuilder);
    	
    	// For the child tokens
    	indentationStringBuilder.Append('\t');
    	var childIndentation = indentationStringBuilder.ToString();
    	
		foreach (var child in GetChildList(node))
		{
			if (child is ISyntaxNode syntaxNode)
			{
				WriteChildrenIndentedRecursive(syntaxNode, "node", indentation + 1);
			}
			else if (child is SyntaxToken syntaxToken)
			{
				Console.WriteLine($"{childIndentation}{child.SyntaxKind}__{syntaxToken.TextSpan.Text}");
			}
		}
		
		if (indentation == 0)
			Console.WriteLine();
    }
    
    public IReadOnlyList<ISyntax> GetChildList(ISyntaxNode node)
    {
    	if (node is ICodeBlockOwner codeBlockOwner)
    	{
    		return codeBlockOwner.CodeBlock.ChildList;
    	}
    	return Array.Empty<ISyntaxNode>();
    }
    
    public void WriteNodeName(ISyntaxNode node, StringBuilder indentationStringBuilder)
    {
    	Console.WriteLine($"{indentationStringBuilder.ToString()}{node.SyntaxKind}");
    }
}
