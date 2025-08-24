using Microsoft.Extensions.DependencyInjection;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;

namespace Walk.TextEditor.RazorLib.Installations.Models;

/// <remarks>
/// This class is an exception to the naming convention, "don't use the word 'Walk' in class names".
/// 
/// Reason for this exception: when one first starts interacting with this project,
///     this type might be one of the first types they interact with. So, the redundancy of namespace
///     and type containing 'Walk' feels reasonable here.
/// </remarks>
public record WalkTextEditorConfig
{
    /// <summary>
    /// The initial theme for the text editor is NOT the same as <see cref="WalkCommonConfig.InitialThemeKey"/>.
    /// The text editor and application theme are separate.
    /// </summary>
    public int InitialThemeKey { get; init; } = CommonFacts.VisualStudioDarkThemeClone.Key;
    /// <summary>
    /// When a user hits the keymap { Control + , } then a dialog will be rendered where they can
    /// search for text through "all files". Where "all files" relates to the implementation details
    /// of the given find all dialog.
    /// </summary>
    public FindAllDialogConfig FindAllDialogConfig { get; init; } = new();
    /// <summary>
    /// Default value is <see cref="true"/>.<br/>
    /// 
    /// If one wishes to configure Walk.Common themselves, then set this to false, and invoke
    /// <see cref="Common.RazorLib.Installations.Models.ServiceCollectionExtensions.AddWalkCommonServices(IServiceCollection, WalkHostingInformation, Func{WalkCommonConfig, WalkCommonConfig}?)"/>
    /// prior to invoking Walk.TextEditor's
    /// </summary>
    public bool AddWalkCommon { get; init; } = true;
}
