using System.Text;
using Walk.Common.RazorLib.Notifications.Models;

namespace Walk.Common.RazorLib.FileSystems.Models;

public class LocalDirectoryHandler : IDirectoryHandler
{
    private const bool IS_DIRECTORY_RESPONSE = true;

    private readonly CommonService _commonService;

    public LocalDirectoryHandler(CommonService commonService)
    {
        _commonService = commonService;
    }

    public async Task CreateDirectoryAsync(
        string absolutePathString,
        CancellationToken cancellationToken = default)
    {
        // This method will add the path to IEnvironmentProvider.DeletionPermittedPaths
        // and therefore, if the directory already exists, then return early.
        //
        // An example of the concern is that someone tries to create the
        // root directory. It already existed, so the 'create' part did nothing.
        // But now they're allowed to delete the root directory.
        // (note: there are double checks in place such that this described
        //        situation couldn't happen regardless)
        if (await ExistsAsync(absolutePathString, cancellationToken).ConfigureAwait(false))
            return;

        Directory.CreateDirectory(absolutePathString);

        _commonService.EnvironmentProvider.DeletionPermittedRegister(
            new SimplePath(absolutePathString, IS_DIRECTORY_RESPONSE),
            tokenBuilder: new StringBuilder(),
            formattedBuilder: new StringBuilder());
    }

    public Task DeleteAsync(
        string absolutePathString,
        bool recursive,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _commonService.EnvironmentProvider.AssertDeletionPermitted(absolutePathString, IS_DIRECTORY_RESPONSE);
            Directory.Delete(absolutePathString, recursive);
        }
        catch (Exception exception)
        {
            NotifyUserOfException(exception);
            throw;
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(
        string absolutePathString,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            Directory.Exists(
                absolutePathString));
    }

    public Task CopyAsync(
        string sourceAbsolutePathString,
        string destinationAbsolutePathString,
        CancellationToken cancellationToken = default)
    {
        /*
            Walk.Ide.RazorLib.Menus.Models.MenuOptionsFactory.cs
            currently uses the method "CopyFilesRecursively" (which exists in the same file).
            To copy and paste a directory.

            TODO: Implement this method.
        */
        throw new NotImplementedException();
    }

    public async Task MoveAsync(
        string sourceAbsolutePathString,
        string destinationAbsolutePathString,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _commonService.EnvironmentProvider.AssertDeletionPermitted(sourceAbsolutePathString, IS_DIRECTORY_RESPONSE);

            if (await ExistsAsync(destinationAbsolutePathString).ConfigureAwait(false))
                _commonService.EnvironmentProvider.AssertDeletionPermitted(destinationAbsolutePathString, IS_DIRECTORY_RESPONSE);

            Directory.Move(
                sourceAbsolutePathString,
                destinationAbsolutePathString);

            _commonService.EnvironmentProvider.DeletionPermittedRegister(
                new SimplePath(destinationAbsolutePathString, true),
                tokenBuilder: new StringBuilder(),
                formattedBuilder: new StringBuilder());
        }
        catch (Exception exception)
        {
            NotifyUserOfException(exception);
            throw;
        }
    }

    public string[] GetDirectories(string absolutePathString)
    {
        return Directory.GetDirectories(absolutePathString);
    }
    
    public Task<string[]> GetDirectoriesAsync(
        string absolutePathString,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            Directory.GetDirectories(
                absolutePathString));
    }

    public string[] GetFiles(string absolutePathString)
    {
        return Directory.GetFiles(absolutePathString);
    }
    
    public Task<string[]> GetFilesAsync(
        string absolutePathString,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            Directory.GetFiles(
                absolutePathString));
    }

    public Task<IEnumerable<string>> EnumerateFileSystemEntriesAsync(
        string absolutePathString,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            Directory.EnumerateFileSystemEntries(
                absolutePathString));
    }

    private void NotifyUserOfException(Exception exception)
    {
        var title = "FILESYSTEM ERROR";

        if (exception.Message.StartsWith(PermittanceChecker.ERROR_PREFIX))
            title = PermittanceChecker.ERROR_PREFIX;

        NotificationHelper.DispatchError(
            title,
            exception.ToString(),
            _commonService,
            TimeSpan.FromSeconds(10));
    }
}
