using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Reactives.Models;

namespace Walk.Common.RazorLib.Notifications.Displays;

public partial class CommonProgressNotificationDisplay : ComponentBase
{
	[Parameter, EditorRequired]
	public ProgressBarModel ProgressBarModel { get; set; } = null!;
}
