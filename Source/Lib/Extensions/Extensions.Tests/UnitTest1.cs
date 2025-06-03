using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Lexers.Models;
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
    	var content = "";
		var cSharpCompilationUnit = new CSharpCompilationUnit(resourceUri, content);
		var lexerOutput = CSharpLexer.Lex(binder, resourceUri, content, shouldUseSharedStringWalker: true);
		cSharpCompilationUnit.TokenList = lexerOutput.SyntaxTokenList;
		cSharpCompilationUnit.MiscTextSpanList = lexerOutput.MiscTextSpanList;
		binder.StartCompilationUnit(resourceUri);
		CSharpParser.Parse(cSharpCompilationUnit, binder, ref lexerOutput);
    }
}
