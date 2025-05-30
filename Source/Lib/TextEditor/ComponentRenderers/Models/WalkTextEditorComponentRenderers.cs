namespace Walk.TextEditor.RazorLib.ComponentRenderers.Models;

public class WalkTextEditorComponentRenderers : IWalkTextEditorComponentRenderers
{
    public WalkTextEditorComponentRenderers(Type diagnosticRendererType)
    {
        DiagnosticRendererType = diagnosticRendererType;
    }

    public Type DiagnosticRendererType { get; }
}