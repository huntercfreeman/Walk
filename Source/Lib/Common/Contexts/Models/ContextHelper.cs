using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Panels.Models;
using Walk.Common.RazorLib.JsRuntimes.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Options.Models;

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
        CommonUtilityService commonUtilityService)
    {
        return new CommonCommand(
            displayName, internalIdentifier, false,
            async commandArgs =>
            {
                var success = await TrySetFocus().ConfigureAwait(false);

                if (!success)
                {
                    commonUtilityService.SetPanelTabAsActiveByContextRecordKey(
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
