using System.Reflection;
using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Reflectives.Models;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Common.RazorLib.Reflectives.Displays;

public partial class ReflectiveConstructor : ComponentBase
{
    [Inject]
    private IAppOptionsService AppOptionsService { get; set; } = null!;
    
    [CascadingParameter, EditorRequired]
    public ReflectiveModel DisplayState { get; set; } = null!;

    [Parameter, EditorRequired]
    public ConstructorInfo ConstructorInfo { get; set; } = null!;
    [Parameter, EditorRequired]
    public bool IsChosenConstructor { get; set; }
    [Parameter, EditorRequired]
    public Action<ConstructorInfo> OnClick { get; set; } = null!;
    [Parameter, EditorRequired]
    public Action OnUnsetChosenConstructorInfo { get; set; } = null!;
    [Parameter, EditorRequired]
    public string ParametersKey { get; set; } = null!;

    private string IsActiveCssClass => IsChosenConstructor
        ? "di_active"
        : string.Empty;

    private void InvokeOnClick()
    {
        OnClick.Invoke(ConstructorInfo);
    }

    private void InvokeOnUnsetChosenConstructorInfo()
    {
        OnUnsetChosenConstructorInfo.Invoke();
    }
}