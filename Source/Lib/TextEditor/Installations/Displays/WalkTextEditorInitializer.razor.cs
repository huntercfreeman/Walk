using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Contexts.Models;
using Walk.Common.RazorLib.Dimensions.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Options.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;
using Walk.TextEditor.RazorLib.Lexers.Models;

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
    public ITextEditorRegistryWrap TextEditorRegistryWrap { get; set; } = null!;
    [Inject]
    public ICompilerServiceRegistry CompilerServiceRegistry { get; set; } = null!;
    [Inject]
    public IDecorationMapperRegistry DecorationMapperRegistry { get; set; } = null!;
    [Inject]
    private TextEditorService TextEditorService { get; set; } = null!;

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
    	
    	TextEditorRegistryWrap.CompilerServiceRegistry = CompilerServiceRegistry;
    	TextEditorRegistryWrap.DecorationMapperRegistry = DecorationMapperRegistry;
    	
    	TextEditorService.OptionsApi.NeedsMeasured += OnNeedsMeasured;

        TextEditorService.Enqueue_TextEditorInitializationBackgroundTaskGroupWorkKind();
            
        base.OnInitialized();
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
    	if (firstRender)
    	{
    		await Ready();
    		QueueRemeasureBackgroundTask();
    	}
    	
    	base.OnAfterRender(firstRender);
    }
    
    private async Task Ready()
    {
    	_wrapperCssClass = $"di_te_text-editor-css-wrapper {TextEditorService.ThemeCssClassString} ";
    	
    	var options = TextEditorService.OptionsApi.GetTextEditorOptionsState().Options;
    	
    	var fontSizeInPixels = TextEditorOptionsState.DEFAULT_FONT_SIZE_IN_PIXELS;
    	if (options.CommonOptions?.FontSizeInPixels is not null)
            fontSizeInPixels = options!.CommonOptions.FontSizeInPixels;
    	var fontSizeCssStyle = $"font-size: {fontSizeInPixels.ToCssValue()}px;";
    	
    	var fontFamily = TextEditorRenderBatch.DEFAULT_FONT_FAMILY;
    	if (!string.IsNullOrWhiteSpace(options?.CommonOptions?.FontFamily))
        	fontFamily = options!.CommonOptions!.FontFamily;
    	var fontFamilyCssStyle = $"font-family: {fontFamily};";
    	
    	_wrapperCssStyle = $"{fontSizeCssStyle} {fontFamilyCssStyle}";
    	
    	await InvokeAsync(StateHasChanged);
    }
    
	private async void OnNeedsMeasured()
	{
		await Ready();
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
	            
	        TextEditorService.OptionsApi.SetCharAndLineMeasurements(editContext, charAndLineMeasurements);
        });
    }
    
    public void Dispose()
    {
    	TextEditorService.OptionsApi.NeedsMeasured -= OnNeedsMeasured;
    }
}