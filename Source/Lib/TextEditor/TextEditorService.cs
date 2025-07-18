using System.Text;
using Microsoft.JSInterop;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.JsRuntimes.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib.Lines.Models;
using Walk.TextEditor.RazorLib.Diffs.Models;
using Walk.TextEditor.RazorLib.FindAlls.Models;
using Walk.TextEditor.RazorLib.Groups.Models;
using Walk.TextEditor.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.Edits.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.TextEditor.RazorLib.Installations.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.JsRuntimes.Models;
using Walk.TextEditor.RazorLib.BackgroundTasks.Models;
using Walk.TextEditor.RazorLib.ComponentRenderers.Models;
/* Start ModelApi */
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Keymaps.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.Lines.Models;
/*namespace*/ using Walk.TextEditor.RazorLib.TextEditors.Models;
/* End ModelApi */
/* Start ViewModelApi */
using System.Diagnostics;
using System.Text;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.Keyboards.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib.Characters.Models;
using Walk.TextEditor.RazorLib.Cursors.Models;
using Walk.TextEditor.RazorLib.Exceptions;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;
/*namespace*/ using Walk.TextEditor.RazorLib.TextEditors.Models;
/* End ViewModelApi */
/* Start GroupApi */
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
/*namespace*/ using Walk.TextEditor.RazorLib.Groups.Models;
/* End GroupApi */
/* Start DiffApi */
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
/*namespace*/ using Walk.TextEditor.RazorLib.Diffs.Models;
/* End DiffApi */
/* Start OptionsApi */
using System.Text.Json;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.RenderStates.Models;
using Walk.Common.RazorLib.Themes.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib.Installations.Models;
using Walk.TextEditor.RazorLib.Keymaps.Models;
using Walk.TextEditor.RazorLib.JavaScriptObjects.Models;
using Walk.Common.RazorLib.BackgroundTasks.Models;
/*namespace*/ using Walk.TextEditor.RazorLib.Options.Models;
/* End OptionsApi */

namespace Walk.TextEditor.RazorLib;

public sealed class TextEditorService
{
    private readonly IDirtyResourceUriService _dirtyResourceUriService;
    private readonly ITextEditorRegistryWrap _textEditorRegistryWrap;
    private readonly IJSRuntime _jsRuntime;
    private readonly IServiceProvider _serviceProvider;

    public TextEditorService(
        WalkTextEditorConfig textEditorConfig,
        IWalkTextEditorComponentRenderers textEditorComponentRenderers,
        IFindAllService findAllService,
        IDirtyResourceUriService dirtyResourceUriService,
        ITextEditorRegistryWrap textEditorRegistryWrap,
        IJSRuntime jsRuntime,
        CommonUtilityService commonUtilityService,
		IServiceProvider serviceProvider)
    {
    	__TextEditorViewModelLiason = new(this);
    
    	WorkerUi = new(this);
    	WorkerArbitrary = new(this);
    
		CommonUtilityService = commonUtilityService;
		
		PostScrollAndRemeasure_DebounceExtraEvent = new(
        	TimeSpan.FromMilliseconds(250),
        	CancellationToken.None,
        	(_, _) =>
        	{
        	    CommonUtilityService.AppDimension_NotifyIntraAppResize(useExtraEvent: false);
        	    return Task.CompletedTask;
    	    });
		
		_serviceProvider = serviceProvider;

        FindAllService = findAllService;
        _dirtyResourceUriService = dirtyResourceUriService;
        TextEditorConfig = textEditorConfig;
        TextEditorComponentRenderers = textEditorComponentRenderers;
        _textEditorRegistryWrap = textEditorRegistryWrap;
        _jsRuntime = jsRuntime;
		JsRuntimeTextEditorApi = _jsRuntime.GetWalkTextEditorApi();

        ModelApi = new TextEditorModelApi(this, _textEditorRegistryWrap, CommonUtilityService);
        ViewModelApi = new TextEditorViewModelApi(this, CommonUtilityService);
        GroupApi = new TextEditorGroupApi(this, CommonUtilityService);
        DiffApi = new TextEditorDiffApi(this);
        OptionsApi = new TextEditorOptionsApi(this, CommonUtilityService);
        
        TextEditorState = new();
    }

    public CommonUtilityService CommonUtilityService { get; }
    public IFindAllService FindAllService { get; }

	public WalkTextEditorJavaScriptInteropApi JsRuntimeTextEditorApi { get; }
	public WalkCommonJavaScriptInteropApi JsRuntimeCommonApi => CommonUtilityService.JsRuntimeCommonApi;
	public WalkTextEditorConfig TextEditorConfig { get; }
	public IWalkTextEditorComponentRenderers TextEditorComponentRenderers { get; }

#if DEBUG
    public string StorageKey => "di_te_text-editor-options-debug";
#else
    public string StorageKey => "di_te_text-editor-options";
#endif

    public string ThemeCssClassString { get; set; }
    
    public TextEditorState TextEditorState { get; }
    
    public TextEditorWorkerUi WorkerUi { get; }
	public TextEditorWorkerArbitrary WorkerArbitrary { get; }
	
	public object IdeBackgroundTaskApi { get; set; }
    
    /// <summary>
	/// Do not touch this property, it is used for the VirtualizationResult.
	/// </summary>
	public StringBuilder __StringBuilder { get; } = new StringBuilder();
	
	/// <summary>
	/// Do not touch this property, it is used for the ICompilerService implementations.
	/// </summary>
	public StringWalker __StringWalker { get; } = new StringWalker();
	
    /// <summary>
	/// Do not touch this property, it is used for the TextEditorEditContext.
	/// </summary>
    public Dictionary<Key<TextEditorDiffModel>, TextEditorDiffModelModifier?> __DiffModelCache { get; } = new();
 
	/// <summary>
	/// Do not touch this property, it is used for the TextEditorEditContext.
	/// </summary>
	public List<TextEditorModel> __ModelList { get; } = new();   
    /// <summary>
	/// Do not touch this property, it is used for the TextEditorEditContext.
	/// </summary>
    public List<TextEditorViewModel> __ViewModelList { get; } = new();
    
    /// <summary>
	/// Do not touch this property, it is used for the 'TextEditorModel.InsertMetadata(...)' method.
	/// </summary>
    public List<LineEnd> __LocalLineEndList { get; } = new();
    /// <summary>
	/// Do not touch this property, it is used for the 'TextEditorModel.InsertMetadata(...)' method.
	/// </summary>
    public List<int> __LocalTabPositionList { get; } = new();
    /// <summary>
	/// Do not touch this property, it is used for the 'TextEditorModel.InsertMetadata(...)' method.
	/// </summary>
    public TextEditorViewModelLiason __TextEditorViewModelLiason { get; }
    
    /// <summary>
	/// Do not touch this property, it is used for the 'TextEditorVirtualizationResult.PresentationVirtualizeAndShiftTextSpans(...)' method.
	/// </summary>
    public List<TextEditorTextSpan> __VirtualizedTextSpanList { get; set; } = new();
    /// <summary>
	/// Do not touch this property, it is used for the 'TextEditorVirtualizationResult.PresentationVirtualizeAndShiftTextSpans(...)' method.
	/// </summary>
    public List<TextEditorTextSpan> __OutTextSpansList { get; set; } = new();
    
    public int SeenTabWidth { get; set; }
    public string TabKeyOutput_ShowWhitespaceTrue { get; set; }
    public string TabKeyOutput_ShowWhitespaceFalse { get; set; }
    
    public int TabKeyBehavior_SeenTabWidth { get; set; }
	public string TabKeyBehavior_TabSpaces { get; set; }
	
    public event Action? TextEditorStateChanged;
    
    private readonly Dictionary<int, List<string>> _stringMap = new();
	
	// private int _listCount;
	// private int _collisionCount;
	// private int _stringAllocationCount;
	
	/// <summary>
    /// To avoid unexpected HTML movements when responding to a PostScrollAndRemeasure(...)
    /// this debounce will add 1 extra event after everything has "settled".
    ///
    /// `byte` is just a throwaway generic type, it isn't used.
    /// </summary>
    public Debounce<byte> PostScrollAndRemeasure_DebounceExtraEvent { get; }
	
	/// <summary>
	/// This is only safe to use if you're in a TextEditorEditContext.
	/// (i.e.: there is an instance of TextEditorEditContext in scope)
	/// </summary>
	public string EditContext_GetText(ReadOnlySpan<char> span)
	{
		var key = 0;
		
		for (int i = 0; i < span.Length && i < 11; i++)
		{
			key += (int)span[i];
		}
		
		if (_stringMap.TryGetValue(key, out var stringList))
		{
			foreach (var stringValue in stringList)
			{
				if (span.SequenceEqual(stringValue))
					return stringValue;
			}
			
			var str = span.ToString();
			// Console.Write("_stringAllocationCount_");
			// Console.WriteLine(++_stringAllocationCount);
			// Console.Write("_collisionCount_");
			// Console.WriteLine(++_collisionCount);
			stringList.Add(str);
			return str;
		}
		else
		{
			var str = span.ToString();
			// Console.Write("_stringAllocationCount_");
			// Console.WriteLine(++_stringAllocationCount);
			// Console.Write("_listCount_");
			// Console.WriteLine(++_listCount);
			_stringMap.Add(key, new List<string> { str });
			return str;
		}
	}

	public void EditContext_GetText_Clear()
	{
		_stringMap.Clear();
	}

	public void InsertTab(TextEditorEditContext editContext, TextEditorModel modelModifier, TextEditorViewModel viewModel)
	{
	    if (OptionsApi.GetOptions().TabKeyBehavior)
		{
        	modelModifier.Insert(
                "\t",
                viewModel);
        }
        else
        {
        	if (TabKeyBehavior_SeenTabWidth != OptionsApi.GetOptions().TabWidth)
        	{
        	    TabKeyBehavior_SeenTabWidth = OptionsApi.GetOptions().TabWidth;
        	    TabKeyBehavior_TabSpaces = new string(' ', TabKeyBehavior_SeenTabWidth);
        	}
        	modelModifier.Insert(
                TabKeyBehavior_TabSpaces,
                viewModel);
        }
	}

	public async ValueTask FinalizePost(TextEditorEditContext editContext)
	{
        for (int modelIndex = 0; modelIndex < __ModelList.Count; modelIndex++)
        {
        	var modelModifier = __ModelList[modelIndex];
        	
            for (int viewModelIndex = 0; viewModelIndex < modelModifier.PersistentState.ViewModelKeyList.Count; viewModelIndex++)
            {
                // Invoking 'GetViewModelModifier' marks the view model to be updated.
                var viewModelModifier = editContext.GetViewModelModifier(modelModifier.PersistentState.ViewModelKeyList[viewModelIndex]);

				if (!viewModelModifier.Virtualization.ShouldCalculateVirtualizationResult)
					viewModelModifier.Virtualization.ShouldCalculateVirtualizationResult = modelModifier.ShouldCalculateVirtualizationResult;
            }

            if (modelModifier.WasDirty != modelModifier.IsDirty)
            {
            	var model = ModelApi.GetOrDefault(modelModifier.PersistentState.ResourceUri);
            	model.IsDirty = modelModifier.IsDirty;
            
                if (modelModifier.IsDirty)
                    _dirtyResourceUriService.AddDirtyResourceUri(modelModifier.PersistentState.ResourceUri);
                else
                    _dirtyResourceUriService.RemoveDirtyResourceUri(modelModifier.PersistentState.ResourceUri);
            }
            
			TextEditorState._modelMap[modelModifier.PersistentState.ResourceUri] = modelModifier;
        }
		
        for (int viewModelIndex = 0; viewModelIndex < __ViewModelList.Count; viewModelIndex++)
        {
        	var viewModelModifier = __ViewModelList[viewModelIndex];
        
        	TextEditorModel? modelModifier = null;
        	if (viewModelModifier.PersistentState.ShouldRevealCursor || viewModelModifier.Virtualization.ShouldCalculateVirtualizationResult || viewModelModifier.ScrollWasModified || viewModelModifier.PersistentState.Changed_Cursor_AnyState)
        		modelModifier = editContext.GetModelModifier(viewModelModifier.PersistentState.ResourceUri, isReadOnly: true);
        
        	if (viewModelModifier.PersistentState.ShouldRevealCursor)
            {
        		ViewModelApi.RevealCursor(
            		editContext,
			        modelModifier,
			        viewModelModifier);
            }
            
            // This if expression exists below, to check if 'CalculateVirtualizationResult(...)' should be invoked.
            //
            // But, note that these cannot be combined at the bottom, we need to check if an edit
            // reduced the scrollWidth or scrollHeight of the editor's content.
            // 
            // This is done here, so that the 'ScrollWasModified' bool can be set, and downstream if statements will be entered,
            // which go on to scroll the editor.
            if (viewModelModifier.Virtualization.ShouldCalculateVirtualizationResult)
			{
				ValidateMaximumScrollLeftAndScrollTop(editContext, modelModifier, viewModelModifier, textEditorDimensionsChanged: false);
			}

            if (!viewModelModifier.Virtualization.ShouldCalculateVirtualizationResult &&
            	viewModelModifier.ScrollWasModified)
            {
            	// If not already going to reload virtualization result,
            	// then check if the virtualization needs to be refreshed due to a
            	// change in scroll position.
            	//
            	// This code only needs to run if the scrollbar was modified.
            	
            	if (viewModelModifier.Virtualization.Count > 0)
            	{
            		if (viewModelModifier.PersistentState.ScrollTop < viewModelModifier.Virtualization.VirtualTop)
            		{
            			viewModelModifier.Virtualization.ShouldCalculateVirtualizationResult = true;
            		}
            		else
            		{
            			var bigTop = viewModelModifier.PersistentState.ScrollTop + viewModelModifier.PersistentState.TextEditorDimensions.Height;
            			var virtualEnd = viewModelModifier.Virtualization.VirtualTop + viewModelModifier.Virtualization.VirtualHeight;
            				
            			if (bigTop > virtualEnd)
            				viewModelModifier.Virtualization.ShouldCalculateVirtualizationResult = true;
            		}
            	}
            	
            	// A check for horizontal virtualization still needs to be done.
            	//
            	// If we didn't already determine the necessity of calculating the virtualization
            	// result when checking the vertical virtualization, then we check horizontal.
            	if (!viewModelModifier.Virtualization.ShouldCalculateVirtualizationResult)
            	{
            		var scrollLeft = viewModelModifier.PersistentState.ScrollLeft;
            		if (scrollLeft < (viewModelModifier.Virtualization.VirtualLeft))
            		{
            			viewModelModifier.Virtualization.ShouldCalculateVirtualizationResult = true;
            		}
            		else
            		{
						var bigLeft = scrollLeft + viewModelModifier.PersistentState.TextEditorDimensions.Width;
            			if (bigLeft > viewModelModifier.Virtualization.VirtualLeft + viewModelModifier.Virtualization.VirtualWidth)
            				viewModelModifier.Virtualization.ShouldCalculateVirtualizationResult = true;
            		}
            	}
            }

			if (viewModelModifier.Virtualization.ShouldCalculateVirtualizationResult)
			{
				var componentData = viewModelModifier.PersistentState.ComponentData;
				if (componentData is not null)
				{
					// TODO: This 'CalculateVirtualizationResultFactory' invocation is horrible for performance.
		            editContext.TextEditorService.ViewModelApi.CalculateVirtualizationResult(
		            	editContext,
		            	modelModifier,
				        viewModelModifier,
				        componentData);
				}
			}
			else if (viewModelModifier.PersistentState.Changed_Cursor_AnyState)
			{
			    // If `CalculateVirtualizationResult` is invoked, then `CalculateCursorUi`
			    // gets invoked as part of `CalculateVirtualizationResult`.
			    //
			    // This is done to permit the `CalculateCursorUi` to use the cache even if
			    // the cursor isn't on screen.
			    //
			    // (otherwise `CalculateVirtualizationResult` would remove that line index from the cache,
			    //  since it was offscreen).
			
			    var componentData = viewModelModifier.PersistentState.ComponentData;
			    if (componentData is not null)
			    {
			        viewModelModifier.PersistentState.Changed_Cursor_AnyState = false;
			    
			        viewModelModifier.Virtualization = new TextEditorVirtualizationResult(
			            modelModifier,
			            viewModelModifier,
			            componentData,
			            viewModelModifier.Virtualization);

			        viewModelModifier.Virtualization.External_GetCursorCss();
			        componentData.Virtualization = viewModelModifier.Virtualization;
		        }
			}
			
			TextEditorState._viewModelMap[viewModelModifier.PersistentState.ViewModelKey] = viewModelModifier;
        }
	    
	    // __DiffModelCache.Clear();
	    
	    __ModelList.Clear();
		__ViewModelList.Clear();

        TextEditorStateChanged?.Invoke();
	    
	    // SetModelAndViewModelRange(editContext);
	}
	
	/// <summary>
	/// The argument 'bool textEditorDimensionsChanged' was added on (2024-09-20).
	/// 
	/// The issue is that this method was originally written for fixing the scrollLeft or scrollTop
	/// when the scrollWidth or scrollHeight changed to a smaller value.
	///
	/// The if statements therefore check that the originalScrollWidth was higher,
	/// because some invokers of this method won't need to take time doing this calculation.
	///
	/// For example, a pure insertion of text won't need to run this method. So the if statements
	/// allow for a quick exit.
	///
	/// But, it was discovered that this same logic is needed when the text editor width changes.
	///
	/// The text editor width changing results in a very specific event firing. So if we could
	/// just have a bool here to say, "I'm that specific event" then we can know that the width changed.
	/// 
	/// Because we cannot track the before and after of the width from this method since it already was changed.
	/// We need the specific event to instead tell us that it had changed.
	/// 
	/// So, the bool is a bit hacky.
	/// </summary>
	public void ValidateMaximumScrollLeftAndScrollTop(
		TextEditorEditContext editContext,
		TextEditorModel? modelModifier,
		TextEditorViewModel viewModelModifier,
		bool textEditorDimensionsChanged)
	{
    	if (modelModifier is null)
    		return;
		
		var originalScrollWidth = viewModelModifier.PersistentState.ScrollWidth;
		var originalScrollHeight = viewModelModifier.PersistentState.ScrollHeight;
		var tabWidth = editContext.TextEditorService.OptionsApi.GetOptions().TabWidth;
	
		var totalWidth = (int)Math.Ceiling(modelModifier.MostCharactersOnASingleLineTuple.lineLength *
			viewModelModifier.PersistentState.CharAndLineMeasurements.CharacterWidth);

		// Account for any tab characters on the 'MostCharactersOnASingleLineTuple'
		//
		// TODO: This code is not fully correct...
		//       ...if the longest line is 50 non-tab characters,
		//       and the second longest line is 49 tab characters,
		//       this code will erroneously take the '50' non-tab characters
		//       to be the longest line.
		{
			var lineIndex = modelModifier.MostCharactersOnASingleLineTuple.lineIndex;
			var longestLineInformation = modelModifier.GetLineInformation(lineIndex);

			var tabCountOnLongestLine = modelModifier.GetTabCountOnSameLineBeforeCursor(
				longestLineInformation.Index,
				longestLineInformation.LastValidColumnIndex);

			// 1 of the character width is already accounted for
			var extraWidthPerTabKey = tabWidth - 1;

			totalWidth += (int)Math.Ceiling(extraWidthPerTabKey *
				tabCountOnLongestLine *
				viewModelModifier.PersistentState.CharAndLineMeasurements.CharacterWidth);
		}

		var totalHeight = (modelModifier.LineEndList.Count - viewModelModifier.PersistentState.HiddenLineIndexHashSet.Count) *
			viewModelModifier.PersistentState.CharAndLineMeasurements.LineHeight;

		// Add vertical margin so the user can scroll beyond the final line of content
		int marginScrollHeight;
		{
			var percentOfMarginScrollHeightByPageUnit = 0.4;

			marginScrollHeight = (int)Math.Ceiling(viewModelModifier.PersistentState.TextEditorDimensions.Height * percentOfMarginScrollHeightByPageUnit);
			totalHeight += marginScrollHeight;
		}

		viewModelModifier.PersistentState.ScrollWidth = totalWidth;
		viewModelModifier.PersistentState.ScrollHeight = totalHeight;
		viewModelModifier.PersistentState.MarginScrollHeight = marginScrollHeight;
		
		// var validateScrollWidth = totalWidth;
		// var validateScrollHeight = totalHeight;
		// var validateMarginScrollHeight = marginScrollHeight;
		
		if (originalScrollWidth > viewModelModifier.PersistentState.ScrollWidth ||
			textEditorDimensionsChanged)
		{
			viewModelModifier.SetScrollLeft(
				(int)viewModelModifier.PersistentState.ScrollLeft,
				viewModelModifier.PersistentState.TextEditorDimensions);
		}
		
		if (originalScrollHeight > viewModelModifier.PersistentState.ScrollHeight ||
			textEditorDimensionsChanged)
		{
			viewModelModifier.SetScrollTop(
				(int)viewModelModifier.PersistentState.ScrollTop,
				viewModelModifier.PersistentState.TextEditorDimensions);
			
			// The scrollLeft currently does not have any margin. Therefore subtracting the margin isn't needed.
			//
			// For scrollTop however, if one does not subtract the MarginScrollHeight in the case of
			// 'textEditorDimensionsChanged'
			//
			// Then a "void" will render at the top portion of the text editor, seemingly the size
			// of the MarginScrollHeight.
			if (textEditorDimensionsChanged &&
				viewModelModifier.PersistentState.ScrollTop != viewModelModifier.PersistentState.ScrollTop) // TODO: Why are these comparing eachother?
			{
				viewModelModifier.SetScrollTop(
					(int)viewModelModifier.PersistentState.ScrollTop - (int)viewModelModifier.PersistentState.MarginScrollHeight,
					viewModelModifier.PersistentState.TextEditorDimensions);
			}
		}
		
		var changeOccurred =
			viewModelModifier.PersistentState.ScrollLeft != viewModelModifier.PersistentState.ScrollLeft || // TODO: Why are these comparing eachother?
			viewModelModifier.PersistentState.ScrollTop != viewModelModifier.PersistentState.ScrollTop; // TODO: Why are these comparing eachother?
		
		if (changeOccurred)
		{
			viewModelModifier.ScrollWasModified = true;
		}
	}
	
	public async Task OpenInEditorAsync(
		TextEditorEditContext editContext,
		string absolutePath,
		bool shouldSetFocusToEditor,
		int? cursorPositionIndex,
		Category category,
		Key<TextEditorViewModel> preferredViewModelKey)
	{
		var resourceUri = new ResourceUri(absolutePath);
		var actualViewModelKey = await CommonLogic_OpenInEditorAsync(
			editContext,
			resourceUri,
			shouldSetFocusToEditor,
			category,
			preferredViewModelKey);
		
		var modelModifier = editContext.GetModelModifier(resourceUri);
		var viewModelModifier = editContext.GetViewModelModifier(actualViewModelKey);
		if (modelModifier is null || viewModelModifier is null)
			return;
			
		if (cursorPositionIndex is not null)
		{
		    var lineAndColumnIndices = modelModifier.GetLineAndColumnIndicesFromPositionIndex(cursorPositionIndex.Value);
			viewModelModifier.LineIndex = lineAndColumnIndices.lineIndex;
			viewModelModifier.ColumnIndex = lineAndColumnIndices.columnIndex;
		}
		
		viewModelModifier.PersistentState.ShouldRevealCursor = true;
		FireAndForgetTask_OpenInTextEditor(actualViewModelKey, shouldSetFocusToEditor);
	}
	
	public async Task OpenInEditorAsync(
		TextEditorEditContext editContext,
		string absolutePath,
		bool shouldSetFocusToEditor,
		int? lineIndex,
		int? columnIndex,
		Category category,
		Key<TextEditorViewModel> preferredViewModelKey)
	{
		// Standardize Resource Uri
		if (TextEditorConfig.AbsolutePathStandardizeFunc is null)
			return;
			
		var standardizedFilePathString = await TextEditorConfig.AbsolutePathStandardizeFunc
			.Invoke(absolutePath, CommonUtilityService)
			.ConfigureAwait(false);
			
		var resourceUri = new ResourceUri(standardizedFilePathString);

		var actualViewModelKey = await CommonLogic_OpenInEditorAsync(
			editContext,
			resourceUri,
			shouldSetFocusToEditor,
			category,
			preferredViewModelKey);
			
		var modelModifier = editContext.GetModelModifier(resourceUri);
		var viewModelModifier = editContext.GetViewModelModifier(actualViewModelKey);

		if (modelModifier is null || viewModelModifier is null)
			return;
	    
		if (lineIndex is not null)
			viewModelModifier.LineIndex = lineIndex.Value;
		if (columnIndex is not null)
			viewModelModifier.ColumnIndex = columnIndex.Value;
		
		if (viewModelModifier.LineIndex > modelModifier.LineCount - 1)
			viewModelModifier.LineIndex = modelModifier.LineCount - 1;
		
		var lineInformation = modelModifier.GetLineInformation(viewModelModifier.LineIndex);
		
		if (viewModelModifier.ColumnIndex > lineInformation.LastValidColumnIndex)
			viewModelModifier.SetColumnIndexAndPreferred(lineInformation.LastValidColumnIndex);
			
		viewModelModifier.PersistentState.ShouldRevealCursor = true;
		FireAndForgetTask_OpenInTextEditor(actualViewModelKey, shouldSetFocusToEditor);
	}
	
	private void FireAndForgetTask_OpenInTextEditor(Key<TextEditorViewModel> actualViewModelKey, bool shouldSetFocusToEditor)
	{
	    _ = Task.Run(async () =>
		{
			await Task.Delay(150).ConfigureAwait(false);
			WorkerArbitrary.PostUnique(editContext =>
			{
				var viewModelModifier = editContext.GetViewModelModifier(actualViewModelKey);
				viewModelModifier.PersistentState.ShouldRevealCursor = true;
				
				if (shouldSetFocusToEditor)
				    return viewModelModifier.FocusAsync();
				return ValueTask.CompletedTask;
			});
		});
	}
	
	/// <summary>
	/// Returns Key<TextEditorViewModel>.Empty if it failed to open in editor.
	/// Returns the ViewModel's key (non Key<TextEditorViewModel>.Empty value) if it successfully opened in editor.
	/// </summary>
	private async Task<Key<TextEditorViewModel>> CommonLogic_OpenInEditorAsync(
		TextEditorEditContext editContext,
		ResourceUri resourceUri,
		bool shouldSetFocusToEditor,
		Category category,
		Key<TextEditorViewModel> preferredViewModelKey)
	{
	    // RegisterModelFunc
		if (TextEditorConfig.RegisterModelFunc is null)
			return Key<TextEditorViewModel>.Empty;
		await TextEditorConfig.RegisterModelFunc
			.Invoke(new RegisterModelArgs(editContext, resourceUri, CommonUtilityService, IdeBackgroundTaskApi))
			.ConfigureAwait(false);
	
		// TryRegisterViewModelFunc
		if (TextEditorConfig.TryRegisterViewModelFunc is null)
			return Key<TextEditorViewModel>.Empty;
		var actualViewModelKey = await TextEditorConfig.TryRegisterViewModelFunc
			.Invoke(new TryRegisterViewModelArgs(editContext, preferredViewModelKey, resourceUri, category, shouldSetFocusToEditor, CommonUtilityService, IdeBackgroundTaskApi))
			.ConfigureAwait(false);
	
		// TryShowViewModelFunc
		if (actualViewModelKey == Key<TextEditorViewModel>.Empty || TextEditorConfig.TryShowViewModelFunc is null)
			return Key<TextEditorViewModel>.Empty;
		await TextEditorConfig.TryShowViewModelFunc
			.Invoke(new TryShowViewModelArgs(actualViewModelKey, Key<TextEditorGroup>.Empty, shouldSetFocusToEditor, CommonUtilityService, IdeBackgroundTaskApi))
			.ConfigureAwait(false);
		
		return actualViewModelKey;
	}
	
	// Move TextEditorState.Reducer.cs here
	public void RegisterModel(TextEditorEditContext editContext, TextEditorModel model)
	{
	    var inState = TextEditorState;
	
	    var exists = inState._modelMap.TryGetValue(
	        model.PersistentState.ResourceUri,
	        out _);
	
	    if (exists)
	        return;
	
	    inState._modelMap.Add(model.PersistentState.ResourceUri, model);
	
	    TextEditorStateChanged?.Invoke();
	}

    /// <summary>
    /// WARNING/TODO: This method needs to remove from the TextEditorEditContext the removed model...
    /// ...because FinalizePost(...) writes back to the Dictionary but the key won't exist.
    ///
    /// The app doesn't have a case where this is a thing, since an edit context that solely is used to invoke DisposeModel(...)
    /// would throw a caught exception and then things just "move on".
    /// </summary>
	public void DisposeModel(TextEditorEditContext editContext, ResourceUri resourceUri)
	{
	    var inState = TextEditorState;
	
	    var exists = inState._modelMap.TryGetValue(
	        resourceUri,
	        out var model);
	
	    if (!exists)
	        return;
	        
	    foreach (var viewModelKey in model.PersistentState.ViewModelKeyList)
	    {
	        DisposeViewModel(editContext, viewModelKey);
	    }
	
	    inState._modelMap.Remove(resourceUri);
	
	    TextEditorStateChanged?.Invoke();
	}
	
	public void SetModel(
	    TextEditorEditContext editContext,
	    TextEditorModel modelModifier)
	{
		var inState = TextEditorState;

		var exists = inState._modelMap.TryGetValue(
			modelModifier.PersistentState.ResourceUri, out var inModel);

		if (!exists)
            return;

		inState._modelMap[inModel.PersistentState.ResourceUri] = modelModifier;

        TextEditorStateChanged?.Invoke();
    }
	
	/// <summary>
    /// WARNING/TODO: This method needs to remove from the TextEditorEditContext the removed viewmodel...
    /// ...because FinalizePost(...) writes back to the Dictionary but the key won't exist.
    ///
    /// The app doesn't have a case where this is a thing, since an edit context that solely is used to invoke DisposeModel(...)
    /// would throw a caught exception and then things just "move on".
    /// </summary>
	public void RegisterViewModel(TextEditorEditContext editContext, TextEditorViewModel viewModel)
	{
	    var inState = TextEditorState;
	
	    var modelExisting = inState._modelMap.TryGetValue(
	        viewModel.PersistentState.ResourceUri,
	        out var model);
	
	    if (!modelExisting)
	        return;
	
	    if (viewModel.PersistentState.ViewModelKey == Key<TextEditorViewModel>.Empty)
	        throw new InvalidOperationException($"Provided {nameof(Key<TextEditorViewModel>)} cannot be {nameof(Key<TextEditorViewModel>)}.{Key<TextEditorViewModel>.Empty}");
	
	    var viewModelExisting = inState.ViewModelGetOrDefault(viewModel.PersistentState.ViewModelKey);
	    if (viewModelExisting is not null)
	        return;
	
	    model.PersistentState.ViewModelKeyList.Add(viewModel.PersistentState.ViewModelKey);
	
	    inState._viewModelMap.Add(viewModel.PersistentState.ViewModelKey, viewModel);
	
	    TextEditorStateChanged?.Invoke();
	}
	
	public void DisposeViewModel(TextEditorEditContext editContext, Key<TextEditorViewModel> viewModelKey)
	{
	    var inState = TextEditorState;
	    
	    var viewModel = inState.ViewModelGetOrDefault(viewModelKey);
	    if (viewModel is null)
	        return;
	    
	    inState._viewModelMap.Remove(viewModel.PersistentState.ViewModelKey);
	    viewModel.Dispose();
	
	    var model = inState.ModelGetOrDefault(viewModel.PersistentState.ResourceUri);
	    if (model is not null)
	        model.PersistentState.ViewModelKeyList.Remove(viewModel.PersistentState.ViewModelKey);
	    
	    TextEditorStateChanged?.Invoke();
	}
	
	public void SetModelAndViewModelRange(TextEditorEditContext editContext)
	{
		// TextEditorState isn't currently being re-instantiated after the state is modified, so I'm going to comment out this local reference.
		// 
		// var inState = TextEditorState;

		if (__ModelList.Count > 0)
		{
			foreach (var model in __ModelList)
			{
				if (TextEditorState._modelMap.ContainsKey(model.PersistentState.ResourceUri))
					TextEditorState._modelMap[model.PersistentState.ResourceUri] = model;
			}
			
			__ModelList.Clear();
		}
		
		if (__ViewModelList.Count > 0)
		{
			foreach (var viewModel in __ViewModelList)
			{
				if (TextEditorState._viewModelMap.ContainsKey(viewModel.PersistentState.ViewModelKey))
					TextEditorState._viewModelMap[viewModel.PersistentState.ViewModelKey] = viewModel;
			}
			
			__ViewModelList.Clear();
		}

        TextEditorStateChanged?.Invoke();
    }
    
    public void Enqueue_TextEditorInitializationBackgroundTaskGroupWorkKind()
    {
    	CommonUtilityService.Continuous_EnqueueGroup(new BackgroundTask(
    		Key<IBackgroundTaskGroup>.Empty,
    		Do_WalkTextEditorInitializerOnInit));
    }

    public async ValueTask Do_WalkTextEditorInitializerOnInit()
    {
        if (TextEditorConfig.CustomThemeRecordList is not null)
            CommonUtilityService.Theme_RegisterRangeAction(TextEditorConfig.CustomThemeRecordList);

        var initialThemeRecord = CommonUtilityService.GetThemeState().ThemeList.FirstOrDefault(
            x => x.Key == TextEditorConfig.InitialThemeKey);

        if (initialThemeRecord is not null)
            OptionsApi.SetTheme(initialThemeRecord, updateStorage: false);

        await OptionsApi.SetFromLocalStorageAsync().ConfigureAwait(false);

        CommonUtilityService.RegisterContextSwitchGroup(
            new ContextSwitchGroup(
                Walk.TextEditor.RazorLib.Installations.Displays.WalkTextEditorInitializer.ContextSwitchGroupKey,
                "Text Editor",
                () =>
                {
                    var menuOptionList = new List<MenuOptionRecord>();

                    var mainGroup = GroupApi.GetGroups()
                        .FirstOrDefault(x => x.Category.Value == "main");

                    if (mainGroup is not null)
                    {
                        var viewModelList = new List<TextEditorViewModel>();

                        foreach (var viewModelKey in mainGroup.ViewModelKeyList)
                        {
                            var viewModel = ViewModelApi.GetOrDefault(viewModelKey);

                            if (viewModel is not null)
                            {
                                viewModelList.Add(viewModel);

                                var absolutePath = CommonUtilityService.EnvironmentProvider.AbsolutePathFactory(
                                    viewModel.PersistentState.ResourceUri.Value,
                                    false);

                                menuOptionList.Add(new MenuOptionRecord(
                                    absolutePath.NameWithExtension,
                                    MenuOptionKind.Other,
                                    onClickFunc: () =>
                                    {
                                    	WorkerArbitrary.PostUnique(async editContext =>
                                    	{
                                    		await OpenInEditorAsync(
                                    			editContext,
	                                            absolutePath.Value,
	                                            true,
	                                            cursorPositionIndex: null,
	                                            new Category("main"),
	                                            viewModel.PersistentState.ViewModelKey);
                                    	});
                                    	return Task.CompletedTask;
                                    }));
                            }
                        }
                    }

                    var menu = menuOptionList.Count == 0
                        ? new MenuRecord(MenuRecord.NoMenuOptionsExistList)
                        : new MenuRecord(menuOptionList);

                    return Task.FromResult(menu);
                }));

        CommonUtilityService.RegisterKeymapLayer(Walk.TextEditor.RazorLib.Keymaps.Models.Defaults.TextEditorKeymapDefaultFacts.HasSelectionLayer);
    }
    
    
    /* Start ModelApi */
    #region CREATE_METHODS
    public void Model_RegisterCustom(TextEditorEditContext editContext, TextEditorModel model)
    {
        _textEditorService.RegisterModel(editContext, model);
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
            _textEditorRegistryWrap.DecorationMapperRegistry.GetDecorationMapper(extensionNoPeriod),
            _textEditorRegistryWrap.CompilerServiceRegistry.GetCompilerService(extensionNoPeriod),
            _textEditorService);

        _textEditorService.RegisterModel(editContext, model);
    }
    #endregion

    #region READ_METHODS
    [Obsolete("TextEditorModel.PersistentState.ViewModelKeyList")]
    public List<TextEditorViewModel> Model_GetViewModelsOrEmpty(ResourceUri resourceUri)
    {
    	return _textEditorService.TextEditorState.ModelGetViewModelsOrEmpty(resourceUri);
    }

    public string? Model_GetAllText(ResourceUri resourceUri)
    {
    	return GetOrDefault(resourceUri)?.GetAllText();;
    }

    public TextEditorModel? Model_GetOrDefault(ResourceUri resourceUri)
    {
        return _textEditorService.TextEditorState.ModelGetOrDefault(
        	resourceUri);
    }

    public Dictionary<ResourceUri, TextEditorModel> Model_GetModels()
    {
        return _textEditorService.TextEditorState.ModelGetModels();
    }
    
    public int Model_GetModelsCount()
    {
    	return _textEditorService.TextEditorState.ModelGetModelsCount();
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
        TextEditorModel modelModifier)
    {
        var compilerServiceResource = modelModifier.PersistentState.CompilerService.GetResource(modelModifier.PersistentState.ResourceUri);
        if (compilerServiceResource is null)
        	return;
        
        foreach (var viewModelKey in modelModifier.PersistentState.ViewModelKeyList)
        {
        	var viewModel = editContext.GetViewModelModifier(viewModelKey);
        	
        	var componentData = viewModel.PersistentState.ComponentData;
        	if (componentData is not null)
        		componentData.LineIndexCache.IsInvalid = true;
        }

        ApplyDecorationRange(
	        editContext,
	        modelModifier,
	        compilerServiceResource.CompilationUnit?.GetTextTextSpans() ?? Array.Empty<TextEditorTextSpan>());
        
        // TODO: Why does painting reload virtualization result???
        modelModifier.ShouldCalculateVirtualizationResult = true;
    }
    #endregion

    #region DELETE_METHODS
    public void Model_Dispose(TextEditorEditContext editContext, ResourceUri resourceUri)
    {
        _textEditorService.DisposeModel(editContext, resourceUri);
    }
    #endregion
    /* End ModelApi */
    
    /* Start ViewModelApi */
    private Task ViewModel_cursorShouldBlinkTask = Task.CompletedTask;
    private CancellationTokenSource ViewModel_cursorShouldBlinkCancellationTokenSource = new();
    private TimeSpan ViewModel_blinkingCursorTaskDelay = TimeSpan.FromMilliseconds(1000);
    
    public bool ViewModel_CursorShouldBlink { get; private set; } = true;
    public event Action? ViewModel_CursorShouldBlinkChanged;
    
    private bool ViewModel_intentStopCursorBlinking = false;
    private int ViewModel_stopCursorBlinkingId = 0;
    
    /// <summary>
    /// Thread Safety: most invocations of this are from the TextEditorEditContext,...
    /// ...so a decision needs to made whether this is restricted to the TextEditorEditContext
    /// and therefore thread safe, or this isn't restricted and perhaps should have a thread safe
    /// pattern involved.
    ///
    /// Precise debounce timing: I think this implementation has an imprecise debounce delay,
    /// but that is very low importance from a triage perspective. There are more important things to work on.
    /// </summary>
    public void ViewModel_StopCursorBlinking()
    {
        if (CursorShouldBlink)
        {
            CursorShouldBlink = false;
            CursorShouldBlinkChanged?.Invoke();
        }
        
        var localId = ++_stopCursorBlinkingId;
        
        if (!_intentStopCursorBlinking)
        {
        	_intentStopCursorBlinking = true;
        	
            _cursorShouldBlinkTask = Task.Run(async () =>
            {
                while (true)
                {
                	var id = _stopCursorBlinkingId;
                
                    await Task
                        .Delay(_blinkingCursorTaskDelay)
                        .ConfigureAwait(false);
                        
                    if (id == _stopCursorBlinkingId)
                    {
	                    CursorShouldBlink = true;
	                    CursorShouldBlinkChanged?.Invoke();
                    	break;
                    }
                }
                
                _intentStopCursorBlinking = false;
            });
        }
    }

    #region CREATE_METHODS
    public void ViewModel_Register(
    	TextEditorEditContext editContext,
        Key<TextEditorViewModel> viewModelKey,
        ResourceUri resourceUri,
        Category category)
    {
	    var viewModel = new TextEditorViewModel(
			viewModelKey,
			resourceUri,
			_textEditorService,
			_commonUtilityService,
			TextEditorVirtualizationResult.ConstructEmpty(),
			new TextEditorDimensions(0, 0, 0, 0),
			scrollLeft: 0,
	    	scrollTop: 0,
		    scrollWidth: 0,
		    scrollHeight: 0,
		    marginScrollHeight: 0,
			category);
			
		_textEditorService.RegisterViewModel(editContext, viewModel);
    }
    
    public void ViewModel_Register(TextEditorEditContext editContext, TextEditorViewModel viewModel)
    {
        _textEditorService.RegisterViewModel(editContext, viewModel);
    }
    #endregion

    #region READ_METHODS
    public TextEditorViewModel? ViewModel_GetOrDefault(Key<TextEditorViewModel> viewModelKey)
    {
        return _textEditorService.TextEditorState.ViewModelGetOrDefault(
            viewModelKey);
    }

    public Dictionary<Key<TextEditorViewModel>, TextEditorViewModel> ViewModel_GetViewModels()
    {
        return _textEditorService.TextEditorState.ViewModelGetViewModels();
    }

    public TextEditorModel? ViewModel_GetModelOrDefault(Key<TextEditorViewModel> viewModelKey)
    {
        var viewModel = _textEditorService.TextEditorState.ViewModelGetOrDefault(
            viewModelKey);

        if (viewModel is null)
            return null;

        return _textEditorService.ModelApi.GetOrDefault(viewModel.PersistentState.ResourceUri);
    }

    public string? ViewModel_GetAllText(Key<TextEditorViewModel> viewModelKey)
    {
        var textEditorModel = GetModelOrDefault(viewModelKey);

        return textEditorModel is null
            ? null
            : _textEditorService.ModelApi.GetAllText(textEditorModel.PersistentState.ResourceUri);
    }

    public async ValueTask<TextEditorDimensions> ViewModel_GetTextEditorMeasurementsAsync(string elementId)
    {
        return await _textEditorService.JsRuntimeTextEditorApi
            .GetTextEditorMeasurementsInPixelsById(elementId)
            .ConfigureAwait(false);
    }
    #endregion

    #region UPDATE_METHODS
    public void ViewModel_SetScrollPositionBoth(
        TextEditorEditContext editContext,
        TextEditorViewModel viewModel,
        double scrollLeftInPixels,
        double scrollTopInPixels)
    {
    	viewModel.ScrollWasModified = true;

		viewModel.SetScrollLeft((int)Math.Floor(scrollLeftInPixels), viewModel.PersistentState.TextEditorDimensions);

		viewModel.SetScrollTop((int)Math.Floor(scrollTopInPixels), viewModel.PersistentState.TextEditorDimensions);
    }
        
    public void ViewModel_SetScrollPositionLeft(
        TextEditorEditContext editContext,
        TextEditorViewModel viewModel,
        double scrollLeftInPixels)
    {
    	viewModel.ScrollWasModified = true;

		viewModel.SetScrollLeft((int)Math.Floor(scrollLeftInPixels), viewModel.PersistentState.TextEditorDimensions);
    }
    
    public void ViewModel_SetScrollPositionTop(
        TextEditorEditContext editContext,
        TextEditorViewModel viewModel,
        double scrollTopInPixels)
    {
    	viewModel.ScrollWasModified = true;

		viewModel.SetScrollTop((int)Math.Floor(scrollTopInPixels), viewModel.PersistentState.TextEditorDimensions);
    }

    public void ViewModel_MutateScrollVerticalPosition(
        TextEditorEditContext editContext,
        TextEditorViewModel viewModel,
        double pixels)
    {
        viewModel.ScrollWasModified = true;

        viewModel.MutateScrollTop((int)Math.Ceiling(pixels), viewModel.PersistentState.TextEditorDimensions);
    }

    public void ViewModel_MutateScrollHorizontalPosition(
        TextEditorEditContext editContext,
        TextEditorViewModel viewModel,
        double pixels)
    {
        viewModel.ScrollWasModified = true;

		viewModel.MutateScrollLeft((int)Math.Ceiling(pixels), viewModel.PersistentState.TextEditorDimensions);
    }

	/// <summary>
	/// If a given scroll direction is already within view of the text span, do not scroll on that direction.
	///
	/// Measurements are in pixels.
	/// </summary>
    public void ViewModel_ScrollIntoView(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel,
        TextEditorTextSpan textSpan)
    {
        var lineInformation = modelModifier.GetLineInformationFromPositionIndex(textSpan.StartInclusiveIndex);
        var lineIndex = lineInformation.Index;
        var hiddenLineCount = 0;
        foreach (var index in viewModel.PersistentState.HiddenLineIndexHashSet)
        {
        	if (index < lineIndex)
        		hiddenLineCount++;
        }
        // Scroll start
        var targetScrollTop = (lineIndex - hiddenLineCount) * viewModel.PersistentState.CharAndLineMeasurements.LineHeight;
        bool lowerBoundInRange = viewModel.PersistentState.ScrollTop <= targetScrollTop;
        bool upperBoundInRange = targetScrollTop < (viewModel.PersistentState.TextEditorDimensions.Height + viewModel.PersistentState.ScrollTop);
        if (lowerBoundInRange && upperBoundInRange)
        {
            targetScrollTop = viewModel.PersistentState.ScrollTop;
        }
        else
        {
        	var startDistanceToTarget = Math.Abs(targetScrollTop - viewModel.PersistentState.ScrollTop);
        	var endDistanceToTarget = Math.Abs(targetScrollTop - (viewModel.PersistentState.TextEditorDimensions.Height + viewModel.PersistentState.ScrollTop));
        	
    		// Scroll end
        	if (endDistanceToTarget < startDistanceToTarget)
        	{
        		var margin = 3 * viewModel.PersistentState.CharAndLineMeasurements.LineHeight;
        		var maxMargin = viewModel.PersistentState.TextEditorDimensions.Height * .3;
        		if (margin > maxMargin)
        			margin = (int)maxMargin;
        	
        		targetScrollTop -= (viewModel.PersistentState.TextEditorDimensions.Height - margin);
        	}
        }
        
		var columnIndex = textSpan.StartInclusiveIndex - lineInformation.Position_StartInclusiveIndex;
		// Scroll start
        var targetScrollLeft = columnIndex * viewModel.PersistentState.CharAndLineMeasurements.CharacterWidth;
        lowerBoundInRange = viewModel.PersistentState.ScrollLeft <= targetScrollLeft;
        upperBoundInRange = targetScrollLeft < (viewModel.PersistentState.TextEditorDimensions.Width + viewModel.PersistentState.ScrollLeft);
        if (lowerBoundInRange && upperBoundInRange)
        {
        	targetScrollLeft = viewModel.PersistentState.ScrollLeft;
        }
        else
        {
        	var startDistanceToTarget = targetScrollLeft - viewModel.PersistentState.ScrollLeft;
        	var endDistanceToTarget = targetScrollLeft - (viewModel.PersistentState.TextEditorDimensions.Width + viewModel.PersistentState.ScrollLeft);
        	
        	// Scroll end
        	if (endDistanceToTarget < startDistanceToTarget)
        	{
        		var margin = 9 * viewModel.PersistentState.CharAndLineMeasurements.CharacterWidth;
        		var maxMargin = viewModel.PersistentState.TextEditorDimensions.Width * .3;
        		if (margin > maxMargin)
        			margin = maxMargin;
        	
        		targetScrollLeft -= (viewModel.PersistentState.TextEditorDimensions.Width - margin);
        	}
        }
            
        if (targetScrollTop == -1 && targetScrollLeft == -1)
        	return;
        
        viewModel.PersistentState.Changed_Cursor_AnyState = true;
        
        if (targetScrollTop != -1 && targetScrollLeft != -1)
        	SetScrollPositionBoth(editContext, viewModel, targetScrollLeft, targetScrollTop);
        else if (targetScrollTop != -1)
        	SetScrollPositionTop(editContext, viewModel, targetScrollTop);
    	else
        	SetScrollPositionLeft(editContext, viewModel, targetScrollLeft);
    }

    public ValueTask ViewModel_FocusPrimaryCursorAsync(string primaryCursorContentId)
    {
        return _commonUtilityService.JsRuntimeCommonApi
            .FocusHtmlElementById(primaryCursorContentId, preventScroll: true);
    }

    public void ViewModel_MoveCursor(
        string? key,
        string? code,
        bool ctrlKey,
        bool shiftKey,
        bool altKey,
		TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel)
    {
        MoveCursorUnsafe(
            key,
            code,
            ctrlKey,
            shiftKey,
            altKey,
	        editContext,
	        modelModifier,
	        viewModel);

        viewModel.PersistentState.ShouldRevealCursor = true;
    }

    public void ViewModel_MoveCursorUnsafe(
        string? key,
        string? code,
        bool ctrlKey,
        bool shiftKey,
        bool altKey,
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel)
    {
        var shouldClearSelection = false;

        if (shiftKey)
        {
            if (viewModel.SelectionAnchorPositionIndex == -1 ||
                viewModel.SelectionEndingPositionIndex == viewModel.SelectionAnchorPositionIndex)
            {
                var positionIndex = modelModifier.GetPositionIndex(
                    viewModel.LineIndex,
                    viewModel.ColumnIndex);

                viewModel.SelectionAnchorPositionIndex = positionIndex;
            }
        }
        else
        {
            shouldClearSelection = true;
        }

        int lengthOfLine = 0; // This variable is used in multiple switch cases.

        switch (key)
        {
            case KeyboardKeyFacts.MovementKeys.ARROW_LEFT:
                if (TextEditorSelectionHelper.HasSelectedText(viewModel) &&
                    !shiftKey)
                {
                    var selectionBounds = TextEditorSelectionHelper.GetSelectionBounds(viewModel);

                    var lowerLineInformation = modelModifier.GetLineInformationFromPositionIndex(
                        selectionBounds.Position_LowerInclusiveIndex);

                    viewModel.LineIndex = lowerLineInformation.Index;

                    viewModel.ColumnIndex = selectionBounds.Position_LowerInclusiveIndex -
                        lowerLineInformation.Position_StartInclusiveIndex;
                }
                else
                {
                    if (viewModel.ColumnIndex <= 0)
                    {
                        if (viewModel.LineIndex != 0)
                        {
                            viewModel.LineIndex--;

                            lengthOfLine = modelModifier.GetLineLength(viewModel.LineIndex);

                            viewModel.SetColumnIndexAndPreferred(lengthOfLine);
                        }
                        else
                        {
                            viewModel.SetColumnIndexAndPreferred(0);
                        }
                    }
                    else
                    {
                        if (ctrlKey)
                        {
                        	var columnIndexOfCharacterWithDifferingKind = modelModifier.GetColumnIndexOfCharacterWithDifferingKind(
                                viewModel.LineIndex,
                                viewModel.ColumnIndex,
                                true);

                            if (columnIndexOfCharacterWithDifferingKind == -1) // Move to start of line
                            {
                                viewModel.SetColumnIndexAndPreferred(0);
                            }
                            else
                            {
                            	if (!altKey) // Move by character kind
                            	{
                            		viewModel.SetColumnIndexAndPreferred(columnIndexOfCharacterWithDifferingKind);
                            	}
                                else // Move by camel case
                                {
                                	var positionIndex = modelModifier.GetPositionIndex(viewModel);
									var rememberStartPositionIndex = positionIndex;
									
									var minPositionIndex = columnIndexOfCharacterWithDifferingKind;
									var infiniteLoopPrediction = false;
									
									if (minPositionIndex > positionIndex)
										infiniteLoopPrediction = true;
									
									bool useCamelCaseResult = false;
									
									if (!infiniteLoopPrediction)
									{
										while (--positionIndex > minPositionIndex)
										{
											var currentRichCharacter = modelModifier.RichCharacterList[positionIndex];
											
											if (Char.IsUpper(currentRichCharacter.Value) || currentRichCharacter.Value == '_')
											{
												useCamelCaseResult = true;
												break;
											}
										}
									}
									
									if (useCamelCaseResult)
									{
										var columnDisplacement = positionIndex - rememberStartPositionIndex;
										viewModel.SetColumnIndexAndPreferred(viewModel.ColumnIndex + columnDisplacement);
									}
									else
									{
										viewModel.SetColumnIndexAndPreferred(columnIndexOfCharacterWithDifferingKind);
									}
                                }
                            }
                        }
                        else
                        {
                            viewModel.SetColumnIndexAndPreferred(viewModel.ColumnIndex - 1);
                        }
                    }
                }

                break;
            case KeyboardKeyFacts.MovementKeys.ARROW_DOWN:
                if (viewModel.LineIndex < modelModifier.LineCount - 1)
                {
                    viewModel.LineIndex++;

                    lengthOfLine = modelModifier.GetLineLength(viewModel.LineIndex);

                    viewModel.ColumnIndex = lengthOfLine < viewModel.PreferredColumnIndex
                        ? lengthOfLine
                        : viewModel.PreferredColumnIndex;
                }

                break;
            case KeyboardKeyFacts.MovementKeys.ARROW_UP:
                if (viewModel.LineIndex > 0)
                {
                    viewModel.LineIndex--;

                    lengthOfLine = modelModifier.GetLineLength(viewModel.LineIndex);

                    viewModel.ColumnIndex = lengthOfLine < viewModel.PreferredColumnIndex
                        ? lengthOfLine
                        : viewModel.PreferredColumnIndex;
                }

                break;
            case KeyboardKeyFacts.MovementKeys.ARROW_RIGHT:
            	if (viewModel.PersistentState.VirtualAssociativityKind == VirtualAssociativityKind.Left)
            	{
            		viewModel.PersistentState.VirtualAssociativityKind = VirtualAssociativityKind.Right;
            	}
                else if (TextEditorSelectionHelper.HasSelectedText(viewModel) && !shiftKey)
                {
                    var selectionBounds = TextEditorSelectionHelper.GetSelectionBounds(viewModel);

                    var upperLineMetaData = modelModifier.GetLineInformationFromPositionIndex(
                        selectionBounds.Position_UpperExclusiveIndex);

                    viewModel.LineIndex = upperLineMetaData.Index;

                    if (viewModel.LineIndex >= modelModifier.LineCount)
                    {
                        viewModel.LineIndex = modelModifier.LineCount - 1;

                        var upperLineLength = modelModifier.GetLineLength(viewModel.LineIndex);

                        viewModel.ColumnIndex = upperLineLength;
                    }
                    else
                    {
                        viewModel.ColumnIndex =
                            selectionBounds.Position_UpperExclusiveIndex - upperLineMetaData.Position_StartInclusiveIndex;
                    }
                }
                else
                {
                    lengthOfLine = modelModifier.GetLineLength(viewModel.LineIndex);

                    if (viewModel.ColumnIndex >= lengthOfLine &&
                        viewModel.LineIndex < modelModifier.LineCount - 1)
                    {
                        viewModel.SetColumnIndexAndPreferred(0);
                        viewModel.LineIndex++;
                    }
                    else if (viewModel.ColumnIndex != lengthOfLine)
                    {
                        if (ctrlKey)
                        {
                        	var columnIndexOfCharacterWithDifferingKind = modelModifier.GetColumnIndexOfCharacterWithDifferingKind(
                                viewModel.LineIndex,
                                viewModel.ColumnIndex,
                                false);

                            if (columnIndexOfCharacterWithDifferingKind == -1) // Move to end of line
                            {
                                viewModel.SetColumnIndexAndPreferred(lengthOfLine);
                            }
                            else
                            {
                            	if (!altKey) // Move by character kind
                            	{
                            		viewModel.SetColumnIndexAndPreferred(
                                    	columnIndexOfCharacterWithDifferingKind);
                            	}
                                else // Move by camel case
                                {
                                	var positionIndex = modelModifier.GetPositionIndex(viewModel);
									var rememberStartPositionIndex = positionIndex;
									
									var maxPositionIndex = columnIndexOfCharacterWithDifferingKind;
									
									var infiniteLoopPrediction = false;
									
									if (maxPositionIndex < positionIndex)
										infiniteLoopPrediction = true;
									
									bool useCamelCaseResult = false;
									
									if (!infiniteLoopPrediction)
									{
										while (++positionIndex < maxPositionIndex)
										{
											var currentRichCharacter = modelModifier.RichCharacterList[positionIndex];
											
											if (Char.IsUpper(currentRichCharacter.Value) || currentRichCharacter.Value == '_')
											{
												useCamelCaseResult = true;
												break;
											}
										}
									}
									
									if (useCamelCaseResult)
									{
										var columnDisplacement = positionIndex - rememberStartPositionIndex;
										viewModel.SetColumnIndexAndPreferred(viewModel.ColumnIndex + columnDisplacement);
									}
									else
									{
										viewModel.SetColumnIndexAndPreferred(
                                    		columnIndexOfCharacterWithDifferingKind);
									}
                                }
                            }
                        }
                        else
                        {
                            viewModel.SetColumnIndexAndPreferred(viewModel.ColumnIndex + 1);
                        }
                    }
                }

                break;
            case KeyboardKeyFacts.MovementKeys.HOME:
                if (ctrlKey)
                {
                    viewModel.LineIndex = 0;
                    viewModel.SetColumnIndexAndPreferred(0);
                }
				else
				{
					var originalPositionIndex = modelModifier.GetPositionIndex(viewModel);
					
					var lineInformation = modelModifier.GetLineInformation(viewModel.LineIndex);
					var lastValidPositionIndex = lineInformation.Position_StartInclusiveIndex + lineInformation.LastValidColumnIndex;
					
					viewModel.ColumnIndex = 0; // This column index = 0 is needed for the while loop below.
					var indentationPositionIndexExclusiveEnd = modelModifier.GetPositionIndex(viewModel);
					
					var cursorWithinIndentation = false;
		
					while (indentationPositionIndexExclusiveEnd < lastValidPositionIndex)
					{
						var possibleIndentationChar = modelModifier.RichCharacterList[indentationPositionIndexExclusiveEnd].Value;
		
						if (possibleIndentationChar == '\t' || possibleIndentationChar == ' ')
						{
							if (indentationPositionIndexExclusiveEnd == originalPositionIndex)
								cursorWithinIndentation = true;
						}
						else
						{
							break;
						}
						
						indentationPositionIndexExclusiveEnd++;
					}
					
					if (originalPositionIndex == indentationPositionIndexExclusiveEnd)
						viewModel.SetColumnIndexAndPreferred(0);
					else
						viewModel.SetColumnIndexAndPreferred(
							indentationPositionIndexExclusiveEnd - lineInformation.Position_StartInclusiveIndex);
				}

                break;
            case KeyboardKeyFacts.MovementKeys.END:
                if (ctrlKey)
                    viewModel.LineIndex = modelModifier.LineCount - 1;

                lengthOfLine = modelModifier.GetLineLength(viewModel.LineIndex);

                viewModel.SetColumnIndexAndPreferred(lengthOfLine);

                break;
        }
        
        if (viewModel.PersistentState.HiddenLineIndexHashSet.Contains(viewModel.LineIndex))
        {
        	switch (key)
        	{
        		case KeyboardKeyFacts.MovementKeys.ARROW_LEFT:
        		{
        			CollapsePoint encompassingCollapsePoint = new CollapsePoint(-1, false, string.Empty, -1);;

        			foreach (var collapsePoint in viewModel.PersistentState.AllCollapsePointList)
        			{
        				var firstToHideLineIndex = collapsePoint.AppendToLineIndex + 1;
						for (var lineOffset = 0; lineOffset < collapsePoint.EndExclusiveLineIndex - collapsePoint.AppendToLineIndex - 1; lineOffset++)
						{
							if (viewModel.LineIndex == firstToHideLineIndex + lineOffset)
								encompassingCollapsePoint = collapsePoint;
						}
        			}
        			
        			if (encompassingCollapsePoint.AppendToLineIndex != -1)
        			{
        				var lineIndex = encompassingCollapsePoint.EndExclusiveLineIndex - 1;
        				var lineInformation = modelModifier.GetLineInformation(lineIndex);
        				
        				if (viewModel.ColumnIndex != lineInformation.LastValidColumnIndex)
        				{
        					for (int i = viewModel.LineIndex; i >= 0; i--)
		        			{
		        				if (!viewModel.PersistentState.HiddenLineIndexHashSet.Contains(i))
		        				{
		        					viewModel.LineIndex = i;
		        					lineInformation = modelModifier.GetLineInformation(i);
		        					viewModel.ColumnIndex = lineInformation.LastValidColumnIndex;
		        					break;
		        				}
		        			}
        				}
        			}
        		
        			break;
        		}
        		case KeyboardKeyFacts.MovementKeys.ARROW_DOWN:
        		{
        			var success = false;
        		
        			for (int i = viewModel.LineIndex + 1; i < modelModifier.LineCount; i++)
        			{
        				if (!viewModel.PersistentState.HiddenLineIndexHashSet.Contains(i))
        				{
        					success = true;
        					viewModel.LineIndex = i;
        					
        					var lineInformation = modelModifier.GetLineInformation(i);
        					
        					if (viewModel.ColumnIndex > lineInformation.LastValidColumnIndex)
        						viewModel.ColumnIndex = lineInformation.LastValidColumnIndex;
        					
        					break;
        				}
        			}
        			
        			if (!success)
        			{
        				for (int i = viewModel.LineIndex; i >= 0; i--)
	        			{
	        				if (!viewModel.PersistentState.HiddenLineIndexHashSet.Contains(i))
	        				{
	        					viewModel.LineIndex = i;
	        					
	        					var lineInformation = modelModifier.GetLineInformation(i);
	        					
	        					if (viewModel.ColumnIndex > lineInformation.LastValidColumnIndex)
	        						viewModel.ColumnIndex = lineInformation.LastValidColumnIndex;
	        					
	        					break;
	        				}
	        			}
        			}
        			
        			break;
        		}
        		case KeyboardKeyFacts.MovementKeys.ARROW_UP:
        		{
        			var success = false;
        			
        			for (int i = viewModel.LineIndex; i >= 0; i--)
        			{
        				if (!viewModel.PersistentState.HiddenLineIndexHashSet.Contains(i))
        				{
        					success = true;
        					viewModel.LineIndex = i;
        					
        					var lineInformation = modelModifier.GetLineInformation(i);
        					
        					if (viewModel.PreferredColumnIndex <= lineInformation.LastValidColumnIndex)
        						viewModel.ColumnIndex = viewModel.PreferredColumnIndex;
        					else if (viewModel.ColumnIndex > lineInformation.LastValidColumnIndex)
        						viewModel.ColumnIndex = lineInformation.LastValidColumnIndex;
        					
        					break;
        				}
        			}
        		
        			if (!success)
        			{
        				for (int i = viewModel.LineIndex + 1; i < modelModifier.LineCount; i++)
	        			{
	        				if (!viewModel.PersistentState.HiddenLineIndexHashSet.Contains(i))
	        				{
	        					viewModel.LineIndex = i;
	        					
	        					var lineInformation = modelModifier.GetLineInformation(i);
	        					
	        					if (viewModel.ColumnIndex > lineInformation.LastValidColumnIndex)
	        						viewModel.ColumnIndex = lineInformation.LastValidColumnIndex;
	        					
	        					break;
	        				}
	        			}
        			}
        			
        			break;
        		}
        		case KeyboardKeyFacts.MovementKeys.ARROW_RIGHT:
    			{
        			CollapsePoint encompassingCollapsePoint = new CollapsePoint(-1, false, string.Empty, -1);;

        			foreach (var collapsePoint in viewModel.PersistentState.AllCollapsePointList)
        			{
        				var firstToHideLineIndex = collapsePoint.AppendToLineIndex + 1;
						for (var lineOffset = 0; lineOffset < collapsePoint.EndExclusiveLineIndex - collapsePoint.AppendToLineIndex - 1; lineOffset++)
						{
							if (viewModel.LineIndex == firstToHideLineIndex + lineOffset)
								encompassingCollapsePoint = collapsePoint;
						}
        			}
        			
        			if (encompassingCollapsePoint.AppendToLineIndex != -1)
        			{
        				var lineIndex = encompassingCollapsePoint.EndExclusiveLineIndex - 1;
        			
        				var lineInformation = modelModifier.GetLineInformation(lineIndex);
						viewModel.LineIndex = lineIndex;
						viewModel.SetColumnIndexAndPreferred(lineInformation.LastValidColumnIndex);
        			}
	        			
        			break;
        		}
        		case KeyboardKeyFacts.MovementKeys.HOME:
        		case KeyboardKeyFacts.MovementKeys.END:
        		{
        			break;
        		}
        	}
        }
        
        (int lineIndex, int columnIndex) lineAndColumnIndices = (0, 0);
		var inlineUi = new InlineUi(0, InlineUiKind.None);
		
		foreach (var inlineUiTuple in viewModel.PersistentState.InlineUiList)
		{
			lineAndColumnIndices = modelModifier.GetLineAndColumnIndicesFromPositionIndex(inlineUiTuple.InlineUi.PositionIndex);
			
			if (lineAndColumnIndices.lineIndex == viewModel.LineIndex &&
				lineAndColumnIndices.columnIndex == viewModel.ColumnIndex)
			{
				inlineUi = inlineUiTuple.InlineUi;
			}
		}
		
		if (viewModel.PersistentState.VirtualAssociativityKind == VirtualAssociativityKind.None &&
			inlineUi.InlineUiKind != InlineUiKind.None)
		{
			viewModel.PersistentState.VirtualAssociativityKind = VirtualAssociativityKind.Left;
		}
		
		if (inlineUi.InlineUiKind == InlineUiKind.None)
			viewModel.PersistentState.VirtualAssociativityKind = VirtualAssociativityKind.None;

        if (shiftKey)
        {
            viewModel.SelectionEndingPositionIndex = modelModifier.GetPositionIndex(
                viewModel.LineIndex,
                viewModel.ColumnIndex);
        }
        else if (!shiftKey && shouldClearSelection)
        {
            // The active selection is needed, and cannot be touched until the end.
            viewModel.SelectionAnchorPositionIndex = -1;
            viewModel.SelectionEndingPositionIndex = 0;
        }
    }

    public void ViewModel_CursorMovePageTop(
        TextEditorEditContext editContext,
        TextEditorViewModel viewModel)
    {
        CursorMovePageTopUnsafe(
	        editContext,
        	viewModel);
    }

    public void ViewModel_CursorMovePageTopUnsafe(
        TextEditorEditContext editContext,
        TextEditorViewModel viewModel)
    {
        if (viewModel.Virtualization.Count > 0)
        {
            var firstEntry = viewModel.Virtualization.EntryList[0];
            viewModel.LineIndex = firstEntry.LineIndex;
            viewModel.ColumnIndex = 0;
        }
    }

    public void ViewModel_CursorMovePageBottom(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel)
    {
        ViewModel_CursorMovePageBottomUnsafe(
        	editContext,
        	modelModifier,
        	viewModel);
    }

    public void ViewModel_CursorMovePageBottomUnsafe(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel)
    {
        if (viewModel.Virtualization.Count > 0)
        {
            var lastEntry = viewModel.Virtualization.EntryList[viewModel.Virtualization.Count - 1];
            var lastEntriesLineLength = modelModifier.GetLineLength(lastEntry.LineIndex);

            viewModel.LineIndex = lastEntry.LineIndex;
            viewModel.ColumnIndex = lastEntriesLineLength;
        }
    }
    
    public void ViewModel_RevealCursor(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModel)
    {
    	try
    	{
    		if (!viewModel.PersistentState.ShouldRevealCursor)
    			return;
    			
    		viewModel.PersistentState.ShouldRevealCursor = false;
    	
    		var cursorIsVisible = false;
    		
    		if (cursorIsVisible)
    			return;
    			
    		// Console.WriteLine(nameof(RevealCursor));
		
            var cursorPositionIndex = modelModifier.GetPositionIndex(viewModel);

            var cursorTextSpan = new TextEditorTextSpan(
                cursorPositionIndex,
                cursorPositionIndex + 1,
                0);

            ScrollIntoView(
        		editContext,
		        modelModifier,
		        viewModel,
		        cursorTextSpan);
    	}
    	catch (WalkTextEditorException exception)
    	{
    		Console.WriteLine(exception);
    	}
    }

    public void ViewModel_CalculateVirtualizationResult(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
		TextEditorViewModel viewModel,
		TextEditorComponentData componentData)
    {
    	#if DEBUG
    	var startTime = Stopwatch.GetTimestamp();
    	#endif
    	
		var tabWidth = editContext.TextEditorService.OptionsApi.GetOptions().TabWidth;
		viewModel.Virtualization.ShouldCalculateVirtualizationResult = false;
	
		var verticalStartingIndex = viewModel.PersistentState.ScrollTop /
			viewModel.PersistentState.CharAndLineMeasurements.LineHeight;

		var verticalTake = viewModel.PersistentState.TextEditorDimensions.Height /
			viewModel.PersistentState.CharAndLineMeasurements.LineHeight;

		// Vertical Padding (render some offscreen data)
		verticalTake += 1;

		var horizontalStartingIndex = (int)Math.Floor(
			viewModel.PersistentState.ScrollLeft /
			viewModel.PersistentState.CharAndLineMeasurements.CharacterWidth);

		var horizontalTake = (int)Math.Ceiling(
			viewModel.PersistentState.TextEditorDimensions.Width /
			viewModel.PersistentState.CharAndLineMeasurements.CharacterWidth);
		
		var hiddenCount = 0;
		
		for (int i = 0; i < verticalStartingIndex; i++)
		{
			if (viewModel.PersistentState.HiddenLineIndexHashSet.Contains(i))
			{
				hiddenCount++;
				verticalStartingIndex++;
			}
		}
		
		verticalStartingIndex = Math.Max(0, verticalStartingIndex);
		
		if (verticalStartingIndex + verticalTake > modelModifier.LineEndList.Count)
		    verticalTake = modelModifier.LineEndList.Count - verticalStartingIndex;
		
		verticalTake = Math.Max(0, verticalTake);
		
		var lineCountAvailable = modelModifier.LineEndList.Count - verticalStartingIndex;
		
		var lineCountToReturn = verticalTake < lineCountAvailable
		    ? verticalTake
		    : lineCountAvailable;
		
		var endingLineIndexExclusive = verticalStartingIndex + lineCountToReturn;
		
		var totalWidth = (int)Math.Ceiling(modelModifier.MostCharactersOnASingleLineTuple.lineLength *
			viewModel.PersistentState.CharAndLineMeasurements.CharacterWidth);

		// Account for any tab characters on the 'MostCharactersOnASingleLineTuple'
		//
		// TODO: This code is not fully correct...
		//       ...if the longest line is 50 non-tab characters,
		//       and the second longest line is 49 tab characters,
		//       this code will erroneously take the '50' non-tab characters
		//       to be the longest line.
		var lineIndex = modelModifier.MostCharactersOnASingleLineTuple.lineIndex;
		var longestLineInformation = modelModifier.GetLineInformation(lineIndex);
		var tabCountOnLongestLine = modelModifier.GetTabCountOnSameLineBeforeCursor(
			longestLineInformation.Index,
			longestLineInformation.LastValidColumnIndex);
		// 1 of the character width is already accounted for
		var extraWidthPerTabKey = tabWidth - 1;
		totalWidth += (int)Math.Ceiling(extraWidthPerTabKey *
			tabCountOnLongestLine *
			viewModel.PersistentState.CharAndLineMeasurements.CharacterWidth);

		var totalHeight = (modelModifier.LineEndList.Count - viewModel.PersistentState.HiddenLineIndexHashSet.Count) *
			viewModel.PersistentState.CharAndLineMeasurements.LineHeight;

		// Add vertical margin so the user can scroll beyond the final line of content
		var percentOfMarginScrollHeightByPageUnit = 0.4;
		int marginScrollHeight = (int)Math.Ceiling(viewModel.PersistentState.TextEditorDimensions.Height * percentOfMarginScrollHeightByPageUnit);
		totalHeight += marginScrollHeight;
		
		var virtualizedLineList = new TextEditorVirtualizationLine[lineCountToReturn];
		
		viewModel.Virtualization = new TextEditorVirtualizationResult(
			virtualizedLineList,
    		new List<TextEditorVirtualizationSpan>(),
	        resultWidth: (int)Math.Ceiling(horizontalTake * viewModel.PersistentState.CharAndLineMeasurements.CharacterWidth),
	        resultHeight: verticalTake * viewModel.PersistentState.CharAndLineMeasurements.LineHeight,
	        left: (int)(horizontalStartingIndex * viewModel.PersistentState.CharAndLineMeasurements.CharacterWidth),
	        top: verticalStartingIndex * viewModel.PersistentState.CharAndLineMeasurements.LineHeight,
	        componentData,
	        modelModifier,
	        viewModel,
	        viewModel.Virtualization);
		
		viewModel.PersistentState.ScrollWidth = totalWidth;
		viewModel.PersistentState.ScrollHeight = totalHeight;
		viewModel.PersistentState.MarginScrollHeight = marginScrollHeight;
		
		viewModel.PersistentState.GutterWidth = GetGutterWidthInPixels(modelModifier, viewModel, componentData);
		
		var absDiffScrollLeft = Math.Abs(componentData.LineIndexCache.ScrollLeftMarker - viewModel.PersistentState.ScrollLeft);
		var useCache = absDiffScrollLeft < 0.01 && componentData.LineIndexCache.ViewModelKeyMarker == viewModel.PersistentState.ViewModelKey;
		
		if (!useCache)
		    componentData.LineIndexCache.IsInvalid = true;
		
		if (componentData.LineIndexCache.IsInvalid)
			componentData.LineIndexCache.Clear();
		else
			componentData.LineIndexCache.UsedKeyHashSet.Clear();
		
		if (_textEditorService.SeenTabWidth != _textEditorService.OptionsApi.GetTextEditorOptionsState().Options.TabWidth)
		{
			_textEditorService.SeenTabWidth = _textEditorService.OptionsApi.GetTextEditorOptionsState().Options.TabWidth;
			_textEditorService.TabKeyOutput_ShowWhitespaceTrue = new string('-', _textEditorService.SeenTabWidth - 1) + '>';
			
			var stringBuilder = new StringBuilder();
			
			for (int i = 0; i < _textEditorService.SeenTabWidth; i++)
			{
				stringBuilder.Append("&nbsp;");
			}
			_textEditorService.TabKeyOutput_ShowWhitespaceFalse = stringBuilder.ToString();
		}
		
		string tabKeyOutput;
		string spaceKeyOutput;
		
		if (_textEditorService.OptionsApi.GetTextEditorOptionsState().Options.ShowWhitespace)
		{
			tabKeyOutput = _textEditorService.TabKeyOutput_ShowWhitespaceTrue;
			spaceKeyOutput = "·";
		}
		else
		{
			tabKeyOutput = _textEditorService.TabKeyOutput_ShowWhitespaceFalse;
			spaceKeyOutput = "&nbsp;";
		}
		
		_textEditorService.__StringBuilder.Clear();
		
		var minLineWidthToTriggerVirtualizationExclusive = 2 * viewModel.PersistentState.TextEditorDimensions.Width;
			
		int lineOffset = -1;
		
		var entireSpan = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(componentData.LineIndexCache.VirtualizationSpanList);
		
		int position_StartInclusiveIndex;
		int position_EndExclusiveIndex;
		string leftCssValue;
		string topCssValue;
        int virtualizationSpan_StartInclusiveIndex;
		int virtualizationSpan_EndExclusiveIndex;
		
		while (true)
		{
			lineOffset++;
		
			if (viewModel.Virtualization.Count >= lineCountToReturn)
				break;
			// TODO: Is this '>' or '>='?
			if (verticalStartingIndex + lineOffset >= modelModifier.LineEndList.Count)
				break;
		
			lineIndex = verticalStartingIndex + lineOffset;

			if (viewModel.PersistentState.HiddenLineIndexHashSet.Contains(lineIndex))
			{
				hiddenCount++;
				continue;
			}
			
			useCache = componentData.LineIndexCache.Map.ContainsKey(lineIndex) &&
				       !componentData.LineIndexCache.ModifiedLineIndexList.Contains(lineIndex);
			
			if (useCache)
			{
			    componentData.LineIndexCache.UsedKeyHashSet.Add(lineIndex);
    			var cacheEntryCopy = componentData.LineIndexCache.Map[lineIndex];
    			
    			cacheEntryCopy.VirtualizationSpan_StartInclusiveIndex = viewModel.Virtualization.VirtualizationSpanList.Count;
			    var smallSpan = entireSpan.Slice(
    			    componentData.LineIndexCache.Map[lineIndex].VirtualizationSpan_StartInclusiveIndex,
    			    componentData.LineIndexCache.Map[lineIndex].VirtualizationSpan_EndExclusiveIndex - componentData.LineIndexCache.Map[lineIndex].VirtualizationSpan_StartInclusiveIndex);
    			foreach (var virtualizedSpan in smallSpan)
    			{
    				viewModel.Virtualization.VirtualizationSpanList.Add(virtualizedSpan);
    			}
    			cacheEntryCopy.VirtualizationSpan_EndExclusiveIndex = viewModel.Virtualization.VirtualizationSpanList.Count;
    			
    			componentData.LineIndexCache.Map[cacheEntryCopy.LineIndex] = cacheEntryCopy;
    			virtualizedLineList[viewModel.Virtualization.Count++] = new TextEditorVirtualizationLine(
    			    cacheEntryCopy.LineIndex,
            	    cacheEntryCopy.Position_StartInclusiveIndex,
            	    cacheEntryCopy.Position_EndExclusiveIndex,
            	    cacheEntryCopy.VirtualizationSpan_StartInclusiveIndex,
            	    cacheEntryCopy.VirtualizationSpan_EndExclusiveIndex,
            	    cacheEntryCopy.LeftCssValue,
            	    cacheEntryCopy.TopCssValue,
            	    cacheEntryCopy.GutterCssStyle,
                    cacheEntryCopy.LineCssStyle,
                    cacheEntryCopy.LineNumberString);
			    continue;
			}
            
            // "inline" of 'TextEditorModel.GetLineInformation(...)'. This isn't a 1 to 1 inline, it avoids some redundant bounds checking.
            var lineEndLower = lineIndex == 0
                ? new(0, 0, Walk.TextEditor.RazorLib.Lines.Models.LineEndKind.StartOfFile)
                : modelModifier.LineEndList[lineIndex - 1];
			var lineEndUpper = modelModifier.LineEndList[lineIndex];
			var lineInformation = new Walk.TextEditor.RazorLib.Lines.Models.LineInformation(
                lineIndex,
                lineEndLower.Position_EndExclusiveIndex,
                lineEndUpper.Position_EndExclusiveIndex,
                lineEndLower,
                lineEndUpper);
			
			// TODO: Was this code using length including line ending or excluding? (2024-12-29)
			var lineLength = lineInformation.Position_EndExclusiveIndex - lineInformation.Position_StartInclusiveIndex;
			
			// Don't bother with the extra width due to tabs until the very end.
			// It is thought to be too costly on average to get the tab count for the line in order to take less text overall
			// than to just take the estimated amount of characters.
			
			var widthInPixels = (int)Math.Ceiling(lineLength * viewModel.PersistentState.CharAndLineMeasurements.CharacterWidth);

			if (widthInPixels > minLineWidthToTriggerVirtualizationExclusive)
			{
				var localHorizontalStartingIndex = horizontalStartingIndex;
				var localHorizontalTake = horizontalTake;
				
				// Tab key adjustments
				var firstInlineUiOnLineIndex = -1;
				var foundLine = false;
				var tabCharPositionIndexListCount = modelModifier.TabCharPositionIndexList.Count;
		
				// Move the horizontal starting index based on the extra character width from 'tab' characters.
				for (int i = 0; i < tabCharPositionIndexListCount; i++)
				{
					var tabCharPositionIndex = modelModifier.TabCharPositionIndexList[i];
					var tabKeyColumnIndex = tabCharPositionIndex - lineInformation.Position_StartInclusiveIndex;
				
					if (!foundLine)
					{
						if (tabCharPositionIndex >= lineInformation.Position_StartInclusiveIndex)
						{
							firstInlineUiOnLineIndex = i;
							foundLine = true;
						}
					}
					
					if (foundLine)
					{
						if (tabKeyColumnIndex >= localHorizontalStartingIndex + localHorizontalTake)
							break;
					
						localHorizontalStartingIndex -= extraWidthPerTabKey;
					}
				}

				if (localHorizontalStartingIndex + localHorizontalTake > lineLength)
					localHorizontalTake = lineLength - localHorizontalStartingIndex;

				localHorizontalStartingIndex = Math.Max(0, localHorizontalStartingIndex);
				localHorizontalTake = Math.Max(0, localHorizontalTake);
				
				var foundSplit = false;
				var unrenderedTabCount = 0;
				var resultTabCount = 0;
				
				// Count the 'tab' characters that preceed the text to display so that the 'left' can be modified by the extra width.
				// Count the 'tab' characters that are among the text to display so that the 'width' can be modified by the extra width.
				if (firstInlineUiOnLineIndex != -1)
				{
					for (int i = firstInlineUiOnLineIndex; i < tabCharPositionIndexListCount; i++)
					{
						var tabCharPositionIndex = modelModifier.TabCharPositionIndexList[i];
						var tabKeyColumnIndex = tabCharPositionIndex - lineInformation.Position_StartInclusiveIndex;
						
						if (tabKeyColumnIndex >= localHorizontalStartingIndex + localHorizontalTake)
							break;
					
						if (!foundSplit)
						{
							if (tabKeyColumnIndex < localHorizontalStartingIndex)
								unrenderedTabCount++;
							else
								foundSplit = true;
						}
						
						if (foundSplit)
							resultTabCount++;
					}
				}
				
				widthInPixels = (int)Math.Ceiling(((localHorizontalTake - localHorizontalStartingIndex) + (extraWidthPerTabKey * resultTabCount)) *
					viewModel.PersistentState.CharAndLineMeasurements.CharacterWidth);

				double leftInPixels = localHorizontalStartingIndex *
					viewModel.PersistentState.CharAndLineMeasurements.CharacterWidth;

				// Adjust the unrendered for tab key width
				leftInPixels += (extraWidthPerTabKey *
					unrenderedTabCount *
					viewModel.PersistentState.CharAndLineMeasurements.CharacterWidth);

				leftInPixels = Math.Max(0, leftInPixels);

				var topInPixels = lineIndex * viewModel.PersistentState.CharAndLineMeasurements.LineHeight;
				position_StartInclusiveIndex = lineInformation.Position_StartInclusiveIndex + localHorizontalStartingIndex;
				
				position_EndExclusiveIndex = position_StartInclusiveIndex + localHorizontalTake;
				if (position_EndExclusiveIndex > lineInformation.UpperLineEnd.Position_StartInclusiveIndex)
					position_EndExclusiveIndex = lineInformation.UpperLineEnd.Position_StartInclusiveIndex;
				
				if (leftInPixels == 0)
				{
				    leftCssValue = viewModel.PersistentState.GetGutterWidthCssValue();
				}
				else
				{
				    // Although 'GutterWidth' is an int, the 'leftInPixels' is a double,
				    // so 'System.Globalization.CultureInfo.InvariantCulture' is necessary
				    // to avoid invalid CSS due to periods becoming commas for decimal points.
				    leftCssValue = (viewModel.PersistentState.GutterWidth + leftInPixels).ToString(System.Globalization.CultureInfo.InvariantCulture);
				}
				
				topCssValue = (topInPixels - (viewModel.PersistentState.CharAndLineMeasurements.LineHeight * hiddenCount)).ToString();
			}
			else
			{
				var foundLine = false;
				var resultTabCount = 0;
		
				// Count the tabs that are among the rendered content.
				foreach (var tabCharPositionIndex in modelModifier.TabCharPositionIndexList)
				{
					if (!foundLine)
					{
						if (tabCharPositionIndex >= lineInformation.Position_StartInclusiveIndex)
							foundLine = true;
					}
					
					if (foundLine)
					{
						if (tabCharPositionIndex >= lineInformation.LastValidColumnIndex)
							break;
					
						resultTabCount++;
					}
				}
				
				widthInPixels += (int)Math.Ceiling((extraWidthPerTabKey * resultTabCount) *
					viewModel.PersistentState.CharAndLineMeasurements.CharacterWidth);
			
				position_StartInclusiveIndex = lineInformation.Position_StartInclusiveIndex;
				position_EndExclusiveIndex = lineInformation.UpperLineEnd.Position_StartInclusiveIndex;
				
				leftCssValue = viewModel.PersistentState.GetGutterWidthCssValue();
				topCssValue = ((lineIndex * viewModel.PersistentState.CharAndLineMeasurements.LineHeight) - (viewModel.PersistentState.CharAndLineMeasurements.LineHeight * hiddenCount)).ToString();
			}

			componentData.LineIndexCache.UsedKeyHashSet.Add(lineIndex);
			
			if (position_EndExclusiveIndex - position_StartInclusiveIndex <= 0)
    		{
        	    virtualizationSpan_StartInclusiveIndex = viewModel.Virtualization.VirtualizationSpanList.Count;
        	    virtualizationSpan_EndExclusiveIndex = viewModel.Virtualization.VirtualizationSpanList.Count;
    		}
    		else
    		{
        		virtualizationSpan_StartInclusiveIndex = viewModel.Virtualization.VirtualizationSpanList.Count;
        		
        	    var richCharacterSpan = new Span<RichCharacter>(
        	        modelModifier.RichCharacterList,
        	        position_StartInclusiveIndex,
        	        position_EndExclusiveIndex - position_StartInclusiveIndex);
        	
        		var currentDecorationByte = richCharacterSpan[0].DecorationByte;
        	    
        	    foreach (var richCharacter in richCharacterSpan)
        	    {
        			if (currentDecorationByte == richCharacter.DecorationByte)
        		    {
        		        // AppendTextEscaped(textEditorService.__StringBuilder, richCharacter, tabKeyOutput, spaceKeyOutput);
        		        switch (richCharacter.Value)
        		        {
        		            case '\t':
        		                _textEditorService.__StringBuilder.Append(tabKeyOutput);
        		                break;
        		            case ' ':
        		                _textEditorService.__StringBuilder.Append(spaceKeyOutput);
        		                break;
        		            case '\r':
        		                break;
        		            case '\n':
        		                break;
        		            case '<':
        		                _textEditorService.__StringBuilder.Append("&lt;");
        		                break;
        		            case '>':
        		                _textEditorService.__StringBuilder.Append("&gt;");
        		                break;
        		            case '"':
        		                _textEditorService.__StringBuilder.Append("&quot;");
        		                break;
        		            case '\'':
        		                _textEditorService.__StringBuilder.Append("&#39;");
        		                break;
        		            case '&':
        		                _textEditorService.__StringBuilder.Append("&amp;");
        		                break;
        		            default:
        		                _textEditorService.__StringBuilder.Append(richCharacter.Value);
        		                break;
        		        }
        		        // END OF INLINING AppendTextEscaped
        		    }
        		    else
        		    {
        		    	viewModel.Virtualization.VirtualizationSpanList.Add(new TextEditorVirtualizationSpan(
        		    		cssClass: modelModifier.PersistentState.DecorationMapper.Map(currentDecorationByte),
        		    		text: _textEditorService.__StringBuilder.ToString()));
        		        _textEditorService.__StringBuilder.Clear();
        		        
        		        // AppendTextEscaped(textEditorService.__StringBuilder, richCharacter, tabKeyOutput, spaceKeyOutput);
        		        switch (richCharacter.Value)
        		        {
        		            case '\t':
        		                _textEditorService.__StringBuilder.Append(tabKeyOutput);
        		                break;
        		            case ' ':
        		                _textEditorService.__StringBuilder.Append(spaceKeyOutput);
        		                break;
        		            case '\r':
        		                break;
        		            case '\n':
        		                break;
        		            case '<':
        		                _textEditorService.__StringBuilder.Append("&lt;");
        		                break;
        		            case '>':
        		                _textEditorService.__StringBuilder.Append("&gt;");
        		                break;
        		            case '"':
        		                _textEditorService.__StringBuilder.Append("&quot;");
        		                break;
        		            case '\'':
        		                _textEditorService.__StringBuilder.Append("&#39;");
        		                break;
        		            case '&':
        		                _textEditorService.__StringBuilder.Append("&amp;");
        		                break;
        		            default:
        		                _textEditorService.__StringBuilder.Append(richCharacter.Value);
        		                break;
        		        }
        		        // END OF INLINING AppendTextEscaped
        		        
        				currentDecorationByte = richCharacter.DecorationByte;
        		    }
        	    }
        	    
        	    /* Final grouping of contiguous characters */
        		viewModel.Virtualization.VirtualizationSpanList.Add(new TextEditorVirtualizationSpan(
            		cssClass: modelModifier.PersistentState.DecorationMapper.Map(currentDecorationByte),
            		text: _textEditorService.__StringBuilder.ToString()));
        		_textEditorService.__StringBuilder.Clear();

        		virtualizationSpan_EndExclusiveIndex = viewModel.Virtualization.VirtualizationSpanList.Count;
    		}
    		
    		virtualizedLineList[viewModel.Virtualization.Count++] = new TextEditorVirtualizationLine(
    		    lineIndex,
        	    position_StartInclusiveIndex,
        	    position_EndExclusiveIndex,
        	    virtualizationSpan_StartInclusiveIndex,
        	    virtualizationSpan_EndExclusiveIndex,
        	    leftCssValue,
        	    topCssValue,
        	    viewModel.Virtualization.GetGutterStyleCss(topCssValue),
                viewModel.Virtualization.RowSection_GetRowStyleCss(topCssValue, leftCssValue),
                (lineIndex + 1).ToString());
    		
    		if (componentData.LineIndexCache.Map.ContainsKey(lineIndex))
    		{
    			componentData.LineIndexCache.Map[lineIndex] = new TextEditorLineIndexCacheEntry(
    			    topCssValue,
        			leftCssValue,
    				lineNumberString: virtualizedLineList[viewModel.Virtualization.Count - 1].LineNumberString,
    			    lineIndex,
            	    position_StartInclusiveIndex,
            	    position_EndExclusiveIndex,
            	    virtualizationSpan_StartInclusiveIndex,
            	    virtualizationSpan_EndExclusiveIndex,
            	    virtualizedLineList[viewModel.Virtualization.Count - 1].GutterCssStyle,
            	    virtualizedLineList[viewModel.Virtualization.Count - 1].LineCssStyle);
    		}
    		else
    		{
    			componentData.LineIndexCache.ExistsKeyList.Add(lineIndex);
    			componentData.LineIndexCache.Map.Add(lineIndex, new TextEditorLineIndexCacheEntry(
    			    topCssValue,
        			leftCssValue,
    				lineNumberString: virtualizedLineList[viewModel.Virtualization.Count - 1].LineNumberString,
    			    lineIndex,
            	    position_StartInclusiveIndex,
            	    position_EndExclusiveIndex,
            	    virtualizationSpan_StartInclusiveIndex,
            	    virtualizationSpan_EndExclusiveIndex,
            	    virtualizedLineList[viewModel.Virtualization.Count - 1].GutterCssStyle,
            	    virtualizedLineList[viewModel.Virtualization.Count - 1].LineCssStyle));
    		}
    		
    	    _textEditorService.__StringBuilder.Clear();
		}
		
		viewModel.Virtualization.CreateUi();
		
		componentData.LineIndexCache.ModifiedLineIndexList.Clear();
	
		componentData.LineIndexCache.ViewModelKeyMarker = viewModel.PersistentState.ViewModelKey;
		componentData.LineIndexCache.ScrollLeftMarker = viewModel.PersistentState.ScrollLeft;
		componentData.LineIndexCache.VirtualizationSpanList = viewModel.Virtualization.VirtualizationSpanList;
		
		for (var i = componentData.LineIndexCache.ExistsKeyList.Count - 1; i >= 0; i--)
		{
			if (!componentData.LineIndexCache.UsedKeyHashSet.Contains(componentData.LineIndexCache.ExistsKeyList[i]))
			{
				componentData.LineIndexCache.Map.Remove(componentData.LineIndexCache.ExistsKeyList[i]);
				componentData.LineIndexCache.ExistsKeyList.RemoveAt(i);
			}
		}
		
		componentData.Virtualization = viewModel.Virtualization;
		
		#if DEBUG
		WalkDebugSomething.SetTextEditorViewModelApi(Stopwatch.GetElapsedTime(startTime));
		#endif
    }
    
    private static int ViewModel_CountDigits(int argumentNumber)
    {
    	var digitCount = 1;
    	var runningNumber = argumentNumber;
    	
    	while ((runningNumber /= 10) > 0)
    	{
    		digitCount++;
    	}
    	
    	return digitCount;
    }

    private int ViewModel_GetGutterWidthInPixels(TextEditorModel model, TextEditorViewModel viewModel, TextEditorComponentData componentData)
    {
        if (!componentData.ViewModelDisplayOptions.IncludeGutterComponent)
            return 0;

        var mostDigitsInARowLineNumber = CountDigits(model!.LineCount);

        var gutterWidthInPixels = mostDigitsInARowLineNumber *
            viewModel!.PersistentState.CharAndLineMeasurements.CharacterWidth;

        gutterWidthInPixels += TextEditorModel.GUTTER_PADDING_LEFT_IN_PIXELS + TextEditorModel.GUTTER_PADDING_RIGHT_IN_PIXELS;

        return (int)Math.Ceiling(gutterWidthInPixels);
    }
    
    /// <summary>
    /// Inlining this instead of invoking the function definition just to see what happens.
    /// </summary>
    /*private void ViewModel_AppendTextEscaped(
        StringBuilder spanBuilder,
        RichCharacter richCharacter,
        string tabKeyOutput,
        string spaceKeyOutput)
    {
        switch (richCharacter.Value)
        {
            case '\t':
                spanBuilder.Append(tabKeyOutput);
                break;
            case ' ':
                spanBuilder.Append(spaceKeyOutput);
                break;
            case '\r':
                break;
            case '\n':
                break;
            case '<':
                spanBuilder.Append("&lt;");
                break;
            case '>':
                spanBuilder.Append("&gt;");
                break;
            case '"':
                spanBuilder.Append("&quot;");
                break;
            case '\'':
                spanBuilder.Append("&#39;");
                break;
            case '&':
                spanBuilder.Append("&amp;");
                break;
            default:
                spanBuilder.Append(richCharacter.Value);
                break;
        }
    }*/

    public async ValueTask ViewModel_RemeasureAsync(
        TextEditorEditContext editContext,
        TextEditorViewModel viewModel)
    {
        var options = _textEditorService.OptionsApi.GetOptions();
        
        var componentData = viewModel.PersistentState.ComponentData;
        if (componentData is null)
        	return;
		
		var textEditorMeasurements = await _textEditorService.ViewModelApi
			.GetTextEditorMeasurementsAsync(componentData.RowSectionElementId)
			.ConfigureAwait(false);

		viewModel.PersistentState.CharAndLineMeasurements = options.CharAndLineMeasurements;
		viewModel.PersistentState.TextEditorDimensions = textEditorMeasurements;
    }

    public void ViewModel_ForceRender(
        TextEditorEditContext editContext,
        TextEditorViewModel viewModel)
    {
        // Getting the ViewModel from the 'editContext' triggers a re-render
        //
        // A lot code is being changed and one result is this method now reads like non-sense,
        // (or more non-sense than it previously did)
        // Because we get a viewModel passed in to this method as an argument.
        // So this seems quite silly.
		_ = editContext.GetViewModelModifier(viewModel.PersistentState.ViewModelKey);
    }
    #endregion

    #region DELETE_METHODS
    public void ViewModel_Dispose(TextEditorEditContext editContext, Key<TextEditorViewModel> viewModelKey)
    {
        _textEditorService.DisposeViewModel(editContext, viewModelKey);
    }
    #endregion
    /* End ViewModelApi */
    
    /* Start GroupApi */
    // TODO: Is this lock used?
	private readonly object Group_stateModificationLock = new();
	
    public void Group_SetActiveViewModel(Key<TextEditorGroup> textEditorGroupKey, Key<TextEditorViewModel> textEditorViewModelKey)
    {
        SetActiveViewModelOfGroup(
            textEditorGroupKey,
            textEditorViewModelKey);
    }

    public void Group_RemoveViewModel(Key<TextEditorGroup> textEditorGroupKey, Key<TextEditorViewModel> textEditorViewModelKey)
    {
        RemoveViewModelFromGroup(
            textEditorGroupKey,
            textEditorViewModelKey);
    }

    public void Group_Register(Key<TextEditorGroup> textEditorGroupKey, Category? category = null)
    {
    	category ??= new Category("main");
    
        var textEditorGroup = new TextEditorGroup(
            textEditorGroupKey,
            Key<TextEditorViewModel>.Empty,
            new List<Key<TextEditorViewModel>>(),
            category.Value,
            _textEditorService,
            _commonUtilityService);

        Register(textEditorGroup);
    }

    public TextEditorGroup? Group_GetOrDefault(Key<TextEditorGroup> textEditorGroupKey)
    {
        return _textEditorService.GroupApi.GetTextEditorGroupState().GroupList.FirstOrDefault(
            x => x.GroupKey == textEditorGroupKey);
    }

    public void Group_AddViewModel(Key<TextEditorGroup> textEditorGroupKey, Key<TextEditorViewModel> textEditorViewModelKey)
    {
        AddViewModelToGroup(
            textEditorGroupKey,
            textEditorViewModelKey);
    }

    public List<TextEditorGroup> Group_GetGroups()
    {
        return _textEditorService.GroupApi.GetTextEditorGroupState().GroupList;
    }
    
    // TextEditorGroupService.cs
    private TextEditorGroupState Group_textEditorGroupState = new();
	
	public event Action? Group_TextEditorGroupStateChanged;
	
	public TextEditorGroupState Group_GetTextEditorGroupState() => _textEditorGroupState;
        
    public void Group_Register(TextEditorGroup group)
    {
        lock (_stateModificationLock)
        {
            var inState = GetTextEditorGroupState();

            var inGroup = inState.GroupList.FirstOrDefault(
                x => x.GroupKey == group.GroupKey);

            if (inGroup is not null)
				goto finalize;

			var outGroupList = new List<TextEditorGroup>(inState.GroupList);
            outGroupList.Add(group);

            _textEditorGroupState = new TextEditorGroupState
            {
                GroupList = outGroupList
            };

            goto finalize;
        }

        finalize:
		TextEditorGroupStateChanged?.Invoke();
	}

    public void Group_AddViewModelToGroup(
        Key<TextEditorGroup> groupKey,
        Key<TextEditorViewModel> viewModelKey)
    {
        lock (_stateModificationLock)
        {
            var inState = GetTextEditorGroupState();

            var inGroupIndex = inState.GroupList.FindIndex(
                x => x.GroupKey == groupKey);

            if (inGroupIndex == -1)
				goto finalize;

			var inGroup = inState.GroupList[inGroupIndex];

            if (inGroup is null)
				goto finalize;

			if (inGroup.ViewModelKeyList.Contains(viewModelKey))
				goto finalize;

			var outViewModelKeyList = new List<Key<TextEditorViewModel>>(inGroup.ViewModelKeyList);
            outViewModelKeyList.Add(viewModelKey);

            var outGroup = inGroup with
            {
                ViewModelKeyList = outViewModelKeyList
            };

            if (outGroup.ViewModelKeyList.Count == 1)
            {
                outGroup = outGroup with
                {
                    ActiveViewModelKey = viewModelKey
                };
            }

            var outGroupList = new List<TextEditorGroup>(inState.GroupList);
            outGroupList[inGroupIndex] = outGroup;

            _textEditorGroupState = new TextEditorGroupState
            {
                GroupList = outGroupList
            };

			goto finalize;
		}

		finalize:
		TextEditorGroupStateChanged?.Invoke();
		PostScroll(groupKey, viewModelKey);
	}

    public void Group_RemoveViewModelFromGroup(
        Key<TextEditorGroup> groupKey,
        Key<TextEditorViewModel> viewModelKey)
    {
        lock (_stateModificationLock)
        {
            var inState = GetTextEditorGroupState();

            var inGroupIndex = inState.GroupList.FindIndex(
                x => x.GroupKey == groupKey);

            if (inGroupIndex == -1)
				goto finalize;

			var inGroup = inState.GroupList[inGroupIndex];

            if (inGroup is null)
				goto finalize;

			var indexOfViewModelKeyToRemove = inGroup.ViewModelKeyList.FindIndex(
                x => x == viewModelKey);

            if (indexOfViewModelKeyToRemove == -1)
				goto finalize;

			var viewModelKeyToRemove = inGroup.ViewModelKeyList[indexOfViewModelKeyToRemove];

            var nextViewModelKeyList = new List<Key<TextEditorViewModel>>(inGroup.ViewModelKeyList);
            nextViewModelKeyList.RemoveAt(indexOfViewModelKeyToRemove);

            Key<TextEditorViewModel> nextActiveTextEditorModelKey;

            if (inGroup.ActiveViewModelKey != Key<TextEditorViewModel>.Empty &&
                inGroup.ActiveViewModelKey != viewModelKeyToRemove)
            {
                // Because the active tab was not removed, do not bother setting a different
                // active tab.
                nextActiveTextEditorModelKey = inGroup.ActiveViewModelKey;
            }
            else
            {
                // The active tab was removed, therefore a new active tab must be chosen.

                // This variable is done for renaming
                var activeViewModelKeyIndex = indexOfViewModelKeyToRemove;

                // If last item in list
                if (activeViewModelKeyIndex >= inGroup.ViewModelKeyList.Count - 1)
                {
                    activeViewModelKeyIndex--;
                }
                else
                {
                    // ++ operation because this calculation is using the immutable list where
                    // the view model was not removed.
                    activeViewModelKeyIndex++;
                }

                // If removing the active will result in empty list set the active as an Empty TextEditorViewModelKey
                if (inGroup.ViewModelKeyList.Count - 1 == 0)
                    nextActiveTextEditorModelKey = Key<TextEditorViewModel>.Empty;
                else
                    nextActiveTextEditorModelKey = inGroup.ViewModelKeyList[activeViewModelKeyIndex];
            }

            var outGroupList = new List<TextEditorGroup>(inState.GroupList);

            outGroupList[inGroupIndex] = inGroup with
            {
                ViewModelKeyList = nextViewModelKeyList,
                ActiveViewModelKey = nextActiveTextEditorModelKey
            };

            _textEditorGroupState = new TextEditorGroupState
            {
                GroupList = outGroupList
            };

            goto finalize;
		}

		finalize:
		TextEditorGroupStateChanged?.Invoke();
		PostScroll(groupKey, _textEditorService.GroupApi.GetOrDefault(groupKey).ActiveViewModelKey);
	}

    public void Group_SetActiveViewModelOfGroup(
        Key<TextEditorGroup> groupKey,
        Key<TextEditorViewModel> viewModelKey)
    {
        lock (_stateModificationLock)
        {
            var inState = GetTextEditorGroupState();

            var inGroupIndex = inState.GroupList.FindIndex(
                x => x.GroupKey == groupKey);

            if (inGroupIndex == -1)
				goto finalize;

			var inGroup = inState.GroupList[inGroupIndex];

            if (inGroup is null)
				goto finalize;

			var outGroupList = new List<TextEditorGroup>(inState.GroupList);

            outGroupList[inGroupIndex] = inGroup with
            {
                ActiveViewModelKey = viewModelKey
            };

            _textEditorGroupState = new TextEditorGroupState
            {
                GroupList = outGroupList
            };

			goto finalize;
		}

		finalize:
		PostScroll(groupKey, viewModelKey);
		TextEditorGroupStateChanged?.Invoke();
	}

    public void Group_Dispose(Key<TextEditorGroup> groupKey)
    {
        lock (_stateModificationLock)
        {
            var inState = GetTextEditorGroupState();

            var inGroup = inState.GroupList.FirstOrDefault(
                x => x.GroupKey == groupKey);

            if (inGroup is null)
				goto finalize;

			var outGroupList = new List<TextEditorGroup>(inState.GroupList);
            outGroupList.Remove(inGroup);

            _textEditorGroupState = new TextEditorGroupState
            {
                GroupList = outGroupList
            };

			goto finalize;
		}

		finalize:
		TextEditorGroupStateChanged?.Invoke();
	}

	private void Group_PostScroll(
		Key<TextEditorGroup> groupKey,
    	Key<TextEditorViewModel> viewModelKey)
	{
		_textEditorService.WorkerArbitrary.PostUnique(editContext =>
		{
			var viewModelModifier = editContext.GetViewModelModifier(viewModelKey);
            if (viewModelModifier is null)
                return ValueTask.CompletedTask;

			viewModelModifier.ScrollWasModified = true;
			return ValueTask.CompletedTask;
		});
	}
    /* End GroupApi */
    
    /* Start DiffApi */
    public void Diff_Register(
        Key<TextEditorDiffModel> diffModelKey,
        Key<TextEditorViewModel> inViewModelKey,
        Key<TextEditorViewModel> outViewModelKey)
    {
        ReduceRegisterAction(
            diffModelKey,
            inViewModelKey,
            outViewModelKey);
    }

    public TextEditorDiffModel? Diff_GetOrDefault(Key<TextEditorDiffModel> diffModelKey)
    {
        return GetTextEditorDiffState().DiffModelList
            .FirstOrDefault(x => x.DiffKey == diffModelKey);
    }

    public void Diff_Dispose(Key<TextEditorDiffModel> diffModelKey)
    {
        ReduceDisposeAction(diffModelKey);
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
            editContext.TextEditorService.ModelApi.StartPendingCalculatePresentationModel(
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
            editContext.TextEditorService.ModelApi.StartPendingCalculatePresentationModel(
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
        return GetTextEditorDiffState().DiffModelList;
    }
    
    private TextEditorDiffState Diff_textEditorDiffState = new();
    
    public event Action? Diff_TextEditorDiffStateChanged;
    
    public TextEditorDiffState Diff_GetTextEditorDiffState() => _textEditorDiffState;
    
    public void Diff_ReduceDisposeAction(Key<TextEditorDiffModel> diffKey)
    {
    	var inState = GetTextEditorDiffState();
    
        var inDiff = inState.DiffModelList.FirstOrDefault(
            x => x.DiffKey == diffKey);

        if (inDiff is null)
        {
            TextEditorDiffStateChanged?.Invoke();
            return;
        }

		var outDiffModelList = new List<TextEditorDiffModel>(inState.DiffModelList);
		outDiffModelList.Remove(inDiff);

		_textEditorDiffState = new TextEditorDiffState
        {
            DiffModelList = outDiffModelList
        };
        
        TextEditorDiffStateChanged?.Invoke();
        return;
    }

    public void Diff_ReduceRegisterAction(
        Key<TextEditorDiffModel> diffKey,
        Key<TextEditorViewModel> inViewModelKey,
        Key<TextEditorViewModel> outViewModelKey)
    {
    	var inState = GetTextEditorDiffState();
    
        var inDiff = inState.DiffModelList.FirstOrDefault(
            x => x.DiffKey == diffKey);

        if (inDiff is not null)
        {
            TextEditorDiffStateChanged?.Invoke();
            return;
        }

        var diff = new TextEditorDiffModel(
            diffKey,
            inViewModelKey,
            outViewModelKey);

        var outDiffModelList = new List<TextEditorDiffModel>(inState.DiffModelList);
        outDiffModelList.Add(diff);

        _textEditorDiffState = new TextEditorDiffState
        {
            DiffModelList = outDiffModelList
        };
        
        TextEditorDiffStateChanged?.Invoke();
        return;
    }
    /* End DiffApi */
    
    /* Start OptionsApi */
	public const int Options_TAB_WIDTH_MIN = 2;
	public const int Options_TAB_WIDTH_MAX = 4;

    private readonly TextEditorService Options_textEditorService;
    private readonly CommonUtilityService Options_commonUtilityService;
    
    private TextEditorOptionsState Options_textEditorOptionsState = new();

    private IDialog? Options_findAllDialog;

    /// <summary>
    /// Step 1: Notifies the TextEditorViewModelDisplay to recalculate `_componentData.SetWrapperCssAndStyle();`
    ///         and invoke `StateHasChanged()`.
    /// </summary>
	public event Action? Options_StaticStateChanged;
	/// <summary>
    /// Step 1: Notifies the WalkTextEditorInitializer to measure a tiny UI element that has the options applied to it.
    /// Step 2: WalkTextEditorInitializer then invokes `MeasuredStateChanged`.
    /// Step 3: TextEditorViewModelDisplay sees that second event fire, it enqueues a re-calculation of the virtualization result.
    /// Step 4: Eventually that virtualization result is finished and the editor re-renders.
    /// </summary>
	public event Action? Options_NeedsMeasured;
	/// <summary>
	/// Step 1: Notifies TextEditorViewModelDisplay to enqueue a re-calculation of the virtualization result.
	/// Step 2: Eventually that virtualization result is finished and the editor re-renders.
	/// </summary>
    public event Action? Options_MeasuredStateChanged;
    /// <summary>
    /// This event communicates from the text editor UI to the header and footer.
    /// </summary>
    public event Action? Options_TextEditorWrapperCssStateChanged;

	public TextEditorOptionsState Options_GetTextEditorOptionsState() => _textEditorOptionsState;

    public TextEditorOptions Options_GetOptions()
    {
        return _textEditorService.OptionsApi.GetTextEditorOptionsState().Options;
    }
    
    public void Options_InvokeTextEditorWrapperCssStateChanged()
    {
        TextEditorWrapperCssStateChanged?.Invoke();
    }

    public void Options_ShowSettingsDialog(bool? isResizableOverride = null, string? cssClassString = null)
    {
        // TODO: determine the actively focused element at time of invocation,
        //       then restore focus to that element when this dialog is closed.
        var settingsDialog = new DialogViewModel(
            Key<IDynamicViewModel>.NewKey(),
            "Text Editor Settings",
            _textEditorService.TextEditorConfig.SettingsDialogConfig.ComponentRendererType,
            null,
            cssClassString,
            isResizableOverride ?? _textEditorService.TextEditorConfig.SettingsDialogConfig.ComponentIsResizable,
            null);

        _commonUtilityService.Dialog_ReduceRegisterAction(settingsDialog);
    }

    public void Options_ShowFindAllDialog(bool? isResizableOverride = null, string? cssClassString = null)
    {
        // TODO: determine the actively focused element at time of invocation,
        //       then restore focus to that element when this dialog is closed.
        _findAllDialog ??= new DialogViewModel(
            Key<IDynamicViewModel>.NewKey(),
            "Find All",
            _textEditorService.TextEditorConfig.FindAllDialogConfig.ComponentRendererType,
            null,
            cssClassString,
            isResizableOverride ?? _textEditorService.TextEditorConfig.FindAllDialogConfig.ComponentIsResizable,
            null);

        _commonUtilityService.Dialog_ReduceRegisterAction(_findAllDialog);
    }

    public void Options_SetTheme(ThemeRecord theme, bool updateStorage = true)
    {
    	var inState = GetTextEditorOptionsState();
    
        _textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                CommonOptions = inState.Options.CommonOptions with
                {
                    ThemeKey = theme.Key
                },
                RenderStateKey = Key<RenderState>.NewKey(),
            },
        };
        
        // I'm optimizing all the expression bound properties that construct
		// a string, and specifically the ones that are rendered in the UI many times.
		//
		// Can probably use 'theme' variable here but
		// I don't want to touch that right now -- incase there are unexpected consequences.
        var usingThemeCssClassString = _commonUtilityService.GetThemeState().ThemeList
        	.FirstOrDefault(x => x.Key == GetTextEditorOptionsState().Options.CommonOptions.ThemeKey)
        	?.CssClassString
            ?? ThemeFacts.VisualStudioDarkThemeClone.CssClassString;
        _textEditorService.ThemeCssClassString = usingThemeCssClassString;
        
        StaticStateChanged?.Invoke();

        if (updateStorage)
            WriteToStorage();
    }

    public void Options_SetShowWhitespace(bool showWhitespace, bool updateStorage = true)
    {
    	var inState = GetTextEditorOptionsState();
    
        _textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                ShowWhitespace = showWhitespace,
                RenderStateKey = Key<RenderState>.NewKey(),
            },
        };
        // ShowWhitespace needs virtualization result to be re-calculated.
        MeasuredStateChanged?.Invoke();

        if (updateStorage)
            WriteToStorage();
    }

    public void Options_SetUseMonospaceOptimizations(bool useMonospaceOptimizations, bool updateStorage = true)
    {
    	var inState = GetTextEditorOptionsState();
    
		_textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                UseMonospaceOptimizations = useMonospaceOptimizations,
                RenderStateKey = Key<RenderState>.NewKey(),
            },
        };
        StaticStateChanged?.Invoke();
        
        if (updateStorage)
            WriteToStorage();
    }

    public void Options_SetShowNewlines(bool showNewlines, bool updateStorage = true)
    {
    	var inState = GetTextEditorOptionsState();
    
		_textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                ShowNewlines = showNewlines,
                RenderStateKey = Key<RenderState>.NewKey(),
            },
        };
        StaticStateChanged?.Invoke();
        
        if (updateStorage)
            WriteToStorage();
    }
    
    public void Options_SetTabKeyBehavior(bool tabKeyBehavior, bool updateStorage = true)
    {
    	var inState = GetTextEditorOptionsState();
    
		_textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                TabKeyBehavior = tabKeyBehavior,
                RenderStateKey = Key<RenderState>.NewKey(),
            },
        };
        StaticStateChanged?.Invoke();
        
        if (updateStorage)
            WriteToStorage();
    }
    
    public void Options_SetTabWidth(int tabWidth, bool updateStorage = true)
    {
    	if (tabWidth < TAB_WIDTH_MIN || tabWidth > TAB_WIDTH_MAX)
    		return;
    
    	var inState = GetTextEditorOptionsState();
    
		_textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                TabWidth = tabWidth,
                RenderStateKey = Key<RenderState>.NewKey(),
            },
        };
        MeasuredStateChanged?.Invoke();
        
        if (updateStorage)
            WriteToStorage();
    }

    public void Options_SetKeymap(ITextEditorKeymap keymap, bool updateStorage = true)
    {
    	var inState = GetTextEditorOptionsState();
    
        _textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                Keymap = keymap,
                RenderStateKey = Key<RenderState>.NewKey(),
            },
        };
        StaticStateChanged?.Invoke();

        /*var activeKeymap = _textEditorService.OptionsApi.GetTextEditorOptionsState().Options.Keymap;

        if (activeKeymap is not null)
        {
            _contextService.SetContextKeymap(
                ContextFacts.TextEditorContext.ContextKey,
                activeKeymap);
        }

        if (updateStorage)
            WriteToStorage();*/
    }

    public void Options_SetHeight(int? heightInPixels, bool updateStorage = true)
    {
    	var inState = GetTextEditorOptionsState();
    
        _textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                TextEditorHeightInPixels = heightInPixels,
                RenderStateKey = Key<RenderState>.NewKey(),
            },
        };
        StaticStateChanged?.Invoke();

        if (updateStorage)
            WriteToStorage();
    }

    public void Options_SetFontSize(int fontSizeInPixels, bool updateStorage = true)
    {
    	var inState = GetTextEditorOptionsState();
    
        _textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                CommonOptions = inState.Options.CommonOptions with
                {
                    FontSizeInPixels = fontSizeInPixels
                },
                RenderStateKey = Key<RenderState>.NewKey(),
            },
        };
        NeedsMeasured?.Invoke();

        if (updateStorage)
            WriteToStorage();
    }

    public void Options_SetFontFamily(string? fontFamily, bool updateStorage = true)
    {
    	var inState = GetTextEditorOptionsState();
    
        _textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                CommonOptions = inState.Options.CommonOptions with
                {
                    FontFamily = fontFamily
                },
                RenderStateKey = Key<RenderState>.NewKey(),
            },
        };
        NeedsMeasured?.Invoke();

        if (updateStorage)
            WriteToStorage();
    }

    public void Options_SetCursorWidth(double cursorWidthInPixels, bool updateStorage = true)
    {
    	var inState = GetTextEditorOptionsState();

        _textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                CursorWidthInPixels = cursorWidthInPixels,
                RenderStateKey = Key<RenderState>.NewKey(),
            },
        };
        StaticStateChanged?.Invoke();

        if (updateStorage)
            WriteToStorage();
    }

    public void Options_SetRenderStateKey(Key<RenderState> renderStateKey)
    {
    	var inState = GetTextEditorOptionsState();
    
        _textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                RenderStateKey = renderStateKey
            },
        };
        StaticStateChanged?.Invoke();
    }
    
    public void Options_SetCharAndLineMeasurements(TextEditorEditContext editContext, CharAndLineMeasurements charAndLineMeasurements)
    {
    	var inState = GetTextEditorOptionsState();

        _textEditorOptionsState = new TextEditorOptionsState
        {
            Options = inState.Options with
            {
                CharAndLineMeasurements = charAndLineMeasurements,
                RenderStateKey = Key<RenderState>.NewKey(),
            },
        };
        
    	MeasuredStateChanged?.Invoke();
    }

    public void Options_WriteToStorage()
    {
        _commonUtilityService.Enqueue(new CommonWorkArgs
        {
    		WorkKind = CommonWorkKind.WriteToLocalStorage,
        	WriteToLocalStorage_Key = _textEditorService.StorageKey,
            WriteToLocalStorage_Value = new TextEditorOptionsJsonDto(_textEditorService.OptionsApi.GetTextEditorOptionsState().Options),
        });
    }

    public async Task Options_SetFromLocalStorageAsync()
    {
        var optionsJsonString = await _commonUtilityService.Storage_GetValue(_textEditorService.StorageKey).ConfigureAwait(false) as string;

        if (string.IsNullOrWhiteSpace(optionsJsonString))
            return;

        var optionsJson = JsonSerializer.Deserialize<TextEditorOptionsJsonDto>(optionsJsonString);

        if (optionsJson is null)
            return;

        if (optionsJson.CommonOptionsJsonDto?.ThemeKey is not null)
        {
            var matchedTheme = _textEditorService.CommonUtilityService.GetThemeState().ThemeList.FirstOrDefault(
                x => x.Key == optionsJson.CommonOptionsJsonDto.ThemeKey);

            SetTheme(matchedTheme ?? ThemeFacts.VisualStudioDarkThemeClone, false);
        }

        /*if (optionsJson.Keymap is not null)
        {
            var matchedKeymap = TextEditorKeymapFacts.AllKeymapsList.FirstOrDefault(
                x => x.Key == optionsJson.Keymap.Key);

            SetKeymap(matchedKeymap ?? TextEditorKeymapFacts.DefaultKeymap, false);

            var activeKeymap = _textEditorService.OptionsApi.GetTextEditorOptionsState().Options.Keymap;

            if (activeKeymap is not null)
            {
                _contextService.SetContextKeymap(
                    ContextFacts.TextEditorContext.ContextKey,
                    activeKeymap);
            }
        }*/

        if (optionsJson.CommonOptionsJsonDto?.FontSizeInPixels is not null)
            SetFontSize(optionsJson.CommonOptionsJsonDto.FontSizeInPixels.Value, false);

        if (optionsJson.CursorWidthInPixels is not null)
            SetCursorWidth(optionsJson.CursorWidthInPixels.Value, false);

        if (optionsJson.TextEditorHeightInPixels is not null)
            SetHeight(optionsJson.TextEditorHeightInPixels.Value, false);

        if (optionsJson.ShowNewlines is not null)
        	SetShowNewlines(optionsJson.ShowNewlines.Value, false);
        
        if (optionsJson.TabKeyBehavior is not null)
            SetTabKeyBehavior(optionsJson.TabKeyBehavior.Value, false);
        
        if (optionsJson.TabWidth is not null)
            SetTabWidth(optionsJson.TabWidth.Value, false);

        // TODO: OptionsSetUseMonospaceOptimizations will always get set to false (default for bool)
        // for a first time user. This leads to a bad user experience since the proportional
        // font logic is still being optimized. Therefore don't read in UseMonospaceOptimizations
        // from local storage.
        //
        // OptionsSetUseMonospaceOptimizations(options.UseMonospaceOptimizations);

        if (optionsJson.ShowWhitespace is not null)
            SetShowWhitespace(optionsJson.ShowWhitespace.Value, false);
    }
    /* End OptionsApi */
}
