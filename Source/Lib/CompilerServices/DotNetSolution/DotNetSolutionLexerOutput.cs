using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.CompilerServices.DotNetSolution.Models.Project;

namespace Walk.CompilerServices.DotNetSolution;

public struct DotNetSolutionLexerOutput
{
    public DotNetSolutionLexerOutput()
    {
        TextSpanList = new();
        DotNetProjectList = new();
        SolutionFolderList = new();
        GuidNestedProjectEntryList = new();
    }
    
    public List<TextEditorTextSpan> TextSpanList { get; }
    public List<IDotNetProject> DotNetProjectList { get; set; }
    public List<SolutionFolder> SolutionFolderList { get; init; }
    public List<GuidNestedProjectEntry> GuidNestedProjectEntryList { get; init; }
}
