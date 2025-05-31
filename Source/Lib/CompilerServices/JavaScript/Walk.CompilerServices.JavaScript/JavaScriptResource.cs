using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.CompilerServices;

namespace Walk.CompilerServices.JavaScript;

public class JavaScriptResource : ICompilerServiceResource
{
	public JavaScriptResource(
		ResourceUri resourceUri,
		ICompilerService compilerService)
	{
		ResourceUri = resourceUri;
		CompilerService = compilerService;
	}
	
	public ResourceUri ResourceUri { get; }
	public ICompilerService CompilerService { get; }
	public ICompilationUnit CompilationUnit { get; set; }
}
