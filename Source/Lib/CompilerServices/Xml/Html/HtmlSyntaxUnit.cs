using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.CompilerServices.Xml.Html.SyntaxObjects;

namespace Walk.CompilerServices.Xml.Html;

public class HtmlSyntaxUnit
{
    public HtmlSyntaxUnit(
        TagNode rootTagSyntax//,
        /*List<TextEditorDiagnostic> diagnosticList*/)
    {
        RootTagSyntax = rootTagSyntax;
        //DiagnosticList = diagnosticList;
    }

    public TagNode RootTagSyntax { get; }
    // public List<TextEditorDiagnostic> DiagnosticList { get; }
}
