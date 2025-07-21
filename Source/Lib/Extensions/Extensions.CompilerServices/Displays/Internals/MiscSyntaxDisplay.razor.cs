using Microsoft.AspNetCore.Components;

namespace Walk.Extensions.CompilerServices.Displays.Internals;

public partial class MiscSyntaxDisplay : ComponentBase
{
	[Parameter, EditorRequired]
	public SyntaxViewModel SyntaxViewModel { get; set; } = default!;
}
