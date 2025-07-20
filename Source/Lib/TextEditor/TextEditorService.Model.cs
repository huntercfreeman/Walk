using Walk.Common.RazorLib.Keymaps.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.Lines.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;

namespace Walk.TextEditor.RazorLib;

public partial class TextEditorService
{
	#region CREATE_METHODS
	public void Model_RegisterCustom(TextEditorEditContext editContext, TextEditorModel model)
	{
		RegisterModel(editContext, model);
	}

	public void Model_RegisterTemplated(
		TextEditorEditContext editContext,
		string extensionNoPeriod,
		ResourceUri resourceUri,
		DateTime resourceLastWriteTime,
		string initialContent,
		string? overrideDisplayTextForFileExtension = null)
	{
		var model = new TextEditorModel(
			resourceUri,
			resourceLastWriteTime,
			overrideDisplayTextForFileExtension ?? extensionNoPeriod,
			initialContent,
			GetDecorationMapper(extensionNoPeriod),
			GetCompilerService(extensionNoPeriod),
			this);

		RegisterModel(editContext, model);
	}
	#endregion

	#region READ_METHODS
	[Obsolete("TextEditorModel.PersistentState.ViewModelKeyList")]
	public List<TextEditorViewModel> Model_GetViewModelsOrEmpty(ResourceUri resourceUri)
	{
		return TextEditorState.ModelGetViewModelsOrEmpty(resourceUri);
	}

	public string? Model_GetAllText(ResourceUri resourceUri)
	{
		return Model_GetOrDefault(resourceUri)?.GetAllText(); ;
	}

	public TextEditorModel? Model_GetOrDefault(ResourceUri resourceUri)
	{
		return TextEditorState.ModelGetOrDefault(
			resourceUri);
	}

	public Dictionary<ResourceUri, TextEditorModel> Model_GetModels()
	{
		return TextEditorState.ModelGetModels();
	}

	public int Model_GetModelsCount()
	{
		return TextEditorState.ModelGetModelsCount();
	}
	#endregion

	#region UPDATE_METHODS
	/*public void Model_UndoEdit(
	    TextEditorEditContext editContext,
        TextEditorModel modelModifier)
    {
        modelModifier.UndoEdit();
    }*/

	public void Model_SetUsingLineEndKind(
		TextEditorEditContext editContext,
		TextEditorModel modelModifier,
		LineEndKind lineEndKind)
	{
		modelModifier.SetLineEndKindPreference(lineEndKind);
	}

	public void Model_SetResourceData(
		TextEditorEditContext editContext,
		TextEditorModel modelModifier,
		DateTime resourceLastWriteTime)
	{
		modelModifier.SetResourceData(modelModifier.PersistentState.ResourceUri, resourceLastWriteTime);
	}

	public void Model_Reload(
		TextEditorEditContext editContext,
		TextEditorModel modelModifier,
		string content,
		DateTime resourceLastWriteTime)
	{
		modelModifier.SetContent(content);
		modelModifier.SetResourceData(modelModifier.PersistentState.ResourceUri, resourceLastWriteTime);
	}

	/*public void Model_RedoEdit(
    	TextEditorEditContext editContext,
        TextEditorModel modelModifier)
    {
        modelModifier.RedoEdit();
    }*/

	public void Model_InsertText(
		TextEditorEditContext editContext,
		TextEditorModel modelModifier,
		TextEditorViewModel viewModel,
		string content)
	{
		modelModifier.Insert(content, viewModel);
	}

	public void Model_InsertTextUnsafe(
		TextEditorEditContext editContext,
		TextEditorModel modelModifier,
		TextEditorViewModel viewModel,
		string content)
	{
		modelModifier.Insert(content, viewModel);
	}

	public void Model_HandleKeyboardEvent(
		TextEditorEditContext editContext,
		TextEditorModel modelModifier,
		TextEditorViewModel viewModel,
		KeymapArgs keymapArgs)
	{
		modelModifier.HandleKeyboardEvent(keymapArgs, viewModel);
	}

	public void Model_HandleKeyboardEventUnsafe(
		TextEditorEditContext editContext,
		TextEditorModel modelModifier,
		TextEditorViewModel viewModel,
		KeymapArgs keymapArgs)
	{
		modelModifier.HandleKeyboardEvent(keymapArgs, viewModel);
	}

	public void Model_DeleteTextByRange(
		TextEditorEditContext editContext,
		TextEditorModel modelModifier,
		TextEditorViewModel viewModel,
		int count)
	{
		modelModifier.DeleteByRange(count, viewModel);
	}

	public void Model_DeleteTextByRangeUnsafe(
		TextEditorEditContext editContext,
		TextEditorModel modelModifier,
		TextEditorViewModel viewModel,
		int count)
	{
		modelModifier.DeleteByRange(count, viewModel);
	}

	public void Model_DeleteTextByMotion(
		TextEditorEditContext editContext,
		TextEditorModel modelModifier,
		TextEditorViewModel viewModel,
		MotionKind motionKind)
	{
		modelModifier.DeleteTextByMotion(motionKind, viewModel);
	}

	public void Model_DeleteTextByMotionUnsafe(
		TextEditorEditContext editContext,
		TextEditorModel modelModifier,
		TextEditorViewModel viewModel,
		MotionKind motionKind)
	{
		modelModifier.DeleteTextByMotion(motionKind, viewModel);
	}

	public void Model_AddPresentationModel(
		TextEditorEditContext editContext,
		TextEditorModel modelModifier,
		TextEditorPresentationModel emptyPresentationModel)
	{
		modelModifier.PerformRegisterPresentationModelAction(emptyPresentationModel);
	}

	public void Model_StartPendingCalculatePresentationModel(
		TextEditorEditContext editContext,
		TextEditorModel modelModifier,
		Key<TextEditorPresentationModel> presentationKey,
		TextEditorPresentationModel emptyPresentationModel)
	{
		modelModifier.StartPendingCalculatePresentationModel(presentationKey, emptyPresentationModel);
	}

	public void Model_CompletePendingCalculatePresentationModel(
		TextEditorEditContext editContext,
		TextEditorModel modelModifier,
		Key<TextEditorPresentationModel> presentationKey,
		TextEditorPresentationModel emptyPresentationModel,
		List<TextEditorTextSpan> calculatedTextSpans)
	{
		modelModifier.CompletePendingCalculatePresentationModel(
			presentationKey,
			emptyPresentationModel,
			calculatedTextSpans);
	}

	public void Model_ApplyDecorationRange(
		TextEditorEditContext editContext,
		TextEditorModel modelModifier,
		IEnumerable<TextEditorTextSpan> textSpans)
	{
		var localRichCharacterList = modelModifier.RichCharacterList;

		var positionsPainted = new HashSet<int>();

		foreach (var textEditorTextSpan in textSpans)
		{
			for (var i = textEditorTextSpan.StartInclusiveIndex; i < textEditorTextSpan.EndExclusiveIndex; i++)
			{
				if (i < 0 || i >= localRichCharacterList.Length)
					continue;

				modelModifier.__SetDecorationByte(i, textEditorTextSpan.DecorationByte);
				positionsPainted.Add(i);
			}
		}

		for (var i = 0; i < localRichCharacterList.Length - 1; i++)
		{
			if (!positionsPainted.Contains(i))
			{
				// DecorationByte of 0 is to be 'None'
				modelModifier.__SetDecorationByte(i, 0);
			}
		}

		modelModifier.ShouldCalculateVirtualizationResult = true;
	}

	public void Model_ApplySyntaxHighlighting(
		TextEditorEditContext editContext,
		TextEditorModel modelModifier,
		IEnumerable<TextEditorTextSpan> textSpanList)
	{
		foreach (var viewModelKey in modelModifier.PersistentState.ViewModelKeyList)
		{
			var viewModel = editContext.GetViewModelModifier(viewModelKey);

			var componentData = viewModel.PersistentState.ComponentData;
			if (componentData is not null)
				componentData.LineIndexCache.IsInvalid = true;
		}

		Model_ApplyDecorationRange(
			editContext,
			modelModifier,
			textSpanList);

		// TODO: Why does painting reload virtualization result???
		modelModifier.ShouldCalculateVirtualizationResult = true;
	}
	#endregion

	#region DELETE_METHODS
	public void Model_Dispose(TextEditorEditContext editContext, ResourceUri resourceUri)
	{
		DisposeModel(editContext, resourceUri);
	}
	#endregion
}
