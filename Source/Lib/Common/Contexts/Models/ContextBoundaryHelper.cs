using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Keymaps.Models;
using Walk.Common.RazorLib.Dynamics.Models;

namespace Walk.Common.RazorLib.Contexts.Models;

public static class ContextBoundaryHelper
{
    public static void SetActiveContextState(IContextService contextService, Key<ContextRecord> contextKey)
    {
        contextService.SetFocusedContextKey(contextKey);
    }
    
    /// <summary>NOTE: 'onfocus' event does not bubble, whereas 'onfocusin' does bubble. Usage of both events in this file is intentional.</summary>
    public static void HandleOnFocus(ICommonUiService commonUiService, string contextElementId)
    {
    	commonUiService.SetOutline(
	    	contextElementId,
	    	null,
	    	true);
    }
    
    public static void HandleOnBlur(ICommonUiService commonUiService)
    {
    	commonUiService.SetOutline(
	    	null,
	    	null,
	    	false);
    }

    /// <summary>NOTE: 'onfocus' event does not bubble, whereas 'onfocusin' does bubble. Usage of both events in this file is intentional.</summary>
    public static void HandleOnFocusIn(IContextService contextService, Key<ContextRecord> contextKey)
    {
    	if (contextService.GetContextState().FocusedContextKey != contextKey)
    		SetActiveContextState(contextService, contextKey);
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
