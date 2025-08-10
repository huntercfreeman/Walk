// The way JS Interop is done here is a bit outdated see export syntax:
// https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/

window.walkTextEditor = {
    scrollElementIntoView: function (elementId) {

        let element = document.getElementById(elementId);

        if (!element) {
            return;
        }
        
        element.scrollIntoView({
            block: "nearest",
            inline: "nearest"
        });
    },
    setPreventDefaultsAndStopPropagations: function (dotNetHelper, contentElementId, rowSectionElementId, HORIZONTAL_ScrollbarElementId, VERTICAL_ScrollbarElementId, CONNECTOR_ScrollbarElementId) {
        let contentElement = document.getElementById(contentElementId);
        
        if (!contentElement)
            return;
            
        // contentElement.dotNetHelper = dotNetHelper;
        
        if (contentElement) {
        
            contentElement.addEventListener('wheel', (event) => {
                event.preventDefault();
            }, {
                passive: false,
            });
            /*contentElement.addEventListener('touchstart', (event) => {
                dotNetHelper.invokeMethodAsync("ReceiveOnTouchStart", event);
                event.preventDefault();
            }, {
                passive: false,
            });*/
        
            contentElement.addEventListener('keydown', (event) => {
            
                switch(event.key) {
                    case "Shift":
                    case "Control":
                    case "Alt":
                    case "Meta":
                        break;
                    default:
                        dotNetHelper.invokeMethodAsync("ReceiveOnKeyDown",
                        {
                            Key: event.key,
                            Code: event.code,
                            CtrlKey: event.ctrlKey,
                            ShiftKey: event.shiftKey,
                            AltKey: event.altKey,
                            MetaKey: event.metaKey,
                        });
                        break;
                }
                
                event.preventDefault();
            });
            
            contentElement.addEventListener('focusin', (event) => {
                dotNetHelper.invokeMethodAsync("HandleFocusIn");
            });
            
            contentElement.addEventListener('focusout', (event) => {
                dotNetHelper.invokeMethodAsync("HandleFocusOut");
            });
            
            contentElement.addEventListener('click', (event) => {
                dotNetHelper.invokeMethodAsync("FocusTextEditorAsync");
            });
            
            contentElement.addEventListener('contextmenu', (event) => {
                dotNetHelper.invokeMethodAsync("ReceiveOnContextMenu");
                event.preventDefault();
            });
            
            contentElement.addEventListener('mousedown', (event) => {
                dotNetHelper.invokeMethodAsync("ReceiveContentOnMouseDown", 
                {
                    Buttons: event.buttons,
                    X: event.clientX,
                    Y: event.clientY,
                    ShiftKey: event.shiftKey,
                });
            });
            
            contentElement.addEventListener('mousemove', (event) => {
                dotNetHelper.invokeMethodAsync("ReceiveContentOnMouseMove", 
                {
                    Buttons: event.buttons,
                    X: event.clientX,
                    Y: event.clientY,
                    ShiftKey: event.shiftKey,
                });
            });
            
            contentElement.addEventListener('mouseout', (event) => {
                dotNetHelper.invokeMethodAsync("ReceiveContentOnMouseOut", 
                {
                    Buttons: event.buttons,
                    X: event.clientX,
                    Y: event.clientY,
                    ShiftKey: event.shiftKey,
                });
            });
            
            contentElement.addEventListener('dblclick', (event) => {
                dotNetHelper.invokeMethodAsync("ReceiveOnDoubleClick",
                {
                    Buttons: event.buttons,
                    X: event.clientX,
                    Y: event.clientY,
                    ShiftKey: event.shiftKey,
                });
            });
            
            contentElement.addEventListener('wheel', (event) => {
                dotNetHelper.invokeMethodAsync("ReceiveOnWheel",
                {
                    X: event.deltaX,
                    Y: event.deltaY,
                    ShiftKey: event.shiftKey,
                });
            });
            
            /*contentElement.addEventListener('touchmove', (event) => {
                dotNetHelper.invokeMethodAsync("ReceiveOnTouchMove", event);
            });
            
            contentElement.addEventListener('touchend', (event) => {
                dotNetHelper.invokeMethodAsync("ClearTouch", event);
            });
            
            contentElement.addEventListener('touchcancel', (event) => {
                dotNetHelper.invokeMethodAsync("ClearTouch", event);
            });
            
            contentElement.addEventListener('touchleave', (event) => {
                dotNetHelper.invokeMethodAsync("ClearTouch", event);
            });*/
        }
        
        let HORIZONTAL_ScrollbarElement = document.getElementById(HORIZONTAL_ScrollbarElementId);
        if (HORIZONTAL_ScrollbarElement) {
        
            HORIZONTAL_ScrollbarElement.addEventListener('mousemove', (event) => {
                event.stopPropagation();
            });
            
            HORIZONTAL_ScrollbarElement.addEventListener('dblclick', (event) => {
                event.stopPropagation();
            });
            
            HORIZONTAL_ScrollbarElement.addEventListener('click', (event) => {
                event.stopPropagation();
            });
            
            HORIZONTAL_ScrollbarElement.addEventListener('contextmenu', (event) => {
                event.stopPropagation();
            });
            
            HORIZONTAL_ScrollbarElement.addEventListener('mousedown', (event) => {
                dotNetHelper.invokeMethodAsync("HORIZONTAL_HandleOnMouseDownAsync", 
                {
                    Buttons: event.buttons,
                    X: event.clientX,
                    Y: event.clientY,
                    ShiftKey: event.shiftKey,
                });
                event.stopPropagation();
            });
        }
        
        let VERTICAL_ScrollbarElement = document.getElementById(VERTICAL_ScrollbarElementId);
        if (VERTICAL_ScrollbarElement) {
        
            VERTICAL_ScrollbarElement.addEventListener('mousemove', (event) => {
                event.stopPropagation();
            });
            
            VERTICAL_ScrollbarElement.addEventListener('dblclick', (event) => {
                event.stopPropagation();
            });
            
            VERTICAL_ScrollbarElement.addEventListener('click', (event) => {
                event.stopPropagation();
            });
            
            VERTICAL_ScrollbarElement.addEventListener('contextmenu', (event) => {
                event.stopPropagation();
            });
            
            VERTICAL_ScrollbarElement.addEventListener('mousedown', (event) => {
                dotNetHelper.invokeMethodAsync("VERTICAL_HandleOnMouseDownAsync", 
                {
                    Buttons: event.buttons,
                    X: event.clientX,
                    Y: event.clientY,
                    ShiftKey: event.shiftKey,
                });
                event.stopPropagation();
            });
        }
        
        let CONNECTOR_ScrollbarElement = document.getElementById(CONNECTOR_ScrollbarElementId);
        if (CONNECTOR_ScrollbarElement) {
            
            CONNECTOR_ScrollbarElement.addEventListener('mousemove', (event) => {
                event.stopPropagation();
            });
            
            CONNECTOR_ScrollbarElement.addEventListener('dblclick', (event) => {
                event.stopPropagation();
            });
            
            CONNECTOR_ScrollbarElement.addEventListener('click', (event) => {
                event.stopPropagation();
            });
            
            CONNECTOR_ScrollbarElement.addEventListener('contextmenu', (event) => {
                event.stopPropagation();
            });
            
            CONNECTOR_ScrollbarElement.addEventListener('mousedown', (event) => {
                event.stopPropagation();
            });
        }
    },
    getCharAndLineMeasurementsInPixelsById: function (elementId) {
        let element = document.getElementById(elementId);

        if (!element) {
            return {
                CharacterWidth: 5,
                LineHeight: 5
            }
        }
        
        let elevenTimesAlphabetAndDigits = "abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789abcdefghijklmnopqrstuvwxyz0123456789";
        element.innerText = elevenTimesAlphabetAndDigits;
        
        let fontWidth = element.offsetWidth / elevenTimesAlphabetAndDigits.length;
        let lineHeight = element.offsetHeight;
        
        element.innerText = "";

        return {
            CharacterWidth: fontWidth,
            LineHeight: lineHeight
        }
    },
    getRelativePosition: function (elementId, clientX, clientY) {
        let element = document.getElementById(elementId);

        if (!element) {
            return {
                RelativeX: 0,
                RelativeY: 0,
                RelativeScrollLeft: 0,
                RelativeScrollTop: 0
            }
        }

        let bounds = element.getBoundingClientRect();

        let x = clientX - bounds.left;
        let y = clientY - bounds.top;

        return {
            RelativeX: x,
            RelativeY: y,
            RelativeScrollLeft: element.scrollLeft,
            RelativeScrollTop: element.scrollTop
        }
    },
    setScrollPositionBoth: function (textEditorBodyId, scrollLeft, scrollTop) {
        let textEditorBody = document.getElementById(textEditorBodyId);

        if (!textEditorBody) {
            return;
        }
        
		// 0 is falsey
        if (scrollLeft || scrollLeft === 0) {
            textEditorBody.scrollLeft = scrollLeft;
        }
        
		// 0 is falsey
        if (scrollTop || scrollTop === 0) {
            textEditorBody.scrollTop = scrollTop;
        }
    },
    setScrollPositionLeft: function (textEditorBodyId, scrollLeft) {
        let textEditorBody = document.getElementById(textEditorBodyId);

        if (!textEditorBody) {
            return;
        }
        
		// 0 is falsey
        if (scrollLeft || scrollLeft === 0) {
            textEditorBody.scrollLeft = scrollLeft;
        }
    },
    setScrollPositionTop: function (textEditorBodyId, scrollTop) {
        let textEditorBody = document.getElementById(textEditorBodyId);

        if (!textEditorBody) {
            return;
        }
        
		// 0 is falsey
        if (scrollTop || scrollTop === 0) {
            textEditorBody.scrollTop = scrollTop;
        }
    },
    getTextEditorMeasurementsInPixelsById: function (elementId) {
        let elementReference = document.getElementById(elementId);

        if (!elementReference) {
            return {
                Width: 0,
                Height: 0,
				BoundingClientRectLeft: 0,
				BoundingClientRectTop: 0,
            };
        }

		let boundingClientRect = elementReference.getBoundingClientRect();

        return {
            Width: Math.ceil(elementReference.offsetWidth),
            Height: Math.ceil(elementReference.offsetHeight),
			BoundingClientRectLeft: boundingClientRect.left,
			BoundingClientRectTop: boundingClientRect.top,
        };
    },
    getBoundingClientRect: function (elementId) {
        let element = document.getElementById(elementId);

        if (!element) {
            return {
                Left: -1,
                Top: -1,
            };
        }

        let boundingClientRect = element.getBoundingClientRect();

        return {
            Left: boundingClientRect.left,
            Top: boundingClientRect.top,
        };
    }
}