using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.TextEditor.RazorLib.Installations.Models;

public struct FastParseArgs
{
    public FastParseArgs(
        ResourceUri resourceUri,
        string extensionNoPeriod,
        CommonUtilityService commonUtilityService,
        object ideBackgroundTaskApi)
    {
        ResourceUri = resourceUri;
        ExtensionNoPeriod = extensionNoPeriod;
        CommonUtilityService = commonUtilityService;
        IdeBackgroundTaskApi = ideBackgroundTaskApi;
    }

    /// <summary>
    /// The unique identifier for the <see cref="TextEditorModel"/> which is
    /// providing the underlying data to be rendered by this view model.
    /// </summary>
    public ResourceUri ResourceUri { get; }
    public string ExtensionNoPeriod { get; }
    public CommonUtilityService CommonUtilityService { get; }
    public object IdeBackgroundTaskApi { get; }
    public bool ShouldBlockUntilBackgroundTaskIsCompleted { get; init; }
}
