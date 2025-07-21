namespace Walk.TextEditor.RazorLib.Lexers.Models;

public class ResourceUriFacts
{
    public const string Terminal_ReservedResourceUri_Prefix = "__DEV_IN__/__Terminal__/";
    public const string Git_ReservedResourceUri_Prefix = "__DEV_IN__/__Git__/";
    public const string Diff_ReservedResourceUri_Prefix = "__DEV_IN__/__Diff__/";
    public static readonly ResourceUri SettingsPreviewTextEditorResourceUri = new("__DEV_IN__/__TextEditor__/preview-settings");
    public static readonly ResourceUri TestExplorerDetailsTextEditorResourceUri = new("__DEV_IN__/__TestExplorer__/details");
}
