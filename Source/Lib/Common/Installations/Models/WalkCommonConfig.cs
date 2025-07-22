using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Themes.Models;

namespace Walk.Common.RazorLib.Installations.Models;

/// <remarks>
/// This class is an exception to the naming convention, "don't use the word 'Walk' in class names".
/// 
/// Reason for this exception: when one first starts interacting with this project,
///     this type might be one of the first types they interact with. So, the redundancy of namespace
///     and type containing 'Walk' feels reasonable here.
/// </remarks>
public record struct WalkCommonConfig
{
    public WalkCommonConfig()
    {
    }

    /// <summary>The <see cref="Key{ThemeRecord}"/> to be used when the application starts</summary>
    public int InitialThemeKey { get; init; } = CommonFacts.VisualStudioDarkThemeClone.Key;
    public string IsMaximizedStyleCssString { get; init; } = "width: 100vw; height: 100vh; left: 0; top: 0;";
}
