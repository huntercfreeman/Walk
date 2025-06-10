namespace Walk.Common.RazorLib.Dimensions.Models;

/// <summary>
/// This file relates to HTML markup. Specifically, when one interpolates
/// a double from C# into a HTML tag's style attribute.
/// Depending on where one lives, one might see either, "5.2" or "5,2".
/// If one has the comma formatting, the styling will not parse correctly
/// when being rendered.
/// <br/><br/>
/// So, this file ensures period formatting regardless of localization.
///
/// WARNING: In hot paths numbers will have ToString() invoked directly,...
/// ...rather than ToCssValue();
/// 
/// In non-hot paths ToCssValue is still used on int values
/// in the chance that someone changes the int to a double,
/// or some other number that will require InvariantCulture.
///
/// Therefore, be wary about changing int values to doubles or etc...
/// because depending on how hot the path is, there might actually
/// be direct ToString() invocations.
///
/// I need to look into [MethodImpl(MethodImplOptions.AggressiveInlining)]
/// and under what situations the compiler will inline for you.
/// </summary>
public static class CssInterpolation
{
    public static string ToCssValue(this double value) =>
        value.ToString(System.Globalization.CultureInfo.InvariantCulture);

    public static string ToCssValue(this decimal value) =>
        value.ToString(System.Globalization.CultureInfo.InvariantCulture);

    public static string ToCssValue(this float value) =>
        value.ToString(System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>
    /// WARNING: In hot paths numbers will have ToString() invoked directly,...
    /// ...rather than ToCssValue();
    /// 
    /// In non-hot paths ToCssValue is still used on int values
    /// in the chance that someone changes the int to a double,
    /// or some other number that will require InvariantCulture.
    ///
    /// Therefore, be wary about changing int values to doubles or etc...
    /// because depending on how hot the path is, there might actually
    /// be direct ToString() invocations.
    ///
    /// I need to look into [MethodImpl(MethodImplOptions.AggressiveInlining)]
    /// and under what situations the compiler will inline for you.
    /// </summary>
    public static string ToCssValue(this int value) =>
        value.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
