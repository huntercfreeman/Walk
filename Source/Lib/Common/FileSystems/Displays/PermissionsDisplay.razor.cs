using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Common.RazorLib.FileSystems.Displays;

public partial class PermissionsDisplay : ComponentBase
{
    [Inject]
    private CommonUtilityService CommonUtilityService { get; set; } = null!;

    private string _deleteAllowPathTextInput = string.Empty;
    private bool _deleteAllowPathIsDirectoryInput;

    private string _protectPathTextInput = string.Empty;
    private bool _protectPathIsDirectoryInput;

    private void AddModifyDeleteRightsOnClick(
        string localProtectPathTextInput,
        bool localProtectPathIsDirectoryInput)
    {
        CommonUtilityService.EnvironmentProvider.DeletionPermittedRegister(new SimplePath(
            localProtectPathTextInput,
            localProtectPathIsDirectoryInput));
    }
    
    private void SubmitProtectOnClick(
        string localProtectPathTextInput,
        bool localProtectPathIsDirectoryInput)
    {
        CommonUtilityService.EnvironmentProvider.ProtectedPathsRegister(new SimplePath(
            localProtectPathTextInput,
            localProtectPathIsDirectoryInput));
    }
}