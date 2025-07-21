using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Ide.RazorLib.InputFiles.Models;

namespace Walk.Ide.RazorLib.InputFiles.Displays;

public partial class InputFileBottomControls : ComponentBase
{
    [Inject]
    private IdeService IdeService { get; set; } = null!;

    [CascadingParameter]
    public IDialog? DialogRecord { get; set; }
    [CascadingParameter]
    public InputFileState InputFileState { get; set; }

    private string _searchQuery = string.Empty;

    private void SelectInputFilePatternOnChange(ChangeEventArgs changeEventArgs)
    {
        var patternName = (string)(changeEventArgs.Value ?? string.Empty);

        var pattern = InputFileState.InputFilePatternsList
            .FirstOrDefault(x => x.PatternName == patternName);

        if (pattern.ConstructorWasInvoked)
            IdeService.InputFile_SetSelectedInputFilePattern(pattern);
    }

    private string GetSelectedTreeViewModelAbsolutePathString(InputFileState inputFileState)
    {
        var selectedAbsolutePath = inputFileState.SelectedTreeViewModel?.Item;

        if (selectedAbsolutePath is null)
            return "Selection is null";

        return selectedAbsolutePath.Value.Value;
    }

    private async Task FireOnAfterSubmit()
    {
        var valid = await InputFileState.SelectionIsValidFunc
            .Invoke(InputFileState.SelectedTreeViewModel?.Item ?? default)
            .ConfigureAwait(false);

        if (valid)
        {
            if (DialogRecord is not null)
                IdeService.CommonService.Dialog_ReduceDisposeAction(DialogRecord.DynamicViewModelKey);

            await InputFileState.OnAfterSubmitFunc
                .Invoke(InputFileState.SelectedTreeViewModel?.Item ?? default)
                .ConfigureAwait(false);
        }
    }

    private bool OnAfterSubmitIsDisabled()
    {
        return !InputFileState.SelectionIsValidFunc.Invoke(InputFileState.SelectedTreeViewModel?.Item ?? default)
            .Result;
    }

    private Task CancelOnClick()
    {
        if (DialogRecord is not null)
            IdeService.CommonService.Dialog_ReduceDisposeAction(DialogRecord.DynamicViewModelKey);

        return Task.CompletedTask;
    }
    
    private bool GetInputFilePatternIsSelected(InputFilePattern inputFilePattern, InputFileState localInputFileState)
    {
        return (localInputFileState.SelectedInputFilePattern?.PatternName ?? string.Empty) ==
            inputFilePattern.PatternName;
    }
}
