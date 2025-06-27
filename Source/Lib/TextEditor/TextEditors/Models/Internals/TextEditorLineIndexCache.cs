using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.Virtualizations.Models;

namespace Walk.TextEditor.RazorLib.TextEditors.Models.Internals;

public class TextEditorLineIndexCache
{
    /// <summary>TODO: Don't do this.</summary>
    public static List<VirtualizationSpan> VirtualizationSpanList_Empty { get; } = new();

	/// <summary>
    /// Every virtualized line has its "spans" stored in this flat list.
    ///
    /// Then, 'virtualizationSpan_StartInclusiveIndex' and 'virtualizationSpan_EndExclusiveIndex'
    /// indicate the section of the flat list that relates to each individual line.
    ///
    /// This points to a TextEditorViewModel('s) VirtualizationGrid('s) list directly.
	/// If you clear it that'll cause a UI race condition exception.
    /// </summary>
    public List<VirtualizationSpan> VirtualizationSpanList { get; set; } = new();
    
    public bool IsInvalid { get; set; }
    public HashSet<int> UsedKeyHashSet { get; set; } = new();
    public List<int> ExistsKeyList { get; set; } = new();
	public List<int> ModifiedLineIndexList { get; set; }
	/// <summary>If the scroll left changes you have to discard the virtualized line cache.</summary>
    public int ScrollLeftMarker { get; set; } = -1;
    public Key<TextEditorViewModel> ViewModelKeyMarker { get; set; } = Key<TextEditorViewModel>.Empty;
    public Dictionary<int, TextEditorLineIndexCacheEntry> Map { get; set; } = new();
}
