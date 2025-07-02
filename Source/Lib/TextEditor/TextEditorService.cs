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

namespace Walk.TextEditor.RazorLib;

public sealed class TextEditorService
{
    private readonly IDirtyResourceUriService _dirtyResourceUriService;
    private readonly ITextEditorRegistryWrap _textEditorRegistryWrap;
    private readonly IJSRuntime _jsRuntime;
    private readonly IServiceProvider _serviceProvider;

    public TextEditorService(
        IFindAllService findAllService,
        IDirtyResourceUriService dirtyResourceUriService,
        WalkTextEditorConfig textEditorConfig,
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
        _textEditorRegistryWrap = textEditorRegistryWrap;
        _jsRuntime = jsRuntime;
		JsRuntimeTextEditorApi = _jsRuntime.GetWalkTextEditorApi();

        ModelApi = new TextEditorModelApi(this, _textEditorRegistryWrap, CommonUtilityService);
        ViewModelApi = new TextEditorViewModelApi(this, CommonUtilityService);
        GroupApi = new TextEditorGroupApi(this, CommonUtilityService);
        DiffApi = new TextEditorDiffApi(this);
        OptionsApi = new TextEditorOptionsApi(this, TextEditorConfig, CommonUtilityService);
        
        TextEditorState = new();
    }

    public CommonUtilityService CommonUtilityService { get; }
    public IFindAllService FindAllService { get; }

	public WalkTextEditorJavaScriptInteropApi JsRuntimeTextEditorApi { get; }
	public WalkCommonJavaScriptInteropApi JsRuntimeCommonApi => CommonUtilityService.JsRuntimeCommonApi;
	public WalkTextEditorConfig TextEditorConfig { get; }

#if DEBUG
    public string StorageKey => "di_te_text-editor-options-debug";
#else
    public string StorageKey => "di_te_text-editor-options";
#endif

    public string ThemeCssClassString { get; set; }

    public TextEditorModelApi ModelApi { get; }
    public TextEditorViewModelApi ViewModelApi { get; }
    public TextEditorGroupApi GroupApi { get; }
    public TextEditorDiffApi DiffApi { get; }
    public TextEditorOptionsApi OptionsApi { get; }
    
    public TextEditorState TextEditorState { get; }
    
    public TextEditorWorkerUi WorkerUi { get; }
	public TextEditorWorkerArbitrary WorkerArbitrary { get; }
    
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
        	if (viewModelModifier.PersistentState.ShouldRevealCursor || viewModelModifier.Virtualization.ShouldCalculateVirtualizationResult || viewModelModifier.ScrollWasModified || viewModelModifier.Changed_Cursor_AnyState)
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
			else if (viewModelModifier.Changed_Cursor_AnyState)
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
			        viewModelModifier.Changed_Cursor_AnyState = false;
			    
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
			.Invoke(new RegisterModelArgs(editContext, resourceUri, _serviceProvider))
			.ConfigureAwait(false);
	
		// TryRegisterViewModelFunc
		if (TextEditorConfig.TryRegisterViewModelFunc is null)
			return Key<TextEditorViewModel>.Empty;
		var actualViewModelKey = await TextEditorConfig.TryRegisterViewModelFunc
			.Invoke(new TryRegisterViewModelArgs(editContext, preferredViewModelKey, resourceUri, category, shouldSetFocusToEditor, _serviceProvider))
			.ConfigureAwait(false);
	
		// TryShowViewModelFunc
		if (actualViewModelKey == Key<TextEditorViewModel>.Empty || TextEditorConfig.TryShowViewModelFunc is null)
			return Key<TextEditorViewModel>.Empty;
		await TextEditorConfig.TryShowViewModelFunc
			.Invoke(new TryShowViewModelArgs(actualViewModelKey, Key<TextEditorGroup>.Empty, shouldSetFocusToEditor, _serviceProvider))
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
}
