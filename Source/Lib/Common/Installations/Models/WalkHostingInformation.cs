namespace Walk.Common.RazorLib.Installations.Models;

/// <summary>
/// One use case for <see cref="WalkHostingInformation"/> would be service registration.<br/><br/>
/// If one uses <see cref="WalkHostingKind.ServerSide"/>, then 
/// services.AddHostedService&lt;TService&gt;(...); will be invoked.<br/><br/>
/// Whereas, if one uses <see cref="WalkHostingKind.Wasm"/> then 
/// services.AddSingleton&lt;TService&gt;(...); will be used.
/// Then after the initial render, a Task will be 'fire and forget' invoked to start the service.
/// </summary>
/// <remarks>
/// This class is an exception to the naming convention, "don't use the word 'Walk' in class names".
/// 
/// Reason for this exception: when one first starts interacting with this project,
///     this type might be one of the first types they interact with. So, the redundancy of namespace
///     and type containing 'Walk' feels reasonable here.
/// </remarks>
public record struct WalkHostingInformation
{
    public WalkHostingInformation(
        WalkHostingKind walkHostingKind,
        WalkPurposeKind walkPurposeKind)
    {
        WalkHostingKind = walkHostingKind;
        WalkPurposeKind = walkPurposeKind;
    }

    public WalkHostingKind WalkHostingKind { get; init; }
    public WalkPurposeKind WalkPurposeKind { get; init; }
}
