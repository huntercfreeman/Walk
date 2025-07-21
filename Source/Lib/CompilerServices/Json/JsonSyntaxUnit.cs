using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.CompilerServices.Json.SyntaxObjects;

namespace Walk.CompilerServices.Json;

public class JsonSyntaxUnit
{
    public JsonSyntaxUnit(
        JsonDocumentSyntax jsonDocumentSyntax,
        List<TextEditorDiagnostic> diagnosticList)
    {
        JsonDocumentSyntax = jsonDocumentSyntax;
        DiagnosticList = diagnosticList;
    }

    public JsonDocumentSyntax JsonDocumentSyntax { get; }
    public List<TextEditorDiagnostic> DiagnosticList { get; }
}
