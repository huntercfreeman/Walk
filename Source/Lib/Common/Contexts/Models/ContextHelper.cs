using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.JsRuntimes.Models;

namespace Walk.Common.RazorLib.Contexts.Models;

public static class ContextHelper
{
	/// <summary>
	/// TODO: BAD: Code duplication from 'Walk.Ide.RazorLib.Commands.CommandFactory'
	/// </summary>
	public static CommandNoType ConstructFocusContextElementCommand(
        ContextRecord contextRecord,
        string displayName,
        string internalIdentifier,
        WalkCommonJavaScriptInteropApi jsRuntimeCommonApi,
        IPanelService panelService)
    {
        return new CommonCommand(
            displayName, internalIdentifier, false,
            async commandArgs =>
            {
                var success = await TrySetFocus().ConfigureAwait(false);

                if (!success)
                {
                    panelService.SetPanelTabAsActiveByContextRecordKey(
                        contextRecord.ContextKey);

                    _ = await TrySetFocus().ConfigureAwait(false);
                }
            });

        async Task<bool> TrySetFocus()
        {
            return await jsRuntimeCommonApi
                .TryFocusHtmlElementById(contextRecord.ContextElementId)
                .ConfigureAwait(false);
        }
    }
}
