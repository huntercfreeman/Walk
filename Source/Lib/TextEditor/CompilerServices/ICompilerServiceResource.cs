using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.TextEditor.RazorLib.CompilerServices;

public interface ICompilerServiceResource
{
	public ResourceUri ResourceUri { get; }
	public ICompilationUnit CompilationUnit { get; set; }
}
