using Walk.TextEditor.RazorLib.Installations.Models;

namespace Walk.Ide.RazorLib.Installations.Models;

/// <remarks>
/// This class is an exception to the naming convention, "don't use the word 'Walk' in class names".
/// 
/// Reason for this exception: when one first starts interacting with this project,
/// 	this type might be one of the first types they interact with. So, the redundancy of namespace
/// 	and type containing 'Walk' feels reasonable here.
/// </remarks>
public record WalkIdeConfig
{
    /// <summary>Default value is <see cref="true"/>. If one wishes to configure Walk.TextEditor themselves, then set this to false, and invoke <see cref="TextEditor.RazorLib.Installations.Models.ServiceCollectionExtensions.AddWalkTextEditor(Microsoft.Extensions.DependencyInjection.IServiceCollection, Func{WalkTextEditorConfig, WalkTextEditorConfig}?)"/> prior to invoking Walk.TextEditor's</summary>
    public bool AddWalkTextEditor { get; init; } = true;
}
