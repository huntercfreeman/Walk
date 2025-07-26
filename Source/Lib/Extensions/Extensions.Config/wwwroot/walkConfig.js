// The way JS Interop is done here is a bit outdated see export syntax:
// https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/

window.walkConfig = {
    altIsDown: false,
    controlTabChordIsInProgress: false,
	appWideKeyboardEventsInitialize: function (dotNetHelper) {
        document.body.addEventListener('keydown', (event) => {
            switch(event.key) {
                case "Shift":
                case "Meta":
                case "Control":
                    break;
                case "Alt":
                    if (altIsDown)
                        break;
                    altIsDown = true;
                    dotNetHelper.invokeMethodAsync("ReceiveOnKeyDown", event.key);
                    event.preventDefault();
                    break;
                case "Tab":
                    if (!event.ctrlKey)
                        break;
                    controlTabChordIsInProgress = true;
                    dotNetHelper.invokeMethodAsync("ReceiveOnKeyDown", event.key);
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
                    if (!controlTabChordIsInProgress)
                        break;
                    controlTabChordIsInProgress = false;
                    dotNetHelper.invokeMethodAsync("ReceiveOnKeyUp", event.key);
                    event.preventDefault();
                    break;
                case "Alt":
                    altIsDown = false;
                    dotNetHelper.invokeMethodAsync("ReceiveOnKeyUp", event.key);
                    event.preventDefault();
                    break;
                default:
                    break;
            }
            // event.preventDefault();
        });
        
        window.addEventListener('blur', function() {
          if (!altIsDown && !controlTabChordIsInProgress)
              return;
          altIsDown = false;
          controlTabChordIsInProgress = false;
          dotNetHelper.invokeMethodAsync("OnWindowBlur");
        });
    }
}
