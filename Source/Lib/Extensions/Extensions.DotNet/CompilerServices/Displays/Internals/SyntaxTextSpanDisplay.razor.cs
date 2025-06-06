using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Keyboards.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.Extensions.DotNet.CompilerServices.Displays.Internals;

public partial class SyntaxTextSpanDisplay : ComponentBase
{
	[Inject]
	private TextEditorService TextEditorService { get; set; } = null!;

	[Parameter, EditorRequired]
	public TextEditorTextSpan TextSpan { get; set; } = default!;

	private (TextEditorTextSpan TextEditorTextSpan, string GetTextResult) _textSpanTuple;

	private string InputValue { get; set; } = string.Empty;

	protected override void OnParametersSet()
	{
		if (_textSpanTuple.TextEditorTextSpan != TextSpan)
		{
			_textSpanTuple = (TextSpan, TextSpan.Text);
			InputValue = _textSpanTuple.GetTextResult;
		}

		base.OnParametersSet();
	}

	private void HandleOnKeyDown(KeyboardEventArgs keyboardEventArgs)
	{
		if (keyboardEventArgs.Code == KeyboardKeyFacts.WhitespaceCodes.ENTER_CODE)
			CompleteForm();
	}

	private void CompleteForm()
	{
		/*var localTextSpanTuple = _textSpanTuple;
		var localInputValue = InputValue;
		var model = TextEditorService.ModelApi.GetOrDefault(localTextSpanTuple.TextEditorTextSpan.ResourceUri);

		if (model is null)
			return;

		var modelText = model.GetAllText();

		TextEditorService.WorkerArbitrary.PostUnique(
			nameof(SyntaxTextSpanDisplay),
			editContext =>
			{
				var modelModifier = editContext.GetModelModifier(
					localTextSpanTuple.TextEditorTextSpan.ResourceUri);

				if (modelModifier is null ||
					modelModifier.GetAllText() != modelText)
				{
                    return ValueTask.CompletedTask;
                }

				var rowInfo = modelModifier.GetLineInformationFromPositionIndex(
					localTextSpanTuple.TextEditorTextSpan.StartInclusiveIndex);

				var columnIndex = localTextSpanTuple.TextEditorTextSpan.StartInclusiveIndex -
					rowInfo.Position_StartInclusiveIndex;

				var cursor = new TextEditorCursor(
					rowInfo.Index,
					columnIndex,
					columnIndex,
					true,
					new TextEditorSelection(
						localTextSpanTuple.TextEditorTextSpan.StartInclusiveIndex,
						localTextSpanTuple.TextEditorTextSpan.EndExclusiveIndex));

				var cursorModifierBag = new CursorModifierBagTextEditor(
					Key<TextEditorViewModel>.Empty,
					new(cursor));

				TextEditorService.ModelApi.InsertTextUnsafe(
					editContext,
			        modelModifier,
			        cursorModifierBag,
			        localInputValue);

				modelModifier.PersistentState.CompilerService.ResourceWasModified(
					_textSpanTuple.TextEditorTextSpan.ResourceUri,
					Array.Empty<TextEditorTextSpan>());
                return ValueTask.CompletedTask;
            });*/
	}
}