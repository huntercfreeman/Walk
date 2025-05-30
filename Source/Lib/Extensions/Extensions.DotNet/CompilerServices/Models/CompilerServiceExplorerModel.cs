using Walk.Common.RazorLib.FileSystems.Models;

namespace Walk.Extensions.DotNet.CompilerServices.Models;

public class CompilerServiceExplorerModel
{
	public AbsolutePath? AbsolutePath { get; }
	public bool IsLoadingCompilerServiceExplorer { get; }
}
