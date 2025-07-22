using Microsoft.AspNetCore.Components;

namespace Walk.Extensions.DotNet.DotNetSolutions.Displays.Internals;

public partial class SolutionPropertiesDisplay : ComponentBase, IDisposable
{
    [Inject]
    private DotNetService DotNetService { get; set; } = null!;
    
    protected override void OnInitialized()
    {
        DotNetService.DotNetStateChanged += OnDotNetSolutionStateChanged;
    }
    
    private async void OnDotNetSolutionStateChanged(DotNetStateChangedKind dotNetStateChangedKind)
    {
        if (dotNetStateChangedKind == DotNetStateChangedKind.SolutionStateChanged)
        {
            await InvokeAsync(StateHasChanged);
        }
    }
    
    public void Dispose()
    {
        DotNetService.DotNetStateChanged -= OnDotNetSolutionStateChanged;
    }
}
