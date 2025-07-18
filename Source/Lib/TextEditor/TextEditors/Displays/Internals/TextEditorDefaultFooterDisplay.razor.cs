using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.Exceptions;
using Walk.TextEditor.RazorLib.Lines.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;

namespace Walk.TextEditor.RazorLib.TextEditors.Displays.Internals;

public partial class TextEditorDefaultFooterDisplay : ComponentBase
{
	[Inject]
	private TextEditorService TextEditorService { get; set; } = null!;

	[Parameter, EditorRequired]
	public Key<TextEditorComponentData> ComponentDataKey { get; set; }

	public int _previousPositionNumber;
	
	private string _selectedLineEndKindString = LineEndKind.LineFeed.AsEnumName();
	
	private Key<TextEditorViewModel> _viewModelKeyPrevious = Key<TextEditorViewModel>.Empty;
	private LineEndKind _lineEndKindPreferencePrevious = LineEndKind.LineFeed;
	
	private Key<TextEditorComponentData> _componentDataKeyPrevious = Key<TextEditorComponentData>.Empty;
    private TextEditorComponentData? _componentData;
	
	public string SelectedLineEndKindString
	{
		get => _selectedLineEndKindString;
		set
		{
			var virtualizationResult = GetVirtualizationResult();
		
			if (!virtualizationResult.IsValid)
	    		return;
		    		
	        var model = virtualizationResult.Model;
	        var viewModel = virtualizationResult.ViewModel;
	
	        if (model is null || viewModel is null)
	            return;
	
			_selectedLineEndKindString = value;
	
	        var rowEndingKindString = value;
	
	        if (Enum.TryParse<LineEndKind>(rowEndingKindString, out var rowEndingKind))
	        {
	            TextEditorService.WorkerArbitrary.PostUnique(editContext =>
                {
                	var modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri);
                	
                	if (modelModifier is null)
                		return ValueTask.CompletedTask;
                	
                	TextEditorService.Model_SetUsingLineEndKind(
                		editContext,
	                    modelModifier,
	                    rowEndingKind);
	                return ValueTask.CompletedTask;
	            });
	        }
		}
	}
	
	protected override void OnInitialized()
    {
        TextEditorService.ViewModel_CursorShouldBlinkChanged += OnCursorShouldBlinkChanged;
        TextEditorService.Options_TextEditorWrapperCssStateChanged += OnTextEditorWrapperCssStateChanged;
        OnCursorShouldBlinkChanged();
    }
    
    private TextEditorVirtualizationResult GetVirtualizationResult()
    {
    	return GetComponentData()?.Virtualization ?? TextEditorVirtualizationResult.Empty;
    }
    
    private TextEditorComponentData? GetComponentData()
    {
    	if (_componentDataKeyPrevious != ComponentDataKey)
    	{
    		if (!TextEditorService.TextEditorState._componentDataMap.TryGetValue(ComponentDataKey, out var componentData) ||
    		    componentData is null)
    		{
    			_componentData = null;
    		}
    		else
    		{
    			_componentData = componentData;
				_componentDataKeyPrevious = ComponentDataKey;
    		}
    	}
    	
		return _componentData;
    }

    private async void OnCursorShouldBlinkChanged()
    {
    	var virtualizationResult = GetVirtualizationResult();
		if (virtualizationResult.IsValid)
		{
			var shouldSetSelectedLineEndKindString = false;
			
			if (_viewModelKeyPrevious != virtualizationResult.ViewModel.PersistentState.ViewModelKey)
			{
				_viewModelKeyPrevious = virtualizationResult.ViewModel.PersistentState.ViewModelKey;
				shouldSetSelectedLineEndKindString = true;
			}
			else if (_lineEndKindPreferencePrevious != virtualizationResult.Model.LineEndKindPreference)
			{
				_lineEndKindPreferencePrevious = virtualizationResult.Model.LineEndKindPreference;
				shouldSetSelectedLineEndKindString = true;
			}
			
			if (shouldSetSelectedLineEndKindString)
				_selectedLineEndKindString = virtualizationResult.Model.LineEndKindPreference.AsEnumName();
    	}
    
    	await InvokeAsync(StateHasChanged);
    }
    
    private async void OnTextEditorWrapperCssStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }
    
    private static int CountDigits(int argumentNumber)
    {
    	var digitCount = 1;
    	var runningNumber = argumentNumber;
    	
    	while ((runningNumber /= 10) > 0)
    	{
    		digitCount++;
    	}
    	
    	return digitCount;
    }
    
    private int _documentLengthDigitCountValuePrevious;
    private string _documentLengthDigitCountCssPrevious = string.Empty;
    private string GetDocumentLengthCssStyle(int documentLength)
    {
    	var digitCount = CountDigits(documentLength);
    	
    	if (_documentLengthDigitCountValuePrevious != digitCount)
    	{
    		_documentLengthDigitCountValuePrevious = digitCount;
    		_documentLengthDigitCountCssPrevious = DigitCountToCssStyle(_documentLengthDigitCountValuePrevious);
    	}
    	
    	return _documentLengthDigitCountCssPrevious;
    }
	
	private int _lineCountDigitCountValuePrevious;
    private string _lineCountDigitCountCssPrevious = string.Empty;
	private string GetLineCountCssStyle(int lineCount)
    {
    	var digitCount = CountDigits(lineCount);
    	
    	if (_lineCountDigitCountValuePrevious != digitCount)
    	{
    		_lineCountDigitCountValuePrevious = digitCount;
    		_lineCountDigitCountCssPrevious = DigitCountToCssStyle(_lineCountDigitCountValuePrevious);
    	}
    	
    	return _lineCountDigitCountCssPrevious;
    }
	
	private int _mostCharactersOnASingleLineTupleLineLengthDigitCountValuePrevious;
    private string _mostCharactersOnASingleLineTupleLineLengthDigitCountCssPrevious = string.Empty;
	private string GetMostCharactersOnASingleLineTupleLineLengthCssStyle(int mostCharactersOnASingleLineTupleLineLength)
    {
    	var digitCount = CountDigits(mostCharactersOnASingleLineTupleLineLength);
    	
    	if (_mostCharactersOnASingleLineTupleLineLengthDigitCountValuePrevious != digitCount)
    	{
    		_mostCharactersOnASingleLineTupleLineLengthDigitCountValuePrevious = digitCount;
    		_mostCharactersOnASingleLineTupleLineLengthDigitCountCssPrevious = DigitCountToCssStyle(_mostCharactersOnASingleLineTupleLineLengthDigitCountValuePrevious);
    	}
    	
    	return _mostCharactersOnASingleLineTupleLineLengthDigitCountCssPrevious;
    }
    
    public string DigitCountToCssStyle(int digitCount)
    {
        // '+1' for padding character width units
        return $"min-width: {digitCount + 1}ch;";
    }

    public int GetPositionNumber(TextEditorModel model, TextEditorViewModel viewModel)
    {
        try
        {
            // This feels a bit hacky, exceptions are happening because the UI isn't accessing
            // the text editor in a thread safe way.
            //
            // When an exception does occur though, the cursor should receive a 'text editor changed'
            // event and re-render anyhow however.
            // 
            // So store the result of this method incase an exception occurs in future invocations,
            // to keep the cursor on screen while the state works itself out.
            return _previousPositionNumber = model.GetPositionIndex(viewModel) + 1;
        }
        catch (WalkTextEditorException)
        {
            return _previousPositionNumber;
        }
    }

	public void Dispose()
    {
    	TextEditorService.ViewModel_CursorShouldBlinkChanged -= OnCursorShouldBlinkChanged;
    	TextEditorService.Options_TextEditorWrapperCssStateChanged -= OnTextEditorWrapperCssStateChanged;
    }
}