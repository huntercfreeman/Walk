using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.Common.RazorLib.Dynamics.Models;

namespace Walk.Common.RazorLib.Reactives.Displays;

public partial class ProgressBarDisplay : ComponentBase, IDisposable
{
    [Inject]
    private CommonService CommonService { get; set; } = null!;

    [CascadingParameter]
    public INotification? Notification { get; set; } = null!;

    [Parameter, EditorRequired]
    public ProgressBarModel ProgressBarModel { get; set; } = null!;

    private bool _hasSeenProgressModelIsDisposed = false;
    
    private int _fillWidthInPercentage;
    private int _cancelButtonWidthInPercentage;
    
    private string _progressBarFillCssStyle;
    private string _cancelButtonContainingDivCssStyle;

    protected override void OnInitialized()
    {
    	_fillWidthInPercentage = 100;
    	_cancelButtonWidthInPercentage = 30;
    	
    	_progressBarFillCssStyle = $"position: relative; height: 2em; width: {_fillWidthInPercentage}%;";
    	_cancelButtonContainingDivCssStyle = $"width: {_cancelButtonWidthInPercentage}%;";
    	
    	if (ProgressBarModel.IsCancellable)
    		_fillWidthInPercentage -= _cancelButtonWidthInPercentage;
    
        if (!ProgressBarModel.IsDisposed)
            ProgressBarModel.ProgressChanged += OnProgressChanged;
    }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (Notification is not null && !_hasSeenProgressModelIsDisposed && ProgressBarModel.IsDisposed)
        {
            _hasSeenProgressModelIsDisposed = true;

            _ = Task.Run((Func<Task?>)(async () =>
            {
                await Task.Delay(4_000);
                
                CommonService.Notification_ReduceDisposeAction(Notification.DynamicViewModelKey);
            }));
        }
        
        return Task.CompletedTask;
    }

    public async void OnProgressChanged(bool isDisposing)
    {
        if (isDisposing)
            ProgressBarModel.ProgressChanged -= OnProgressChanged;

        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        ProgressBarModel.ProgressChanged -= OnProgressChanged;
    }
}
