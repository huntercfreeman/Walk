using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Dimensions.Models;

namespace Walk.Extensions.Config.Installations.Displays;

public partial class IdeMainLayout
{
    private Task WIDGET_RemoveWidget()
    {
        DotNetService.CommonService.SetWidget(null);
        return Task.CompletedTask;
    }

    private async Task DROPDOWN_ClearActiveKeyList()
    {
        var firstDropdown = DotNetService.CommonService.GetDropdownState().DropdownList.FirstOrDefault();

        if (firstDropdown is not null)
        {
            var restoreFocusOnCloseFunc = firstDropdown.RestoreFocusOnClose;

            if (restoreFocusOnCloseFunc is not null)
                await restoreFocusOnCloseFunc.Invoke();
        }

        DotNetService.CommonService.Dropdown_ReduceClearAction();
    }

    public string OUTLINE_GetStyleCssLeft(OutlineState localOutlineState)
    {
        var width = OUTLINE_THICKNESS;

        var height = localOutlineState.MeasuredHtmlElementDimensions.HeightInPixels;

        var left = localOutlineState.MeasuredHtmlElementDimensions.LeftInPixels;

        var top = localOutlineState.MeasuredHtmlElementDimensions.TopInPixels;

        _styleBuilder.Clear();

        _styleBuilder.Append("width: ");
        _styleBuilder.Append(width.ToCssValue());
        _styleBuilder.Append("px; ");

        _styleBuilder.Append("height: ");
        _styleBuilder.Append(height.ToCssValue());
        _styleBuilder.Append("px; ");

        _styleBuilder.Append($"left: ");
        _styleBuilder.Append(left.ToCssValue());
        _styleBuilder.Append("px; ");

        _styleBuilder.Append("top: ");
        _styleBuilder.Append(top.ToCssValue());
        _styleBuilder.Append("px; ");

        return _styleBuilder.ToString();
    }

    public string OUTLINE_GetStyleCssRight(OutlineState localOutlineState)
    {
        var width = OUTLINE_THICKNESS;

        var height = localOutlineState.MeasuredHtmlElementDimensions.HeightInPixels;

        var left = localOutlineState.MeasuredHtmlElementDimensions.LeftInPixels +
            localOutlineState.MeasuredHtmlElementDimensions.WidthInPixels -
            OUTLINE_THICKNESS;

        var top = localOutlineState.MeasuredHtmlElementDimensions.TopInPixels;

        _styleBuilder.Clear();

        _styleBuilder.Append("width: ");
        _styleBuilder.Append(width.ToCssValue());
        _styleBuilder.Append("px; ");

        _styleBuilder.Append("height: ");
        _styleBuilder.Append(height.ToCssValue());
        _styleBuilder.Append("px; ");

        _styleBuilder.Append("left: ");
        _styleBuilder.Append(left.ToCssValue());
        _styleBuilder.Append("px; ");

        _styleBuilder.Append("top: ");
        _styleBuilder.Append(top.ToCssValue());
        _styleBuilder.Append("px; ");

        return _styleBuilder.ToString();
    }

    public string OUTLINE_GetStyleCssTop(OutlineState localOutlineState)
    {
        var width = localOutlineState.MeasuredHtmlElementDimensions.WidthInPixels;

        var height = OUTLINE_THICKNESS;

        var left = localOutlineState.MeasuredHtmlElementDimensions.LeftInPixels;

        var top = localOutlineState.MeasuredHtmlElementDimensions.TopInPixels;

        _styleBuilder.Clear();

        _styleBuilder.Append("width: ");
        _styleBuilder.Append(width.ToCssValue());
        _styleBuilder.Append("px; ");

        _styleBuilder.Append("height: ");
        _styleBuilder.Append(height.ToCssValue());
        _styleBuilder.Append("px; ");

        _styleBuilder.Append("left: ");
        _styleBuilder.Append(left.ToCssValue());
        _styleBuilder.Append("px; ");

        _styleBuilder.Append("top: ");
        _styleBuilder.Append(top.ToCssValue());
        _styleBuilder.Append("px; ");

        return _styleBuilder.ToString();
    }

    public string OUTLINE_GetStyleCssBottom(OutlineState localOutlineState)
    {
        var width = localOutlineState.MeasuredHtmlElementDimensions.WidthInPixels;

        var height = OUTLINE_THICKNESS;

        var left = localOutlineState.MeasuredHtmlElementDimensions.LeftInPixels;

        var top = localOutlineState.MeasuredHtmlElementDimensions.TopInPixels +
            localOutlineState.MeasuredHtmlElementDimensions.HeightInPixels -
            OUTLINE_THICKNESS;

        _styleBuilder.Clear();

        _styleBuilder.Append($"width: ");
        _styleBuilder.Append(width.ToCssValue());
        _styleBuilder.Append("px; ");

        _styleBuilder.Append($"height: ");
        _styleBuilder.Append(height.ToCssValue());
        _styleBuilder.Append("px; ");

        _styleBuilder.Append($"left: ");
        _styleBuilder.Append(left.ToCssValue());
        _styleBuilder.Append("px; ");

        _styleBuilder.Append($"top: ");
        _styleBuilder.Append(top.ToCssValue());
        _styleBuilder.Append("px; ");

        return _styleBuilder.ToString();
    }
}
