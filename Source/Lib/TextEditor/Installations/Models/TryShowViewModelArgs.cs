using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.Groups.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;

namespace Walk.TextEditor.RazorLib.Installations.Models;
    
public struct TryShowViewModelArgs
{
    public TryShowViewModelArgs(
        Key<TextEditorViewModel> viewModelKey,
        Key<TextEditorGroup> groupKey,
        bool shouldSetFocusToEditor,
        CommonService commonService,
        object ideBackgroundTaskApi)
    {
        ViewModelKey = viewModelKey;
        GroupKey = groupKey;
        ShouldSetFocusToEditor = shouldSetFocusToEditor;
        CommonService = commonService;
        IdeBackgroundTaskApi = ideBackgroundTaskApi;
    }

    /// <summary>
    /// The identifier for which view model is to be used.
    /// </summary>
    public Key<TextEditorViewModel> ViewModelKey { get; }
    /// <summary>
    /// If this view model should be rendered as a tab within a group then provide this key.
    /// Otherwise pass in <see cref="Key{T}.Empty"/>
    /// </summary>
    public Key<TextEditorGroup> GroupKey { get; }
    public bool ShouldSetFocusToEditor { get; }
    public CommonService CommonService { get; }
    public object IdeBackgroundTaskApi { get; }
}
