using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.TextEditor.RazorLib;

public partial class TextEditorService
{
	private DirtyResourceUriState _dirtyResourceUriState = new();

	public event Action DirtyResourceUriStateChanged;

	public DirtyResourceUriState GetDirtyResourceUriState() => _dirtyResourceUriState;

	public void AddDirtyResourceUri(ResourceUri resourceUri)
	{
		lock (_stateModificationLock)
		{
			if (!resourceUri.Value.StartsWith(ResourceUriFacts.Terminal_ReservedResourceUri_Prefix) &&
				!resourceUri.Value.StartsWith(ResourceUriFacts.Git_ReservedResourceUri_Prefix))
			{
				if (resourceUri != ResourceUriFacts.SettingsPreviewTextEditorResourceUri &&
					resourceUri != ResourceUriFacts.TestExplorerDetailsTextEditorResourceUri)
				{
					var outDirtyResourceUriList = new List<ResourceUri>(_dirtyResourceUriState.DirtyResourceUriList);
					outDirtyResourceUriList.Add(resourceUri);

					_dirtyResourceUriState = _dirtyResourceUriState with
					{
						DirtyResourceUriList = outDirtyResourceUriList
					};
				}
			}
		}

		DirtyResourceUriStateChanged?.Invoke();
	}

	public void RemoveDirtyResourceUri(ResourceUri resourceUri)
	{
		lock (_stateModificationLock)
		{
			var outDirtyResourceUriList = new List<ResourceUri>(_dirtyResourceUriState.DirtyResourceUriList);
			outDirtyResourceUriList.Remove(resourceUri);

			_dirtyResourceUriState = _dirtyResourceUriState with
			{
				DirtyResourceUriList = outDirtyResourceUriList
			};
		}

		DirtyResourceUriStateChanged?.Invoke();
	}

	public record struct DirtyResourceUriState(List<ResourceUri> DirtyResourceUriList)
	{
		public DirtyResourceUriState() : this(new())
		{
		}
	}
}
