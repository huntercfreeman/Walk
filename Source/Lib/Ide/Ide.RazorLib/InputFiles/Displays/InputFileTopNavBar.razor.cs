using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Ide.RazorLib.Exceptions;
using Walk.Ide.RazorLib.InputFiles.Models;

namespace Walk.Ide.RazorLib.InputFiles.Displays;

public partial class InputFileTopNavBar : ComponentBase
{
    [Inject]
    private IdeService IdeService { get; set; } = null!;

    [CascadingParameter(Name="SetInputFileContentTreeViewRootFunc")]
    public Func<AbsolutePath, Task> SetInputFileContentTreeViewRootFunc { get; set; } = null!;
    [CascadingParameter]
    public InputFileState InputFileState { get; set; }
    
    private bool _showInputTextEditForAddress;

    public ElementReference? SearchElementReference { get; private set; }

    public string SearchQuery
    {
        get => InputFileState.SearchQuery;
        set => IdeService.InputFile_SetSearchQuery(value);
    }

    private async Task HandleBackButtonOnClick()
    {
        IdeService.InputFile_MoveBackwardsInHistory();
        await ChangeContentRootToOpenedTreeView().ConfigureAwait(false);
    }

    private async Task HandleForwardButtonOnClick()
    {
        IdeService.InputFile_MoveForwardsInHistory();
        await ChangeContentRootToOpenedTreeView().ConfigureAwait(false);
    }

    private async Task HandleUpwardButtonOnClick()
    {
        IdeService.InputFile_OpenParentDirectory(
            IdeService.IdeComponentRenderers,
            IdeService.CommonService,
            parentDirectoryTreeViewModel: null);

        await ChangeContentRootToOpenedTreeView().ConfigureAwait(false);
    }

    private async Task HandleRefreshButtonOnClick()
    {
        IdeService.InputFile_RefreshCurrentSelection(currentSelection: null);
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
            if (!await IdeService.CommonService.FileSystemProvider.Directory.ExistsAsync(address).ConfigureAwait(false))
            {
                if (await IdeService.CommonService.FileSystemProvider.File.ExistsAsync(address).ConfigureAwait(false))
                    throw new WalkIdeException($"Address provided was a file. Provide a directory instead. {address}");

                throw new WalkIdeException($"Address provided does not exist. {address}");
            }

            var absolutePath = IdeService.CommonService.EnvironmentProvider.AbsolutePathFactory(address, true);
            _showInputTextEditForAddress = false;

            await SetInputFileContentTreeViewRootFunc.Invoke(absolutePath).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            NotificationHelper.DispatchError($"ERROR: {nameof(InputFileTopNavBar)}", exception.ToString(), IdeService.CommonService, TimeSpan.FromSeconds(14));
        }
    }

    private async Task HideInputFileEditAddressAsync()
    {
        _showInputTextEditForAddress = false;
        await InvokeAsync(StateHasChanged);
    }
}