using Walk.Common.RazorLib;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.TextEditor.RazorLib.Installations.Models;

/// <summary>
/// Not using 'ref' with this since 'WalkTextEditorConfig' currently has it used by a Func<...>,
/// and making it a delegate with 'ref' didn't seem to make much of a difference.
///
/// For this exact type the issue isn't really a thing since it only copies the three properties
/// but some of the other 'Installation.Models' structs have about 2 or 3 times as many properties.
///
/// The idea for this type is not to bombard the app with 1,643 object allocations immediately
/// upon starting the app. Since there are 1,643 C# files in the Walk.Ide.sln,
/// when dogfooding this would be the exact situation.
///
/// Perhaps the other ones though are not used often enough, and have too much data to copy,
/// to be a struct without 'ref' usage.
/// </summary>
public struct RegisterModelArgs
{
    public RegisterModelArgs(
    	TextEditorEditContext editContext,
        ResourceUri resourceUri,
        CommonService commonService,
        object ideBackgroundTaskApi)
    {
        EditContext = editContext;
        ResourceUri = resourceUri;
        CommonService = commonService;
        IdeBackgroundTaskApi = ideBackgroundTaskApi;
    }

	public TextEditorEditContext EditContext { get; }

    /// <summary>
    /// The unique identifier for the <see cref="TextEditorModel"/> which is
    /// providing the underlying data to be rendered by this view model.
    /// </summary>
    public ResourceUri ResourceUri { get; }
    public CommonService CommonService { get; }
    public object IdeBackgroundTaskApi { get; }
    public bool ShouldBlockUntilBackgroundTaskIsCompleted { get; init; }
}
