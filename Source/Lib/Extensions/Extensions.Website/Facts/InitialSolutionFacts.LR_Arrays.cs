namespace Walk.Ide.Wasm.Facts;

public partial class InitialSolutionFacts
{
    public const string PERSON_CS_ABSOLUTE_FILE_PATH = @"/BlazorCrudApp/BlazorCrudApp.Wasm/Persons/Person.cs";
    public const string PERSON_CS_CONTENTS =
"""""""""
using Microsoft.AspNetCore.Components;

namespace Walk.Website.RazorLib;

public partial class WalkWebsiteInitializer : ComponentBase
{
    [Inject]
    private ITextEditorRegistryWrap TextEditorRegistryWrap { get; set; } = null!;
    [Inject]
    private IDecorationMapperRegistry DecorationMapperRegistry { get; set; } = null!;
    [Inject]
    private ICompilerServiceRegistry CompilerServiceRegistry { get; set; } = null!;
    [Inject]
    private TextEditorService TextEditorService { get; set; } = null!;
    [Inject]
    private ITextEditorHeaderRegistry TextEditorHeaderRegistry { get; set; } = null!;
    [Inject]
    private DotNetBackgroundTaskApi DotNetBackgroundTaskApi { get; set; } = null!;
    [Inject]
    private CommonUtilityService CommonUtilityService { get; set; } = null!;

    protected override void OnInitialized()
    {
        TextEditorRegistryWrap.DecorationMapperRegistry = DecorationMapperRegistry;
        TextEditorRegistryWrap.CompilerServiceRegistry = CompilerServiceRegistry;
    }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
        	CommonUtilityService.Continuous_EnqueueGroup(new BackgroundTask(
        		Key<IBackgroundTaskGroup>.Empty,
        		Do_WalkWebsiteInitializerOnAfterRenderAsync));
        }
        
        return Task.CompletedTask;
    }
    
    public async ValueTask Do_WalkWebsiteInitializerOnAfterRenderAsync()
    {
        await WriteFileSystemInMemoryAsync().ConfigureAwait(false);

        await ParseSolutionAsync().ConfigureAwait(false);

        // This code block is hacky. I want the Solution Explorer to from the get-go be fully expanded, so the user can see 'Program.cs'
        {
            CommonUtilityService.TreeView_MoveRight(
                DotNetSolutionState.TreeViewSolutionExplorerStateKey,
                false,
                false);

            CommonUtilityService.TreeView_MoveRight(
                DotNetSolutionState.TreeViewSolutionExplorerStateKey,
                false,
                false);

            CommonUtilityService.TreeView_MoveRight(
                DotNetSolutionState.TreeViewSolutionExplorerStateKey,
            false,
                false);
        }
    }

    private async Task WriteFileSystemInMemoryAsync()
    {
        // Create a Blazor Wasm app
        await WebsiteProjectTemplateFacts.HandleNewCSharpProjectAsync(
                WebsiteProjectTemplateFacts.BlazorWasmEmptyProjectTemplate.ShortName!,
                InitialSolutionFacts.BLAZOR_CRUD_APP_WASM_CSPROJ_ABSOLUTE_FILE_PATH,
                CommonUtilityService)
            .ConfigureAwait(false);

        await CommonUtilityService.FileSystemProvider.File.WriteAllTextAsync(
                InitialSolutionFacts.PERSON_CS_ABSOLUTE_FILE_PATH,
                InitialSolutionFacts.PERSON_CS_CONTENTS)
            .ConfigureAwait(false);

        await CommonUtilityService.FileSystemProvider.File.WriteAllTextAsync(
                InitialSolutionFacts.PERSON_DISPLAY_RAZOR_CS_ABSOLUTE_FILE_PATH,
                InitialSolutionFacts.PERSON_DISPLAY_RAZOR_CS_CONTENTS)
            .ConfigureAwait(false);

        await CommonUtilityService.FileSystemProvider.File.WriteAllTextAsync(
                InitialSolutionFacts.PERSON_DISPLAY_RAZOR_ABSOLUTE_FILE_PATH,
                InitialSolutionFacts.PERSON_DISPLAY_RAZOR_CONTENTS)
            .ConfigureAwait(false);

        /*await _fileSystemProvider.File.WriteAllTextAsync(
                InitialSolutionFacts.BLAZOR_CRUD_APP_ALL_C_SHARP_SYNTAX_ABSOLUTE_FILE_PATH,
                InitialSolutionFacts.BLAZOR_CRUD_APP_ALL_C_SHARP_SYNTAX_CONTENTS)
            .ConfigureAwait(false);*/

        // ExampleSolution.sln
        await CommonUtilityService.FileSystemProvider.File.WriteAllTextAsync(
                InitialSolutionFacts.SLN_ABSOLUTE_FILE_PATH,
                InitialSolutionFacts.SLN_CONTENTS)
            .ConfigureAwait(false);

        var solutionAbsolutePath = CommonUtilityService.EnvironmentProvider.AbsolutePathFactory(
            InitialSolutionFacts.SLN_ABSOLUTE_FILE_PATH,
            false);

        // This line is also in WalkExtensionsDotNetInitializer,
        // but its duplicated here because the website
        // won't open the first file correctly without this.
        TextEditorHeaderRegistry.UpsertHeader("cs", typeof(Walk.Extensions.CompilerServices.Displays.TextEditorCompilerServiceHeaderDisplay));

        DotNetBackgroundTaskApi.Enqueue(new DotNetBackgroundTaskApiWorkArgs
        {
        	WorkKind = DotNetBackgroundTaskApiWorkKind.SetDotNetSolution,
        	DotNetSolutionAbsolutePath = solutionAbsolutePath,
    	});
    }

    private async Task ParseSolutionAsync()
    {
        var allFiles = new List<string>();

        await RecursiveStep(
                new List<string> { "/" },
        allFiles)
        .ConfigureAwait(false);

        async Task RecursiveStep(IEnumerable<string> directories, List<string> allFiles)
        {
            foreach (var directory in directories)
            {
                var childDirectories = await CommonUtilityService.FileSystemProvider.Directory
                    .GetDirectoriesAsync(directory)
                    .ConfigureAwait(false);
                allFiles.AddRange(
                    await CommonUtilityService.FileSystemProvider.Directory.GetFilesAsync(directory).ConfigureAwait(false));

                await RecursiveStep(childDirectories, allFiles).ConfigureAwait(false);
            }
        }

        foreach (var file in allFiles)
        {
            var absolutePath = CommonUtilityService.EnvironmentProvider.AbsolutePathFactory(file, false);
            var resourceUri = new ResourceUri(file);
            var fileLastWriteTime = await CommonUtilityService.FileSystemProvider.File.GetLastWriteTimeAsync(file).ConfigureAwait(false);
            var content = await CommonUtilityService.FileSystemProvider.File.ReadAllTextAsync(file).ConfigureAwait(false);

            var decorationMapper = DecorationMapperRegistry.GetDecorationMapper(absolutePath.ExtensionNoPeriod);
            var compilerService = CompilerServiceRegistry.GetCompilerService(absolutePath.ExtensionNoPeriod);

            var textEditorModel = new TextEditorModel(
                resourceUri,
                fileLastWriteTime,
                absolutePath.ExtensionNoPeriod,
                content,
                decorationMapper,
                compilerService,
                TextEditorService);

            TextEditorService.WorkerArbitrary.PostUnique(editContext =>
            {
            	TextEditorService.ModelApi.RegisterCustom(editContext, textEditorModel);
            	
                var modelModifier = editContext.GetModelModifier(textEditorModel.PersistentState.ResourceUri);

                if (modelModifier is null)
                    return ValueTask.CompletedTask;

                TextEditorService.ModelApi.AddPresentationModel(
                    editContext,
                    modelModifier,
                    CompilerServiceDiagnosticPresentationFacts.EmptyPresentationModel);

                TextEditorService.ModelApi.AddPresentationModel(
                    editContext,
                    modelModifier,
                    FindOverlayPresentationFacts.EmptyPresentationModel);

                TextEditorService.ModelApi.AddPresentationModel(
                    editContext,
                    modelModifier,
                    DiffPresentationFacts.EmptyInPresentationModel);

                TextEditorService.ModelApi.AddPresentationModel(
                    editContext,
                    modelModifier,
                    DiffPresentationFacts.EmptyOutPresentationModel);

                textEditorModel.PersistentState.CompilerService.RegisterResource(
                    textEditorModel.PersistentState.ResourceUri,
                    shouldTriggerResourceWasModified: true);
                return ValueTask.CompletedTask;
            });
        }
        
		TextEditorService.WorkerArbitrary.PostUnique(async editContext =>
		{
			// Display a file from the get-go so the user is less confused on what the website is.
	        var absolutePath = CommonUtilityService.EnvironmentProvider.AbsolutePathFactory(
	            InitialSolutionFacts.PERSON_CS_ABSOLUTE_FILE_PATH,
	            false);
		
			await TextEditorService.OpenInEditorAsync(
				editContext,
	            absolutePath.Value,
	            false,
	            null,
	            new Category("main"),
	        	Key<TextEditorViewModel>.NewKey());
		});
    }
}
""""""""";
}
