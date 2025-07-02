using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Keyboards.Models;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.Exceptions;
using Walk.TextEditor.RazorLib.Edits.Models;

namespace Walk.TextEditor.RazorLib.TextEditors.Displays.Internals;

public partial class FindOverlayDisplay : ComponentBase, IDisposable
{
    [Inject]
    private TextEditorService TextEditorService { get; set; } = null!;
    [Inject]
    private ICommonUtilityService CommonUtilityService { get; set; } = null!;

    [Parameter, EditorRequired]
    public Key<TextEditorComponentData> ComponentDataKey { get; set; }

    private bool _lastSeenShowFindOverlayValue = false;
    private bool _lastFindOverlayValueExternallyChangedMarker = false;
    private string _inputValue = string.Empty;
    private string _inputReplace = string.Empty;
    private int? _activeIndexMatchedTextSpan = null;

    private Throttle _throttleInputValueChange = new Throttle(TimeSpan.FromMilliseconds(150));
    private TextEditorTextSpan? _decorationByteChangedTargetTextSpan;
    
    private Key<TextEditorComponentData> _componentDataKeyPrevious = Key<TextEditorComponentData>.Empty;
    private TextEditorComponentData? _componentData;

    private string InputValue
    {
        get => _inputValue;
        set
        {
	    	var virtualizationResult = GetVirtualizationResult();
	    	if (!virtualizationResult.IsValid)
	    		return;
        
            _inputValue = value;
            
            _throttleInputValueChange.Run(_ =>
            {
            	TextEditorService.WorkerArbitrary.PostUnique(editContext =>
                {
                    var viewModelModifier = editContext.GetViewModelModifier(virtualizationResult.ViewModel.PersistentState.ViewModelKey);

                    if (viewModelModifier is null)
                        return ValueTask.CompletedTask;

                    viewModelModifier.Virtualization.ShouldCalculateVirtualizationResult = true;
                    
                    var localInputValue = _inputValue;

                    viewModelModifier.PersistentState.FindOverlayValue = localInputValue;

                    var modelModifier = editContext.GetModelModifier(virtualizationResult.Model.PersistentState.ResourceUri);

                    if (modelModifier is null)
                        return ValueTask.CompletedTask;

                    List<TextEditorTextSpan> textSpanMatches;

                    if (!string.IsNullOrWhiteSpace(localInputValue))
                        textSpanMatches = modelModifier.FindMatches(localInputValue);
                    else
                        textSpanMatches = new();

					TextEditorService.ModelApi.StartPendingCalculatePresentationModel(
                    	editContext,
                        modelModifier,
                        FindOverlayPresentationFacts.PresentationKey,
                        FindOverlayPresentationFacts.EmptyPresentationModel);

                    var presentationModel = modelModifier.PresentationModelList.First(
                        x => x.TextEditorPresentationKey == FindOverlayPresentationFacts.PresentationKey);

                    if (presentationModel.PendingCalculation is null)
                        throw new WalkTextEditorException($"{nameof(presentationModel)}.{nameof(presentationModel.PendingCalculation)} was not expected to be null here.");

                    modelModifier.CompletePendingCalculatePresentationModel(
                        FindOverlayPresentationFacts.PresentationKey,
                        FindOverlayPresentationFacts.EmptyPresentationModel,
                        textSpanMatches);

                    _activeIndexMatchedTextSpan = null;
                    _decorationByteChangedTargetTextSpan = null;
                    
                    return ValueTask.CompletedTask;
                });
				return Task.CompletedTask;
            });
        }
    }
    
    private string InputReplace
    {
        get => _inputReplace;
        set
        {
	    	var virtualizationResult = GetVirtualizationResult();
	    	if (!virtualizationResult.IsValid)
	    		return;
        
            _inputReplace = value;
            
        	TextEditorService.WorkerArbitrary.PostUnique(editContext =>
            {
                var viewModelModifier = editContext.GetViewModelModifier(virtualizationResult.ViewModel.PersistentState.ViewModelKey);

                if (viewModelModifier is null)
                    return ValueTask.CompletedTask;

                viewModelModifier.PersistentState.ReplaceValueInFindOverlay = value;
                return ValueTask.CompletedTask;
            });
        }
    }

	protected override void OnInitialized()
    {
        TextEditorService.ViewModelApi.CursorShouldBlinkChanged += OnCursorShouldBlinkChanged;
        OnCursorShouldBlinkChanged();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
    	var virtualizationResult = GetVirtualizationResult();
    	if (!virtualizationResult.IsValid)
    		return;
    		
    	var becameShown = false;
    		
        if (_lastSeenShowFindOverlayValue != virtualizationResult.ViewModel.PersistentState.ShowFindOverlay)
        {
            _lastSeenShowFindOverlayValue = virtualizationResult.ViewModel.PersistentState.ShowFindOverlay;

            // If it changes from 'false' to 'true', focus the input element
            if (_lastSeenShowFindOverlayValue)
            {
            	becameShown = true;
            	
            	var componentData = virtualizationResult.ViewModel.PersistentState.ComponentData;
            	if (componentData is not null)
            	{
	                await CommonUtilityService.JsRuntimeCommonApi
	                    .FocusHtmlElementById(componentData.FindOverlayId)
	                    .ConfigureAwait(false);
                }
            }
        }
        
        if (becameShown ||
        	_lastFindOverlayValueExternallyChangedMarker != virtualizationResult.ViewModel.PersistentState.FindOverlayValueExternallyChangedMarker)
        {
        	_lastFindOverlayValueExternallyChangedMarker = virtualizationResult.ViewModel.PersistentState.FindOverlayValueExternallyChangedMarker;
        	InputValue = virtualizationResult.ViewModel.PersistentState.FindOverlayValue;
        	InputReplace = virtualizationResult.ViewModel.PersistentState.ReplaceValueInFindOverlay;
        }
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

    private async Task HandleOnKeyDownAsync(KeyboardEventArgs keyboardEventArgs)
    {
    	var virtualizationResult = GetVirtualizationResult();
    	if (!virtualizationResult.IsValid)
    		return;
    	
        if (keyboardEventArgs.Key == KeyboardKeyFacts.MetaKeys.ESCAPE)
        {
        	var componentData = virtualizationResult.ViewModel.PersistentState.ComponentData;
        	if (componentData is not null)
        	{
	            await CommonUtilityService.JsRuntimeCommonApi
	                .FocusHtmlElementById(componentData.PrimaryCursorContentId)
	                .ConfigureAwait(false);
            }

            TextEditorService.WorkerArbitrary.PostUnique(editContext =>
            {
                var viewModelModifier = editContext.GetViewModelModifier(virtualizationResult.ViewModel.PersistentState.ViewModelKey);

                if (viewModelModifier is null)
                    return ValueTask.CompletedTask;

                viewModelModifier.PersistentState.ShowFindOverlay = false;

                var modelModifier = editContext.GetModelModifier(virtualizationResult.Model.PersistentState.ResourceUri);

                if (modelModifier is null)
                    return ValueTask.CompletedTask;

                TextEditorService.ModelApi.StartPendingCalculatePresentationModel(
            		editContext,
                    modelModifier,
                    FindOverlayPresentationFacts.PresentationKey,
                    FindOverlayPresentationFacts.EmptyPresentationModel);

                var presentationModel = modelModifier.PresentationModelList.First(
                    x => x.TextEditorPresentationKey == FindOverlayPresentationFacts.PresentationKey);

                if (presentationModel.PendingCalculation is null)
                    throw new WalkTextEditorException($"{nameof(presentationModel)}.{nameof(presentationModel.PendingCalculation)} was not expected to be null here.");

                modelModifier.CompletePendingCalculatePresentationModel(
                    FindOverlayPresentationFacts.PresentationKey,
                    FindOverlayPresentationFacts.EmptyPresentationModel,
                    new());
                return ValueTask.CompletedTask;
            });
        }
    }

    private async Task MoveActiveIndexMatchedTextSpanUp()
    {
    	var virtualizationResult = GetVirtualizationResult();
    	if (!virtualizationResult.IsValid)
    		return;
    	
        var findOverlayPresentationModel = virtualizationResult.Model.PresentationModelList.FirstOrDefault(
            x => x.TextEditorPresentationKey == FindOverlayPresentationFacts.PresentationKey);

        if (findOverlayPresentationModel is null)
            return;

        var completedCalculation = findOverlayPresentationModel.CompletedCalculation;

        if (completedCalculation is null)
            return;

        if (_activeIndexMatchedTextSpan is null)
        {
            _activeIndexMatchedTextSpan = completedCalculation.TextSpanList.Count - 1;
        }
        else
        {
			if (completedCalculation.TextSpanList.Count == 0)
			{
				_activeIndexMatchedTextSpan = null;
			}
			else
			{
				_activeIndexMatchedTextSpan--;
	            if (_activeIndexMatchedTextSpan <= -1)
					_activeIndexMatchedTextSpan = completedCalculation.TextSpanList.Count - 1;
			}
        }

        await HandleActiveIndexMatchedTextSpanChanged();
    }

    private async Task MoveActiveIndexMatchedTextSpanDown()
    {
    	var virtualizationResult = GetVirtualizationResult();
    	if (!virtualizationResult.IsValid)
    		return;
    	
        var findOverlayPresentationModel = virtualizationResult.Model.PresentationModelList.FirstOrDefault(
            x => x.TextEditorPresentationKey == FindOverlayPresentationFacts.PresentationKey);

        if (findOverlayPresentationModel is null)
            return;

        var completedCalculation = findOverlayPresentationModel.CompletedCalculation;

        if (completedCalculation is null)
            return;

        if (_activeIndexMatchedTextSpan is null)
        {
            _activeIndexMatchedTextSpan = 0;
        }
        else
        {
			if (completedCalculation.TextSpanList.Count == 0)
			{
				_activeIndexMatchedTextSpan = null;
			}
			else
			{
            	_activeIndexMatchedTextSpan++;
				if (_activeIndexMatchedTextSpan >= completedCalculation.TextSpanList.Count)
					_activeIndexMatchedTextSpan = 0;
			}
        }

        await HandleActiveIndexMatchedTextSpanChanged();
    }

    private Task HandleActiveIndexMatchedTextSpanChanged()
    {
    	var virtualizationResult = GetVirtualizationResult();
    	if (!virtualizationResult.IsValid)
    		return Task.CompletedTask;
    	
        TextEditorService.WorkerArbitrary.PostUnique(editContext =>
        {
            var localActiveIndexMatchedTextSpan = _activeIndexMatchedTextSpan;

            if (localActiveIndexMatchedTextSpan is null)
                return ValueTask.CompletedTask;

            var viewModelModifier = editContext.GetViewModelModifier(virtualizationResult.ViewModel.PersistentState.ViewModelKey);

            if (viewModelModifier is null)
                return ValueTask.CompletedTask;
                
            viewModelModifier.Virtualization.ShouldCalculateVirtualizationResult = true;
            
            var modelModifier = editContext.GetModelModifier(virtualizationResult.Model.PersistentState.ResourceUri);

            if (modelModifier is null)
                return ValueTask.CompletedTask;

            var presentationModel = modelModifier.PresentationModelList.FirstOrDefault(x =>
                x.TextEditorPresentationKey == FindOverlayPresentationFacts.PresentationKey);

            if (presentationModel?.CompletedCalculation is not null)
            {
            	var outTextSpanList = new List<TextEditorTextSpan>(presentationModel.CompletedCalculation.TextSpanList);
            
            	var decorationByteChangedTargetTextSpanLocal = _decorationByteChangedTargetTextSpan;
                if (decorationByteChangedTargetTextSpanLocal is not null)
                {
                	TextEditorTextSpan needsColorResetSinceNoLongerActive = default;
                	int indexNeedsColorResetSinceNoLongerActive = -1;
                	
                	for (int i = 0; i < presentationModel.CompletedCalculation.TextSpanList.Count; i++)
                	{
                		var x = presentationModel.CompletedCalculation.TextSpanList[i];
                		
                		if (x.StartInclusiveIndex == decorationByteChangedTargetTextSpanLocal.Value.StartInclusiveIndex &&
                            x.EndExclusiveIndex == decorationByteChangedTargetTextSpanLocal.Value.EndExclusiveIndex &&
                            x.ResourceUri == decorationByteChangedTargetTextSpanLocal.Value.ResourceUri &&
                            x.Text == decorationByteChangedTargetTextSpanLocal.Value.Text)
                		{
                			needsColorResetSinceNoLongerActive = x;
                			indexNeedsColorResetSinceNoLongerActive = i;
                		}
                	}
                
                    if (needsColorResetSinceNoLongerActive != default && indexNeedsColorResetSinceNoLongerActive != -1)
                    {
                    	outTextSpanList[indexNeedsColorResetSinceNoLongerActive] = needsColorResetSinceNoLongerActive with
                        {
                            DecorationByte = decorationByteChangedTargetTextSpanLocal.Value.DecorationByte
                        };
                    }
                }

                var targetTextSpan = presentationModel.CompletedCalculation.TextSpanList[localActiveIndexMatchedTextSpan.Value];
                _decorationByteChangedTargetTextSpan = targetTextSpan;

                outTextSpanList[localActiveIndexMatchedTextSpan.Value] = targetTextSpan with
                {
                    DecorationByte = (byte)FindOverlayDecorationKind.Insertion,
                };
                    
                presentationModel.CompletedCalculation.TextSpanList = outTextSpanList;
            }

			{
				var decorationByteChangedTargetTextSpanLocal = _decorationByteChangedTargetTextSpan;
				
				if (decorationByteChangedTargetTextSpanLocal is not null)
				{
					TextEditorService.ViewModelApi.ScrollIntoView(
						editContext,
						modelModifier,						
						viewModelModifier,
						decorationByteChangedTargetTextSpanLocal.Value);
				}
			}
			
            return ValueTask.CompletedTask;
        });
		return Task.CompletedTask;
    }
    
    private void ToggleShowReplace()
    {
    	var virtualizationResult = GetVirtualizationResult();
    	if (!virtualizationResult.IsValid)
    		return;
    	
    	TextEditorService.WorkerArbitrary.PostUnique((TextEditorEditContext editContext) =>
        {
            var viewModelModifier = editContext.GetViewModelModifier(virtualizationResult.ViewModel.PersistentState.ViewModelKey);

            if (viewModelModifier is null)
                return ValueTask.CompletedTask;

            viewModelModifier.PersistentState.ShowReplaceButtonInFindOverlay = !viewModelModifier.PersistentState.ShowReplaceButtonInFindOverlay;

            return ValueTask.CompletedTask;
        });
    }
    
    private void ReplaceCurrent()
    {
    	var virtualizationResult = GetVirtualizationResult();
    	if (!virtualizationResult.IsValid)
    		return;
    
    	TextEditorService.WorkerArbitrary.PostUnique((TextEditorEditContext editContext) =>
        {
        	var modelModifier = editContext.GetModelModifier(virtualizationResult.Model.PersistentState.ResourceUri);
            var viewModelModifier = editContext.GetViewModelModifier(virtualizationResult.ViewModel.PersistentState.ViewModelKey);
            var localActiveIndexMatchedTextSpan = _activeIndexMatchedTextSpan;

            if (modelModifier is null || viewModelModifier is null || localActiveIndexMatchedTextSpan is null)
                return ValueTask.CompletedTask;

            var presentationModel = modelModifier.PresentationModelList.FirstOrDefault(x =>
                x.TextEditorPresentationKey == FindOverlayPresentationFacts.PresentationKey);

            if (presentationModel?.CompletedCalculation is null)
            	return ValueTask.CompletedTask;

            var targetTextSpan = presentationModel.CompletedCalculation.TextSpanList[localActiveIndexMatchedTextSpan.Value];
            
            var (lineIndex, columnIndex) = modelModifier.GetLineAndColumnIndicesFromPositionIndex(
            	targetTextSpan.StartInclusiveIndex);
            	
            viewModelModifier.LineIndex = lineIndex;
            viewModelModifier.SetColumnIndexAndPreferred(columnIndex);
            viewModelModifier.SelectionAnchorPositionIndex = -1;
            
            modelModifier.Delete(
		        viewModelModifier,
		        columnCount: targetTextSpan.Length,
		        expandWord: false,
		        TextEditorModel.DeleteKind.Delete);
		        
		    modelModifier.Insert(
		        viewModelModifier.PersistentState.ReplaceValueInFindOverlay,
		        viewModelModifier);

            return ValueTask.CompletedTask;
        });
    }
    
    private void ReplaceAll()
    {
    	var virtualizationResult = GetVirtualizationResult();
    	if (!virtualizationResult.IsValid)
    		return;
    
    	TextEditorService.WorkerArbitrary.PostUnique((TextEditorEditContext editContext) =>
        {
        	var modelModifier = editContext.GetModelModifier(virtualizationResult.Model.PersistentState.ResourceUri);
            var viewModelModifier = editContext.GetViewModelModifier(virtualizationResult.ViewModel.PersistentState.ViewModelKey);
            var localActiveIndexMatchedTextSpan = _activeIndexMatchedTextSpan;

            if (modelModifier is null || viewModelModifier is null || localActiveIndexMatchedTextSpan is null)
                return ValueTask.CompletedTask;

            var presentationModel = modelModifier.PresentationModelList.FirstOrDefault(x =>
                x.TextEditorPresentationKey == FindOverlayPresentationFacts.PresentationKey);

            if (presentationModel?.CompletedCalculation is null)
            	return ValueTask.CompletedTask;
            
            modelModifier.EnsureUndoPoint(new TextEditorEdit(
            	TextEditorEditKind.OtherOpen,
            	"ReplaceAll",
            	beforePositionIndex: modelModifier.GetPositionIndex(viewModelModifier),
				before_LineIndex: viewModelModifier.LineIndex,
				before_ColumnIndex: viewModelModifier.ColumnIndex,
				before_PreferredColumnIndex: viewModelModifier.PreferredColumnIndex,
				before_SelectionAnchorPositionIndex: viewModelModifier.SelectionAnchorPositionIndex,
				before_SelectionEndingPositionIndex: viewModelModifier.SelectionEndingPositionIndex,
				after_LineIndex: viewModelModifier.LineIndex,
				after_ColumnIndex: viewModelModifier.ColumnIndex,
				after_PreferredColumnIndex: viewModelModifier.PreferredColumnIndex,
				after_SelectionAnchorPositionIndex: viewModelModifier.SelectionAnchorPositionIndex,
				after_SelectionEndingPositionIndex: viewModelModifier.SelectionEndingPositionIndex,
            	editedTextBuilder: null));
            
            for (int i = presentationModel.CompletedCalculation.TextSpanList.Count - 1; i >= 0; i--)
            {
            	var targetTextSpan = presentationModel.CompletedCalculation.TextSpanList[i];
            	
                var (lineIndex, columnIndex) = modelModifier.GetLineAndColumnIndicesFromPositionIndex(
                	targetTextSpan.StartInclusiveIndex);
                	
                viewModelModifier.LineIndex = lineIndex;
                viewModelModifier.SetColumnIndexAndPreferred(columnIndex);
                viewModelModifier.SelectionAnchorPositionIndex = -1;
                
                modelModifier.Delete(
			        viewModelModifier,
			        columnCount: targetTextSpan.Length,
			        expandWord: false,
			        TextEditorModel.DeleteKind.Delete);
			        
			    modelModifier.Insert(
			        viewModelModifier.PersistentState.ReplaceValueInFindOverlay,
			        viewModelModifier);
            }
            
            modelModifier.EnsureUndoPoint(new TextEditorEdit(
            	TextEditorEditKind.OtherClose,
            	"ReplaceAll",
            	modelModifier.GetPositionIndex(viewModelModifier),
            	before_LineIndex: viewModelModifier.LineIndex,
				before_ColumnIndex: viewModelModifier.ColumnIndex,
				before_PreferredColumnIndex: viewModelModifier.PreferredColumnIndex,
				before_SelectionAnchorPositionIndex: viewModelModifier.SelectionAnchorPositionIndex,
				before_SelectionEndingPositionIndex: viewModelModifier.SelectionEndingPositionIndex,
				after_LineIndex: viewModelModifier.LineIndex,
				after_ColumnIndex: viewModelModifier.ColumnIndex,
				after_PreferredColumnIndex: viewModelModifier.PreferredColumnIndex,
				after_SelectionAnchorPositionIndex: viewModelModifier.SelectionAnchorPositionIndex,
				after_SelectionEndingPositionIndex: viewModelModifier.SelectionEndingPositionIndex,
            	editedTextBuilder: null));

            return ValueTask.CompletedTask;
        });
    }
    
    private async void OnCursorShouldBlinkChanged()
    {
    	await InvokeAsync(StateHasChanged);
    }
    
    public void Dispose()
    {
    	TextEditorService.ViewModelApi.CursorShouldBlinkChanged -= OnCursorShouldBlinkChanged;
    }
}