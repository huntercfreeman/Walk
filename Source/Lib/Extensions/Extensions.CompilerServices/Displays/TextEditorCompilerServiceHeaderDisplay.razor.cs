using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;
using Walk.TextEditor.RazorLib.Exceptions;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.Extensions.CompilerServices.Syntax.Nodes.Interfaces;

namespace Walk.Extensions.CompilerServices.Displays;

// Project ---- CurrentCodeBlockOwner --- Definitions inside the code block

public partial class TextEditorCompilerServiceHeaderDisplay : ComponentBase, ITextEditorDependentComponent
{
    [Inject]
    public TextEditorService TextEditorService { get; set; } = null!;

    [Parameter, EditorRequired]
    public Key<TextEditorComponentData> ComponentDataKey { get; set; }
    
    private ResourceUri _resourceUriPrevious = ResourceUri.Empty;
    
    private int _lineIndexPrevious = -1;
    private int _columnIndexPrevious = -1;
    
    private ICodeBlockOwner _codeBlockOwner;
    private bool _shouldRender = false;
    
    private bool _showDefaultToolbar = false;
    
    private CancellationTokenSource _cancellationTokenSource = new();
    
    private Key<TextEditorComponentData> _componentDataKeyPrevious = Key<TextEditorComponentData>.Empty;
    private TextEditorComponentData? _componentData;
    
    protected override void OnInitialized()
    {
        TextEditorService.SecondaryChanged += OnTextEditorWrapperCssStateChanged;
        OnCursorShouldBlinkChanged();
    }
    
    public string GetText(TextEditorTextSpan textSpan, TextEditorComponentData componentData)
    {
        if (componentData?.Virtualization?.Model is null)
            return null;
    
        return textSpan.GetText(componentData.Virtualization.Model.GetAllText(), TextEditorService);
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
        if (TextEditorService.ViewModel_CursorShouldBlink)
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
                
            viewModelModifier.Virtualization.ShouldCalculateVirtualizationResult = true;
            
            _lineIndexPrevious = viewModelModifier.LineIndex;
            _columnIndexPrevious = viewModelModifier.ColumnIndex;
            
            if (!viewModelModifier.PersistentState.FirstPresentationLayerKeysList.Contains(
                    TextEditorFacts.DevToolsPresentation_PresentationKey))
            {
                var copy = new List<Key<TextEditorPresentationModel>>(viewModelModifier.PersistentState.FirstPresentationLayerKeysList);
                copy.Add(TextEditorFacts.DevToolsPresentation_PresentationKey);

                viewModelModifier.PersistentState.FirstPresentationLayerKeysList = copy;
            }
        
            TextEditorService.Model_StartPendingCalculatePresentationModel(
                editContext,
                modelModifier,
                TextEditorFacts.DevToolsPresentation_PresentationKey,
                TextEditorFacts.DevToolsPresentation_EmptyPresentationModel);
    
            var presentationModel = modelModifier.PresentationModelList.First(
                x => x.TextEditorPresentationKey == TextEditorFacts.DevToolsPresentation_PresentationKey);
    
            if (presentationModel.PendingCalculation is null)
                throw new WalkTextEditorException($"{nameof(presentationModel)}.{nameof(presentationModel.PendingCalculation)} was not expected to be null here.");
    
            var resourceUri = modelModifier.PersistentState.ResourceUri;
    
            if (modelModifier.PersistentState.CompilerService is not IExtendedCompilerService extendedCompilerService)
                return;
    
            var codeBlockTuple = extendedCompilerService.GetCodeBlockTupleByPositionIndex(
                resourceUri,
                modelModifier.GetPositionIndex(viewModelModifier));
            
            if (codeBlockTuple.CodeBlockOwner is null)
                return;
    
            TextEditorTextSpan textSpanStart;
            
            if (codeBlockTuple.CodeBlockValue.CodeBlock_StartInclusiveIndex == -1)
            {
                textSpanStart = new TextEditorTextSpan(
                    codeBlockTuple.CodeBlockValue.Scope_StartInclusiveIndex,
                    codeBlockTuple.CodeBlockValue.Scope_StartInclusiveIndex + 1,
                    (byte)TextEditorDevToolsDecorationKind.Scope);
            }
            else
            {
                textSpanStart = new TextEditorTextSpan(
                    codeBlockTuple.CodeBlockValue.CodeBlock_StartInclusiveIndex,
                    codeBlockTuple.CodeBlockValue.CodeBlock_StartInclusiveIndex + 1,
                    (byte)TextEditorDevToolsDecorationKind.Scope);
            }

            int useStartInclusiveIndex;
            if (codeBlockTuple.CodeBlockValue.Scope_EndExclusiveIndex == -1)
                useStartInclusiveIndex = presentationModel.PendingCalculation.ContentAtRequest.Length - 1;
            else
                useStartInclusiveIndex = codeBlockTuple.CodeBlockValue.Scope_EndExclusiveIndex - 1;

            if (useStartInclusiveIndex < 0)
                useStartInclusiveIndex = 0;

            var useEndExclusiveIndex = codeBlockTuple.CodeBlockValue.Scope_EndExclusiveIndex;
            if (useEndExclusiveIndex == -1)
                useEndExclusiveIndex = presentationModel.PendingCalculation.ContentAtRequest.Length;
                
            var textSpanEnd = new TextEditorTextSpan(
                useStartInclusiveIndex,
                useEndExclusiveIndex,
                (byte)TextEditorDevToolsDecorationKind.Scope);
    
            var diagnosticTextSpans = new List<TextEditorTextSpan> { textSpanStart, textSpanEnd };

            modelModifier.CompletePendingCalculatePresentationModel(
                TextEditorFacts.DevToolsPresentation_PresentationKey,
                TextEditorFacts.DevToolsPresentation_EmptyPresentationModel,
                diagnosticTextSpans);
            
            if (viewModelModifier.Virtualization.Count > 0)
            {
                var lowerLineIndexInclusive = viewModelModifier.Virtualization.EntryList[0].LineIndex;
                var upperLineIndexInclusive = viewModelModifier.Virtualization.EntryList[viewModelModifier.Virtualization.Count - 1].LineIndex;
                
                var lowerLine = modelModifier.GetLineInformation(lowerLineIndexInclusive);
                var upperLine = modelModifier.GetLineInformation(upperLineIndexInclusive);
            }
            
            if (_codeBlockOwner != codeBlockTuple.CodeBlockOwner)
            {
                _codeBlockOwner = codeBlockTuple.CodeBlockOwner;
                _shouldRender = true;
            }
            
            await InvokeAsync(StateHasChanged);
        });
    }
    
    private async void OnTextEditorWrapperCssStateChanged(SecondaryChangedKind secondaryChangedKind)
    {
        if (secondaryChangedKind == SecondaryChangedKind.TextEditorWrapperCssStateChanged)
        {
            await InvokeAsync(StateHasChanged);
        }
        else if (secondaryChangedKind == SecondaryChangedKind.ViewModel_CursorShouldBlinkChanged)
        {
            OnCursorShouldBlinkChanged();
        }
    }

    public void Dispose()
    {
        TextEditorService.SecondaryChanged -= OnTextEditorWrapperCssStateChanged;
        
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }
}
