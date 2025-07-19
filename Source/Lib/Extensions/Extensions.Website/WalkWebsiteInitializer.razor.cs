using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.Diffs.Models;
using Walk.Extensions.DotNet;
using Walk.Extensions.DotNet.DotNetSolutions.Models;
using Walk.Extensions.DotNet.Websites.ProjectTemplates.Models;
using Walk.Ide.Wasm.Facts;

namespace Walk.Website.RazorLib;

public partial class WalkWebsiteInitializer : ComponentBase
{
    [Inject]
    private DotNetService DotNetService { get; set; } = null!;

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
        	DotNetService.TextEditorService.CommonService.Continuous_EnqueueGroup(new BackgroundTask(
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
            DotNetService.TextEditorService.CommonService.TreeView_MoveRight(
                DotNetSolutionState.TreeViewSolutionExplorerStateKey,
                false,
                false);

            DotNetService.TextEditorService.CommonService.TreeView_MoveRight(
                DotNetSolutionState.TreeViewSolutionExplorerStateKey,
                false,
                false);

            DotNetService.TextEditorService.CommonService.TreeView_MoveRight(
                DotNetSolutionState.TreeViewSolutionExplorerStateKey,
            false,
                false);
        }
    }

    private async Task WriteFileSystemInMemoryAsync()
    {
        // Create a Blazor Wasm app
        await WebsiteProjectTemplateFacts.HandleNewCSharpProjectAsync(
                WebsiteProjectTemplateFacts.ConsoleAppProjectTemplate.ShortName!,
                InitialSolutionFacts.BLAZOR_CRUD_APP_WASM_CSPROJ_ABSOLUTE_FILE_PATH,
                DotNetService.TextEditorService.CommonService)
            .ConfigureAwait(false);

        await DotNetService.TextEditorService.CommonService.FileSystemProvider.File.WriteAllTextAsync(
                InitialSolutionFacts.PERSON_CS_ABSOLUTE_FILE_PATH,
                InitialSolutionFacts.PERSON_CS_CONTENTS)
            .ConfigureAwait(false);

        await DotNetService.TextEditorService.CommonService.FileSystemProvider.File.WriteAllTextAsync(
                InitialSolutionFacts.PERSON_DISPLAY_RAZOR_CS_ABSOLUTE_FILE_PATH,
                InitialSolutionFacts.PERSON_DISPLAY_RAZOR_CS_CONTENTS)
            .ConfigureAwait(false);

        await DotNetService.TextEditorService.CommonService.FileSystemProvider.File.WriteAllTextAsync(
                InitialSolutionFacts.PERSON_DISPLAY_RAZOR_ABSOLUTE_FILE_PATH,
                InitialSolutionFacts.PERSON_DISPLAY_RAZOR_CONTENTS)
            .ConfigureAwait(false);

        /*await _fileSystemProvider.File.WriteAllTextAsync(
                InitialSolutionFacts.BLAZOR_CRUD_APP_ALL_C_SHARP_SYNTAX_ABSOLUTE_FILE_PATH,
                InitialSolutionFacts.BLAZOR_CRUD_APP_ALL_C_SHARP_SYNTAX_CONTENTS)
            .ConfigureAwait(false);*/

        // ExampleSolution.sln
        await DotNetService.TextEditorService.CommonService.FileSystemProvider.File.WriteAllTextAsync(
                InitialSolutionFacts.SLN_ABSOLUTE_FILE_PATH,
                InitialSolutionFacts.SLN_CONTENTS)
            .ConfigureAwait(false);

        var solutionAbsolutePath = DotNetService.TextEditorService.CommonService.EnvironmentProvider.AbsolutePathFactory(
            InitialSolutionFacts.SLN_ABSOLUTE_FILE_PATH,
            false);

        // This line is also in WalkExtensionsDotNetInitializer,
        // but its duplicated here because the website
        // won't open the first file correctly without this.
        DotNetService.TextEditorService.UpsertHeader("cs", typeof(Walk.Extensions.CompilerServices.Displays.TextEditorCompilerServiceHeaderDisplay));

        DotNetService.Enqueue(new DotNetWorkArgs
        {
        	WorkKind = DotNetWorkKind.SetDotNetSolution,
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
                var childDirectories = await DotNetService.TextEditorService.CommonService.FileSystemProvider.Directory
                    .GetDirectoriesAsync(directory)
                    .ConfigureAwait(false);
                allFiles.AddRange(
                    await DotNetService.TextEditorService.CommonService.FileSystemProvider.Directory.GetFilesAsync(directory).ConfigureAwait(false));

                await RecursiveStep(childDirectories, allFiles).ConfigureAwait(false);
            }
        }

        foreach (var file in allFiles)
        {
            var absolutePath = DotNetService.TextEditorService.CommonService.EnvironmentProvider.AbsolutePathFactory(file, false);
            var resourceUri = new ResourceUri(file);
            var fileLastWriteTime = await DotNetService.TextEditorService.CommonService.FileSystemProvider.File.GetLastWriteTimeAsync(file).ConfigureAwait(false);
            var content = await DotNetService.TextEditorService.CommonService.FileSystemProvider.File.ReadAllTextAsync(file).ConfigureAwait(false);

            var decorationMapper = DotNetService.TextEditorService.GetDecorationMapper(absolutePath.ExtensionNoPeriod);
            var compilerService = DotNetService.TextEditorService.GetCompilerService(absolutePath.ExtensionNoPeriod);

            var textEditorModel = new TextEditorModel(
                resourceUri,
                fileLastWriteTime,
                absolutePath.ExtensionNoPeriod,
                content,
                decorationMapper,
                compilerService,
                DotNetService.TextEditorService);

            DotNetService.TextEditorService.WorkerArbitrary.PostUnique(editContext =>
            {
            	DotNetService.TextEditorService.Model_RegisterCustom(editContext, textEditorModel);
            	
                var modelModifier = editContext.GetModelModifier(textEditorModel.PersistentState.ResourceUri);

                if (modelModifier is null)
                    return ValueTask.CompletedTask;

                DotNetService.TextEditorService.Model_AddPresentationModel(
                    editContext,
                    modelModifier,
                    CompilerServiceDiagnosticPresentationFacts.EmptyPresentationModel);

                DotNetService.TextEditorService.Model_AddPresentationModel(
                    editContext,
                    modelModifier,
                    FindOverlayPresentationFacts.EmptyPresentationModel);

                DotNetService.TextEditorService.Model_AddPresentationModel(
                    editContext,
                    modelModifier,
                    DiffPresentationFacts.EmptyInPresentationModel);

                DotNetService.TextEditorService.Model_AddPresentationModel(
                    editContext,
                    modelModifier,
                    DiffPresentationFacts.EmptyOutPresentationModel);

                textEditorModel.PersistentState.CompilerService.RegisterResource(
                    textEditorModel.PersistentState.ResourceUri,
                    shouldTriggerResourceWasModified: true);
                return ValueTask.CompletedTask;
            });
        }
        
		DotNetService.TextEditorService.WorkerArbitrary.PostUnique(async editContext =>
		{
			// Display a file from the get-go so the user is less confused on what the website is.
	        var absolutePath = DotNetService.TextEditorService.CommonService.EnvironmentProvider.AbsolutePathFactory(
	            InitialSolutionFacts.PERSON_CS_ABSOLUTE_FILE_PATH,
	            false);
		
			await DotNetService.TextEditorService.OpenInEditorAsync(
				editContext,
	            absolutePath.Value,
	            false,
	            null,
	            new Category("main"),
	        	Key<TextEditorViewModel>.NewKey());
		});
    }
}