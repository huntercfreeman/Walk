using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.CompilerServices.Css.SyntaxObjects;

namespace Walk.CompilerServices.Css;

public class CssSyntaxUnit
{
    public CssSyntaxUnit(
        CssDocumentSyntax cssDocumentSyntax,
        List<TextEditorDiagnostic> diagnosticList)
    {
        CssDocumentSyntax = cssDocumentSyntax;
        DiagnosticList = diagnosticList;
    }

    public CssDocumentSyntax CssDocumentSyntax { get; }
    public List<TextEditorDiagnostic> DiagnosticList { get; }
}