using Walk.CompilerServices.CSharp.BinderCase;
using Walk.CompilerServices.CSharp.CompilerServiceCase;
using Walk.CompilerServices.CSharp.LexerCase;
using Walk.CompilerServices.CSharp.ParserCase;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.Tests.csproj;

public class UnitTest1
{
    // Ref struct to make `CSharpLexerOutput` permissible as a property.
    public ref struct Test
    {
        public Test(byte[] contentArray, string contentString)
        {
            TextEditorService = new TextEditorService(
                jsRuntime: null,
                commonService: null);
    
            CompilerService = new CSharpCompilerService(TextEditorService);
    
            Binder = new CSharpBinder(
                TextEditorService,
                CompilerService);
            
            ResourceUri = new ResourceUri("/luthetusUnitTest1.cs");
            
            Content = "int;";

            // CompilationUnit = new CSharpCompilationUnit(CompilationUnitKind.IndividualFile_AllData);

            var compilationUnit = new CSharpCompilationUnit(CompilationUnitKind.IndividualFile_AllData);

            CompilerService._currentFileBeingParsedTuple = (ResourceUri.Value, contentString);
            TextEditorService.EditContext_GetText_Clear();
            
            using MemoryStream ms = new MemoryStream(contentArray);
            using StreamReader sr = new StreamReader(ms);
            var streamReaderWrap = new StreamReaderWrap(sr);
            var lexerOutput = CSharpLexer.Lex(Binder, ResourceUri, streamReaderWrap, shouldUseSharedStringWalker: true);

            Binder.StartCompilationUnit(ResourceUri);
            CSharpParser.Parse(ResourceUri, ref compilationUnit, Binder, ref lexerOutput);
        }
        
        public TextEditorService TextEditorService { get; set; }
        public CSharpCompilerService CompilerService { get; set; }
        public CSharpBinder Binder { get; set; }
        public ResourceUri ResourceUri { get; set; }
        public string Content { get; set; }
        public CSharpCompilationUnit CompilationUnit { get; set; }
    }

    [Fact]
    public void Namespace()
    {
        var test = new Test("namespace Walk;"u8.ToArray(), "namespace Walk;");
    }
}
