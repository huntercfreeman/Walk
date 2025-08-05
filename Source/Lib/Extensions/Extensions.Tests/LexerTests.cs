using System.IO;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax;
using Walk.CompilerServices.CSharp.BinderCase;
using Walk.CompilerServices.CSharp.CompilerServiceCase;
using Walk.CompilerServices.CSharp.LexerCase;

namespace Walk.Tests.csproj;

public class UnitTest1
{
    // Ref struct to make `CSharpLexerOutput` permissible as a property.
    public ref struct Test
    {
        public Test(byte[] contentArray, string contentString)
        {
            TextEditorService = new TextEditorService(
                textEditorConfig: null,
                jsRuntime: null,
                commonService: null);
    
            CompilerService = new CSharpCompilerService(TextEditorService);
    
            Binder = new CSharpBinder(
                TextEditorService,
                CompilerService);
            
            ResourceUri = new ResourceUri("/luthetusUnitTest1.cs");
            
            Content = "int;";
        
            CompilationUnit = new CSharpCompilationUnit(CompilationUnitKind.IndividualFile_AllData);
    
            CompilerService.SetSourceText(ResourceUri.Value, contentString);
            TextEditorService.EditContext_GetText_Clear();
            
            using MemoryStream ms = new MemoryStream(contentArray);
            using StreamReader sr = new StreamReader(ms);
            LexerOutput = CSharpLexer.Lex(Binder, contentString, sr, shouldUseSharedStringWalker: true);
        }
        
        public TextEditorService TextEditorService { get; set; }
        public CSharpCompilerService CompilerService { get; set; }
        public CSharpBinder Binder { get; set; }
        public ResourceUri ResourceUri { get; set; }
        public string Content { get; set; }
        public CSharpCompilationUnit CompilationUnit { get; set; }
        public CSharpLexerOutput LexerOutput { get; set; }
        
        public void Assert_CountEquals0_And_SyntaxKindEquals(SyntaxKind syntaxKind)
        {
            // Assert.Single(...) triggers me
            Assert.Equal(1, LexerOutput.SyntaxTokenList.Count);
            Assert.Equal(syntaxKind, LexerOutput.SyntaxTokenList[0].SyntaxKind);
        }
    }

    [Fact]
    public void Keyword()
    {
        var test = new Test("int"u8.ToArray(), "int");
        test.Assert_CountEquals0_And_SyntaxKindEquals(SyntaxKind.IntTokenKeyword);
    }
    
    [Fact]
    public void Identifier()
    {
        var test = new Test("\"Apple\""u8.ToArray(), "\"Apple\"");
        test.Assert_CountEquals0_And_SyntaxKindEquals(SyntaxKind.StringLiteralToken);
    }
}
