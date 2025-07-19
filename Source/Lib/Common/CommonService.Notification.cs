using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.ListExtensions;

namespace Walk.Common.RazorLib;

public partial class CommonService
{
	private NotificationState _notificationState = new();
	
	public NotificationState GetNotificationState() => _notificationState;

    public void Notification_ReduceRegisterAction(INotification notification)
    {
    	lock (_stateModificationLock)
    	{
	        var outDefaultList = new List<INotification>(_notificationState.DefaultList);
	        outDefaultList.Add(notification);
	        _notificationState = _notificationState with { DefaultList = outDefaultList };
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }

    public void Notification_ReduceDisposeAction(Key<IDynamicViewModel> key)
    {
    	lock (_stateModificationLock)
    	{
	        var indexNotification = _notificationState.DefaultList.FindIndex(
	            x => x.DynamicViewModelKey == key);
	
	        if (indexNotification != -1)
	        {
    	        var inNotification = _notificationState.DefaultList[indexNotification];
    	        var outDefaultList = new List<INotification>(_notificationState.DefaultList);
    	        outDefaultList.RemoveAt(indexNotification);
    	        _notificationState = _notificationState with { DefaultList = outDefaultList };
	        }
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }

    public void Notification_ReduceMakeReadAction(Key<IDynamicViewModel> key)
    {
    	lock (_stateModificationLock)
    	{
	        var inNotificationIndex = _notificationState.DefaultList.FindIndex(
	            x => x.DynamicViewModelKey == key);
	
	        if (inNotificationIndex != -1)
	        {
    	        var inNotification = _notificationState.DefaultList[inNotificationIndex];
    	
    	        var outDefaultList = new List<INotification>(_notificationState.DefaultList);
    	        outDefaultList.RemoveAt(inNotificationIndex);
    	        
    	        var outReadList = new List<INotification>(_notificationState.ReadList);
    	        outReadList.Add(inNotification);
    	
    	        _notificationState = _notificationState with
    	        {
    	            DefaultList = outDefaultList,
    	            ReadList = outReadList
    	        };
	        }
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }
    
    public void Notification_ReduceUndoMakeReadAction(Key<IDynamicViewModel> key)
    {
    	lock (_stateModificationLock)
    	{
	        var inNotificationIndex = _notificationState.ReadList.FindIndex(
	            x => x.DynamicViewModelKey == key);
	
	        if (inNotificationIndex != -1)
	        {
    	        var inNotification = _notificationState.ReadList[inNotificationIndex];
    	
    	        var outReadList = new List<INotification>(_notificationState.ReadList);
    	        outReadList.RemoveAt(inNotificationIndex);
    	        
    	        var outDefaultList = new List<INotification>(_notificationState.DefaultList);
    	        outDefaultList.Add(inNotification);
    	
    	        _notificationState = _notificationState with
    	        {
    	            DefaultList = outDefaultList,
    	            ReadList = outReadList
    	        };
    	    }
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }

    public void Notification_ReduceMakeDeletedAction(Key<IDynamicViewModel> key)
    {
    	lock (_stateModificationLock)
    	{
	        var inNotificationIndex = _notificationState.DefaultList.FindIndex(
	            x => x.DynamicViewModelKey == key);
	
	        if (inNotificationIndex != -1)
	        {
    	        var inNotification = _notificationState.DefaultList[inNotificationIndex];
    	
    	        var outDefaultList = new List<INotification>(_notificationState.DefaultList);
    	        outDefaultList.RemoveAt(inNotificationIndex);
    	        
    	        var outDeletedList = new List<INotification>(_notificationState.DeletedList);
    	        outDeletedList.Add(inNotification);
    	
    	        _notificationState = _notificationState with
    	        {
    	            DefaultList = outDefaultList,
    	            DeletedList = outDeletedList
    	        };
	        }
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }

    public void Notification_ReduceUndoMakeDeletedAction(Key<IDynamicViewModel> key)
    {
    	lock (_stateModificationLock)
    	{
	        var inNotificationIndex = _notificationState.DeletedList.FindIndex(
	            x => x.DynamicViewModelKey == key);
	
	        if (inNotificationIndex != -1)
	        {
    	        var inNotification = _notificationState.DeletedList[inNotificationIndex];
    	
    	        var outDeletedList = new List<INotification>(_notificationState.DeletedList);
    	        outDeletedList.RemoveAt(inNotificationIndex);
    	
    	        var outDefaultList = new List<INotification>(_notificationState.DefaultList);
    	        outDefaultList.Add(inNotification);
    	
    	        _notificationState = _notificationState with
    	        {
    	            DefaultList = outDefaultList,
    	            DeletedList = outDeletedList
    	        };
	        }
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }

    public void Notification_ReduceMakeArchivedAction(Key<IDynamicViewModel> key)
    {
    	lock (_stateModificationLock)
    	{
	        var inNotificationIndex = _notificationState.DefaultList.FindIndex(
	            x => x.DynamicViewModelKey == key);
	
	        if (inNotificationIndex != -1)
	        {
    	        var inNotification = _notificationState.DefaultList[inNotificationIndex];
    	
    	        var outDefaultList = new List<INotification>(_notificationState.DefaultList);
    	        outDefaultList.RemoveAt(inNotificationIndex);
    	
    	        var outArchivedList = new List<INotification>(_notificationState.ArchivedList);
    	        outArchivedList.Add(inNotification);
    	
    	        _notificationState = _notificationState with
    	        {
    	            DefaultList = outDefaultList,
    	            ArchivedList = outArchivedList
    	        };
	        }
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }
    
    public void Notification_ReduceUndoMakeArchivedAction(Key<IDynamicViewModel> key)
    {
    	lock (_stateModificationLock)
    	{
	        var inNotificationIndex = _notificationState.ArchivedList.FindIndex(
	            x => x.DynamicViewModelKey == key);
	
	        if (inNotificationIndex != -1)
	        {
    	        var inNotification = _notificationState.ArchivedList[inNotificationIndex];
    	
    	        var outArchivedList = new List<INotification>(_notificationState.ArchivedList);
    	        outArchivedList.RemoveAt(inNotificationIndex);
    	        
    	        var outDefaultList = new List<INotification>(_notificationState.DefaultList);
    	        outDefaultList.Add(inNotification);
    	
    	        _notificationState = _notificationState with
    	        {
    	            DefaultList = outDefaultList,
    	            ArchivedList = outArchivedList
    	        };
	        }
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }

    public void Notification_ReduceClearDefaultAction()
    {
    	lock (_stateModificationLock)
    	{
	        _notificationState = _notificationState with
	        {
	            DefaultList = new List<INotification>()
	        };
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }
    
    public void Notification_ReduceClearReadAction()
    {
    	lock (_stateModificationLock)
    	{
	        _notificationState = _notificationState with
	        {
	            ReadList = new List<INotification>()
	        };
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }
    
    public void Notification_ReduceClearDeletedAction()
    {
    	lock (_stateModificationLock)
    	{
	        _notificationState = _notificationState with
	        {
	            DeletedList = new List<INotification>()
	        };
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }

    public void Notification_ReduceClearArchivedAction()
    {
    	lock (_stateModificationLock)
    	{
	        _notificationState = _notificationState with
	        {
	            ArchivedList = new List<INotification>()
	        };
	    }
	    
	    CommonUiStateChanged?.Invoke(CommonUiEventKind.NotificationStateChanged);
    }
}
