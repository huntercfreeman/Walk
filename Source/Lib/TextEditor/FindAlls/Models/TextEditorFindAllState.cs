using Walk.Common.RazorLib.Reactives.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.TextEditor.RazorLib.FindAlls.Models;

public record struct TextEditorFindAllState(
	string SearchQuery,
	string StartingDirectoryPath,
	List<TextEditorTextSpan> SearchResultList,
	ProgressBarModel? ProgressBarModel)
{
	public static readonly Key<TreeViewContainer> TreeViewFindAllContainerKey = Key<TreeViewContainer>.NewKey();

    public TextEditorFindAllState() : this(
    	string.Empty,
    	string.Empty,
    	new(),
    	null)
    {
    }
}