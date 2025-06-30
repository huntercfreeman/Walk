using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Namespaces.Models;
using Walk.Common.RazorLib.Dynamics.Models;

namespace Walk.Ide.RazorLib.Menus.Models;

public interface IMenuOptionsFactory
{
    public MenuOptionRecord NewEmptyFile(AbsolutePath parentDirectory, Func<Task> onAfterCompletion);
    public MenuOptionRecord NewTemplatedFile(NamespacePath parentDirectory, Func<Task> onAfterCompletion);
    public MenuOptionRecord NewDirectory(AbsolutePath parentDirectory, Func<Task> onAfterCompletion);
    public MenuOptionRecord DeleteFile(AbsolutePath absolutePath, Func<Task> onAfterCompletion);
    public MenuOptionRecord CopyFile(AbsolutePath absolutePath, Func<Task> onAfterCompletion);
    public MenuOptionRecord CutFile(AbsolutePath absolutePath, Func<Task> onAfterCompletion);

    public MenuOptionRecord RenameFile(
        AbsolutePath sourceAbsolutePath,
        ICommonUiService commonUiService,
        Func<Task> onAfterCompletion);

    public MenuOptionRecord PasteClipboard(
        AbsolutePath directoryAbsolutePath,
        Func<Task> onAfterCompletion);
}