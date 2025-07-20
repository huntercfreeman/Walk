using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.Diffs.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;

namespace Walk.TextEditor.RazorLib;

public partial class TextEditorService
{
    private TextEditorDiffState Diff_textEditorDiffState = new();

	public event Action? Diff_TextEditorDiffStateChanged;

	public TextEditorDiffState Diff_GetTextEditorDiffState() => Diff_textEditorDiffState;

	public void Diff_Register(
		Key<TextEditorDiffModel> diffModelKey,
		Key<TextEditorViewModel> inViewModelKey,
		Key<TextEditorViewModel> outViewModelKey)
	{
		Diff_ReduceRegisterAction(
			diffModelKey,
			inViewModelKey,
			outViewModelKey);
	}

	public TextEditorDiffModel? Diff_GetOrDefault(Key<TextEditorDiffModel> diffModelKey)
	{
		return Diff_GetTextEditorDiffState().DiffModelList
			.FirstOrDefault(x => x.DiffKey == diffModelKey);
	}

	public void Diff_Dispose(Key<TextEditorDiffModel> diffModelKey)
	{
		Diff_ReduceDisposeAction(diffModelKey);
	}

	public Func<TextEditorEditContext, Task> Diff_CalculateFactory(
		Key<TextEditorDiffModel> diffModelKey,
		CancellationToken cancellationToken)
	{
		return editContext =>
		{
			if (cancellationToken.IsCancellationRequested)
				return Task.CompletedTask;

			var diffModelModifier = editContext.GetDiffModelModifier(diffModelKey);

			if (diffModelModifier is null)
				return Task.CompletedTask;

			var inViewModel = editContext.GetViewModelModifier(diffModelModifier.DiffModel.InViewModelKey);
			var outViewModel = editContext.GetViewModelModifier(diffModelModifier.DiffModel.OutViewModelKey);

			if (inViewModel is null || outViewModel is null)
				return Task.CompletedTask;

			var inModelModifier = editContext.GetModelModifier(inViewModel.PersistentState.ResourceUri);
			var outModelModifier = editContext.GetModelModifier(outViewModel.PersistentState.ResourceUri);

			if (inModelModifier is null || outModelModifier is null)
				return Task.CompletedTask;

			// In
			editContext.TextEditorService.Model_StartPendingCalculatePresentationModel(
				editContext,
				inModelModifier,
				DiffPresentationFacts.InPresentationKey,
				DiffPresentationFacts.EmptyInPresentationModel);
			var inPresentationModel = inModelModifier.PresentationModelList.First(
				x => x.TextEditorPresentationKey == DiffPresentationFacts.InPresentationKey);
			if (inPresentationModel.PendingCalculation is null)
				return Task.CompletedTask;
			var inText = inPresentationModel.PendingCalculation.ContentAtRequest;

			// Out
			editContext.TextEditorService.Model_StartPendingCalculatePresentationModel(
				editContext,
				outModelModifier,
				DiffPresentationFacts.OutPresentationKey,
				DiffPresentationFacts.EmptyOutPresentationModel);
			var outPresentationModel = outModelModifier.PresentationModelList.First(
				x => x.TextEditorPresentationKey == DiffPresentationFacts.OutPresentationKey);
			if (outPresentationModel.PendingCalculation is null)
				return Task.CompletedTask;
			var outText = outPresentationModel.PendingCalculation.ContentAtRequest;

			var diffResult = TextEditorDiffResult.Calculate(
				inModelModifier.PersistentState.ResourceUri,
				inText,
				outModelModifier.PersistentState.ResourceUri,
				outText);

			inModelModifier.CompletePendingCalculatePresentationModel(
				DiffPresentationFacts.InPresentationKey,
				DiffPresentationFacts.EmptyInPresentationModel,
				diffResult.InResultTextSpanList);

			outModelModifier.CompletePendingCalculatePresentationModel(
				DiffPresentationFacts.OutPresentationKey,
				DiffPresentationFacts.EmptyOutPresentationModel,
				diffResult.OutResultTextSpanList);

			return Task.CompletedTask;
		};
	}

	public IReadOnlyList<TextEditorDiffModel> Diff_GetDiffModels()
	{
		return Diff_GetTextEditorDiffState().DiffModelList;
	}

	public void Diff_ReduceDisposeAction(Key<TextEditorDiffModel> diffKey)
	{
		var inState = Diff_GetTextEditorDiffState();

		var inDiff = inState.DiffModelList.FirstOrDefault(
			x => x.DiffKey == diffKey);

		if (inDiff is null)
		{
			Diff_TextEditorDiffStateChanged?.Invoke();
			return;
		}

		var outDiffModelList = new List<TextEditorDiffModel>(inState.DiffModelList);
		outDiffModelList.Remove(inDiff);

		Diff_textEditorDiffState = new TextEditorDiffState
		{
			DiffModelList = outDiffModelList
		};

		Diff_TextEditorDiffStateChanged?.Invoke();
		return;
	}

	public void Diff_ReduceRegisterAction(
		Key<TextEditorDiffModel> diffKey,
		Key<TextEditorViewModel> inViewModelKey,
		Key<TextEditorViewModel> outViewModelKey)
	{
		var inState = Diff_GetTextEditorDiffState();

		var inDiff = inState.DiffModelList.FirstOrDefault(
			x => x.DiffKey == diffKey);

		if (inDiff is not null)
		{
			Diff_TextEditorDiffStateChanged?.Invoke();
			return;
		}

		var diff = new TextEditorDiffModel(
			diffKey,
			inViewModelKey,
			outViewModelKey);

		var outDiffModelList = new List<TextEditorDiffModel>(inState.DiffModelList);
		outDiffModelList.Add(diff);

		Diff_textEditorDiffState = new TextEditorDiffState
		{
			DiffModelList = outDiffModelList
		};

		Diff_TextEditorDiffStateChanged?.Invoke();
		return;
	}
}
