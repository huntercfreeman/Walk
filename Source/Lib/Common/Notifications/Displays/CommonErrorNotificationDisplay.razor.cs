using Microsoft.AspNetCore.Components;

namespace Walk.Common.RazorLib.Notifications.Displays;

public partial class CommonErrorNotificationDisplay : ComponentBase
{
    [Parameter, EditorRequired]
    public string Message { get; set; } = null!;
}
