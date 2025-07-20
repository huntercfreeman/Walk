using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Extensions.CompilerServices.Syntax;

namespace Walk.CompilerServices.CSharp.CompilerServiceCase;

public sealed class CSharpResource : ICompilerServiceResource
{
    public CSharpResource(ResourceUri resourceUri, CSharpCompilerService cSharpCompilerService)
    {
    	ResourceUri = resourceUri;
        CompilerService = cSharpCompilerService;
    }
	
	public ResourceUri ResourceUri { get; }
    public ICompilerService CompilerService { get; }
	public CSharpCompilationUnit? CompilationUnit { get; set; }
	
	ICompilationUnit? ICompilerServiceResource.CompilationUnit { get => CompilationUnit; set => _ = value; }
    
    public IReadOnlyList<Symbol> GetSymbols()
    {
        var localCompilationUnit = CompilationUnit;

        if (localCompilationUnit is null)
            return Array.Empty<Symbol>();

        return localCompilationUnit.SymbolList;
    }

    public IReadOnlyList<TextEditorDiagnostic> GetDiagnostics()
    {
        var localCompilationUnit = CompilationUnit;

        if (localCompilationUnit?.DiagnosticList is null)
            return Array.Empty<TextEditorDiagnostic>();

        return localCompilationUnit.DiagnosticList;
    }
}