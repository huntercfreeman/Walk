using System.Collections.Concurrent;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.JavaScriptObjects.Models;
using Walk.TextEditor.RazorLib.Commands.Models.Defaults;
using Walk.TextEditor.RazorLib.Cursors.Models;
using Walk.TextEditor.RazorLib.Events.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;

namespace Walk.TextEditor.RazorLib.BackgroundTasks.Models;

public class TextEditorWorkerUi : IBackgroundTaskGroup
{
    private readonly TextEditorService _textEditorService;
    
    public TextEditorWorkerUi(TextEditorService textEditorService)
    {
        _textEditorService = textEditorService;
    }
    
    public Key<IBackgroundTaskGroup> BackgroundTaskKey { get; } = Key<IBackgroundTaskGroup>.NewKey();
    
    public bool __TaskCompletionSourceWasCreated { get; set; }
    
    public ConcurrentQueue<(
        TextEditorEventArgs WorkerUiArgs,
        TextEditorComponentData ComponentData,
        Key<TextEditorViewModel> ViewModelKey,
        TextEditorWorkUiKind WorkUiKind)> WorkQueue { get; } = new();
    
    public void Enqueue(
        TextEditorEventArgs workerUiArgs,
        TextEditorComponentData componentData,
        Key<TextEditorViewModel> viewModelKey,
        TextEditorWorkUiKind workUiKind)
    {
        WorkQueue.Enqueue(
            (
                workerUiArgs,
                componentData,
                viewModelKey,
                workUiKind
            ));
        _textEditorService.CommonService.Continuous_Enqueue(this);
    }
    
    public async ValueTask HandleEvent()
    {
        if (!WorkQueue.TryDequeue(out (
            TextEditorEventArgs WorkerUiArgs,
            TextEditorComponentData ComponentData,
            Key<TextEditorViewModel> ViewModelKey,
            TextEditorWorkUiKind WorkUiKind) workArgsTuple))
            return;
        
        switch (workArgsTuple.WorkUiKind)
        {
            case TextEditorWorkUiKind.OnDoubleClick:
            {
                var editContext = new TextEditorEditContext(workArgsTuple.ComponentData.TextEditorViewModelSlimDisplay.TextEditorService);
    
                var viewModel = editContext.GetViewModelModifier(workArgsTuple.ViewModelKey);
                var modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri, isReadOnly: true);
        
                if (modelModifier is null || viewModel is null)
                    return;
                    
                var hasSelectedText = TextEditorSelectionHelper.HasSelectedText(viewModel);
        
                if ((workArgsTuple.WorkerUiArgs.Buttons & 1) != 1 && hasSelectedText)
                    return; // Not pressing the left mouse button so assume ContextMenu is desired result.
        
                if (workArgsTuple.WorkerUiArgs.ShiftKey)
                    return; // Do not expand selection if user is holding shift
        
                // Labeling any ITextEditorEditContext -> JavaScript interop or Blazor StateHasChanged.
                // Reason being, these are likely to be huge optimizations (2024-05-29).
                var lineAndColumnIndex = await EventUtils.CalculateLineAndColumnIndex(
                        modelModifier,
                        viewModel,
                        workArgsTuple.WorkerUiArgs.X,
                        workArgsTuple.WorkerUiArgs.Y,
                        workArgsTuple.ComponentData,
                        editContext)
                    .ConfigureAwait(false);
        
                var lowerColumnIndexExpansion = modelModifier.GetColumnIndexOfCharacterWithDifferingKind(
                    lineAndColumnIndex.LineIndex,
                    lineAndColumnIndex.ColumnIndex,
                    true);
        
                lowerColumnIndexExpansion = lowerColumnIndexExpansion == -1
                    ? 0
                    : lowerColumnIndexExpansion;
        
                var higherColumnIndexExpansion = modelModifier.GetColumnIndexOfCharacterWithDifferingKind(
                    lineAndColumnIndex.LineIndex,
                    lineAndColumnIndex.ColumnIndex,
                    false);
        
                higherColumnIndexExpansion = higherColumnIndexExpansion == -1
                    ? modelModifier.GetLineLength(lineAndColumnIndex.LineIndex)
                    : higherColumnIndexExpansion;
        
                // Move user's cursor position to the higher expansion
                {
                    viewModel.LineIndex = lineAndColumnIndex.LineIndex;
                    viewModel.ColumnIndex = higherColumnIndexExpansion;
                    viewModel.PreferredColumnIndex = lineAndColumnIndex.ColumnIndex;
                }
        
                // Set text selection ending to higher expansion
                {
                    var cursorPositionOfHigherExpansion = modelModifier.GetPositionIndex(
                        lineAndColumnIndex.LineIndex,
                        higherColumnIndexExpansion);
        
                    viewModel.SelectionEndingPositionIndex = cursorPositionOfHigherExpansion;
                }
        
                // Set text selection anchor to lower expansion
                {
                    var cursorPositionOfLowerExpansion = modelModifier.GetPositionIndex(
                        lineAndColumnIndex.LineIndex,
                        lowerColumnIndexExpansion);
        
                    viewModel.SelectionAnchorPositionIndex = cursorPositionOfLowerExpansion;
                }
                
                await editContext.TextEditorService
                    .FinalizePost(editContext)
                    .ConfigureAwait(false);
                
                return;
            }
            case TextEditorWorkUiKind.OnKeyDown:
            {
                await workArgsTuple.ComponentData.Options.Keymap.HandleEvent(
                    workArgsTuple.ComponentData,
                    workArgsTuple.ViewModelKey,
                    workArgsTuple.WorkerUiArgs.Key,
                    workArgsTuple.WorkerUiArgs.Code,
                    workArgsTuple.WorkerUiArgs.CtrlKey,
                    workArgsTuple.WorkerUiArgs.ShiftKey,
                    workArgsTuple.WorkerUiArgs.AltKey,
                    workArgsTuple.WorkerUiArgs.MetaKey);
                return;
            }
            case TextEditorWorkUiKind.OnMouseDown:
            {
                var editContext = new TextEditorEditContext(workArgsTuple.ComponentData.TextEditorViewModelSlimDisplay.TextEditorService);
    
                var viewModel = editContext.GetViewModelModifier(workArgsTuple.ViewModelKey);
                var modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri, isReadOnly: true);
        
                if (modelModifier is null || viewModel is null)
                    return;
        
                viewModel.PersistentState.ShouldRevealCursor = false;
        
                var hasSelectedText = TextEditorSelectionHelper.HasSelectedText(viewModel);
                
                if ((workArgsTuple.WorkerUiArgs.Buttons & 1) != 1 && hasSelectedText)
                    return; // Not pressing the left mouse button so assume ContextMenu is desired result.
        
                if (viewModel.PersistentState.MenuKind != MenuKind.None)
                {
                    TextEditorCommandDefaultFunctions.RemoveDropdown(
                        editContext,
                        viewModel,
                        workArgsTuple.ComponentData.TextEditorViewModelSlimDisplay.TextEditorService.CommonService);
                }
        
                // Remember the current cursor position prior to doing anything
                var inLineIndex = viewModel.LineIndex;
                var inColumnIndex = viewModel.ColumnIndex;
        
                // Move the cursor position
                //
                // Labeling any ITextEditorEditContext -> JavaScript interop or Blazor StateHasChanged.
                // Reason being, these are likely to be huge optimizations (2024-05-29).
                var lineAndColumnIndex = await EventUtils.CalculateLineAndColumnIndex(
                        modelModifier,
                        viewModel,
                        workArgsTuple.WorkerUiArgs.X,
                        workArgsTuple.WorkerUiArgs.Y,
                        workArgsTuple.ComponentData,
                        editContext)
                    .ConfigureAwait(false);
        
                viewModel.LineIndex = lineAndColumnIndex.LineIndex;
                viewModel.ColumnIndex = lineAndColumnIndex.ColumnIndex;
                viewModel.PreferredColumnIndex = lineAndColumnIndex.ColumnIndex;
        
                var cursorPositionIndex = modelModifier.GetPositionIndex(
                    lineAndColumnIndex.LineIndex,
                    lineAndColumnIndex.ColumnIndex);
        
                if (workArgsTuple.WorkerUiArgs.ShiftKey)
                {
                    if (!hasSelectedText)
                    {
                        // If user does not yet have a selection then place the text selection anchor were they were
                        viewModel.SelectionAnchorPositionIndex = modelModifier
                            .GetPositionIndex(inLineIndex, inColumnIndex);
                    }
        
                    // If user ALREADY has a selection then do not modify the text selection anchor
                }
                else
                {
                    viewModel.SelectionAnchorPositionIndex = cursorPositionIndex;
                }
        
                viewModel.SelectionEndingPositionIndex = cursorPositionIndex;
                
                finalize:
                
                editContext.TextEditorService.ViewModel_StopCursorBlinking();
                
                await editContext.TextEditorService
                    .FinalizePost(editContext)
                    .ConfigureAwait(false);
                
                return;
            }
            case TextEditorWorkUiKind.OnMouseMove:
            {
                var editContext = new TextEditorEditContext(workArgsTuple.ComponentData.TextEditorViewModelSlimDisplay.TextEditorService);
    
                var viewModel = editContext.GetViewModelModifier(workArgsTuple.ViewModelKey);
                var modelModifier = editContext.GetModelModifier(viewModel.PersistentState.ResourceUri, isReadOnly: true);
        
                if (modelModifier is null || viewModel is null)
                    return;
                    
                // Labeling any ITextEditorEditContext -> JavaScript interop or Blazor StateHasChanged.
                // Reason being, these are likely to be huge optimizations (2024-05-29).
                var rowAndColumnIndex = await EventUtils.CalculateLineAndColumnIndex(
                        modelModifier,
                        viewModel,
                        workArgsTuple.WorkerUiArgs.X,
                        workArgsTuple.WorkerUiArgs.Y,
                        workArgsTuple.ComponentData,
                        editContext)
                    .ConfigureAwait(false);
                
                var positionIndex = modelModifier.GetPositionIndex(rowAndColumnIndex.LineIndex, rowAndColumnIndex.ColumnIndex);
                
                if (viewModel.LineIndex == rowAndColumnIndex.LineIndex &&
                    viewModel.ColumnIndex == rowAndColumnIndex.ColumnIndex &&
                    viewModel.PreferredColumnIndex == rowAndColumnIndex.ColumnIndex &&
                    viewModel.SelectionEndingPositionIndex == positionIndex)
                {
                    editContext.TextEditorService.ViewModel_StopCursorBlinking();
                    return;
                }
                
                viewModel.LineIndex = rowAndColumnIndex.LineIndex;
                viewModel.ColumnIndex = rowAndColumnIndex.ColumnIndex;
                viewModel.PreferredColumnIndex = rowAndColumnIndex.ColumnIndex;
        
                viewModel.SelectionEndingPositionIndex = positionIndex;
                
                finalize:
            
                editContext.TextEditorService.ViewModel_StopCursorBlinking();
            
                await editContext.TextEditorService
                    .FinalizePost(editContext)
                    .ConfigureAwait(false);
                
                return;
            }
            case TextEditorWorkUiKind.OnScrollHorizontal:
            {
                var editContext = new TextEditorEditContext(workArgsTuple.ComponentData.TextEditorViewModelSlimDisplay.TextEditorService);
    
                var viewModelModifier = editContext.GetViewModelModifier(workArgsTuple.ViewModelKey);
                if (viewModelModifier is null)
                    return;
        
                editContext.TextEditorService.ViewModel_SetScrollPositionLeft(
                    editContext,
                    viewModelModifier,
                    workArgsTuple.WorkerUiArgs.X);
                    
                await editContext.TextEditorService
                    .FinalizePost(editContext)
                    .ConfigureAwait(false);
                
                return;
            }
            case TextEditorWorkUiKind.OnScrollVertical:
            {
                var editContext = new TextEditorEditContext(workArgsTuple.ComponentData.TextEditorViewModelSlimDisplay.TextEditorService);
    
                var viewModelModifier = editContext.GetViewModelModifier(workArgsTuple.ViewModelKey);
                if (viewModelModifier is null)
                    return;
        
                editContext.TextEditorService.ViewModel_SetScrollPositionTop(
                    editContext,
                    viewModelModifier,
                    workArgsTuple.WorkerUiArgs.Y);
                    
                await editContext.TextEditorService
                    .FinalizePost(editContext)
                    .ConfigureAwait(false);
                
                return;
            }
            case TextEditorWorkUiKind.OnWheel:
            {
                var editContext = new TextEditorEditContext(workArgsTuple.ComponentData.TextEditorViewModelSlimDisplay.TextEditorService);
    
                var viewModelModifier = editContext.GetViewModelModifier(workArgsTuple.ViewModelKey);
                if (viewModelModifier is null)
                    return;
                    
                // TODO: Why was this made as 'if' 'else' whereas the OnWheelBatch...
                //       ...is doing 'if' 'if'.
                //       |
                //       The OnWheelBatch doesn't currently batch horizontal with vertical
                //       the OnWheel events have to be the same axis to batch.
                if (workArgsTuple.WorkerUiArgs.ShiftKey)
                {
                    editContext.TextEditorService.ViewModel_MutateScrollHorizontalPosition(
                        editContext,
                        viewModelModifier,
                        workArgsTuple.WorkerUiArgs.Y / 2);
                }
                else
                {
                    editContext.TextEditorService.ViewModel_MutateScrollVerticalPosition(
                        editContext,
                        viewModelModifier,
                        workArgsTuple.WorkerUiArgs.Y);
                }
                
                await editContext.TextEditorService
                    .FinalizePost(editContext)
                    .ConfigureAwait(false);
                
                return;
            }
            default:
            {
                Console.WriteLine($"{nameof(TextEditorWorkerUi)} {nameof(HandleEvent)} default case");
                return;
            }
        }
    }
}
