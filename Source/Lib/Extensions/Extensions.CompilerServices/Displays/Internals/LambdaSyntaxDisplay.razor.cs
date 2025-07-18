using Microsoft.AspNetCore.Components;

namespace Walk.Extensions.CompilerServices.Displays.Internals;

public partial class LambdaSyntaxDisplay : ComponentBase
{
	[Parameter, EditorRequired]
	public SyntaxViewModel SyntaxViewModel { get; set; } = default!;
}
