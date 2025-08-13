using System.Text;
using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.Keys.Models;
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
            var absolutePathString = DotNetService.CommonService.EnvironmentProvider.JoinPaths(
                ancestorDirectory,
                ConsoleAppFacts.PROGRAM_CS_RELATIVE_FILE_PATH);
            var absolutePath = DotNetService.TextEditorService.CommonService.EnvironmentProvider.AbsolutePathFactory(absolutePathString, false, tokenBuilder, formattedBuilder);
    
            DotNetService.CommonService.FileSystemProvider.File.WriteAllText(
                absolutePath.Value,
                InitialSolutionFacts.PERSON_CS_CONTENTS);

            var resourceUri = new ResourceUri(absolutePath.Value);
            var fileLastWriteTime = DateTime.UtcNow;
            var content = InitialSolutionFacts.PERSON_CS_CONTENTS;
            
            var decorationMapper = DotNetService.TextEditorService.GetDecorationMapper(absolutePath.ExtensionNoPeriod);
            var compilerService = DotNetService.TextEditorService.GetCompilerService(absolutePath.ExtensionNoPeriod);

            programCsModel = new TextEditorModel(
                resourceUri,
                fileLastWriteTime,
                absolutePath.ExtensionNoPeriod,
                content,
                decorationMapper,
                compilerService,
                DotNetService.TextEditorService);
        }
        
        TextEditorModel csprojModel;
        // Csproj
        {
            var content = ConsoleAppFacts.GetCsprojContents(cSharpProjectAbsolutePath.NameNoExtension);
        
            DotNetService.CommonService.FileSystemProvider.File.WriteAllText(
                InitialSolutionFacts.BLAZOR_CRUD_APP_WASM_CSPROJ_ABSOLUTE_FILE_PATH,
                content);
        
            var absolutePath = DotNetService.TextEditorService.CommonService.EnvironmentProvider.AbsolutePathFactory(InitialSolutionFacts.BLAZOR_CRUD_APP_WASM_CSPROJ_ABSOLUTE_FILE_PATH, false, tokenBuilder, formattedBuilder);
            var resourceUri = new ResourceUri(InitialSolutionFacts.BLAZOR_CRUD_APP_WASM_CSPROJ_ABSOLUTE_FILE_PATH);
            var fileLastWriteTime = DateTime.UtcNow;

            var decorationMapper = DotNetService.TextEditorService.GetDecorationMapper(absolutePath.ExtensionNoPeriod);
            var compilerService = DotNetService.TextEditorService.GetCompilerService(absolutePath.ExtensionNoPeriod);

            csprojModel = new TextEditorModel(
                resourceUri,
                fileLastWriteTime,
                absolutePath.ExtensionNoPeriod,
                content,
                decorationMapper,
                compilerService,
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
            DotNetService.TextEditorService.CommonService.FileSystemProvider.File.WriteAllText(
                InitialSolutionFacts.SLN_ABSOLUTE_FILE_PATH,
                InitialSolutionFacts.SLN_CONTENTS);
            
            var absolutePath = solutionAbsolutePath;
            var resourceUri = new ResourceUri(InitialSolutionFacts.SLN_ABSOLUTE_FILE_PATH);
            var fileLastWriteTime = DateTime.UtcNow;
            var content = InitialSolutionFacts.SLN_CONTENTS;

            var decorationMapper = DotNetService.TextEditorService.GetDecorationMapper(absolutePath.ExtensionNoPeriod);
            var compilerService = DotNetService.TextEditorService.GetCompilerService(absolutePath.ExtensionNoPeriod);

            exampleSolutionModel = new TextEditorModel(
                resourceUri,
                fileLastWriteTime,
                absolutePath.ExtensionNoPeriod,
                content,
                decorationMapper,
                compilerService,
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
}
