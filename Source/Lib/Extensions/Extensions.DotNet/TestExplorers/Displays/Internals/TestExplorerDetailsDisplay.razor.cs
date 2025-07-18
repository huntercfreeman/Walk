using System.Text;
using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.Ide.RazorLib;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.Extensions.DotNet.TestExplorers.Models;

namespace Walk.Extensions.DotNet.TestExplorers.Displays.Internals;

public partial class TestExplorerDetailsDisplay : ComponentBase, IDisposable
{
	[Inject]
	private IdeService IdeService { get; set; } = null!;

	[CascadingParameter]
	public TestExplorerRenderBatchValidated RenderBatch { get; set; } = null!;

	[Parameter, EditorRequired]
	public ElementDimensions ElementDimensions { get; set; } = null!;

	public static readonly Key<TextEditorViewModel> DetailsTextEditorViewModelKey = Key<TextEditorViewModel>.NewKey();

	private string? _previousContent = string.Empty;
	private Throttle _updateContentThrottle = new Throttle(TimeSpan.FromMilliseconds(333));
	
	private ITerminal _executionTerminal;
	
	private ViewModelDisplayOptions _textEditorViewModelDisplayOptions = new()
	{
		HeaderComponentType = null,
		FooterComponentType = null,
		IncludeGutterComponent = false,
		ContextRecord = ContextFacts.TerminalContext,
	};
	
	protected override void OnInitialized()
	{
		var terminalState = IdeService.GetTerminalState();
		_executionTerminal = terminalState.TerminalMap[TerminalFacts.EXECUTION_KEY];
		_executionTerminal.TerminalOutput.OnWriteOutput += OnWriteOutput;
	}

	protected override void OnParametersSet()
	{
		_updateContentThrottle.Run(_ => UpdateContent());
		base.OnParametersSet();
	}

	private async Task UpdateContent()
	{
		var newContent = string.Empty;
		var newDecorationTextSpanList = new List<TextEditorTextSpan>();
		var textOffset = 0;

		if (RenderBatch.TreeViewContainer.SelectedNodeList.Count > 1)
		{
			var newContentBuilder = new StringBuilder();

			for (var i = 0; i < RenderBatch.TreeViewContainer.SelectedNodeList.Count; i++)
			{
				var node = RenderBatch.TreeViewContainer.SelectedNodeList[i];

				var subContent = await GetNodeContent(node, newDecorationTextSpanList, textOffset);
				textOffset += subContent.Length;

				newContentBuilder.Append(subContent);

				if (i != RenderBatch.TreeViewContainer.SelectedNodeList.Count - 1)
				{
					var spacingBetweenEntries = "\n\n==================================================\n\n";
					newContentBuilder.Append(spacingBetweenEntries);

					// Decoration text span
					{
						var startPositionInclusive = textOffset;
						var endPositionExclusive = textOffset + spacingBetweenEntries.Length;

						// TODO: Bad idea to use string.Empty here as the source text for the text span...
						//       ...If one invokes '.GetText()' this will throw and index out of bounds exception.
						//       |
						//       The source text is determined after all the nodes have been handled however,
						//       and therefore this string.Empty hack is here for now.
						newDecorationTextSpanList.Add(new TextEditorTextSpan(
							startPositionInclusive,
							endPositionExclusive,
							(byte)TerminalDecorationKind.Comment));
					}

					textOffset += spacingBetweenEntries.Length;
				}
			}

			newContent = newContentBuilder.ToString();
		}
		else
		{
			newContent = await GetNodeContent(
				RenderBatch.TreeViewContainer.SelectedNodeList.Single(),
				newDecorationTextSpanList,
				textOffset);
		}

		if (newContent != _previousContent)
		{
			_previousContent = newContent;

			for (int i = 0; i < newDecorationTextSpanList.Count; i++)
			{
				var textSpan = newDecorationTextSpanList[i];
				
				// 2025-06-01
				/*newDecorationTextSpanList[i] = textSpan with
				{
					SourceText = newContent
				};*/
			}

			IdeService.TextEditorService.WorkerArbitrary.PostUnique(editContext =>
			{
				var modelModifier = editContext.GetModelModifier(ResourceUriFacts.TestExplorerDetailsTextEditorResourceUri);
				var viewModelModifier = editContext.GetViewModelModifier(DetailsTextEditorViewModelKey);

				if (modelModifier is null || viewModelModifier is null)
					return ValueTask.CompletedTask;
				
				var showingFinalLine = false;
				
				if (viewModelModifier.Virtualization.Count > 0)
				{
					var last = viewModelModifier.Virtualization.EntryList[viewModelModifier.Virtualization.Count - 1];
					if (last.LineIndex == modelModifier.LineCount - 1)
						showingFinalLine = true;
				}

				modelModifier.SetContent(newContent ?? string.Empty);
				
				var lineIndexOriginal = viewModelModifier.LineIndex;
				var columnIndexOriginal = viewModelModifier.ColumnIndex;
				
				// Move Cursor, try to preserve the current cursor position.
				{
					if (viewModelModifier.LineIndex > modelModifier.LineCount - 1)
						viewModelModifier.LineIndex = modelModifier.LineCount - 1;
					
					var lineInformation = modelModifier.GetLineInformation(viewModelModifier.LineIndex);
					
					if (viewModelModifier.ColumnIndex > lineInformation.LastValidColumnIndex)
						viewModelModifier.SetColumnIndexAndPreferred(lineInformation.LastValidColumnIndex);
				}
				
				if (showingFinalLine)
				{
					var lineInformation = modelModifier.GetLineInformation(modelModifier.LineCount - 1);
					
					var originalScrollLeft = viewModelModifier.PersistentState.ScrollLeft;
					
					var textSpan = new TextEditorTextSpan(
					    StartInclusiveIndex: lineInformation.Position_StartInclusiveIndex,
					    EndExclusiveIndex: lineInformation.Position_StartInclusiveIndex + 1,
					    DecorationByte: 0);
				
					IdeService.TextEditorService.ViewModel_ScrollIntoView(
				        editContext,
				        modelModifier,
				        viewModelModifier,
				        textSpan);
				        
			        viewModelModifier.SetScrollLeft(
			        	(int)originalScrollLeft,
			        	viewModelModifier.PersistentState.TextEditorDimensions);
				}
				else if (lineIndexOriginal != viewModelModifier.LineIndex ||
					     columnIndexOriginal != viewModelModifier.ColumnIndex)
				{
					viewModelModifier.PersistentState.ShouldRevealCursor = true;
				}

				var compilerServiceResource = modelModifier.PersistentState.CompilerService.GetResource(
					ResourceUriFacts.TestExplorerDetailsTextEditorResourceUri);

				if (compilerServiceResource is TerminalResource terminalResource)
				{
					terminalResource.CompilationUnit.ManualDecorationTextSpanList.Clear();
					terminalResource.CompilationUnit.ManualDecorationTextSpanList.AddRange(newDecorationTextSpanList);

					editContext.TextEditorService.Model_ApplySyntaxHighlighting(
						editContext,
						modelModifier);
				}

				return ValueTask.CompletedTask;
			});
		}
	}

	/// <param name="newDecorationTextSpanList">
	/// The list in which to add any decoration <see cref="TextEditorTextSpan"/>(s).
	/// </param>
	private Task<string> GetNodeContent(
		TreeViewNoType node,
		List<TextEditorTextSpan> newDecorationTextSpanList,
		int textOffset)
	{
		var newContent = string.Empty;

		if (node is TreeViewStringFragment treeViewStringFragment)
		{
			var terminalCommand = treeViewStringFragment.Item.TerminalCommandRequest;

			/*
			//// (2024-08-02)
			//// =================
			if (terminalCommand is not null)
			{
				terminalCommand.StateChangedCallbackFunc = () =>
				{
					TreeViewService.ReRenderNode(TestExplorerState.TreeViewTestExplorerKey, treeViewStringFragment);
					return Task.CompletedTask;
				};
			}
			*/

			// Decoration text span
			{
				var startPositionInclusive = textOffset;
				var endPositionExclusive = textOffset + treeViewStringFragment.Item.Value.Length;

				// TODO: Bad idea to use string.Empty here as the source text for the text span...
				//       ...If one invokes '.GetText()' this will throw and index out of bounds exception.
				//       |
				//       The source text is determined after all the nodes have been handled however,
				//       and therefore this string.Empty hack is here for now.
				newDecorationTextSpanList.Add(new TextEditorTextSpan(
					startPositionInclusive,
					endPositionExclusive,
					(byte)TerminalDecorationKind.Keyword));
			}

			newContent = $"{treeViewStringFragment.Item.Value}:\n";
			
			//// (2024-08-02)
			//// =================
			if (treeViewStringFragment.Item.TerminalCommandParsed is not null)
				newContent += treeViewStringFragment.Item.TerminalCommandParsed.OutputCache.ToString();
			else
				newContent += "TerminalCommand was null";
		}
		else if (node is TreeViewProjectTestModel treeViewProjectTestModel)
		{
			var terminalCommand = treeViewProjectTestModel.Item.TerminalCommandRequest;

			if (terminalCommand is not null)
			{
				/*
				//// (2024-08-02)
				//// =================
				terminalCommand.StateChangedCallbackFunc = () =>
				{
					TreeViewService.ReRenderNode(TestExplorerState.TreeViewTestExplorerKey, treeViewProjectTestModel);
					return Task.CompletedTask;
				};
				*/
			}

			// Decoration text span
			{
				var startPositionInclusive = textOffset;
				var endPositionExclusive = textOffset + treeViewProjectTestModel.Item.AbsolutePath.NameWithExtension.Length;

				// TODO: Bad idea to use string.Empty here as the source text for the text span...
				//       ...If one invokes '.GetText()' this will throw and index out of bounds exception.
				//       |
				//       The source text is determined after all the nodes have been handled however,
				//       and therefore this string.Empty hack is here for now.
				newDecorationTextSpanList.Add(new TextEditorTextSpan(
					startPositionInclusive,
					endPositionExclusive,
					(byte)TerminalDecorationKind.Keyword));
			}

			newContent = $"{treeViewProjectTestModel.Item.AbsolutePath.NameWithExtension}:\n";

			//// (2024-08-02)
			//// =================
			if (treeViewProjectTestModel.Item.TerminalCommandParsed is not null)
				newContent += treeViewProjectTestModel.Item.TerminalCommandParsed.OutputCache.ToString();
			else
				newContent += "terminalCommand was null";
		}
		else if (node is not null)
		{
			newContent = node.GetType().Name;
		}
		else
		{
			newContent = "singularNode was null";
		}

		return Task.FromResult(newContent ?? string.Empty);
	}
	
	private async void OnWriteOutput()
	{
		_updateContentThrottle.Run(_ => UpdateContent());
	}

	public void Dispose()
	{
		_executionTerminal.TerminalOutput.OnWriteOutput -= OnWriteOutput;
	}
}