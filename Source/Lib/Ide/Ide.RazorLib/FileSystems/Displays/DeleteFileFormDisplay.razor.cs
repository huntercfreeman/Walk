using Walk.Common.RazorLib;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Menus.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Walk.Ide.RazorLib.FileSystems.Displays;

public partial class DeleteFileFormDisplay : ComponentBase
{
    [Inject]
    private CommonService CommonService { get; set; } = null!;

    [CascadingParameter]
    public MenuOptionCallbacks? MenuOptionCallbacks { get; set; }

    [Parameter, EditorRequired]
    public AbsolutePath AbsolutePath { get; set; }
    [Parameter, EditorRequired]
    public bool IsDirectory { get; set; }
    [Parameter, EditorRequired]
    public Func<AbsolutePath, Task> OnAfterSubmitFunc { get; set; } = null!;

    private AbsolutePath _previousAbsolutePath = default;

    private int? _countOfImmediateChildren;
    private ElementReference? _cancelButtonElementReference;

    protected override async Task OnParametersSetAsync()
    {
        if (_previousAbsolutePath.ExactInput is null ||
            _previousAbsolutePath.Value !=
            AbsolutePath.Value)
        {
            _countOfImmediateChildren = null;

            _previousAbsolutePath = AbsolutePath;

            if (AbsolutePath.IsDirectory)
            {
                var fileSystemEntryList = await CommonService.FileSystemProvider.Directory
                    .EnumerateFileSystemEntriesAsync(AbsolutePath.Value)
                    .ConfigureAwait(false);

                _countOfImmediateChildren = fileSystemEntryList.Count();
            }
        }

        await base.OnParametersSetAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (MenuOptionCallbacks is not null &&
                _cancelButtonElementReference is not null)
            {
                try
                {
                    await _cancelButtonElementReference.Value
                        .FocusAsync()
                        .ConfigureAwait(false);
                }
                catch (Exception)
                {
                    // 2023-04-18: The app has had a bug where it "freezes" and must be restarted.
                    //             This bug is seemingly happening randomly. I have a suspicion
                    //             that there are race-condition exceptions occurring with "FocusAsync"
                    //             on an ElementReference.
                }
            }
        }
    }

    private async Task HandleOnKeyDown(KeyboardEventArgs keyboardEventArgs)
    {
        if (MenuOptionCallbacks is not null &&
            keyboardEventArgs.Key == CommonFacts.ESCAPE)
        {
            await MenuOptionCallbacks.HideWidgetAsync
                .Invoke()
                .ConfigureAwait(false);
        }
    }

    private async Task DeleteFileOnClick()
    {
        var localAbsolutePath = AbsolutePath;

        if (MenuOptionCallbacks is not null)
        {
            await MenuOptionCallbacks.CompleteWidgetAsync
                .Invoke(async () => await OnAfterSubmitFunc.Invoke(localAbsolutePath))
                .ConfigureAwait(false);
        }
    }

    private async Task CancelOnClick()
    {
        if (MenuOptionCallbacks is not null)
        {
            await MenuOptionCallbacks.HideWidgetAsync
                .Invoke()
                .ConfigureAwait(false);
        }
    }
}