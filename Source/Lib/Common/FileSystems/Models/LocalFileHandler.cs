using System.Text;
using Walk.Common.RazorLib.Notifications.Models;

namespace Walk.Common.RazorLib.FileSystems.Models;

public class LocalFileHandler : IFileHandler
{
    private const bool IS_DIRECTORY_RESPONSE = true;

    private readonly CommonService _commonService;

    public LocalFileHandler(CommonService commonService)
    {
        _commonService = commonService;
    }

    public bool Exists(string absolutePathString)
    {
        return File.Exists(absolutePathString);
    }
    
    public Task<bool> ExistsAsync(
        string absolutePathString,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            File.Exists(absolutePathString));
    }

    public Task DeleteAsync(
        string absolutePathString,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _commonService.EnvironmentProvider.AssertDeletionPermitted(absolutePathString, IS_DIRECTORY_RESPONSE);
            File.Delete(absolutePathString);
        }
        catch (Exception exception)
        {
            NotifyUserOfException(exception);
            throw;
        }

        return Task.CompletedTask;
    }

    public Task CopyAsync(
        string sourceAbsolutePathString,
        string destinationAbsolutePathString,
        CancellationToken cancellationToken = default)
    {
        File.Copy(
            sourceAbsolutePathString,
            destinationAbsolutePathString);

        _commonService.EnvironmentProvider.DeletionPermittedRegister(
            new SimplePath(destinationAbsolutePathString, IS_DIRECTORY_RESPONSE),
            tokenBuilder: new StringBuilder(),
            formattedBuilder: new StringBuilder());

        return Task.CompletedTask;
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

            File.Move(
                sourceAbsolutePathString,
                destinationAbsolutePathString);

            _commonService.EnvironmentProvider.DeletionPermittedRegister(
                new SimplePath(destinationAbsolutePathString, IS_DIRECTORY_RESPONSE),
                tokenBuilder: new StringBuilder(),
                formattedBuilder: new StringBuilder());
        }
        catch (Exception exception)
        {
            NotifyUserOfException(exception);
            throw;
        }
    }

    public Task<DateTime> GetLastWriteTimeAsync(
        string absolutePathString,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            File.GetLastWriteTime(
                absolutePathString));
    }

    public async Task<string> ReadAllTextAsync(
        string absolutePathString,
        CancellationToken cancellationToken = default)
    {
        return await File.ReadAllTextAsync(
                absolutePathString,
                cancellationToken)
            .ConfigureAwait(false);
    }
    
    public string ReadAllText(string absolutePathString)
    {
        return File.ReadAllText(absolutePathString);
    }

    public async Task WriteAllTextAsync(
        string absolutePathString,
        string contents,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (await ExistsAsync(absolutePathString, cancellationToken).ConfigureAwait(false))
                _commonService.EnvironmentProvider.AssertDeletionPermitted(absolutePathString, IS_DIRECTORY_RESPONSE);

            await File.WriteAllTextAsync(
                    absolutePathString,
                    contents,
                    cancellationToken)
                .ConfigureAwait(false);

            _commonService.EnvironmentProvider.DeletionPermittedRegister(
                new SimplePath(absolutePathString, IS_DIRECTORY_RESPONSE),
                tokenBuilder: new StringBuilder(),
                formattedBuilder: new StringBuilder());
        }
        catch (Exception exception)
        {
            NotifyUserOfException(exception);
            throw;
        }
    }
    
    public void WriteAllText(
        string absolutePathString,
        string contents)
    {
        try
        {
            if (Exists(absolutePathString))
                _commonService.EnvironmentProvider.AssertDeletionPermitted(absolutePathString, IS_DIRECTORY_RESPONSE);

            File.WriteAllText(absolutePathString, contents);

            _commonService.EnvironmentProvider.DeletionPermittedRegister(
                new SimplePath(absolutePathString, IS_DIRECTORY_RESPONSE),
                tokenBuilder: new StringBuilder(),
                formattedBuilder: new StringBuilder());
        }
        catch (Exception exception)
        {
            NotifyUserOfException(exception);
            throw;
        }
    }

    private void NotifyUserOfException(Exception exception)
    {
        var title = "FILESYSTEM ERROR";

        if (exception.Message.StartsWith(CommonFacts.ERROR_PREFIX))
            title = CommonFacts.ERROR_PREFIX;

        NotificationHelper.DispatchError(
            title,
            exception.ToString(),
            _commonService,
            TimeSpan.FromSeconds(10));
    }
}
