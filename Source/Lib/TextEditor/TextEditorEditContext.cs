using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.Diffs.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.TextEditor.RazorLib;

public struct TextEditorEditContext
{
    public TextEditorEditContext(TextEditorService textEditorService)
    {
        TextEditorService = textEditorService;
    }

    public TextEditorService TextEditorService { get; }

	/// <summary>
	/// 'isReadOnly == true' will not allocate a new TextEditorModel as well,
	/// nothing will be added to the '__ModelList'.
	/// </summary>
    public TextEditorModel? GetModelModifier(
        ResourceUri modelResourceUri,
        bool isReadOnly = false)
    {
    	if (modelResourceUri == ResourceUri.Empty)
    		return null;
    		
    	TextEditorModel? modelModifier = null;
    		
    	for (int i = 0; i < TextEditorService.__ModelList.Count; i++)
    	{
    		if (TextEditorService.__ModelList[i].PersistentState.ResourceUri == modelResourceUri)
    			modelModifier = TextEditorService.__ModelList[i];
    	}
    	
    	if (modelModifier is null)
    	{
    		var exists = TextEditorService.TextEditorState._modelMap.TryGetValue(
				modelResourceUri,
				out var model);
    		
    		if (isReadOnly || model is null)
    			return model;
    		
			modelModifier = model is null ? null : new(model);
        	TextEditorService.__ModelList.Add(modelModifier);
    	}

        return modelModifier;
    }

    public TextEditorViewModel? GetViewModelModifier(
        Key<TextEditorViewModel> viewModelKey,
        bool isReadOnly = false)
    {
    	if (viewModelKey == Key<TextEditorViewModel>.Empty)
    		return null;
    		
    	TextEditorViewModel? viewModelModifier = null;
    		
    	for (int i = 0; i < TextEditorService.__ViewModelList.Count; i++)
    	{
    		if (TextEditorService.__ViewModelList[i].PersistentState.ViewModelKey == viewModelKey)
    			viewModelModifier = TextEditorService.__ViewModelList[i];
    	}
    	
    	if (viewModelModifier is null)
    	{
    		var exists = TextEditorService.TextEditorState._viewModelMap.TryGetValue(
				viewModelKey,
				out var viewModel);
    		
    		if (isReadOnly || viewModel is null)
    			return viewModel;
    		
			viewModelModifier = viewModel is null ? null : new(viewModel);
        	TextEditorService.__ViewModelList.Add(viewModelModifier);
    	}

        return viewModelModifier;
    }
    
    public TextEditorDiffModelModifier? GetDiffModelModifier(
        Key<TextEditorDiffModel> diffModelKey,
        bool isReadOnly = false)
    {
        if (diffModelKey != Key<TextEditorDiffModel>.Empty)
        {
            if (!TextEditorService.__DiffModelCache.TryGetValue(diffModelKey, out var diffModelModifier))
            {
                var diffModel = TextEditorService.Diff_GetOrDefault(diffModelKey);
                diffModelModifier = diffModel is null ? null : new(diffModel);

                TextEditorService.__DiffModelCache.Add(diffModelKey, diffModelModifier);
            }

            if (!isReadOnly && diffModelModifier is not null)
                diffModelModifier.WasModified = true;

            return diffModelModifier;
        }

        return null;
    }
}
