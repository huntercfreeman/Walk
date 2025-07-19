using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.Common.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;

namespace Walk.TextEditor.RazorLib.Installations.Displays;

/// <remarks>
/// This class is an exception to the naming convention, "don't use the word 'Walk' in class names".
/// 
/// Reason for this exception: when one first starts interacting with this project,
/// 	this type might be one of the first types they interact with. So, the redundancy of namespace
/// 	and type containing 'Walk' feels reasonable here.
/// </remarks>
public partial class WalkTextEditorInitializer : ComponentBase, IDisposable
{
    [Inject]
    private TextEditorService TextEditorService { get; set; } = null!;
    [Inject]
    private CommonService CommonService { get; set; } = null!;

    public static Key<ContextSwitchGroup> ContextSwitchGroupKey { get; } = Key<ContextSwitchGroup>.NewKey();
    
    private const string TEST_STRING_FOR_MEASUREMENT = "abcdefghijklmnopqrstuvwxyz0123456789";
    private const int TEST_STRING_REPEAT_COUNT = 6;
    
    private int _countOfTestCharacters;
    private string _measureCharacterWidthAndLineHeightElementId = "di_te_measure-character-width-and-line-height";
    
    private string _wrapperCssClass;
    private string _wrapperCssStyle;
    
    protected override void OnInitialized()
    {
    	_countOfTestCharacters = TEST_STRING_REPEAT_COUNT * TEST_STRING_FOR_MEASUREMENT.Length;
    	
    	TextEditorService.Options_NeedsMeasured += OnNeedsMeasured;

        TextEditorService.Enqueue_TextEditorInitializationBackgroundTaskGroupWorkKind();
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
    	if (firstRender)
    	{
    		await InvokeAsync(Ready);
    		QueueRemeasureBackgroundTask();
    	}
    }
    
    /// <summary>
    /// Only invoke this method from the UI thread due to the usage of the shared UiStringBuilder.
    /// </summary>
    private async Task Ready()
    {
        CommonService.UiStringBuilder.Clear();
        CommonService.UiStringBuilder.Append("di_te_text-editor-css-wrapper ");
        CommonService.UiStringBuilder.Append(TextEditorService.ThemeCssClassString);
    	_wrapperCssClass = CommonService.UiStringBuilder.ToString();
    	
    	var options = TextEditorService.Options_GetTextEditorOptionsState().Options;
    	
    	var fontSizeInPixels = TextEditorOptionsState.DEFAULT_FONT_SIZE_IN_PIXELS;
    	if (options.CommonOptions?.FontSizeInPixels is not null)
            fontSizeInPixels = options!.CommonOptions.FontSizeInPixels;
        CommonService.UiStringBuilder.Clear();
        CommonService.UiStringBuilder.Append("font-size: ");
        CommonService.UiStringBuilder.Append(fontSizeInPixels.ToCssValue());
        CommonService.UiStringBuilder.Append("px;");
    	var fontSizeCssStyle = CommonService.UiStringBuilder.ToString();
    	
    	var fontFamily = TextEditorVirtualizationResult.DEFAULT_FONT_FAMILY;
    	if (!string.IsNullOrWhiteSpace(options?.CommonOptions?.FontFamily))
        	fontFamily = options!.CommonOptions!.FontFamily;
    	CommonService.UiStringBuilder.Clear();
    	CommonService.UiStringBuilder.Append("font-family: ");
    	CommonService.UiStringBuilder.Append(fontFamily);
    	CommonService.UiStringBuilder.Append(";");
    	var fontFamilyCssStyle = CommonService.UiStringBuilder.ToString();
    	
    	CommonService.UiStringBuilder.Clear();
    	CommonService.UiStringBuilder.Append(fontSizeCssStyle);
    	CommonService.UiStringBuilder.Append(" ");
    	CommonService.UiStringBuilder.Append(fontFamilyCssStyle);
    	CommonService.UiStringBuilder.Append(" position:absolute;");
    	_wrapperCssStyle = CommonService.UiStringBuilder.ToString();
    	
    	// I said "Only invoke this method from the UI thread due to the usage of the shared UiStringBuilder."
    	// But I'm still going to keep this InvokeAsync for the StateHasChanged due to superstituous anxiety.
    	await InvokeAsync(StateHasChanged);
    }
    
	private async void OnNeedsMeasured()
	{
		await InvokeAsync(Ready);
		QueueRemeasureBackgroundTask();
	}
	
    private void QueueRemeasureBackgroundTask()
    {
        TextEditorService.WorkerArbitrary.PostUnique(async editContext =>
        {
        	var charAndLineMeasurements = await TextEditorService.JsRuntimeTextEditorApi
	            .GetCharAndLineMeasurementsInPixelsById(
	                _measureCharacterWidthAndLineHeightElementId,
	                _countOfTestCharacters)
	            .ConfigureAwait(false);
	            
	        TextEditorService.Options_SetCharAndLineMeasurements(editContext, charAndLineMeasurements);
        });
    }
    
    public void Dispose()
    {
    	TextEditorService.Options_NeedsMeasured -= OnNeedsMeasured;
    }
}