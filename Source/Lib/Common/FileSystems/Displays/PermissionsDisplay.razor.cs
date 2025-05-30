using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.FileSystems.Models;

namespace Walk.Common.RazorLib.FileSystems.Displays;

public partial class PermissionsDisplay : ComponentBase
{
    [Inject]
    private IEnvironmentProvider EnvironmentProvider { get; set; } = null!;

    private string _deleteAllowPathTextInput = string.Empty;
    private bool _deleteAllowPathIsDirectoryInput;

    private string _protectPathTextInput = string.Empty;
    private bool _protectPathIsDirectoryInput;

    private void AddModifyDeleteRightsOnClick(
        string localProtectPathTextInput,
        bool localProtectPathIsDirectoryInput)
    {
        EnvironmentProvider.DeletionPermittedRegister(new SimplePath(
            localProtectPathTextInput,
            localProtectPathIsDirectoryInput));
    }
    
    private void SubmitProtectOnClick(
        string localProtectPathTextInput,
        bool localProtectPathIsDirectoryInput)
    {
        EnvironmentProvider.ProtectedPathsRegister(new SimplePath(
            localProtectPathTextInput,
            localProtectPathIsDirectoryInput));
    }
}