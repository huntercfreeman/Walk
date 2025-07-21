namespace Walk.Common.RazorLib.Exceptions;

/// <remarks>
/// This class is an exception to the naming convention, "don't use the word 'Walk' in class names".
/// 
/// Reason for this exception: the 'Exception' datatype is far more common in code,
///     than some specific type (example: DialogDisplay.razor).
///     So, adding 'Walk' in the class name for redundancy seems meaningful here.
/// </remarks>
public class WalkCommonException : WalkException
{
    public WalkCommonException(string? message = null, Exception? innerException = null)
        : base(message, innerException)
    {

    }
}
