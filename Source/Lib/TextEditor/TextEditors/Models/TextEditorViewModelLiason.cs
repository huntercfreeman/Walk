using Walk.Common.RazorLib.Keys.Models;

namespace Walk.TextEditor.RazorLib.TextEditors.Models;

public class TextEditorViewModelLiason
{
	private readonly TextEditorService _textEditorService;
	
	public TextEditorViewModelLiason(TextEditorService textEditorService)
	{
		_textEditorService = textEditorService;
	}

	/// <summary>
	/// 'TextEditorEditContext' is more-so just a way to indicate thread safety
	/// for a given method.
	///
	/// It doesn't actually store anything, all the state is still on the ITextEditorService.
	///
	/// This method 'InsertRepositionInlineUiList(...)' is quite deep inside a chain of calls,
	/// and this method is meant for internal use.
	///
	/// Therefore, I'm going to construct the 'TextEditorEditContext' out of thin air.
	/// But, everything will still work because 'TextEditorEditContext' never actually stored anything.
	///
	/// I need the 'TextEditorEditContext' because if they have a pending edit on the viewmodel
	/// that I'm about to reposition the InlineUiList for, then everything will get borked.
	///
	/// This will get me their pending edit if it exists, otherwise it will start a pending edit.
	/// </summary>
	public void InsertRepositionInlineUiList(
		int initialCursorPositionIndex,
		int insertionLength,
		List<Key<TextEditorViewModel>> viewModelKeyList,
		int initialCursorLineIndex,
		bool lineEndPositionWasAdded)
	{
		var editContext = new TextEditorEditContext(_textEditorService);
		
		foreach (var viewModelKey in viewModelKeyList)
		{
			var viewModel = editContext.GetViewModelModifier(viewModelKey);
			
			var componentData = viewModel.PersistentState.ComponentData;
			if (componentData is not null)
			{
				if (lineEndPositionWasAdded)
					componentData.LineIndexCache.IsInvalid = true;
				else
					componentData.LineIndexCache.ModifiedLineIndexList.Add(initialCursorLineIndex);
			}
		}
	}
	
	/// <summary>
	/// See: 'InsertRepositionInlineUiList(...)' summary
	///      for 'TextEditorEditContext' explanation.
	/// </summary>
	public void DeleteRepositionInlineUiList(
		int startInclusiveIndex,
		int endExclusiveIndex,
		List<Key<TextEditorViewModel>> viewModelKeyList,
		int initialCursorLineIndex,
		bool lineEndPositionWasAdded)
	{
		var editContext = new TextEditorEditContext(_textEditorService);
		
		foreach (var viewModelKey in viewModelKeyList)
		{
			var viewModel = editContext.GetViewModelModifier(viewModelKey);
			
			var componentData = viewModel.PersistentState.ComponentData;
			if (componentData is not null)
			{
				if (lineEndPositionWasAdded)
					componentData.LineIndexCache.IsInvalid = true;
				else
					componentData.LineIndexCache.ModifiedLineIndexList.Add(initialCursorLineIndex);
			}
		}
	}
	
	public void SetContent(List<Key<TextEditorViewModel>> viewModelKeyList)
	{
		var editContext = new TextEditorEditContext(_textEditorService);
		
		foreach (var viewModelKey in viewModelKeyList)
		{
			var viewModel = editContext.GetViewModelModifier(viewModelKey);
			
			var componentData = viewModel.PersistentState.ComponentData;
			if (componentData is not null)
				componentData.LineIndexCache.IsInvalid = true;
		}
	}
}
