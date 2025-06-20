using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Reflectives.Models;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Common.RazorLib.Reflectives.Displays;

public partial class ReflectiveDisplay : ComponentBase
{
    [Inject]
    private IReflectiveService ReflectiveService { get; set; } = null!;
    [Inject]
    private IAppOptionsService AppOptionsService { get; set; } = null!;

    [Parameter, EditorRequired]
    public Key<ReflectiveModel> ReflectiveModelKey { get; set; }
    [Parameter, EditorRequired]
    public int Index { get; set; }
    [Parameter, EditorRequired]
    public List<Type> ComponentTypeList { get; set; } = null!;

    private ErrorBoundary _errorBoundaryComponent = null!;
    private ElementReference _selectElementReference;
    private int _chosenComponentChangeCounter;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_chosenComponentChangeCounter > 0)
        {
            _chosenComponentChangeCounter = 0;

            // Invoke 'Recover' directly, do not use 'WrapRecover' here.
            _errorBoundaryComponent.Recover();

            await _selectElementReference.FocusAsync().ConfigureAwait(false);
        }
    }

    private void OnSelectChanged(ChangeEventArgs changeEventArgs)
    {
        var model = ReflectiveService.GetReflectiveModel(ReflectiveModelKey);

        if (model is null)
            return;

        var chosenTypeGuidString = changeEventArgs.Value as string;

        model.CalculateComponentPropertyInfoList(chosenTypeGuidString, ref _chosenComponentChangeCounter);
    }

    private void WrapRecover()
    {
        var displayState = ReflectiveService.GetReflectiveModel(ReflectiveModelKey);

        if (displayState is null)
            return;

        ReflectiveService.With(
            displayState.Key, inDisplayState => inDisplayState with { });

        _errorBoundaryComponent.Recover();
    }

    private void DispatchDisposeAction(ReflectiveModel reflectiveModel)
    {
        ReflectiveService.Dispose(reflectiveModel.Key);
    }

    private void DispatchRegisterAction(int insertionIndex)
    {
        var model = new ReflectiveModel(
                Key<ReflectiveModel>.NewKey(),
                ComponentTypeList,
                Guid.Empty,
                Guid.Empty,
                Array.Empty<PropertyInfo>(),
                new(),
                ReflectiveService);

        ReflectiveService.Register(model, insertionIndex);
    }

    private bool GetIsOptionSelected(ReflectiveModel model, Guid typeGuid)
    {
        return typeGuid == model.ChosenTypeGuid;
    }
}
