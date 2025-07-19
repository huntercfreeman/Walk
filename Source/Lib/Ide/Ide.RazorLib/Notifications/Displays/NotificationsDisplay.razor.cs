using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib;

namespace Walk.Ide.RazorLib.Notifications.Displays;

public partial class NotificationsDisplay : ComponentBase
{
	[Inject]
	private CommonService CommonService { get; set; } = null!;
}