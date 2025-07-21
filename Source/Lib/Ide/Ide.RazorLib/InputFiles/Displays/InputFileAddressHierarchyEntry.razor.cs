using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.FileSystems.Models;

namespace Walk.Ide.RazorLib.InputFiles.Displays;

public partial class InputFileAddressHierarchyEntry : ComponentBase
{
    [Parameter, EditorRequired]
    public AbsolutePath AbsolutePath { get; set; }
}
