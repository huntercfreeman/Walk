using Microsoft.Extensions.DependencyInjection;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Themes.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;

namespace Walk.TextEditor.RazorLib.Installations.Models;

/// <remarks>
/// This class is an exception to the naming convention, "don't use the word 'Walk' in class names".
/// 
/// Reason for this exception: when one first starts interacting with this project,
/// 	this type might be one of the first types they interact with. So, the redundancy of namespace
/// 	and type containing 'Walk' feels reasonable here.
/// </remarks>
public record WalkTextEditorConfig
{
    /// <summary>
    /// The initial theme for the text editor is NOT the same as <see cref="WalkCommonConfig.InitialThemeKey"/>.
    /// The text editor and application theme are separate.
    /// </summary>
    public Key<ThemeRecord>? InitialThemeKey { get; init; } = ThemeFacts.VisualStudioDarkThemeClone.Key;
    /// <summary>
    /// By default the only themes are clones of the application "Visual Studio"('s) colors.
    /// That is to say, 2 themes total, one light, one dark.
    /// <br/><br/>
    /// Adding more themes here will allow them to be selected from the settings theme dropdown.
    /// </summary>
    public List<ThemeRecord>? CustomThemeRecordList { get; init; } = WalkTextEditorCustomThemeFacts.AllCustomThemesList;
    /// <summary>
    /// When a user wants to customize the text editor, this settings dialog will be rendered.
    /// </summary>
    public SettingsDialogConfig SettingsDialogConfig { get; init; } = new();
    /// <summary>
    /// When a user hits the keymap { Control + , } then a dialog will be rendered where they can
    /// search for text through "all files". Where "all files" relates to the implementation details
    /// of the given find all dialog.
    /// </summary>
    public FindAllDialogConfig FindAllDialogConfig { get; init; } = new();
    /// <summary>
    /// Func is given as an argument a string and IServiceProvider, the string will be
    /// made into a <see cref="Walk.TextEditor.RazorLib.Lexers.Models.ResourceUri"/>.
    ///
    /// Returns the standardized format for the absolute path.
    ///
    /// Example: "C:\a.txt" and "\a.txt" are two distinct resource uri's.
    ///          With this Func, if one desires, they can alter
    ///          "C:\a.txt" to remove the 'C:' from its string,
    ///          or add 'C:' to the "\a.txt" or etc...
    ///          to make these resource uris match one another.
    /// </summary>
    public Func<string, CommonService, Task<string>>? AbsolutePathStandardizeFunc { get; set; }
    /// <summary>
    /// The go-to definition implementation makes use of <see cref="RegisterModelFunc"/>.<br/>
    /// 
    /// In the case that a symbol's definition exists within a resource that does not have
    /// an already existing <see cref="TextEditorModel"/>, then this is invoked to create that
    /// instance, so that go-to definition can then be performed.<br/>
    /// 
    /// The Func takes in the resource uri that needs a model.
    /// </summary>
    public Func<RegisterModelArgs, Task>? RegisterModelFunc { get; set; }
    public Func<FastParseArgs, Task>? FastParseFunc { get; set; }
    /// <summary>
    /// The go-to definition implementation makes use of <see cref="RegisterModelFunc"/>.<br/>
    /// 
    /// In the case that a symbol's definition exists within a resource that does not have
    /// an already existing <see cref="TextEditorViewModel"/>, then this is invoked to create that
    /// instance, so that go-to definition can then be performed.<br/>
    /// 
    /// The Func takes in the resource uri that needs a ViewModel.
    /// </summary>
    public Func<TryRegisterViewModelArgs, Task<Key<TextEditorViewModel>>>? TryRegisterViewModelFunc { get; set; }
    /// <summary>
    /// The go-to definition implementation makes use of <see cref="TryShowViewModelFunc"/>.<br/>
    /// 
    /// In the case that a symbol's definition exists within a resource that does not have
    /// an already existing ViewModel, then this is invoked to create that instance, so that
    /// go-to definition can then be performed.<br/>
    /// 
    /// The Func takes in the resource uri that needs a ViewModel.
    /// </summary>
    public Func<TryShowViewModelArgs, Task<bool>>? TryShowViewModelFunc { get; set; }
    /// <summary>
    /// Default value is <see cref="true"/>.<br/>
    /// 
    /// If one wishes to configure Walk.Common themselves, then set this to false, and invoke
    /// <see cref="Common.RazorLib.Installations.Models.ServiceCollectionExtensions.AddWalkCommonServices(IServiceCollection, WalkHostingInformation, Func{WalkCommonConfig, WalkCommonConfig}?)"/>
    /// prior to invoking Walk.TextEditor's
    /// </summary>
    public bool AddWalkCommon { get; init; } = true;
}