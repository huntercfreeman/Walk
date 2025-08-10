using Walk.Common.RazorLib;
using Walk.Ide.RazorLib;
using Walk.TextEditor.RazorLib;

namespace Walk.Extensions.Config.Installations.Displays;

public partial class IdeMainLayout
{
    public void SubscribeEvents()
    {
        DotNetService.CommonService.CommonUiStateChanged += OnCommonUiStateChanged;
        DotNetService.CommonService.CommonUiStateChanged += DragStateWrapOnStateChanged;
        DotNetService.IdeService.IdeStateChanged += OnIdeMainLayoutStateChanged;
        DotNetService.TextEditorService.SecondaryChanged += TextEditorOptionsStateWrap_StateChanged;
        DotNetService.TextEditorService.SecondaryChanged += OnNeedsMeasured;
    }
    
    public void DisposeEvents()
    {
        DotNetService.CommonService.CommonUiStateChanged -= DragStateWrapOnStateChanged;
        DotNetService.CommonService.CommonUiStateChanged -= OnCommonUiStateChanged;
        DotNetService.IdeService.IdeStateChanged -= OnIdeMainLayoutStateChanged;
        DotNetService.TextEditorService.SecondaryChanged -= TextEditorOptionsStateWrap_StateChanged;
        DotNetService.TextEditorService.SecondaryChanged -= OnNeedsMeasured;
    }

    private async void OnCommonUiStateChanged(CommonUiEventKind commonUiEventKind)
    {
        switch (commonUiEventKind)
        {
            case CommonUiEventKind.DialogStateChanged:
            case CommonUiEventKind.WidgetStateChanged:
            case CommonUiEventKind.NotificationStateChanged:
            case CommonUiEventKind.DropdownStateChanged:
            case CommonUiEventKind.OutlineStateChanged:
            case CommonUiEventKind.TooltipStateChanged:
                await InvokeAsync(StateHasChanged);
                break;
            case CommonUiEventKind.LineHeightNeedsMeasured:
                await InvokeAsync(MeasureLineHeight_UiRenderStep);
                break;
            case CommonUiEventKind.PanelStateChanged:
                await InvokeAsync(StateHasChanged);
                break;
        }

        Console.WriteLine(commonUiEventKind.ToString());
    }

    private async void DragStateWrapOnStateChanged(CommonUiEventKind commonUiEventKind)
    {
        if (commonUiEventKind == CommonUiEventKind.AppOptionsStateChanged)
        {
            await InvokeAsync(() =>
            {
                _shouldRecalculateCssStrings = true;
                StateHasChanged();
            }).ConfigureAwait(false);
        }

        if (commonUiEventKind != CommonUiEventKind.DragStateChanged)
            return;

        if (_previousDragStateWrapShouldDisplay != DotNetService.CommonService.GetDragState().ShouldDisplay)
        {
            _previousDragStateWrapShouldDisplay = DotNetService.CommonService.GetDragState().ShouldDisplay;
            await InvokeAsync(() =>
            {
                _shouldRecalculateCssStrings = true;
                StateHasChanged();
            }).ConfigureAwait(false);
        }
    }

    private async void OnIdeMainLayoutStateChanged(IdeStateChangedKind ideStateChangedKind)
    {
        if (ideStateChangedKind == IdeStateChangedKind.Ide_IdeStateChanged)
        {
            await InvokeAsync(StateHasChanged).ConfigureAwait(false);
        }
    }

    private async void TextEditorOptionsStateWrap_StateChanged(SecondaryChangedKind secondaryChangedKind)
    {
        if (secondaryChangedKind == SecondaryChangedKind.StaticStateChanged)
        {
            await InvokeAsync(() =>
            {
                _shouldRecalculateCssStrings = true;
                StateHasChanged();
            }).ConfigureAwait(false);
        }
    }

    private async void OnNeedsMeasured(SecondaryChangedKind secondaryChangedKind)
    {
        if (secondaryChangedKind == SecondaryChangedKind.NeedsMeasured)
        {
            await InvokeAsync(Ready);
        }
    }
}
