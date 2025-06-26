using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Clipboards.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.CompilerServices.CSharp.CompilerServiceCase;
using Walk.CompilerServices.CSharpProject.CompilerServiceCase;
using Walk.CompilerServices.Css;
using Walk.CompilerServices.DotNetSolution.CompilerServiceCase;
using Walk.CompilerServices.Json;
using Walk.CompilerServices.Razor.CompilerServiceCase;
using Walk.CompilerServices.Xml;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.TextEditor.RazorLib.CompilerServices;

namespace Walk.Extensions.Config.CompilerServices;

public class ConfigCompilerServiceRegistry : ICompilerServiceRegistry
{
    private readonly Dictionary<string, ICompilerService> _map = new();

    public IReadOnlyDictionary<string, ICompilerService> Map => _map;
    public IReadOnlyList<ICompilerService> CompilerServiceList => _map.Select(x => x.Value).ToList();

    public ConfigCompilerServiceRegistry(
        TextEditorService textEditorService,
        IEnvironmentProvider environmentProvider,
        ITerminalService terminalService,
        IClipboardService clipboardService)
    {
        CSharpCompilerService = new CSharpCompilerService(textEditorService, clipboardService);
        CSharpProjectCompilerService = new CSharpProjectCompilerService(textEditorService);
        // JavaScriptCompilerService = new JavaScriptCompilerService(textEditorService);
        CssCompilerService = new CssCompilerService(textEditorService);
        DotNetSolutionCompilerService = new DotNetSolutionCompilerService(textEditorService);
        JsonCompilerService = new JsonCompilerService(textEditorService);
        RazorCompilerService = new RazorCompilerService(textEditorService, CSharpCompilerService, environmentProvider);
        XmlCompilerService = new XmlCompilerService(textEditorService);
        TerminalCompilerService = new TerminalCompilerService(textEditorService, terminalService);
        DefaultCompilerService = new CompilerServiceDoNothing();

        _map.Add(ExtensionNoPeriodFacts.HTML, XmlCompilerService);
        _map.Add(ExtensionNoPeriodFacts.XML, XmlCompilerService);
        _map.Add(ExtensionNoPeriodFacts.C_SHARP_PROJECT, CSharpProjectCompilerService);
        _map.Add(ExtensionNoPeriodFacts.C_SHARP_CLASS, CSharpCompilerService);
        // _map.Add(ExtensionNoPeriodFacts.JAVA_SCRIPT, JavaScriptCompilerService);
        _map.Add(ExtensionNoPeriodFacts.RAZOR_CODEBEHIND, CSharpCompilerService);
        _map.Add(ExtensionNoPeriodFacts.RAZOR_MARKUP, RazorCompilerService);
        _map.Add(ExtensionNoPeriodFacts.CSHTML_CLASS, RazorCompilerService);
        _map.Add(ExtensionNoPeriodFacts.CSS, CssCompilerService);
        _map.Add(ExtensionNoPeriodFacts.JSON, JsonCompilerService);
        _map.Add(ExtensionNoPeriodFacts.DOT_NET_SOLUTION, DotNetSolutionCompilerService);
        _map.Add(ExtensionNoPeriodFacts.DOT_NET_SOLUTION_X, DotNetSolutionCompilerService);
        _map.Add(ExtensionNoPeriodFacts.TERMINAL, TerminalCompilerService);
    }

    public CSharpCompilerService CSharpCompilerService { get; }
    public CSharpProjectCompilerService CSharpProjectCompilerService { get; }
    // public JavaScriptCompilerService JavaScriptCompilerService { get; }
    public CssCompilerService CssCompilerService { get; }
    public DotNetSolutionCompilerService DotNetSolutionCompilerService { get; }
    public JsonCompilerService JsonCompilerService { get; }
    public RazorCompilerService RazorCompilerService { get; }
    public XmlCompilerService XmlCompilerService { get; }
    public TerminalCompilerService TerminalCompilerService { get; }
    public CompilerServiceDoNothing DefaultCompilerService { get; }

    public ICompilerService GetCompilerService(string extensionNoPeriod)
    {
        if (_map.TryGetValue(extensionNoPeriod, out var compilerService))
            return compilerService;

        return DefaultCompilerService;
    }
}
