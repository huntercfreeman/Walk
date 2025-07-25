// The way JS Interop is done here is a bit outdated see export syntax:
// https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/

// https://stackoverflow.com/questions/75988682/debounce-in-javascript
// https://stackoverflow.com/a/75988895/19310517
const walkCommonDebounce = (callback, wait) => {
  let timeoutId = null;
  return (...args) => {
    window.clearTimeout(timeoutId);
    timeoutId = window.setTimeout(() => {
      callback(...args);
    }, wait);
  };
}

const walkCommonOnWindowSizeChanged = walkCommonDebounce(() => {
    var localBrowserResizeInteropDotNetObjectReference = walkCommon.browserResizeInteropDotNetObjectReference;

    if (!localBrowserResizeInteropDotNetObjectReference) {
    	return;
    }
    
	localBrowserResizeInteropDotNetObjectReference
		.invokeMethodAsync("OnBrowserResize")
		.then(data => data);
}, 300);

window.walkCommon = {
	browserResizeInteropDotNetObjectReference: null,
    subscribeWindowSizeChanged: function (browserResizeInteropDotNetObjectReference) {
    	// https://github.com/chrissainty/BlazorBrowserResize/blob/master/BrowserResize/BrowserResize/wwwroot/js/browser-resize.js
    	walkCommon.browserResizeInteropDotNetObjectReference = browserResizeInteropDotNetObjectReference;
        window.addEventListener("resize", walkCommonOnWindowSizeChanged);
    },
    disposeWindowSizeChanged: function () {
    	walkCommon.browserResizeInteropDotNetObjectReference = null;
        window.removeEventListener("resize", walkCommonOnWindowSizeChanged);
    },
    treeViewInitialize: function (dotNetHelper, elementId) {
        let element = document.getElementById(elementId);
        
        if (!element)
            return;
        
        if (element) {
            element.addEventListener('keydown', (event) => {
                switch(event.key) {
                    case "Shift":
                    case "Control":
                    case "Alt":
                    case "Meta":
                        break;
                    default:
                        let boundingClientRect = element.getBoundingClientRect();
                        dotNetHelper.invokeMethodAsync("ReceiveOnKeyDown",
                        {
                            Key: event.key,
                            Code: event.code,
                            CtrlKey: event.ctrlKey,
                            ShiftKey: event.shiftKey,
                            AltKey: event.altKey,
                            MetaKey: event.metaKey,
                            ScrollLeft: element.scrollLeft,
                            ScrollTop: element.scrollTop,
                            ViewWidth: element.offsetWidth,
                            ViewHeight: element.offsetHeight,
                            BoundingClientRectLeft: boundingClientRect.left,
                            BoundingClientRectTop: boundingClientRect.top,
                        });
                        break;
                }
                event.preventDefault();
            });
            
            element.addEventListener('contextmenu', (event) => {
                let boundingClientRect = element.getBoundingClientRect();
                dotNetHelper.invokeMethodAsync("ReceiveOnContextMenu", 
                {
                    Buttons: event.buttons,
                    Button: event.button,
                    X: event.clientX,
                    Y: event.clientY,
                    ShiftKey: event.shiftKey,
                    ScrollLeft: element.scrollLeft,
                    ScrollTop: element.scrollTop,
                    ViewWidth: element.offsetWidth,
                    ViewHeight: element.offsetHeight,
                    BoundingClientRectLeft: boundingClientRect.left,
                    BoundingClientRectTop: boundingClientRect.top,
                });
                event.preventDefault();
            });
            
            element.addEventListener('mousedown', (event) => {
                let boundingClientRect = element.getBoundingClientRect();
                dotNetHelper.invokeMethodAsync("ReceiveContentOnMouseDown", 
                {
                    Buttons: event.buttons,
                    Button: event.button,
                    X: event.clientX,
                    Y: event.clientY,
                    ShiftKey: event.shiftKey,
                    ScrollLeft: element.scrollLeft,
                    ScrollTop: element.scrollTop,
                    ViewWidth: element.offsetWidth,
                    ViewHeight: element.offsetHeight,
                    BoundingClientRectLeft: boundingClientRect.left,
                    BoundingClientRectTop: boundingClientRect.top,
                });
            });
            
            element.addEventListener('dblclick', (event) => {
                let boundingClientRect = element.getBoundingClientRect();
                dotNetHelper.invokeMethodAsync("ReceiveOnDoubleClick",
                {
                    Buttons: event.buttons,
                    Button: event.button,
                    X: event.clientX,
                    Y: event.clientY,
                    ShiftKey: event.shiftKey,
                    ScrollLeft: element.scrollLeft,
                    ScrollTop: element.scrollTop,
                    ViewWidth: element.offsetWidth,
                    ViewHeight: element.offsetHeight,
                    BoundingClientRectLeft: boundingClientRect.left,
                    BoundingClientRectTop: boundingClientRect.top,
                });
            });
            
            element.addEventListener('click', (event) => {
                let boundingClientRect = element.getBoundingClientRect();
                dotNetHelper.invokeMethodAsync("ReceiveOnClick",
                {
                    Buttons: event.buttons,
                    Button: event.button,
                    X: event.clientX,
                    Y: event.clientY,
                    ShiftKey: event.shiftKey,
                    ScrollLeft: element.scrollLeft,
                    ScrollTop: element.scrollTop,
                    ViewWidth: element.offsetWidth,
                    ViewHeight: element.offsetHeight,
                    BoundingClientRectLeft: boundingClientRect.left,
                    BoundingClientRectTop: boundingClientRect.top,
                });
            });
        }
        
        return this.measureTreeView(elementId);
    },
    focusAndMeasureTreeView: function (elementId, preventScroll) {
        let element = document.getElementById(elementId);

        if (!element) {
            return {
                ViewWidth: 0,
                ViewHeight: 0,
                BoundingClientRectLeft: 0,
                BoundingClientRectTop: 0,
            };
        }

		if (preventScroll) {
			element.focus({preventScroll: true});
		}
		else {
			element.focus();
		}
		
		return this.measureTreeView(elementId);
    },
    measureTreeView: function (elementId) {
        let element = document.getElementById(elementId);

        if (!element) {
            return {
                ViewWidth: 0,
                ViewHeight: 0,
                BoundingClientRectLeft: 0,
                BoundingClientRectTop: 0,
            };
        }

		let boundingClientRect = element.getBoundingClientRect();
		
		return {
            ViewWidth: element.offsetWidth,
            ViewHeight: element.offsetHeight,
            BoundingClientRectLeft: boundingClientRect.left,
            BoundingClientRectTop: boundingClientRect.top,
        };
    },
    treeViewScrollVertical: function (elementId, changeInScrollTop) {
        let element = document.getElementById(elementId);

        if (!element) {
            return;
        }

		element.scrollTop = element.scrollTop + changeInScrollTop;
    },
    menuInitialize: function (dotNetHelper, elementId) {
        let element = document.getElementById(elementId);
        
        if (!element)
            return;
        
        if (element) {
            element.addEventListener('keydown', (event) => {
                if (event.target != element && event.target.parentElement != element)
                    return;
                switch(event.key) {
                    case "Shift":
                    case "Control":
                    case "Alt":
                    case "Meta":
                        break;
                    default:
                        let boundingClientRect = element.getBoundingClientRect();
                        dotNetHelper.invokeMethodAsync("ReceiveOnKeyDown",
                        {
                            Key: event.key,
                            Code: event.code,
                            CtrlKey: event.ctrlKey,
                            ShiftKey: event.shiftKey,
                            AltKey: event.altKey,
                            MetaKey: event.metaKey,
                            ScrollLeft: element.scrollLeft,
                            ScrollTop: element.scrollTop,
                            ViewWidth: element.offsetWidth,
                            ViewHeight: element.offsetHeight,
                            BoundingClientRectLeft: boundingClientRect.left,
                            BoundingClientRectTop: boundingClientRect.top,
                        });
                        break;
                }
                event.preventDefault();
            });
            
            element.addEventListener('contextmenu', (event) => {
                if (event.target != element && event.target.parentElement != element)
                    return;
                let boundingClientRect = element.getBoundingClientRect();
                dotNetHelper.invokeMethodAsync("ReceiveOnContextMenu", 
                {
                    Buttons: event.buttons,
                    Button: event.button,
                    X: event.clientX,
                    Y: event.clientY,
                    ShiftKey: event.shiftKey,
                    ScrollLeft: element.scrollLeft,
                    ScrollTop: element.scrollTop,
                    ViewWidth: element.offsetWidth,
                    ViewHeight: element.offsetHeight,
                    BoundingClientRectLeft: boundingClientRect.left,
                    BoundingClientRectTop: boundingClientRect.top,
                });
                event.preventDefault();
            });
            
            element.addEventListener('mousedown', (event) => {
                if (event.target != element && event.target.parentElement != element)
                    return;
                let boundingClientRect = element.getBoundingClientRect();
                dotNetHelper.invokeMethodAsync("ReceiveContentOnMouseDown", 
                {
                    Buttons: event.buttons,
                    Button: event.button,
                    X: event.clientX,
                    Y: event.clientY,
                    ShiftKey: event.shiftKey,
                    ScrollLeft: element.scrollLeft,
                    ScrollTop: element.scrollTop,
                    ViewWidth: element.offsetWidth,
                    ViewHeight: element.offsetHeight,
                    BoundingClientRectLeft: boundingClientRect.left,
                    BoundingClientRectTop: boundingClientRect.top,
                });
            });
            
            element.addEventListener('dblclick', (event) => {
                if (event.target != element && event.target.parentElement != element)
                    return;
                let boundingClientRect = element.getBoundingClientRect();
                dotNetHelper.invokeMethodAsync("ReceiveOnDoubleClick",
                {
                    Buttons: event.buttons,
                    Button: event.button,
                    X: event.clientX,
                    Y: event.clientY,
                    ShiftKey: event.shiftKey,
                    ScrollLeft: element.scrollLeft,
                    ScrollTop: element.scrollTop,
                    ViewWidth: element.offsetWidth,
                    ViewHeight: element.offsetHeight,
                    BoundingClientRectLeft: boundingClientRect.left,
                    BoundingClientRectTop: boundingClientRect.top,
                });
            });
            
            element.addEventListener('click', (event) => {
                if (event.target != element && event.target.parentElement != element)
                    return;
                let boundingClientRect = element.getBoundingClientRect();
                dotNetHelper.invokeMethodAsync("ReceiveOnClick",
                {
                    Buttons: event.buttons,
                    Button: event.button,
                    X: event.clientX,
                    Y: event.clientY,
                    ShiftKey: event.shiftKey,
                    ScrollLeft: element.scrollLeft,
                    ScrollTop: element.scrollTop,
                    ViewWidth: element.offsetWidth,
                    ViewHeight: element.offsetHeight,
                    BoundingClientRectLeft: boundingClientRect.left,
                    BoundingClientRectTop: boundingClientRect.top,
                });
            });
        }
        
        return this.measureMenu(elementId);
    },
    measureMenu: function (elementId) {
        let element = document.getElementById(elementId);

        if (!element) {
            return {
                ViewWidth: 0,
                ViewHeight: 0,
                BoundingClientRectLeft: 0,
                BoundingClientRectTop: 0,
            };
        }

		let boundingClientRect = element.getBoundingClientRect();
		
		return {
            ViewWidth: element.offsetWidth,
            ViewHeight: element.offsetHeight,
            BoundingClientRectLeft: boundingClientRect.left,
            BoundingClientRectTop: boundingClientRect.top,
        };
    },
    focusHtmlElementById: function (elementId, preventScroll) {
        let element = document.getElementById(elementId);

        if (!element) {
            return;
        }

		if (preventScroll) {
			element.focus({preventScroll: true});
		}
		else {
			element.focus();
		}
    },
    tryFocusHtmlElementById: function (elementId) {
        let element = document.getElementById(elementId);

        if (!element) {
            return false;
        }

        element.focus();
        return true;
    },
    localStorageSetItem: function (key, value) {
        localStorage.setItem(key, value);
    },
    localStorageGetItem: function (key) {
        return localStorage.getItem(key);
    },
    getTreeViewContextMenuFixedPosition: function (nodeElementId) {

        let treeViewNode = document.getElementById(nodeElementId);
        let treeViewNodeBounds = treeViewNode.getBoundingClientRect();

        return {
            OccurredDueToMouseEvent: false,
            LeftPositionInPixels: treeViewNodeBounds.left,
            TopPositionInPixels: treeViewNodeBounds.top + treeViewNodeBounds.height
        }
    },
    measureElementById: function (elementId) {
        let element = document.getElementById(elementId);

        if (!element) {
            return {
                WidthInPixels: 0,
                HeightInPixels: 0,
                LeftInPixels: 0,
                TopInPixels: 0,
                ZIndex: 0,
            }
        }

        let boundingClientRect = element.getBoundingClientRect();

        return {
            WidthInPixels: boundingClientRect.width,
            HeightInPixels: boundingClientRect.height,
            LeftInPixels: boundingClientRect.left,
            TopInPixels: boundingClientRect.top,
            ZIndex: 0,
        }
    },
    readClipboard: async function () {
        // domexception-on-calling-navigator-clipboard-readtext
        // https://stackoverflow.com/q/56306153/14847452
        // ----------------------------------------------------
        // First, ask the Permissions API if we have some kind of access to
        // the "clipboard-read" feature.
        try {
            return await navigator.permissions.query({name: "clipboard-read"}).then(async (result) => {
                // If permission to read the clipboard is granted or if the user will
                // be prompted to allow it, we proceed.

                if (result.state === "granted" || result.state === "prompt") {
                    return await navigator.clipboard.readText().then((data) => {
                        return data;
                    });
                } else {
                    return "";
                }
            });
        } catch (e) {
            // Debugging Linux-Ubuntu (2024-04-28)
            // -----------------------------------
            // Reading clipboard is not working.
            //
            // Fixed with the following inner-try/catch block.
            //
            // This fix upsets me. Seemingly the permission
            // "clipboard-read" doesn't exist for some user-agents
            // But so long as you don't check for permission it lets you read
            // the clipboard?
            try {
                return navigator.clipboard
                    .readText()
                    .then((clipText) => {
                        return clipText;
                    });
            } catch (innerException) {
                return "";
            }
        }
    },
    setClipboard: function (value) {
        // how-do-i-copy-to-the-clipboard-in-javascript:
        // https://stackoverflow.com/a/33928558/14847452
        // ---------------------------------------------
        // Copies a string to the clipboard. Must be called from within an
        // event handler such as click. May return false if it failed, but
        // this is not always possible. Browser support for Chrome 43+,
        // Firefox 42+, Safari 10+, Edge and Internet Explorer 10+.
        // Internet Explorer: The clipboard feature may be disabled by
        // an administrator. By default a prompt is shown the first
        // time the clipboard is used (per session).
        if (window.clipboardData && window.clipboardData.setData) {
            // Internet Explorer-specific code path to prevent textarea being shown while dialog is visible.
            return window.clipboardData.setData("Text", text);

        } else if (document.queryCommandSupported && document.queryCommandSupported("copy")) {
            var textarea = document.createElement("textarea");
            textarea.textContent = value;
            textarea.style.position = "fixed";  // Prevent scrolling to bottom of page in Microsoft Edge.
            document.body.appendChild(textarea);
            textarea.select();
            try {
                return document.execCommand("copy");  // Security exception may be thrown by some browsers.
            } catch (ex) {
                console.warn("Copy to clipboard failed.", ex);
                return false;
            } finally {
                document.body.removeChild(textarea);
            }
        }
    },
}

Blazor.registerCustomEventType('keydownwithpreventscroll', {
    browserEventName: 'keydown',
    createEventArgs: e => {

        let preventDefaultOnTheseKeys = [
            "ContextMenu",
            "ArrowLeft",
            "ArrowDown",
            "ArrowUp",
            "ArrowRight",
            "Home",
            "End",
            "Space",
            "Enter",
            "PageUp",
            "PageDown"
        ];

        let preventDefaultOnTheseCodes = [
            "Space",
            "Enter",
        ];

        if (preventDefaultOnTheseKeys.indexOf(e.key) !== -1 ||
            preventDefaultOnTheseCodes.indexOf(e.code) !== -1) {
            e.preventDefault();
        }

        return {
            Type: e.type,
            MetaKey: e.metaKey,
            AltKey: e.altKey,
            ShiftKey: e.shiftKey,
            CtrlKey: e.ctrlKey,
            Repeat: e.repeat,
            Location: e.location,
            Code: e.code,
            Key: e.key
        };
    }
});