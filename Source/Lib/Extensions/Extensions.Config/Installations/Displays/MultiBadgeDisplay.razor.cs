using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Badges.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.TextEditor.RazorLib.Edits.Models;
using Walk.Extensions.DotNet;

namespace Walk.Extensions.Config.Installations.Displays;

public partial class MultiBadgeDisplay : ComponentBase, IDisposable
{
    [Inject]
    private DotNetService DotNetService { get; set; } = null!;
    
    private DirtyResourceUriBadge _dirtyResourceUriBadge;
    private int _dirtyResourceUriSeenCount;
    private string _dirtyResourceUriCountCssValue;
    
    private NotificationBadge _notificationBadge;
    private int _notificationSeenCount;
    private string _notificationCountCssValue;
    
    protected override void OnInitialized()
    {
        _dirtyResourceUriBadge = new Walk.TextEditor.RazorLib.Edits.Models.DirtyResourceUriBadge(DotNetService.TextEditorService);
        _notificationBadge = new Walk.Common.RazorLib.Notifications.Models.NotificationBadge(DotNetService.CommonService);
    
        _dirtyResourceUriBadge.AddSubscription(() => InvokeAsync(StateHasChanged));
        _notificationBadge.AddSubscription(() => InvokeAsync(StateHasChanged));
    }
    
    private string DirtyResourceUri_GetCountCssValue()
    {
        var localCount = _dirtyResourceUriBadge.Count;
        
        if (_dirtyResourceUriSeenCount != localCount)
        {
            _dirtyResourceUriSeenCount = localCount;
            _dirtyResourceUriCountCssValue = localCount.ToString();
        }
        
        return _dirtyResourceUriCountCssValue;
    }
    
    private string Notification_GetCountCssValue()
    {
        var localCount = _notificationBadge.Count;
        
        if (_notificationSeenCount != localCount)
        {
            _notificationSeenCount = localCount;
            _notificationCountCssValue = localCount.ToString();
        }
        
        return _notificationCountCssValue;
    }
    
    public void Dispose()
    {
        _dirtyResourceUriBadge.DisposeSubscription();
        _notificationBadge.DisposeSubscription();
    }
}
