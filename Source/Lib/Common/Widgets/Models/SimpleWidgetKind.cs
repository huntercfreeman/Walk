namespace Walk.Common.RazorLib.Widgets.Models;

/// <summary>
/// Example functionality: a form is embedded in the UI that prompts the user to select "Yes", "No", or "Cancel".
///
/// The most common and simple scenarios of user interaction are included in this enum.
/// This permits the common library to automatically embed the form into the UI.
///
/// For more complex scenarios a `WidgetDisplay` or `DialogDisplay` can occur at the app level and renders
/// an element that is fixed position to be shown overtop of the UI.
///
/// In these widget and dialog scenarios, the cost of storing the Type and Dictionary of component parameters
/// is far lower than that of a menu option. Thus those properties exist and you can
/// display anything you want in a DynamicComponent.
///
/// I'm still quite put off by the idea of even the widgets/dialogs storing those properties and using DynamicComponent.
/// But for now, my focus is to just get that logic off the menu options. 
/// </summary>
public enum SimpleWidgetKind
{
    
}
