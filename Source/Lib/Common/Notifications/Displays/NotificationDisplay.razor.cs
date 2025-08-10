using System.Text;
using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Dynamics.Models;

namespace Walk.Common.RazorLib.Notifications.Displays;

public partial class NotificationDisplay : ComponentBase, IDisposable
{
    [Inject]
    private CommonService CommonService { get; set; } = null!;

    [Parameter, EditorRequired]
    public INotification Notification  { get; set; } = null!;
    [Parameter, EditorRequired]
    public int Index { get; set; }

    //private const int WIDTH_IN_PIXELS = 350;
    private const int WIDTH_IN_FONT_WIDTH = 40;
    // private const int HEIGHT_IN_PIXELS = 125;
    private const int HEIGHT_IN_FONT_HEIGHT = 8;
    private const int RIGHT_OFFSET_IN_PIXELS = 15;
    private const int BOTTOM_OFFSET_IN_PIXELS = 15;

    private const int COUNT_OF_CONTROL_BUTTONS = 2;

    private readonly CancellationTokenSource _notificationOverlayCancellationTokenSource = new();
    private readonly Key<IDialog> _dialogKey = Key<IDialog>.NewKey();

    private string CssStyleString => GetCssStyleString();

    private string IconSizeInPixelsCssValue =>
        $"{CommonService.GetAppOptionsState().Options.IconSizeInPixels.ToCssValue()}";

    private string NotificationTitleCssStyleString =>
        "width: calc(100% -" +
        $" ({COUNT_OF_CONTROL_BUTTONS} * ({IconSizeInPixelsCssValue}px)) -" +
        $" ({COUNT_OF_CONTROL_BUTTONS} * ({CommonFacts.HtmlFacts_Button_ButtonPaddingHorizontalTotalInPixelsCssValue})));";

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var localNotification = Notification;

            if (localNotification.NotificationOverlayLifespan is not null)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(
                                localNotification.NotificationOverlayLifespan.Value,
                                _notificationOverlayCancellationTokenSource.Token)
                            .ConfigureAwait(false);

                        await HandleShouldNoLongerRender(wasCausedByUiEvent: false);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }, CancellationToken.None);
            }
        }
        
        return Task.CompletedTask;
    }

    private string GetCssStyleString()
    {
        var styleBuilder = new StringBuilder();

        var widthInFontWidthInvariantCulture = WIDTH_IN_FONT_WIDTH.ToCssValue();
        //var widthInPixelsInvariantCulture = WIDTH_IN_PIXELS.ToCssValue();
        var heightInFontHeightInvariantCulture = HEIGHT_IN_FONT_HEIGHT.ToCssValue();
        //var heightInPixelsInvariantCulture = HEIGHT_IN_PIXELS.ToCssValue();

        styleBuilder.Append($" width: {widthInFontWidthInvariantCulture}ch;");
        styleBuilder.Append($" height: {heightInFontHeightInvariantCulture}em;");

        var rightOffsetInPixelsInvariantCulture = RIGHT_OFFSET_IN_PIXELS.ToCssValue();

        styleBuilder.Append($" right: {rightOffsetInPixelsInvariantCulture}px;");

        var bottomOffsetDueToHeightInFontHeight = HEIGHT_IN_FONT_HEIGHT * Index;
        //var bottomOffsetDueToHeight = HEIGHT_IN_FONT_HEIGHT * Index;

        var bottomOffsetDueToBottomOffsetInPixels = BOTTOM_OFFSET_IN_PIXELS * (1 + Index);
        //var bottomOffsetDueToBottomOffset = BOTTOM_OFFSET_IN_PIXELS * (1 + Index);

        // The bottom panel has:
        //     height="calc(var(--di_panel-tabs-font-size) + var(--di_panel-tabs-margin) + var(--di_panel-tabs-bug-are-not-aligning-need-to-fix-todo))"
        //
        // The height for the bottom panel might need to change? This is pretty gross.
        // I want to add a notifications counter in the bottom panel on the horizontal end.
        // So I'm putting this here to align things.
        //
        // And changing:
        //     'styleBuilder.Append($" bottom: {totalBottomOffsetInvariantCulture}px;");'
        //
        // to include the bottom panel height in the calculation.
        var panelBottomHeight = "0.85em + 4px + 0.7em";

        var bottomOffsetDueToHeightInFontHeightInvariantCulture = bottomOffsetDueToHeightInFontHeight.ToCssValue();
        var bottomOffsetDueToBottomOffsetInPixelsInvariantCulture = bottomOffsetDueToBottomOffsetInPixels.ToCssValue();

        //var totalBottomOffsetInvariantCulture = totalBottomOffset.ToCssValue();

        styleBuilder.Append($" bottom: calc({bottomOffsetDueToHeightInFontHeightInvariantCulture}em + {bottomOffsetDueToBottomOffsetInPixelsInvariantCulture}px + {panelBottomHeight}) ;");

        return styleBuilder.ToString();
    }

    private async Task HandleShouldNoLongerRender(bool wasCausedByUiEvent)
    {
        if (Notification.DeleteNotificationAfterOverlayIsDismissed)
            DeleteNotification();
        else
            MarkNotificationAsRead();
            
        if (wasCausedByUiEvent)
        {
            await CommonService.JsRuntimeCommonApi
                .FocusHtmlElementById(Notification.SetFocusOnCloseElementId
                     ?? IDynamicViewModel.DefaultSetFocusOnCloseElementId)
                .ConfigureAwait(false);
        }
    }

    private void DeleteNotification()
    {
        CommonService.Notification_ReduceMakeDeletedAction(Notification.DynamicViewModelKey);
    }

    private void MarkNotificationAsRead()
    {
        CommonService.Notification_ReduceMakeReadAction(Notification.DynamicViewModelKey);
    }

    private Task ChangeNotificationToDialog()
    {
        var dialogRecord = new DialogViewModel(
            Notification.DynamicViewModelKey,
            Notification.Title,
            Notification.ComponentType,
            Notification.ComponentParameterMap,
            Notification.NotificationCssClass,
            true,
            null);

        CommonService.Dialog_ReduceRegisterAction(dialogRecord);

        return HandleShouldNoLongerRender(wasCausedByUiEvent: false);
    }

    public void Dispose()
    {
        _notificationOverlayCancellationTokenSource.Cancel();
    }
}
