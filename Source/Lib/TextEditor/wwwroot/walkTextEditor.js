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
            contentElement.addEventListener('touchstart', (event) => {
                dotNetHelper.invokeMethodAsync("ReceiveOnTouchStart", event);
                event.preventDefault();
            }, {
                passive: false,
            });
        
            contentElement.addEventListener('keydown', (event) => {
                dotNetHelper.invokeMethodAsync("ReceiveOnKeyDown", event);
                event.preventDefault();
            });
            
            contentElement.addEventListener('click', (event) => {
                dotNetHelper.invokeMethodAsync("FocusTextEditorAsync", event);
            });
            
            contentElement.addEventListener('contextmenu', (event) => {
                dotNetHelper.invokeMethodAsync("ReceiveOnContextMenu", event);
                event.preventDefault();
            });
            
            contentElement.addEventListener('mousedown', (event) => {
                dotNetHelper.invokeMethodAsync("ReceiveContentOnMouseDown", 
                {
                    Buttons: event.buttons,
                    ClientX: event.clientX,
                    ClientY: event.clientY,
                    ShiftKey: event.shiftKey,
                });
            });
            
            contentElement.addEventListener('mousemove', (event) => {
                dotNetHelper.invokeMethodAsync("ReceiveContentOnMouseMove", event);
            });
            
            contentElement.addEventListener('mouseout', (event) => {
                dotNetHelper.invokeMethodAsync("ReceiveContentOnMouseOut", event);
            });
            
            contentElement.addEventListener('dblclick', (event) => {
                dotNetHelper.invokeMethodAsync("ReceiveOnDoubleClick", event);
            });
            
            contentElement.addEventListener('wheel', (event) => {
                dotNetHelper.invokeMethodAsync("ReceiveOnWheel", event);
            });
            
            contentElement.addEventListener('touchmove', (event) => {
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
            });
        }
        
        /*
        let HORIZONTAL_ScrollbarElement = document.getElementById(HORIZONTAL_ScrollbarElementId);
        if (HORIZONTAL_ScrollbarElement) {
        
            HORIZONTAL_ScrollbarElement.addEventListener('onmousemove', (event) => {
                event.stopPropagation();
            });
            
            HORIZONTAL_ScrollbarElement.addEventListener('ondblclick', (event) => {
                event.stopPropagation();
            });
            
            HORIZONTAL_ScrollbarElement.addEventListener('onclick', (event) => {
                event.stopPropagation();
            });
            
            HORIZONTAL_ScrollbarElement.addEventListener('oncontextmenu', (event) => {
                event.stopPropagation();
            });
            
            HORIZONTAL_ScrollbarElement.addEventListener('onmousedown', (event) => {
                dotNetHelper.invokeMethodAsync("HORIZONTAL_HandleOnMouseDownAsync", event);
                event.stopPropagation();
            });
            
        }
        
        let VERTICAL_ScrollbarElement = document.getElementById(VERTICAL_ScrollbarElementId);
        if (VERTICAL_ScrollbarElement) {
        
            VERTICAL_ScrollbarElement.addEventListener('onmousemove', (event) => {
                event.stopPropagation();
            });
            
            VERTICAL_ScrollbarElement.addEventListener('ondblclick', (event) => {
                event.stopPropagation();
            });
            
            VERTICAL_ScrollbarElement.addEventListener('onclick', (event) => {
                event.stopPropagation();
            });
            
            VERTICAL_ScrollbarElement.addEventListener('oncontextmenu', (event) => {
                event.stopPropagation();
            });
            
            VERTICAL_ScrollbarElement.addEventListener('onmousedown', (event) => {
                dotNetHelper.invokeMethodAsync("VERTICAL_HandleOnMouseDownAsync", event);
                event.stopPropagation();
            });
            
        }
        
        let CONNECTOR_ScrollbarElement = document.getElementById(CONNECTOR_ScrollbarElementId);
        if (CONNECTOR_ScrollbarElement) {
            
            CONNECTOR_ScrollbarElement.addEventListener('onmousemove', (event) => {
                event.stopPropagation();
            });
            
            CONNECTOR_ScrollbarElement.addEventListener('ondblclick', (event) => {
                event.stopPropagation();
            });
            
            CONNECTOR_ScrollbarElement.addEventListener('onclick', (event) => {
                event.stopPropagation();
            });
            
            CONNECTOR_ScrollbarElement.addEventListener('oncontextmenu', (event) => {
                event.stopPropagation();
            });
            
            CONNECTOR_ScrollbarElement.addEventListener('onmousedown', (event) => {
                event.stopPropagation();
            });
        }
        */
    },
    getCharAndLineMeasurementsInPixelsById: function (elementId, amountOfCharactersRendered) {
        let element = document.getElementById(elementId);

        if (!element) {
            return {
                CharacterWidth: 5,
                LineHeight: 5
            }
        }
        
        let fontWidth = element.offsetWidth / amountOfCharactersRendered;

        return {
            CharacterWidth: fontWidth,
            LineHeight: element.offsetHeight
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