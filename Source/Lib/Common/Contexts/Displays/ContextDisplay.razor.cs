using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Common.RazorLib.Contexts.Displays;

public partial class ContextDisplay : ComponentBase
{
    [Inject]
    private IContextService ContextService { get; set; } = null!;
    [Inject]
    private IAppOptionsService AppOptionsService { get; set; } = null!;

    [Parameter, EditorRequired]
    public Key<ContextRecord> ContextKey { get; set; }

    private bool _isExpanded;
}