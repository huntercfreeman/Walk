using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Ide.RazorLib.ComponentRenderers.Models;
using Walk.Ide.RazorLib.Exceptions;
using Walk.Ide.RazorLib.InputFiles.Models;
using Walk.Ide.RazorLib.BackgroundTasks.Models;

namespace Walk.Ide.RazorLib.InputFiles.Displays;

public partial class InputFileTopNavBar : ComponentBase
{
    [Inject]
    private IdeBackgroundTaskApi IdeBackgroundTaskApi { get; set; } = null!;

    [CascadingParameter(Name="SetInputFileContentTreeViewRootFunc")]
    public Func<AbsolutePath, Task> SetInputFileContentTreeViewRootFunc { get; set; } = null!;
    [CascadingParameter]
    public InputFileState InputFileState { get; set; }
    
    private bool _showInputTextEditForAddress;

    public ElementReference? SearchElementReference { get; private set; }

    public string SearchQuery
    {
        get => InputFileState.SearchQuery;
        set => InputFileService.SetSearchQuery(value);
    }

    private async Task HandleBackButtonOnClick()
    {
        InputFileService.MoveBackwardsInHistory();
        await ChangeContentRootToOpenedTreeView().ConfigureAwait(false);
    }

    private async Task HandleForwardButtonOnClick()
    {
        InputFileService.MoveForwardsInHistory();
        await ChangeContentRootToOpenedTreeView().ConfigureAwait(false);
    }

    private async Task HandleUpwardButtonOnClick()
    {
        InputFileService.OpenParentDirectory(
            IdeBackgroundTaskApi.IdeComponentRenderers,
            CommonUtilityService,
            parentDirectoryTreeViewModel: null);

        await ChangeContentRootToOpenedTreeView().ConfigureAwait(false);
    }

    private async Task HandleRefreshButtonOnClick()
    {
        InputFileService.RefreshCurrentSelection(currentSelection: null);
        await ChangeContentRootToOpenedTreeView().ConfigureAwait(false);
    }

    private bool GetHandleBackButtonIsDisabled() => !InputFileState.CanMoveBackwardsInHistory;
    private bool GetHandleForwardButtonIsDisabled() => !InputFileState.CanMoveForwardsInHistory;

    private async Task FocusSearchElementReferenceOnClickAsync()
    {
        var localSearchElementReference = SearchElementReference;

        try
        {
            if (localSearchElementReference is not null)
            {
                await localSearchElementReference.Value
                    .FocusAsync()
                    .ConfigureAwait(false);
            }
        }
        catch (Exception)
        {
            // 2023-04-18: The app has had a bug where it "freezes" and must be restarted.
            //             This bug is seemingly happening randomly. I have a suspicion
            //             that there are race-condition exceptions occurring with "FocusAsync"
            //             on an ElementReference.
        }
    }

    private async Task ChangeContentRootToOpenedTreeView()
    {
        var openedTreeView = InputFileState.GetOpenedTreeView();

        if (openedTreeView?.Item is not null)
        {
            await SetInputFileContentTreeViewRootFunc
                .Invoke(openedTreeView.Item)
                .ConfigureAwait(false);
        }
    }

    private async Task InputFileEditAddressOnFocusOutCallbackAsync(string address)
    {
        try
        {
            if (!await CommonUtilityService.FileSystemProvider.Directory.ExistsAsync(address).ConfigureAwait(false))
            {
                if (await CommonUtilityService.FileSystemProvider.File.ExistsAsync(address).ConfigureAwait(false))
                    throw new WalkIdeException($"Address provided was a file. Provide a directory instead. {address}");

                throw new WalkIdeException($"Address provided does not exist. {address}");
            }

            var absolutePath = CommonUtilityService.EnvironmentProvider.AbsolutePathFactory(address, true);
            _showInputTextEditForAddress = false;

            await SetInputFileContentTreeViewRootFunc.Invoke(absolutePath).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            NotificationHelper.DispatchError($"ERROR: {nameof(InputFileTopNavBar)}", exception.ToString(), CommonUtilityService, TimeSpan.FromSeconds(14));
        }
    }

    private async Task HideInputFileEditAddressAsync()
    {
        _showInputTextEditForAddress = false;
        await InvokeAsync(StateHasChanged);
    }
}