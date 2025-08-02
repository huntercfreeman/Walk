namespace Walk.Common.RazorLib.Options.Models;

/// <summary>
/// This type needs to exist so the <see cref="CommonOptions"/> properties can be nullable, as in they were not
/// already in local storage. Whereas throughout the app they should never be null.
/// </summary>
public record CommonOptionsJsonDto(
    int FontSizeInPixels,
    int IconSizeInPixels,
    int ResizeHandleWidthInPixels,
    int ResizeHandleHeightInPixels,
    int ThemeKey,
    string? FontFamily)
{
    public CommonOptionsJsonDto()
        : this(
            FontSizeInPixels: 0,
            IconSizeInPixels: 0,
            ResizeHandleWidthInPixels: 0,
            ResizeHandleHeightInPixels: 0,
            ThemeKey: 0,
            FontFamily: null)
    {
    }

    public CommonOptionsJsonDto(CommonOptions options)
        : this(
              options.FontSizeInPixels,
              options.IconSizeInPixels,
              options.ResizeHandleWidthInPixels,
              options.ResizeHandleHeightInPixels,
              options.ThemeKey,
              options.FontFamily)
    {
        
    }
}
