using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Namespaces.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.Ide.RazorLib.FileSystems.Models;
using Walk.Ide.RazorLib.Clipboards.Models;
using Walk.Ide.RazorLib.Terminals.Models;

namespace Walk.Ide.RazorLib;

public static class IdeFacts
{
    private static readonly List<string> _empty = new();

	/* Start TerminalFacts */
	public static readonly Key<ITerminal> EXECUTION_KEY = Key<ITerminal>.NewKey();
    public static readonly Key<ITerminal> GENERAL_KEY = Key<ITerminal>.NewKey();

    public static readonly IReadOnlyList<Key<ITerminal>> WELL_KNOWN_KEYS = new List<Key<ITerminal>>()
    {
        EXECUTION_KEY,
        GENERAL_KEY,
    };
	/* End TerminalFacts */
	
	/* Start TerminalOutputFacts */
	public const int MAX_COMMAND_COUNT = 100;
	public const int MAX_OUTPUT_LENGTH = 100_000;
	
	/// <summary>MAX_OUTPUT_LENGTH / 2 + 1; so that two terminal commands can sum and cause a clear</summary>
	public const int OUTPUT_LENGTH_PADDING = 50_001;
	/* End TerminalOutputFacts */
	
	/* Start TerminalPresentationFacts */
	public const string Terminal_CssClassString = "di_te_terminal-presentation";

    public static readonly Key<TextEditorPresentationModel> Terminal_PresentationKey = Key<TextEditorPresentationModel>.NewKey();

    public static readonly TextEditorPresentationModel Terminal_EmptyPresentationModel = new(
        Terminal_PresentationKey,
        0,
        Terminal_CssClassString,
        new TerminalDecorationMapper());
	/* End TerminalPresentationFacts */
	
	/* Start TerminalWebsiteFacts */
	public const string TargetFileNames_DOT_NET = "dotnet";
	public const string InitialArguments_RUN = "run";
	public const string Options_PROJECT = "--project";
	/* End TerminalWebsiteFacts */
	
	/* Start FileTemplateFacts */
	private static readonly List<IFileTemplate> _emptyFileTemplateList = new();

    public static readonly IFileTemplate CSharpClass = new FileTemplate(
        "C# Class",
        "di_ide_c-sharp-class",
        FileTemplateKind.CSharp,
        filename => filename.EndsWith('.' + ExtensionNoPeriodFacts.C_SHARP_CLASS),
        _ => _emptyFileTemplateList,
        true,
        CSharpClassCreateFileFunc);

    public static readonly IFileTemplate RazorCodebehind = new FileTemplate(
        "Razor codebehind",
        "di_ide_razor-codebehind-class",
        FileTemplateKind.Razor,
        filename => filename.EndsWith('.' + ExtensionNoPeriodFacts.RAZOR_CODEBEHIND),
        _ => _emptyFileTemplateList,
        true,
        RazorCodebehindCreateFileFunc);

    public static readonly IFileTemplate RazorMarkup = new FileTemplate(
        "Razor markup",
        "di_ide_razor-markup-class",
        FileTemplateKind.Razor,
        filename => filename.EndsWith('.' + ExtensionNoPeriodFacts.RAZOR_MARKUP),
        fileName => new List<IFileTemplate>
        {
            RazorCodebehind
        },
        true,
        RazorMarkupCreateFileFunc);

    /// <summary>
    /// Template should be:
    /// -------------------
    /// namespace Walk.Ide.ClassLib.FileTemplates;
    ///
    /// public class Asdf
    /// {
    ///     
    /// }
    /// 
    /// </summary>
    private static FileTemplateResult CSharpClassCreateFileFunc(FileTemplateParameter templateParameter)
    {
        var emptyFileAbsolutePathString = templateParameter.ParentDirectory.AbsolutePath.Value +
            templateParameter.Filename;

        // Create AbsolutePath as to leverage it for knowing the file extension and other details
        var emptyFileAbsolutePath = templateParameter.EnvironmentProvider.AbsolutePathFactory(
            emptyFileAbsolutePathString,
            false);

        var templatedFileContent = GetContent(
            emptyFileAbsolutePath.NameNoExtension,
            templateParameter.ParentDirectory.Namespace);

        var templatedFileAbsolutePathString = templateParameter.ParentDirectory.AbsolutePath.Value +
            emptyFileAbsolutePath.NameNoExtension +
            '.' +
            ExtensionNoPeriodFacts.C_SHARP_CLASS;

        var templatedFileAbsolutePath = templateParameter.EnvironmentProvider.AbsolutePathFactory(
            templatedFileAbsolutePathString,
            false);

        var templatedFileNamespacePath = new NamespacePath(
            templateParameter.ParentDirectory.Namespace,
            templatedFileAbsolutePath);

        return new FileTemplateResult(templatedFileNamespacePath, templatedFileContent);

        string GetContent(string fileNameNoExtension, string namespaceString) =>
            $@"namespace {namespaceString};

public class {fileNameNoExtension}
{{
	
}}
".ReplaceLineEndings();
    }

    /// <summary>
    /// Template should be:
    /// -------------------
    /// <h3>Asdf</h3>
    /// 
    /// @code {
    ///     
    /// }
    /// 
    /// </summary>
    private static FileTemplateResult RazorMarkupCreateFileFunc(FileTemplateParameter templateParameter)
    {
        var emptyFileAbsolutePathString = templateParameter.ParentDirectory.AbsolutePath.Value +
            templateParameter.Filename;

        // Create AbsolutePath as to leverage it for knowing the file extension and other details
        var emptyFileAbsolutePath = templateParameter.EnvironmentProvider.AbsolutePathFactory(
            emptyFileAbsolutePathString,
            false);

        var templatedFileContent = GetContent(emptyFileAbsolutePath.NameNoExtension);

        var templatedFileAbsolutePathString = templateParameter.ParentDirectory.AbsolutePath.Value +
            emptyFileAbsolutePath.NameNoExtension +
            '.' +
            ExtensionNoPeriodFacts.RAZOR_MARKUP;

        var templatedFileAbsolutePath = templateParameter.EnvironmentProvider.AbsolutePathFactory(
            templatedFileAbsolutePathString,
            false);

        var templatedFileNamespacePath = new NamespacePath(
            templateParameter.ParentDirectory.Namespace,
            templatedFileAbsolutePath);

        return new FileTemplateResult(templatedFileNamespacePath, templatedFileContent);

        string GetContent(string fileNameNoExtension) =>
            $@"<h3>{fileNameNoExtension}</h3>

@code {{
	
}}".ReplaceLineEndings();
    }

    /// <summary>
    /// Template should be:
    /// -------------------
    /// using Microsoft.AspNetCore.Components;
    /// 
    /// namespace Walk.Ide.RazorLib.Menu;
    /// 
    /// public partial class Asdf : ComponentBase
    /// {
    ///     
    /// }
    /// </summary>
    private static FileTemplateResult RazorCodebehindCreateFileFunc(FileTemplateParameter templateParameter)
    {
        string GetContent(string fileNameNoExtension, string namespaceString)
        {
            var className = fileNameNoExtension.Replace('.' + ExtensionNoPeriodFacts.RAZOR_MARKUP, string.Empty);

            var interpolatedResult = $@"using Microsoft.AspNetCore.Components;

namespace {namespaceString};

public partial class {className} : ComponentBase
{{
	
}}".ReplaceLineEndings();

            return interpolatedResult;
        }

        var emptyFileAbsolutePathString = templateParameter.ParentDirectory.AbsolutePath.Value +
            templateParameter.Filename;

        // Create AbsolutePath as to leverage it for knowing the file extension and other details
        var emptyFileAbsolutePath = templateParameter.EnvironmentProvider.AbsolutePathFactory(
            emptyFileAbsolutePathString,
            false);

        var templatedFileContent = GetContent(
            emptyFileAbsolutePath.NameNoExtension,
            templateParameter.ParentDirectory.Namespace);

        var templatedFileAbsolutePathString = templateParameter.ParentDirectory.AbsolutePath.Value +
            emptyFileAbsolutePath.NameNoExtension;

        if (templatedFileAbsolutePathString.EndsWith('.' + ExtensionNoPeriodFacts.RAZOR_MARKUP))
            templatedFileAbsolutePathString += '.' + ExtensionNoPeriodFacts.C_SHARP_CLASS;
        else
            templatedFileAbsolutePathString += '.' + ExtensionNoPeriodFacts.RAZOR_CODEBEHIND;

        var templatedFileAbsolutePath = templateParameter.EnvironmentProvider.AbsolutePathFactory(
            templatedFileAbsolutePathString,
            false);

        var templatedFileNamespacePath = new NamespacePath(
            templateParameter.ParentDirectory.Namespace,
            templatedFileAbsolutePath);

        return new FileTemplateResult(templatedFileNamespacePath, templatedFileContent);
    }
	/* End FileTemplateFacts */
	
	/* Start HiddenFileFacts */
	public const string BIN = "bin";
    public const string OBJ = "obj";

    /// <summary>
    /// If rendering a .csproj file pass in <see cref="ExtensionNoPeriodFacts.C_SHARP_PROJECT"/>
    ///
    /// Then perhaps the returning array would contain { "bin", "obj" } as they should be hidden
    /// with this context.
    /// </summary>
    /// <returns></returns>
    public static List<string> GetHiddenFilesByContainerFileExtension(string extensionNoPeriod)
    {
        return extensionNoPeriod switch
        {
            ExtensionNoPeriodFacts.C_SHARP_PROJECT => new() { BIN, OBJ },
            _ => _empty
		};
    }
	/* End HiddenFileFacts */
	
	/* Start UniqueFileFacts */
	public const string Properties = "Properties";
    public const string WwwRoot = "wwwroot";

	/// <summary>
	/// If rendering a .csproj file pass in <see cref="ExtensionNoPeriodFacts.C_SHARP_PROJECT"/>
	///
	/// Then perhaps the returning array would contain { "Properties", "wwwroot" } as they are unique files
	/// with this context.
	/// </summary>
	/// <returns></returns>
	public static List<string> GetUniqueFilesByContainerFileExtension(string extensionNoPeriod)
    {
        return extensionNoPeriod switch
        {
            ExtensionNoPeriodFacts.C_SHARP_PROJECT => new() { Properties, WwwRoot },
            _ => _empty
		};
    }
	/* End UniqueFileFacts */
	
	/* Start ClipboardFacts */
	/// <summary>
    /// Indicates the start of a phrase.<br/><br/>
    /// Phrase is being defined as a tag, command, datatype and value in string form.<br/><br/>
    /// </summary>
    public const string Tag = "`'\";di_clipboard";
    /// <summary>Deliminates tag_command_datatype_value</summary>
    public const string FieldDelimiter = "_";

    // Commands
    public const string CopyCommand = "copy";
    public const string CutCommand = "cut";
    // DataTypes
    public const string AbsolutePathDataType = "absolute-file-path";

    public static string FormatPhrase(string command, string dataType, string value)
    {
        return Tag + FieldDelimiter + command + FieldDelimiter + dataType + FieldDelimiter + value;
    }

    public static bool TryParseString(string clipboardContents, out ClipboardPhrase? clipboardPhrase)
    {
        clipboardPhrase = null;

        if (clipboardContents.StartsWith(Tag))
        {
            // Skip Tag
            clipboardContents = clipboardContents[Tag.Length..];
            // Skip Delimiter following the Tag
            clipboardContents = clipboardContents[FieldDelimiter.Length..];

            var nextDelimiter = clipboardContents.IndexOf(FieldDelimiter, StringComparison.Ordinal);

            // Take Command
            var command = clipboardContents[..nextDelimiter];

            clipboardContents = clipboardContents[(nextDelimiter + 1)..];

            nextDelimiter = clipboardContents.IndexOf(FieldDelimiter, StringComparison.Ordinal);

            // Take DataType
            var dataType = clipboardContents[..nextDelimiter];

            // Value is whatever remains in the string
            var value = clipboardContents[(nextDelimiter + 1)..];

            clipboardPhrase = new ClipboardPhrase(command, dataType, value);

            return true;
        }

        return false;
    }
	/* End ClipboardFacts */
}
