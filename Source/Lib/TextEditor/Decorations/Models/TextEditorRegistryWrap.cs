using Walk.TextEditor.RazorLib.CompilerServices;

namespace Walk.TextEditor.RazorLib.Decorations.Models;

public class TextEditorRegistryWrap : ITextEditorRegistryWrap
{
    public IDecorationMapperRegistry DecorationMapperRegistry { get; set; } = new DecorationMapperRegistryDefault();
    public ICompilerServiceRegistry CompilerServiceRegistry { get; set; } = new CompilerServiceRegistryDefault();
}
