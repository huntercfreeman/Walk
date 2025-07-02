using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Keymaps.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Common.RazorLib.Contexts.Models;

public static class ContextBoundaryHelper
{
    public static void SetActiveContextState(CommonUtilityService commonUtilityService, Key<ContextRecord> contextKey)
    {
        commonUtilityService.SetFocusedContextKey(contextKey);
    }
    
    /// <summary>NOTE: 'onfocus' event does not bubble, whereas 'onfocusin' does bubble. Usage of both events in this file is intentional.</summary>
    public static void HandleOnFocus(CommonUtilityService commonUtilityService, string contextElementId)
    {
    	commonUtilityService.SetOutline(
	    	contextElementId,
	    	null,
	    	true);
    }
    
    public static void HandleOnBlur(CommonUtilityService commonUtilityService)
    {
    	commonUtilityService.SetOutline(
	    	null,
	    	null,
	    	false);
    }

    /// <summary>NOTE: 'onfocus' event does not bubble, whereas 'onfocusin' does bubble. Usage of both events in this file is intentional.</summary>
    public static void HandleOnFocusIn(CommonUtilityService commonUtilityService, Key<ContextRecord> contextKey)
    {
    	if (commonUtilityService.GetContextState().FocusedContextKey != contextKey)
    		SetActiveContextState(commonUtilityService, contextKey);
    }
    
    public static Task HandleOnKeyDownAsync(ContextRecord contextRecord, KeyboardEventArgs keyboardEventArgs)
    {
        if (keyboardEventArgs.Key == "Shift" ||
            keyboardEventArgs.Key == "Control" ||
            keyboardEventArgs.Key == "Alt" ||
            keyboardEventArgs.Key == "Meta")
        {
            return Task.CompletedTask;
        }

        return HandleKeymapArgumentAsync(contextRecord, new(keyboardEventArgs));
    }

	/// <summary>
	/// (2025-01-24)
	/// ============
	/// Much of Walk.Common was looked at for optimization.
	///
	/// Mostly excessive garbage collector overhead was looked for,
	/// class -> struct, kind of things.
	///
	/// But, after having finished, the next thing that stands out the most
	/// is this 'HandleKeymapArgumentAsync'.
	///
	/// Is it possible to 'short circuit' by caching known no-op keymap arguments?
	/// </summary>
    public static async Task HandleKeymapArgumentAsync(ContextRecord contextRecord, KeymapArgs keymapArgs)
    {
        var success = contextRecord.Keymap.MapFirstOrDefault(keymapArgs, out var command);

        if (success && command is not null)
            await command.CommandFunc(keymapArgs).ConfigureAwait(false);
    }
}
