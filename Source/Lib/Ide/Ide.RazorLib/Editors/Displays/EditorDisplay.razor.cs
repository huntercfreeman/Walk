using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Tabs.Displays;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Groups.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;
using Walk.TextEditor.RazorLib.TextEditors.Displays.Internals;
using Walk.Ide.RazorLib.BackgroundTasks.Models;

namespace Walk.Ide.RazorLib.Editors.Displays;

public partial class EditorDisplay : ComponentBase, IDisposable
{
	[Inject]
    private IdeService IdeService { get; set; } = null!;

    [Parameter, EditorRequired]
    public ElementDimensions EditorElementDimensions { get; set; } = null!;
    
    private static readonly List<HeaderButtonKind> TextEditorHeaderButtonKindsList =
        Enum.GetValues(typeof(HeaderButtonKind))
            .Cast<HeaderButtonKind>()
            .ToList();

    private ViewModelDisplayOptions _viewModelDisplayOptions = null!;

	private TabListDisplay? _tabListDisplay;

	private string? _htmlId = null;
	private string HtmlId => _htmlId ??= $"di_te_group_{IdeService.EditorTextEditorGroupKey.Guid}";
	
	private Key<TextEditorViewModel> _previousActiveViewModelKey = Key<TextEditorViewModel>.Empty;
	
	private Key<TextEditorComponentData> _componentDataKey;

    protected override void OnInitialized()
    {
    	_viewModelDisplayOptions = new()
        {
            TabIndex = 0,
            HeaderButtonKinds = TextEditorHeaderButtonKindsList,
            HeaderComponentType = typeof(TextEditorFileExtensionHeaderDisplay),
            TextEditorHtmlElementId = Guid.NewGuid(),
        };
    
        _componentDataKey = new Key<TextEditorComponentData>(_viewModelDisplayOptions.TextEditorHtmlElementId);
        
        IdeService.TextEditorService.Group_TextEditorGroupStateChanged += TextEditorGroupWrapOnStateChanged;
        IdeService.TextEditorService.DirtyResourceUriStateChanged += DirtyResourceUriServiceOnStateChanged;
    }

    private async void TextEditorGroupWrapOnStateChanged()
    {
    	var textEditorGroup = IdeService.TextEditorService.Group_GetTextEditorGroupState().GroupList.FirstOrDefault(
	        x => x.GroupKey == IdeService.EditorTextEditorGroupKey);
	        
	    if (_previousActiveViewModelKey != textEditorGroup.ActiveViewModelKey)
	    {
	    	_previousActiveViewModelKey = textEditorGroup.ActiveViewModelKey;
	    	IdeService.TextEditorService.ViewModel_StopCursorBlinking();
	    }
    
        await InvokeAsync(StateHasChanged);
    }
    
    private async void DirtyResourceUriServiceOnStateChanged()
    {
		var localTabListDisplay = _tabListDisplay;
		
		if (localTabListDisplay is not null)
		{
			await localTabListDisplay.NotifyStateChangedAsync();
		}
    }

	private List<ITab> GetTabList(TextEditorGroup textEditorGroup)
	{
        var textEditorState = IdeService.TextEditorService.TextEditorState;
		var tabList = new List<ITab>();

		foreach (var viewModelKey in textEditorGroup.ViewModelKeyList)
		{
            var viewModel = textEditorState.ViewModelGetOrDefault(viewModelKey);
            
            if (viewModel is not null)
            {
                viewModel.PersistentState.TabGroup = textEditorGroup;
				tabList.Add(viewModel.PersistentState);
            }
		}

		return tabList;
	}

    public void Dispose()
    {
        IdeService.TextEditorService.Group_TextEditorGroupStateChanged -= TextEditorGroupWrapOnStateChanged;
        IdeService.TextEditorService.DirtyResourceUriStateChanged -= DirtyResourceUriServiceOnStateChanged;
    }
}