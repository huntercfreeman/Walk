namespace Walk.Common.RazorLib;

public partial class CommonService
{
    public async Task<string> ReadClipboard()
    {
        try
        {
            return await JsRuntimeCommonApi
                .ReadClipboard()
                .ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            return string.Empty;
        }
    }

    public async Task SetClipboard(string value)
    {
        try
        {
            await JsRuntimeCommonApi
                .SetClipboard(value)
                .ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
        }
    }
}
