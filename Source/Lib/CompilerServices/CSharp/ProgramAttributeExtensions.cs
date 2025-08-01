using System.Diagnostics.CodeAnalysis;

namespace Walk.CompilerServices.CSharp;

public partial class Program
{
    /// <summary>
    /// FIXME: This is required for EF Core 6.0 as it is not compatible with trimming.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    private static Type _keepDateOnly = typeof(DateOnly);
}
