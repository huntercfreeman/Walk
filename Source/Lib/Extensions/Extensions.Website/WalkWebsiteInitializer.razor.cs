using System.Text;
using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.Extensions.DotNet;
using Walk.Extensions.DotNet.DotNetSolutions.Models;
using Walk.Website.RazorLib.Websites.ProjectTemplates.Models;
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
            DotNetService.TextEditorService.CommonService.Continuous_Enqueue(new BackgroundTask(
                Key<IBackgroundTaskGroup>.Empty,
                Do_WalkWebsiteInitializerOnAfterRenderAsync));
        }
        
        return Task.CompletedTask;
    }
    
    public async ValueTask Do_WalkWebsiteInitializerOnAfterRenderAsync()
    {
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

    private async Task ParseSolutionAsync()
    {
        var tokenBuilder = new StringBuilder();
        var formattedBuilder = new StringBuilder();
    
        // Create a Blazor Wasm app
        var cSharpProjectAbsolutePath = DotNetService.CommonService.EnvironmentProvider.AbsolutePathFactory(
            InitialSolutionFacts.BLAZOR_CRUD_APP_WASM_CSPROJ_ABSOLUTE_FILE_PATH,
            false,
            tokenBuilder,
            formattedBuilder);
            
        var parentDirectoryOfProject = cSharpProjectAbsolutePath.ParentDirectory;

        if (parentDirectoryOfProject is null)
            throw new NotImplementedException();

        var ancestorDirectory = parentDirectoryOfProject;
        
        TextEditorModel programCsModel;
        // ProgramCs
        {
            var absolutePathString = "/BlazorCrudApp/ConsoleApp/Program.cs";
            
            Website_WriteAllText(
                absolutePathString,
                InitialSolutionFacts.PERSON_CS_CONTENTS,
                tokenBuilder,
                formattedBuilder);

            programCsModel = new TextEditorModel(
                new ResourceUri(absolutePathString),
                DateTime.UtcNow,
                "cs",
                InitialSolutionFacts.PERSON_CS_CONTENTS,
                DotNetService.TextEditorService.GetDecorationMapper("cs"),
                DotNetService.TextEditorService.GetCompilerService("cs"),
                DotNetService.TextEditorService);
        }
        
        TextEditorModel csprojModel;
        // Csproj
        {
            
            Website_WriteAllText(
                InitialSolutionFacts.BLAZOR_CRUD_APP_WASM_CSPROJ_ABSOLUTE_FILE_PATH,
                ConsoleAppFacts.CsprojContents,
                tokenBuilder,
                formattedBuilder);
            
            csprojModel = new TextEditorModel(
                new ResourceUri(InitialSolutionFacts.BLAZOR_CRUD_APP_WASM_CSPROJ_ABSOLUTE_FILE_PATH),
                DateTime.UtcNow,
                "csproj",
                ConsoleAppFacts.CsprojContents,
                DotNetService.TextEditorService.GetDecorationMapper("csproj"),
                DotNetService.TextEditorService.GetCompilerService("csproj"),
                DotNetService.TextEditorService);
        }
        
        var solutionAbsolutePath = DotNetService.TextEditorService.CommonService.EnvironmentProvider.AbsolutePathFactory(
            InitialSolutionFacts.SLN_ABSOLUTE_FILE_PATH,
            false,
            tokenBuilder,
            formattedBuilder);
        
        TextEditorModel exampleSolutionModel;
        // ExampleSolution.sln
        {
            Website_WriteAllText(
                InitialSolutionFacts.SLN_ABSOLUTE_FILE_PATH,
                InitialSolutionFacts.SLN_CONTENTS,
                tokenBuilder,
                formattedBuilder);
            
            var absolutePath = solutionAbsolutePath;

            exampleSolutionModel = new TextEditorModel(
                new ResourceUri(InitialSolutionFacts.SLN_ABSOLUTE_FILE_PATH),
                DateTime.UtcNow,
                absolutePath.ExtensionNoPeriod,
                InitialSolutionFacts.SLN_CONTENTS,
                DotNetService.TextEditorService.GetDecorationMapper(absolutePath.ExtensionNoPeriod),
                DotNetService.TextEditorService.GetCompilerService(absolutePath.ExtensionNoPeriod),
                DotNetService.TextEditorService);            
        }

        // This line is also in WalkExtensionsDotNetInitializer,
        // but its duplicated here because the website
        // won't open the first file correctly without this.
        DotNetService.TextEditorService.UpsertHeader("cs", typeof(Walk.Extensions.CompilerServices.Displays.TextEditorCompilerServiceHeaderDisplay));

        DotNetService.Enqueue(new DotNetWorkArgs
        {
            WorkKind = DotNetWorkKind.SetDotNetSolution,
            DotNetSolutionAbsolutePath = solutionAbsolutePath,
        });
        
        DotNetService.TextEditorService.WorkerArbitrary.PostUnique(editContext =>
        {
            // programCsModel
            {
                DotNetService.TextEditorService.Model_RegisterCustom(editContext, programCsModel);
                
                var modelModifier = editContext.GetModelModifier(programCsModel.PersistentState.ResourceUri);
    
                if (modelModifier is null)
                    return ValueTask.CompletedTask;
    
                DotNetService.TextEditorService.Model_AddPresentationModel(
                    editContext,
                    modelModifier,
                    TextEditorFacts.CompilerServiceDiagnosticPresentation_EmptyPresentationModel);
    
                DotNetService.TextEditorService.Model_AddPresentationModel(
                    editContext,
                    modelModifier,
                    TextEditorFacts.FindOverlayPresentation_EmptyPresentationModel);
    
                programCsModel.PersistentState.CompilerService.RegisterResource(
                    programCsModel.PersistentState.ResourceUri,
                    shouldTriggerResourceWasModified: true);
            }
            
            // csprojModel
            {
                DotNetService.TextEditorService.Model_RegisterCustom(editContext, csprojModel);
                
                var modelModifier = editContext.GetModelModifier(csprojModel.PersistentState.ResourceUri);
    
                if (modelModifier is null)
                    return ValueTask.CompletedTask;
    
                DotNetService.TextEditorService.Model_AddPresentationModel(
                    editContext,
                    modelModifier,
                    TextEditorFacts.CompilerServiceDiagnosticPresentation_EmptyPresentationModel);
    
                DotNetService.TextEditorService.Model_AddPresentationModel(
                    editContext,
                    modelModifier,
                    TextEditorFacts.FindOverlayPresentation_EmptyPresentationModel);
    
                csprojModel.PersistentState.CompilerService.RegisterResource(
                    csprojModel.PersistentState.ResourceUri,
                    shouldTriggerResourceWasModified: true);
            }
            
            // exampleSolutionModel
            {
                DotNetService.TextEditorService.Model_RegisterCustom(editContext, exampleSolutionModel);
                
                var modelModifier = editContext.GetModelModifier(exampleSolutionModel.PersistentState.ResourceUri);
    
                if (modelModifier is null)
                    return ValueTask.CompletedTask;
    
                DotNetService.TextEditorService.Model_AddPresentationModel(
                    editContext,
                    modelModifier,
                    TextEditorFacts.CompilerServiceDiagnosticPresentation_EmptyPresentationModel);
    
                DotNetService.TextEditorService.Model_AddPresentationModel(
                    editContext,
                    modelModifier,
                    TextEditorFacts.FindOverlayPresentation_EmptyPresentationModel);
    
                exampleSolutionModel.PersistentState.CompilerService.RegisterResource(
                    exampleSolutionModel.PersistentState.ResourceUri,
                    shouldTriggerResourceWasModified: true);
            }
            
            return ValueTask.CompletedTask;
        });
        
        DotNetService.TextEditorService.WorkerArbitrary.PostUnique(async editContext =>
        {
            // Display a file from the get-go so the user is less confused on what the website is.
            var absolutePath = DotNetService.TextEditorService.CommonService.EnvironmentProvider.AbsolutePathFactory(
                "/BlazorCrudApp/ConsoleApp/Program.cs",
                false,
                tokenBuilder,
                formattedBuilder);
        
            await DotNetService.TextEditorService.OpenInEditorAsync(
                editContext,
                absolutePath.Value,
                false,
                null,
                new Category("main"),
                Key<TextEditorViewModel>.NewKey());
        });
    }
    
    public void Website_WriteAllText(
        string absolutePathString,
        string contents,
        StringBuilder stringBuilder,
        StringBuilder formattedBuilder)
    {
        InMemoryFileSystemProvider inMemoryFileSystemProvider = (InMemoryFileSystemProvider)DotNetService.CommonService.FileSystemProvider;

        // Ensure Parent Directories Exist
        {
            var parentDirectoryList = absolutePathString
                .Split("/")
                // The root directory splits into string.Empty
                .Skip(1)
                // Skip the file being written to itself
                .SkipLast(1)
                .ToArray();

            stringBuilder.Append("/");

            for (int i = 0; i < parentDirectoryList.Length; i++)
            {
                stringBuilder.Append(parentDirectoryList[i]);
                stringBuilder.Append("/");

                ((InMemoryFileSystemProvider.InMemoryDirectoryHandler)inMemoryFileSystemProvider.Directory).UnsafeCreateDirectoryAsync(
                    stringBuilder.ToString());
            }
        
            stringBuilder.Clear();
        }

        var absolutePath = DotNetService.CommonService.EnvironmentProvider.AbsolutePathFactory(
            absolutePathString,
            false,
            tokenBuilder: stringBuilder,
            formattedBuilder);

        var outFile = new InMemoryFile(
            contents,
            absolutePath,
            DateTime.UtcNow,
            false);

        inMemoryFileSystemProvider.__Files.Add(outFile);

        DotNetService.CommonService.EnvironmentProvider.DeletionPermittedRegister(
            new SimplePath(absolutePathString, isDirectory: false),
            tokenBuilder: stringBuilder,
            formattedBuilder);
    }
}
