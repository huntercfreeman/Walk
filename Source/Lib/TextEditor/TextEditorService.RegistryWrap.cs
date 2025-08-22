using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Decorations.Models;

namespace Walk.TextEditor.RazorLib;

public partial class TextEditorService
{
    private Dictionary<string, IDecorationMapper> _decorationMapperMap { get; } = new();

    public IReadOnlyDictionary<string, IDecorationMapper> DecorationMapperMap => _decorationMapperMap;

    public TextEditorDecorationMapperDefault DefaultDecorationMapper { get; } = new TextEditorDecorationMapperDefault();

    public void RegisterDecorationMapper(string fileExtensionNoPeriod, IDecorationMapper decorationMapper)
    {
        _decorationMapperMap.Add(fileExtensionNoPeriod, decorationMapper);
    }

    public IDecorationMapper GetDecorationMapper(string extensionNoPeriod)
    {
        if (_decorationMapperMap.TryGetValue(extensionNoPeriod, out var decorationMapper))
            return decorationMapper;

        return DefaultDecorationMapper;
    }

    private readonly Dictionary<string, ICompilerService> _compilerServiceMap = new();

    public IReadOnlyList<ICompilerService> CompilerServiceList => _compilerServiceMap.Values.ToList();

    public CompilerServiceDoNothing CompilerServiceDoNothing { get; } = new CompilerServiceDoNothing();

    public void RegisterCompilerService(string fileExtensionNoPeriod, ICompilerService compilerService)
    {
        _compilerServiceMap.Add(fileExtensionNoPeriod, compilerService);
    }

    public ICompilerService GetCompilerService(string extensionNoPeriod)
    {
        if (_compilerServiceMap.TryGetValue(extensionNoPeriod, out var compilerService))
            return compilerService;

        return CompilerServiceDoNothing;
    }
}
