using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Badges.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.TextEditor.RazorLib.Edits.Models;

public class DirtyResourceUriBadge : IBadgeModel
{
    public static readonly Key<IBadgeModel> DirtyResourceUriBadgeKey = Key<IBadgeModel>.NewKey();
    public static readonly Key<IDynamicViewModel> DialogRecordKey = Key<IDynamicViewModel>.NewKey();

    private readonly IDirtyResourceUriService _dirtyResourceUriService;
    private readonly CommonUtilityService _commonUtilityService;

    public DirtyResourceUriBadge(
        IDirtyResourceUriService dirtyResourceUriService,
        CommonUtilityService commonUtilityService)
    {
        _dirtyResourceUriService = dirtyResourceUriService;
        _commonUtilityService = commonUtilityService;
    }
    
    private Func<Task>? _updateUiFunc;

    public Key<IBadgeModel> Key => DirtyResourceUriBadgeKey;
	public BadgeKind BadgeKind => BadgeKind.DirtyResourceUri;
	public int Count => _dirtyResourceUriService.GetDirtyResourceUriState().DirtyResourceUriList.Count;
	
	public void OnClick()
	{
	    _commonUtilityService.Dialog_ReduceRegisterAction(new DialogViewModel(
            DialogRecordKey,
            "Unsaved Files",
            typeof(Walk.TextEditor.RazorLib.Edits.Displays.DirtyResourceUriViewDisplay),
            null,
            null,
    		true,
    		setFocusOnCloseElementId: null));
	}
	
	public void AddSubscription(Func<Task> updateUiFunc)
	{
	    _updateUiFunc = updateUiFunc;
	    _dirtyResourceUriService.DirtyResourceUriStateChanged += DoSubscription;
	}
	
	public async void DoSubscription()
	{
	    var localUpdateUiFunc = _updateUiFunc;
	    if (_updateUiFunc is not null)
	        await _updateUiFunc.Invoke();
	}
	
	public void DisposeSubscription()
	{
	    _dirtyResourceUriService.DirtyResourceUriStateChanged -= DoSubscription;
	    _updateUiFunc = null;
	}
}
