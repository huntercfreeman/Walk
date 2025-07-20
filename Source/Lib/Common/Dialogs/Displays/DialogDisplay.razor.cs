using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Resizes.Displays;
using Walk.Common.RazorLib.Dynamics.Models;

namespace Walk.Common.RazorLib.Dialogs.Displays;

public partial class DialogDisplay : ComponentBase, IDisposable
{
    [Inject]
    private CommonService CommonService { get; set; } = null!;

    [Parameter, EditorRequired]
    public IDialog Dialog { get; set; } = null!;

	private const int COUNT_OF_CONTROL_BUTTONS = 2;

    private ResizableDisplay? _resizableDisplay;

    private string IsMaximizedStyleCssString => Dialog.DialogIsMaximized
        ? CommonService.CommonConfig.IsMaximizedStyleCssString
        : string.Empty;

    private string IconSizeInPixelsCssValue =>
        $"{CommonService.GetAppOptionsState().Options.IconSizeInPixels.ToCssValue()}";

    private string DialogTitleCssStyleString =>
        "width: calc(100% -" +
        $" ({COUNT_OF_CONTROL_BUTTONS} * ({IconSizeInPixelsCssValue}px)) -" +
        $" ({COUNT_OF_CONTROL_BUTTONS} * ({CommonFacts.HtmlFacts_Button_ButtonPaddingHorizontalTotalInPixelsCssValue})));";

    protected override void OnInitialized()
    {
        CommonService.AppOptionsStateChanged += AppOptionsStateWrapOnStateChanged;
        CommonService.CommonUiStateChanged += OnCommonUiStateChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await CommonService.JsRuntimeCommonApi
                .FocusHtmlElementById(Dialog.DialogFocusPointHtmlElementId)
                .ConfigureAwait(false);
        }
    }

    private async void OnCommonUiStateChanged(CommonUiEventKind commonUiEventKind)
    {
        if (commonUiEventKind == CommonUiEventKind.ActiveDialogKeyChanged)
            await InvokeAsync(StateHasChanged);
    }

    private async void AppOptionsStateWrapOnStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    private Task ReRenderAsync()
    {
        return InvokeAsync(StateHasChanged);
    }

    private Task SubscribeMoveHandleAsync()
    {
        _resizableDisplay?.SubscribeToDragEventWithMoveHandle();
        return Task.CompletedTask;
    }

    private void ToggleIsMaximized()
    {
        CommonService.Dialog_ReduceSetIsMaximizedAction(
            Dialog.DynamicViewModelKey,
            !Dialog.DialogIsMaximized);
    }

    private async Task DispatchDisposeDialogRecordAction()
    {
        CommonService.Dialog_ReduceDisposeAction(Dialog.DynamicViewModelKey);
        
        await CommonService.JsRuntimeCommonApi
	        .FocusHtmlElementById(Dialog.SetFocusOnCloseElementId
	        	 ?? IDynamicViewModel.DefaultSetFocusOnCloseElementId)
	        .ConfigureAwait(false);
    }

    private string GetCssClassForDialogStateIsActiveSelection(bool isActive)
    {
        return isActive
            ? "di_active"
            : string.Empty;
    }

    private Task HandleOnFocusIn()
    {
        CommonService.Dialog_ReduceSetActiveDialogKeyAction(Dialog.DynamicViewModelKey);
        return Task.CompletedTask;
    }
    
	private Task HandleOnFocusOut()
    {
    	return Task.CompletedTask;
    }

    private void HandleOnMouseDown()
    {
        CommonService.Dialog_ReduceSetActiveDialogKeyAction(Dialog.DynamicViewModelKey);
    }

    public void Dispose()
    {
        CommonService.AppOptionsStateChanged -= AppOptionsStateWrapOnStateChanged;
        CommonService.CommonUiStateChanged -= OnCommonUiStateChanged;
    }
}