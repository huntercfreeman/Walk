using Walk.Common.RazorLib.ComponentRenderers.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.Extensions.DotNet.CSharpProjects.Models;

namespace Walk.Extensions.DotNet.Websites;

public class WebsiteDotNetCliHelper
{
	public static async Task StartNewCSharpProjectCommand(
		CSharpProjectFormViewModelImmutable immutableView,
		IEnvironmentProvider environmentProvider,
		IFileSystemProvider fileSystemProvider,
		DotNetBackgroundTaskApi compilerServicesBackgroundTaskApi,
		CommonService commonService,
		IDialog dialogRecord,
		ICommonComponentRenderers commonComponentRenderers)
	{
		return;
	}
}
