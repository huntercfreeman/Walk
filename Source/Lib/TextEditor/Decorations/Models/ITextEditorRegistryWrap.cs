using Walk.TextEditor.RazorLib.CompilerServices;

namespace Walk.TextEditor.RazorLib.Decorations.Models;

public interface ITextEditorRegistryWrap
{
    public IDecorationMapperRegistry DecorationMapperRegistry { get; set; }
    public ICompilerServiceRegistry CompilerServiceRegistry { get; set; }
}
