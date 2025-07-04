// The way JS Interop is done here is a bit outdated see export syntax:
// https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/

window.walkIde = {
    preventDefaultBrowserKeybindings: function (elementId) {
    	// This function is intended to only be invoked
    	// from C# when the WalkHostingKind is Photino.
    	//
    	// Because when running the native application,
    	// the fact that, for example, 'F5' refreshes
    	// the native application is very frustrating.
        window.addEventListener('keydown', function(e) {
        	if (e.code === 'F5') {
        		e.preventDefault();
        	}

		    if (e.ctrlKey) {
		    
		    	switch (e.code)
		    	{
		    		case 'KeyR':
		    		case 'KeyP':
		    			e.preventDefault();
		    			break;
		    	}
		    }
		});
    },
}
