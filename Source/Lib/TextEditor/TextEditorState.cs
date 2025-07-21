using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;

namespace Walk.TextEditor.RazorLib;

/// <summary>
/// Do not modify the '_modelMap' or '_viewModelMap' directly.
/// Use the 'ITextEditorService'.
/// Optimizations are very "agressively" being added at the moment.
/// Once the optimizations "feel" good then these dictionaries need to have their accessability decided on.
/// </summary>
public record TextEditorState
{
    // Move TextEditorState.Main.cs here (2025-02-08)
    public readonly Dictionary<ResourceUri, TextEditorModel> _modelMap = new();
    public readonly Dictionary<Key<TextEditorViewModel>, TextEditorViewModel> _viewModelMap = new();
    public readonly Dictionary<Key<TextEditorComponentData>, TextEditorComponentData> _componentDataMap = new();

    public (TextEditorModel? TextEditorModel, TextEditorViewModel? TextEditorViewModel)
        GetModelAndViewModelOrDefault(ResourceUri resourceUri, Key<TextEditorViewModel> viewModelKey)
    {
        _ = _modelMap.TryGetValue(resourceUri, out TextEditorModel? inModel);
        _ = _viewModelMap.TryGetValue(viewModelKey, out TextEditorViewModel? inViewModel);
        return (inModel, inViewModel);
    }
    
    /// <summary>
    /// This overload will lookup the model for the given view model, in the case that one only has access to the viewModelKey.
    /// </summary>
    public (TextEditorModel? Model, TextEditorViewModel? ViewModel) GetModelAndViewModelOrDefault(
        Key<TextEditorViewModel> viewModelKey)
    {
        TextEditorModel? inModel;
        TextEditorViewModel? inViewModel;
        
        if (_viewModelMap.TryGetValue(viewModelKey, out inViewModel))
            _ = _modelMap.TryGetValue(inViewModel.PersistentState.ResourceUri, out inModel);
        else
            inModel = null;
        
        return (inModel, inViewModel);
    }
    
    public TextEditorModel? ModelGetOrDefault(ResourceUri resourceUri)
    {
        var exists = _modelMap.TryGetValue(resourceUri, out TextEditorModel? inModel);
        return inModel;
    }
    
    /// <summary>
    /// Returns a shallow copy
    /// </summary>
    public Dictionary<ResourceUri, TextEditorModel> ModelGetModels()
    {
        try
        {
            return new Dictionary<ResourceUri, TextEditorModel>(_modelMap);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        
        return new();
    }
    
    public int ModelGetModelsCount()
    {
        try
        {
            return _modelMap.Count;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        
        return 0;
    }
    
    [Obsolete("TextEditorModel.PersistentState.ViewModelKeyList")]
    public List<TextEditorViewModel> ModelGetViewModelsOrEmpty(ResourceUri resourceUri)
    {
        try
        {
            return _viewModelMap.Values
                .Where(x => x.PersistentState.ResourceUri == resourceUri)
                .ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        
        return new();
    }
    
    public TextEditorViewModel? ViewModelGetOrDefault(Key<TextEditorViewModel> viewModelKey)
    {
        _viewModelMap.TryGetValue(viewModelKey, out TextEditorViewModel? inViewModel);
        return inViewModel;
    }

    /// <summary>
    /// Returns a shallow copy
    /// </summary>
    public Dictionary<Key<TextEditorViewModel>, TextEditorViewModel> ViewModelGetViewModels()
    {
        try
        {
            return new Dictionary<Key<TextEditorViewModel>, TextEditorViewModel>(_viewModelMap);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        
        return new();
    }
    
    public int ViewModelGetViewModelsCount()
    {
        try
        {
            return _viewModelMap.Count;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        
        return 0;
    }
}
