using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.Ide.RazorLib.Shareds.Models;

namespace Walk.Ide.RazorLib;

/// <summary>
/// Topic "Determine whether an instance is default":
/// -------------------------------------------------
///
/// Various options:
/// - record struct will define equality operator `variable == default`
/// - a method for this exact purpose will only exist as class level metadata and not incur overhead per instance
/// - (speculation) an expression bound property I presume will be class level metadata?
///     - (speculation) if not, perhaps it is based on whether you access instance members?
///     - (speculation) even if you were to access instance members, it is common for `this` to be implicitly
///           available and I wouldn't be overly surprised if the
///           expression bound property resulted in a method which takes `this` behinds the scenes.
///           Perhaps you could access instance members from within the expression
///           And this would decide whether the "behind the scenes" method took an implicit `this`
/// - "manually" override the equality operator
///
///
/// All in all, I'm extremely anxious and I cannot in this moment deal with this situation.
/// I will check if `StartupProjectAbsolutePath.Value` is null for now
/// (since it is used as a key).
///
///
/// What I mean is, I need to first know every bit of overhead that comes with defining this a `record struct`,
/// if there even is any (perhaps it is negligible).
///
/// And I suppose what I just said pertains to every other point.
/// I have the entire application in my mind right now, working out optimizations for every line of code.
/// And this is just all I have right now.
/// </summary>
public struct StartupControlModel
{
    public StartupControlModel(
        string title,
        AbsolutePath startupProjectAbsolutePath)
    {
        Title = title;
        StartupProjectAbsolutePath = startupProjectAbsolutePath;
    }
    
    public string Title { get; }
    public AbsolutePath StartupProjectAbsolutePath { get; }
}
