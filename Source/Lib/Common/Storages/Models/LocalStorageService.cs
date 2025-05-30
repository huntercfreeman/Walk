using Walk.Common.RazorLib.JsRuntimes.Models;
using Microsoft.JSInterop;

namespace Walk.Common.RazorLib.Storages.Models;

public class LocalStorageService : IStorageService
{
    private readonly IJSRuntime _jsRuntime;

    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }
    
    private WalkCommonJavaScriptInteropApi _jsRuntimeCommonApi;
	
	private WalkCommonJavaScriptInteropApi JsRuntimeCommonApi => _jsRuntimeCommonApi
		??= _jsRuntime.GetWalkCommonApi();

    public async ValueTask SetValue(string key, object? value)
    {
        await JsRuntimeCommonApi.LocalStorageSetItem(
                key,
                value)
            .ConfigureAwait(false);
    }

    public async ValueTask<object?> GetValue(string key)
    {
        return await JsRuntimeCommonApi.LocalStorageGetItem(
                key)
            .ConfigureAwait(false);
    }
}