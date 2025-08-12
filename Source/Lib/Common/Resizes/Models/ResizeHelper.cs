using Walk.Common.RazorLib.Dimensions.Models;
using Microsoft.AspNetCore.Components.Web;

namespace Walk.Common.RazorLib.Resizes.Models;

public class ResizeHelper
{
    public static void ResizeNorth(
        ElementDimensions elementDimensions,
        MouseEventArgs firstMouseEventArgs,
        MouseEventArgs secondMouseEventArgs)
    {
        var deltaY = secondMouseEventArgs.ClientY - firstMouseEventArgs.ClientY;

        // Height
        elementDimensions.Height_Offset = elementDimensions.Height_Offset with
        {
            Value = elementDimensions.Height_Offset.Value - deltaY
        };

        // Top
        elementDimensions.Top_Offset = elementDimensions.Top_Offset with
        {
            Value = elementDimensions.Top_Offset.Value + deltaY
        };
    }

    public static void ResizeEast(
        ElementDimensions elementDimensions,
        MouseEventArgs firstMouseEventArgs,
        MouseEventArgs secondMouseEventArgs)
    {
        var deltaX = secondMouseEventArgs.ClientX - firstMouseEventArgs.ClientX;

        // Width
        elementDimensions.Width_Offset = elementDimensions.Width_Offset with
        {
            Value = elementDimensions.Width_Offset.Value + deltaX
        };
    }

    public static void ResizeSouth(
        ElementDimensions elementDimensions,
        MouseEventArgs firstMouseEventArgs,
        MouseEventArgs secondMouseEventArgs)
    {
        var deltaY = secondMouseEventArgs.ClientY - firstMouseEventArgs.ClientY;

        // Height
        elementDimensions.Height_Offset = elementDimensions.Height_Offset with
        {
            Value = elementDimensions.Height_Offset.Value + deltaY
        };
    }

    public static void ResizeWest(
        ElementDimensions elementDimensions,
        MouseEventArgs firstMouseEventArgs,
        MouseEventArgs secondMouseEventArgs)
    {
        var deltaX = secondMouseEventArgs.ClientX - firstMouseEventArgs.ClientX;

        // Width
        elementDimensions.Width_Offset = elementDimensions.Width_Offset with
        {
            Value = elementDimensions.Width_Offset.Value - deltaX
        };

        // Left
        elementDimensions.Left_Offset = elementDimensions.Left_Offset with
        {
            Value = elementDimensions.Left_Offset.Value + deltaX
        };
    }

    public static void ResizeNorthEast(
        ElementDimensions elementDimensions,
        MouseEventArgs firstMouseEventArgs,
        MouseEventArgs secondMouseEventArgs)
    {
        ResizeNorth(elementDimensions, firstMouseEventArgs, secondMouseEventArgs);
        ResizeEast(elementDimensions, firstMouseEventArgs, secondMouseEventArgs);
    }

    public static void ResizeSouthEast(
        ElementDimensions elementDimensions,
        MouseEventArgs firstMouseEventArgs,
        MouseEventArgs secondMouseEventArgs)
    {
        ResizeSouth(elementDimensions, firstMouseEventArgs, secondMouseEventArgs);
        ResizeEast(elementDimensions, firstMouseEventArgs, secondMouseEventArgs);
    }

    public static void ResizeSouthWest(
        ElementDimensions elementDimensions,
        MouseEventArgs firstMouseEventArgs,
        MouseEventArgs secondMouseEventArgs)
    {
        ResizeSouth(elementDimensions, firstMouseEventArgs, secondMouseEventArgs);
        ResizeWest(elementDimensions, firstMouseEventArgs, secondMouseEventArgs);
    }

    public static void ResizeNorthWest(
        ElementDimensions elementDimensions,
        MouseEventArgs firstMouseEventArgs,
        MouseEventArgs secondMouseEventArgs)
    {
        ResizeNorth(elementDimensions, firstMouseEventArgs, secondMouseEventArgs);
        ResizeWest(elementDimensions, firstMouseEventArgs, secondMouseEventArgs);
    }

    public static void Move(
        ElementDimensions elementDimensions,
        MouseEventArgs firstMouseEventArgs,
        MouseEventArgs secondMouseEventArgs)
    {
        var deltaY = secondMouseEventArgs.ClientY - firstMouseEventArgs.ClientY;
        var deltaX = secondMouseEventArgs.ClientX - firstMouseEventArgs.ClientX;

        // Top
        elementDimensions.Top_Offset = elementDimensions.Top_Offset with
        {
            Value = elementDimensions.Top_Offset.Value + deltaY
        };

        // Left
        elementDimensions.Left_Offset = elementDimensions.Left_Offset with
        {
            Value = elementDimensions.Left_Offset.Value + deltaX
        };
    }
}
