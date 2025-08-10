// The way JS Interop is done here is a bit outdated see export syntax:
// https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/

window.walkConfig = {
    altIsDown: false,
    controlTabChordIsInProgress: false,
	clickById: function (elementId) {
	    let element = document.getElementById(elementId);
        
        if (!element)
            return;
            
        element.click();
	},
	appWideKeyboardEventsInitialize: function (dotNetHelper) {
        document.body.addEventListener('keydown', (event) => {
            switch(event.key) {
                case "Shift":
                case "Meta":
                case "Control":
                    break;
                case "Alt":
                    if (window.walkConfig.altIsDown)
                        break;
                    window.walkConfig.altIsDown = true;
                    dotNetHelper.invokeMethodAsync("ReceiveOnKeyDown", event.key, event.shiftKey);
                    event.preventDefault();
                    break;
                case "Tab":
                    if (!event.ctrlKey)
                        break;
                    window.walkConfig.controlTabChordIsInProgress = true;
                    dotNetHelper.invokeMethodAsync("ReceiveOnKeyDown", event.key, event.shiftKey);
                    break;
                case "p":
                    if (!event.ctrlKey)
                        break;
                    dotNetHelper.invokeMethodAsync("ReceiveWidgetOnKeyDown");
                    break;
                case "s":
                    if (!event.ctrlKey)
                        break;
                    dotNetHelper.invokeMethodAsync("SaveFileOnKeyDown");
                    break;
                case "S":
                    if (!event.ctrlKey)
                        break;
                    dotNetHelper.invokeMethodAsync("SaveAllFileOnKeyDown");
                    break;
                case "F":
                    if (!event.ctrlKey)
                        break;
                    dotNetHelper.invokeMethodAsync("FindAllOnKeyDown");
                    break;
                case ",":
                    if (!event.ctrlKey)
                        break;
                    dotNetHelper.invokeMethodAsync("CodeSearchOnKeyDown");
                    break;
                case "f":
                    if (!event.altKey)
                        break;
                    window.walkConfig.clickById("di_header-button-file");
                    break;
                case "t":
                    if (!event.altKey)
                        break;
                    window.walkConfig.clickById("di_header-button-tools");
                    break;
                case "v":
                    if (!event.altKey)
                        break;
                    window.walkConfig.clickById("di_header-button-view");
                    break;
                case "r":
                    if (!event.altKey)
                        break;
                    window.walkConfig.clickById("di_header-button-run");
                    break;
                case "Escape":
                    dotNetHelper.invokeMethodAsync("EscapeOnKeyDown");
                    break;
                default:
                    break;
            }
            // event.preventDefault();
        });
        
        document.body.addEventListener('keyup', (event) => {
            switch(event.key) {
                case "Shift":
                case "Meta":
                case "Tab":
                    break;
                case "Control":
                    if (!window.walkConfig.controlTabChordIsInProgress)
                        break;
                    window.walkConfig.controlTabChordIsInProgress = false;
                    dotNetHelper.invokeMethodAsync("ReceiveOnKeyUp", event.key);
                    event.preventDefault();
                    break;
                case "Alt":
                    window.walkConfig.altIsDown = false;
                    dotNetHelper.invokeMethodAsync("ReceiveOnKeyUp", event.key);
                    event.preventDefault();
                    break;
                default:
                    break;
            }
            // event.preventDefault();
        });
        
        window.addEventListener('blur', function() {
          if (!window.walkConfig.altIsDown && !window.walkConfig.controlTabChordIsInProgress)
              return;
          window.walkConfig.altIsDown = false;
          window.walkConfig.controlTabChordIsInProgress = false;
          dotNetHelper.invokeMethodAsync("OnWindowBlur");
        });
    }
}
