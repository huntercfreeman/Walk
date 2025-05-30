using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Keys.Models;

namespace Walk.Common.RazorLib.Dialogs.Models;

public static class DialogFacts
{
    public static readonly Key<IDynamicViewModel> InputFileDialogKey = Key<IDynamicViewModel>.NewKey();
}