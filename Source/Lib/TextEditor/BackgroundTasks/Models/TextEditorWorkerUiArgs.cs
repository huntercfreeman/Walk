using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;
using Walk.TextEditor.RazorLib.JavaScriptObjects.Models;

namespace Walk.TextEditor.RazorLib.BackgroundTasks.Models;

public struct TextEditorWorkerUiArgs
{
	// Only OnKeyDown invokes this constructor.
	public TextEditorWorkerUiArgs(
		TextEditorComponentData componentData,
		Key<TextEditorViewModel> viewModelKey,
		KeyboardEventArgsClass keyboardEventArgsClass)
	{
		ComponentData = componentData;
		ViewModelKey = viewModelKey;
		EventArgs = keyboardEventArgsClass;
	
		TextEditorWorkUiKind = TextEditorWorkUiKind.OnKeyDown;
	}
	
	// Can't distinguish the event kind without accepting it as argument.
	public TextEditorWorkerUiArgs(
		TextEditorComponentData componentData,
		Key<TextEditorViewModel> viewModelKey,
		MouseEventArgsClass mouseEventArgsClass,
		TextEditorWorkUiKind workUiKind)
	{
		ComponentData = componentData;
		ViewModelKey = viewModelKey;
		EventArgs = mouseEventArgsClass;
	
		TextEditorWorkUiKind = workUiKind;
	}
	
	// Can't distinguish the event kind without accepting it as argument.
	public TextEditorWorkerUiArgs(
		TextEditorComponentData componentData,
		Key<TextEditorViewModel> viewModelKey,
		WheelEventArgs wheelEventArgs)
	{
		ComponentData = componentData;
		ViewModelKey = viewModelKey;
		EventArgs = wheelEventArgs;
		
		TextEditorWorkUiKind = TextEditorWorkUiKind.OnWheel;
	}

	public TextEditorComponentData ComponentData { get; set; }
    public Key<TextEditorViewModel> ViewModelKey { get; set; }
    /// <summary>
    /// Hack: I want all the events in a shared queue. All events other than scrolling events...
	/// ...can be stored in the same property as an 'object' type.
	///
	/// Scrolling is a pain since it would mean copying around a double at all times
	/// that is only used for the scrolling events.
	///
	/// Thus:
	///     - MouseEventArgs.ClientX will be used to store the scrollLeft.
	///     - MouseEventArgs.ClientY will be used to store the scrollTop.
    /// </summary>
	public object EventArgs { get; set; }
    public TextEditorWorkUiKind TextEditorWorkUiKind { get; set; }
}
