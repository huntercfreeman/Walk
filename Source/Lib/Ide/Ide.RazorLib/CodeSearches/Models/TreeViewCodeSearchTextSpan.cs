using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.Ide.RazorLib.CodeSearches.Models;

public class TreeViewCodeSearchTextSpan : TreeViewWithType<TextEditorTextSpan>
{
	public TreeViewCodeSearchTextSpan(
			TextEditorTextSpan textSpan,
			IEnvironmentProvider environmentProvider,
			IFileSystemProvider fileSystemProvider,
			bool isExpandable,
			bool isExpanded)
		: base(textSpan, isExpandable, isExpanded)
	{
		EnvironmentProvider = environmentProvider;
		FileSystemProvider = fileSystemProvider;
		// AbsolutePath = EnvironmentProvider.AbsolutePathFactory(textSpan.ResourceUri.Value, false);
		AbsolutePath = EnvironmentProvider.AbsolutePathFactory(string.Empty, false);
	}
	
	public IEnvironmentProvider EnvironmentProvider { get; }
	public IFileSystemProvider FileSystemProvider { get; }
	public AbsolutePath AbsolutePath { get; }
	
	public override bool Equals(object? obj)
	{
		if (obj is not TreeViewCodeSearchTextSpan otherTreeView)
			return false;

		return otherTreeView.GetHashCode() == GetHashCode();
	}

	// public override int GetHashCode() => Item.ResourceUri.Value.GetHashCode();
	public override int GetHashCode() => string.Empty.GetHashCode();
	
	public override string GetDisplayText() => AbsolutePath.NameWithExtension;

    /*public override TreeViewRenderer GetTreeViewRenderer()
	{
	
        using Microsoft.AspNetCore.Components;
        using Walk.Ide.RazorLib.CodeSearches.Models;
        
        namespace Walk.Ide.RazorLib.CodeSearches.Displays;
        
        public partial class TreeViewCodeSearchTextSpanDisplay : ComponentBase
        {
        	[Parameter, EditorRequired]
        	public TreeViewCodeSearchTextSpan TreeViewCodeSearchTextSpan { get; set; } = null!;
        }
	
	
	
	    <div title="@TreeViewCodeSearchTextSpan.AbsolutePath.Value">
        	@(TreeViewCodeSearchTextSpan.AbsolutePath.NameWithExtension)
        </div>

	
	
	
	
	
		return new TreeViewRenderer(
			typeof(TreeViewCodeSearchTextSpanDisplay),
			new Dictionary<string, object?>
			{
				{
					nameof(TreeViewCodeSearchTextSpanDisplay.TreeViewCodeSearchTextSpan),
					this
				}
			});
	}*/
	
	public override Task LoadChildListAsync()
	{
		return Task.CompletedTask;
	}
}

