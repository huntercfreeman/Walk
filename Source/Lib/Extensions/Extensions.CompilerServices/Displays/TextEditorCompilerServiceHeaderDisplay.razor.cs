using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;
using Walk.TextEditor.RazorLib.Exceptions;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Displays;

// Project ---- CurrentCodeBlockOwner --- Definitions inside the code block

public partial class TextEditorCompilerServiceHeaderDisplay : ComponentBase, ITextEditorDependentComponent
{
	[Inject]
	public TextEditorService TextEditorService { get; set; } = null!;
    [Inject]
    private IAppOptionsService AppOptionsService { get; set; } = null!;

	[Parameter, EditorRequired]
	public Key<TextEditorComponentData> ComponentDataKey { get; set; }
	
	private ResourceUri _resourceUriPrevious = ResourceUri.Empty;
	
	private int _lineIndexPrevious = -1;
	private int _columnIndexPrevious = -1;
	
	private ICodeBlockOwner? _codeBlockOwner;
	private bool _shouldRender = false;
	
	private bool _showDefaultToolbar = false;
	
	private CancellationTokenSource _cancellationTokenSource = new();
	
	private Key<TextEditorComponentData> _componentDataKeyPrevious = Key<TextEditorComponentData>.Empty;
    private TextEditorComponentData? _componentData;
	
	protected override void OnInitialized()
    {
        TextEditorService.ViewModelApi.CursorShouldBlinkChanged += OnCursorShouldBlinkChanged;
        TextEditorService.OptionsApi.TextEditorWrapperCssStateChanged += OnTextEditorWrapperCssStateChanged;
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
    	if (TextEditorService.ViewModelApi.CursorShouldBlink)
    		UpdateUi();
    }
    
    private void ToggleDefaultToolbar()
    {
    	_showDefaultToolbar = !_showDefaultToolbar;
    }
    
    private void UpdateUi()
    {
    	if (!GetVirtualizationResult().IsValid)
    		return;
    	
    	TextEditorService.WorkerArbitrary.PostUnique(async editContext =>
    	{
    		var virtualizationResult = GetVirtualizationResult();
    	
    		var modelModifier = editContext.GetModelModifier(virtualizationResult.Model.PersistentState.ResourceUri);
            var viewModelModifier = editContext.GetViewModelModifier(virtualizationResult.ViewModel.PersistentState.ViewModelKey);

            if (modelModifier is null || viewModelModifier is null)
                return;
            
            _lineIndexPrevious = viewModelModifier.LineIndex;
            _columnIndexPrevious = viewModelModifier.ColumnIndex;
            
            if (!viewModelModifier.PersistentState.FirstPresentationLayerKeysList.Contains(
            		TextEditorDevToolsPresentationFacts.PresentationKey))
            {
				var copy = new List<Key<TextEditorPresentationModel>>(viewModelModifier.PersistentState.FirstPresentationLayerKeysList);
				copy.Add(TextEditorDevToolsPresentationFacts.PresentationKey);

				viewModelModifier.PersistentState.FirstPresentationLayerKeysList = copy;
	        }
    	
    		TextEditorService.ModelApi.StartPendingCalculatePresentationModel(
				editContext,
		        modelModifier,
		        TextEditorDevToolsPresentationFacts.PresentationKey,
				TextEditorDevToolsPresentationFacts.EmptyPresentationModel);
	
			var presentationModel = modelModifier.PresentationModelList.First(
				x => x.TextEditorPresentationKey == TextEditorDevToolsPresentationFacts.PresentationKey);
	
			if (presentationModel.PendingCalculation is null)
				throw new WalkTextEditorException($"{nameof(presentationModel)}.{nameof(presentationModel.PendingCalculation)} was not expected to be null here.");
	
	        var resourceUri = modelModifier.PersistentState.ResourceUri;
	
			if (modelModifier.PersistentState.CompilerService is not IExtendedCompilerService extendedCompilerService)
				return;
	
			var targetScope = extendedCompilerService.GetScopeByPositionIndex(
				resourceUri,
				modelModifier.GetPositionIndex(viewModelModifier));
			
			if (!targetScope.ConstructorWasInvoked)
				return;
    
			TextEditorTextSpan textSpanStart;
    		
    		if (!targetScope.CodeBlockOwner.OpenCodeBlockTextSpan.ConstructorWasInvoked)
    		{
    			textSpanStart = new TextEditorTextSpan(
		            targetScope.StartInclusiveIndex,
		            targetScope.StartInclusiveIndex + 1,
				    (byte)TextEditorDevToolsDecorationKind.Scope,
				    resourceUri,
				    sourceText: string.Empty,
				    getTextPrecalculatedResult: string.Empty);
    		}
    		else
    		{
    			textSpanStart = new TextEditorTextSpan(
		            targetScope.CodeBlockOwner.OpenCodeBlockTextSpan.StartInclusiveIndex,
		            targetScope.CodeBlockOwner.OpenCodeBlockTextSpan.StartInclusiveIndex + 1,
				    (byte)TextEditorDevToolsDecorationKind.Scope,
				    resourceUri,
				    sourceText: string.Empty,
				    getTextPrecalculatedResult: string.Empty);
    		}

			int useStartInclusiveIndex;
			if (targetScope.EndExclusiveIndex == -1)
				useStartInclusiveIndex = presentationModel.PendingCalculation.ContentAtRequest.Length - 1;
			else
				useStartInclusiveIndex = targetScope.EndExclusiveIndex - 1;

			if (useStartInclusiveIndex < 0)
				useStartInclusiveIndex = 0;

			var useEndExclusiveIndex = targetScope.EndExclusiveIndex;
    		if (useEndExclusiveIndex == -1)
    			useEndExclusiveIndex = presentationModel.PendingCalculation.ContentAtRequest.Length;
    			
			var textSpanEnd = new TextEditorTextSpan(
	            useStartInclusiveIndex,
			    useEndExclusiveIndex,
			    (byte)TextEditorDevToolsDecorationKind.Scope,
			    resourceUri,
			    sourceText: string.Empty,
			    getTextPrecalculatedResult: string.Empty);
	
			var diagnosticTextSpans = new List<TextEditorTextSpan> { textSpanStart, textSpanEnd };

			modelModifier.CompletePendingCalculatePresentationModel(
				TextEditorDevToolsPresentationFacts.PresentationKey,
				TextEditorDevToolsPresentationFacts.EmptyPresentationModel,
				diagnosticTextSpans);
			
			if (viewModelModifier.Virtualization.Count > 0)
			{
				var lowerLineIndexInclusive = viewModelModifier.Virtualization.EntryList[0].LineIndex;
	            var upperLineIndexInclusive = viewModelModifier.Virtualization.EntryList[viewModelModifier.Virtualization.Count - 1].LineIndex;
	            
	            var lowerLine = modelModifier.GetLineInformation(lowerLineIndexInclusive);
	            var upperLine = modelModifier.GetLineInformation(upperLineIndexInclusive);
				
				viewModelModifier.PersistentState.VirtualizedCollapsePointList = new();
				
				foreach (var collapsePoint in viewModelModifier.PersistentState.AllCollapsePointList)
				{
					if (lowerLine.Index <= collapsePoint.AppendToLineIndex &&
		    	    	upperLine.Index >= collapsePoint.AppendToLineIndex)
		    	    {
		    	    	viewModelModifier.PersistentState.VirtualizedCollapsePointList.Add(collapsePoint);
		    	    }
				}
				
				viewModelModifier.PersistentState.VirtualizedCollapsePointListVersion++;
			}
				
			if (_codeBlockOwner != targetScope.CodeBlockOwner)
			{
				_codeBlockOwner = targetScope.CodeBlockOwner;
				_shouldRender = true;
			}
			
			await InvokeAsync(StateHasChanged);
    	});
    }
    
    private async void OnTextEditorWrapperCssStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

	public void Dispose()
    {
    	TextEditorService.ViewModelApi.CursorShouldBlinkChanged -= OnCursorShouldBlinkChanged;
    	TextEditorService.OptionsApi.TextEditorWrapperCssStateChanged -= OnTextEditorWrapperCssStateChanged;
    	
    	_cancellationTokenSource.Cancel();
    	_cancellationTokenSource.Dispose();
    }
}