using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Ide.RazorLib.InputFiles.Models;

namespace Walk.Ide.RazorLib.InputFiles.Displays;

public partial class InputFileContent : ComponentBase
{
    [Inject]
    private CommonService CommonService { get; set; } = null!;

    [CascadingParameter(Name = "SetInputFileContentTreeViewRootFunc")]
    public Func<AbsolutePath, Task> SetInputFileContentTreeViewRootFunc { get; set; } = null!;
    [CascadingParameter]
    public InputFileTreeViewMouseEventHandler InputFileTreeViewMouseEventHandler { get; set; } = null!;
    [CascadingParameter]
    public InputFileTreeViewKeyboardEventHandler InputFileTreeViewKeyboardEventHandler { get; set; } = null!;
    [CascadingParameter]
    public InputFileState InputFileState { get; set; }

    [Parameter, EditorRequired]
    public ElementDimensions ElementDimensions { get; set; } = null!;
    [Parameter, EditorRequired]
    public Action<AbsolutePath?> SetSelectedAbsolutePath { get; set; } = null!;

    public static readonly Key<TreeViewContainer> TreeViewContainerKey = Key<TreeViewContainer>.NewKey();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!CommonService.TryGetTreeViewContainer(TreeViewContainerKey, out _))
        {
            await SetInputFileContentTreeViewRootFunc
                .Invoke(CommonService.EnvironmentProvider.HomeDirectoryAbsolutePath)
                .ConfigureAwait(false);
        }
    }
}
