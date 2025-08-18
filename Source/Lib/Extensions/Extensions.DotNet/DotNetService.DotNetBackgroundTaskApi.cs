using CliWrap.EventStream;
using System.Runtime.InteropServices;
using System.Text;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Commands.Models;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.Common.RazorLib.TreeViews.Models;
using Walk.CompilerServices.DotNetSolution.CompilerServiceCase;
using Walk.CompilerServices.DotNetSolution.Models;
using Walk.CompilerServices.DotNetSolution.Models.Project;
using Walk.CompilerServices.DotNetSolution.SyntaxActors;
using Walk.CompilerServices.CSharp.CompilerServiceCase;
using Walk.Extensions.DotNet.AppDatas.Models;
using Walk.Extensions.DotNet.CommandLines.Models;
using Walk.Extensions.DotNet.DotNetSolutions.Displays;
using Walk.Extensions.DotNet.DotNetSolutions.Models;
using Walk.Extensions.DotNet.Namespaces.Models;
using Walk.Extensions.DotNet.Nugets.Models;
using Walk.Extensions.DotNet.TestExplorers.Models;
using Walk.CompilerServices.Xml;
using Walk.Ide.RazorLib;
using Walk.Ide.RazorLib.Shareds.Models;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.BackgroundTasks.Models;
using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.Extensions.DotNet;

public partial class DotNetService
{
    #region DotNetSolutionIdeApi
    // private readonly IServiceProvider _serviceProvider;

    private readonly Key<TerminalCommandRequest> _newDotNetSolutionTerminalCommandRequestKey = Key<TerminalCommandRequest>.NewKey();
    private readonly CancellationTokenSource _newDotNetSolutionCancellationTokenSource = new();
    #endregion

    private static readonly Key<IDynamicViewModel> _newDotNetSolutionDialogKey = Key<IDynamicViewModel>.NewKey();

    public async ValueTask Do_SolutionExplorer_TreeView_MultiSelect_DeleteFiles(TreeViewCommandArgs commandArgs)
    {
        foreach (var node in commandArgs.TreeViewContainer.SelectedNodeList)
        {
            var treeViewNamespacePath = (TreeViewNamespacePath)node;

            if (treeViewNamespacePath.Item.IsDirectory)
            {
                await IdeService.TextEditorService.CommonService.FileSystemProvider.Directory
                    .DeleteAsync(treeViewNamespacePath.Item.Value, true, CancellationToken.None)
                    .ConfigureAwait(false);
            }
            else
            {
                await IdeService.TextEditorService.CommonService.FileSystemProvider.File
                    .DeleteAsync(treeViewNamespacePath.Item.Value)
                    .ConfigureAwait(false);
            }

            if (IdeService.TextEditorService.CommonService.TryGetTreeViewContainer(commandArgs.TreeViewContainer.Key, out var mostRecentContainer) &&
                mostRecentContainer is not null)
            {
                var localParent = node.Parent;

                if (localParent is not null)
                {
                    await localParent.LoadChildListAsync().ConfigureAwait(false);
                    IdeService.TextEditorService.CommonService.TreeView_ReRenderNodeAction(mostRecentContainer.Key, localParent);
                }
            }
        }
    }

    public Task OpenNewDotNetSolutionDialog()
    {
        var dialogRecord = new DialogViewModel(
            _newDotNetSolutionDialogKey,
            "New .NET Solution",
            typeof(DotNetSolutionFormDisplay),
            null,
            null,
            true,
            null);

        IdeService.TextEditorService.CommonService.Dialog_ReduceRegisterAction(dialogRecord);
        return Task.CompletedTask;
    }

    public async ValueTask Do_SubmitNuGetQuery(INugetPackageManagerQuery query)
    {
        var localNugetResult = await QueryForNugetPackagesAsync(query)
            .ConfigureAwait(false);

        ReduceSetMostRecentQueryResultAction(localNugetResult);
    }

    public ValueTask Do_RunTestByFullyQualifiedName(TreeViewStringFragment treeViewStringFragment, string fullyQualifiedName, TreeViewProjectTestModel treeViewProjectTestModel)
    {
        var parentDirectory = treeViewProjectTestModel.Item.AbsolutePath.CreateSubstringParentDirectory();
        if (parentDirectory is null)
            return ValueTask.CompletedTask;
    
        RunTestByFullyQualifiedName(
            treeViewStringFragment,
            fullyQualifiedName,
            parentDirectory);

        return ValueTask.CompletedTask;
    }

    private void RunTestByFullyQualifiedName(
        TreeViewStringFragment treeViewStringFragment,
        string fullyQualifiedName,
        string? directoryNameForTestDiscovery)
    {
        var dotNetTestByFullyQualifiedNameFormattedCommand = DotNetCliCommandFormatter
            .FormatDotNetTestByFullyQualifiedName(fullyQualifiedName);

        if (string.IsNullOrWhiteSpace(directoryNameForTestDiscovery) ||
            string.IsNullOrWhiteSpace(fullyQualifiedName))
        {
            return;
        }

        var terminalCommandRequest = new TerminalCommandRequest(
            dotNetTestByFullyQualifiedNameFormattedCommand.Value,
            directoryNameForTestDiscovery,
            treeViewStringFragment.Item.DotNetTestByFullyQualifiedNameFormattedTerminalCommandRequestKey)
        {
            BeginWithFunc = parsedCommand =>
            {
                treeViewStringFragment.Item.TerminalCommandParsed = parsedCommand;
                IdeService.TextEditorService.CommonService.TreeView_ReRenderNodeAction(TestExplorerState.TreeViewTestExplorerKey, treeViewStringFragment);
                return Task.CompletedTask;
            },
            ContinueWithFunc = parsedCommand =>
            {
                treeViewStringFragment.Item.TerminalCommandParsed = parsedCommand;
                var output = treeViewStringFragment.Item.TerminalCommandParsed?.OutputCache.ToString() ?? null;

                if (output is not null && output.Contains("Duration:"))
                {
                    if (output.Contains("Passed!"))
                    {
                        ReduceWithAction(inState =>
                        {
                            var passedTestHashSet = new HashSet<string>(inState.PassedTestHashSet);
                            passedTestHashSet.Add(fullyQualifiedName);

                            var notRanTestHashSet = new HashSet<string>(inState.NotRanTestHashSet);
                            notRanTestHashSet.Remove(fullyQualifiedName);

                            var failedTestHashSet = new HashSet<string>(inState.FailedTestHashSet);
                            failedTestHashSet.Remove(fullyQualifiedName);

                            return inState with
                            {
                                PassedTestHashSet = passedTestHashSet,
                                NotRanTestHashSet = notRanTestHashSet,
                                FailedTestHashSet = failedTestHashSet,
                            };
                        });
                    }
                    else
                    {
                        ReduceWithAction(inState =>
                        {
                            var failedTestHashSet = new HashSet<string>(inState.FailedTestHashSet);
                            failedTestHashSet.Add(fullyQualifiedName);

                            var notRanTestHashSet = new HashSet<string>(inState.NotRanTestHashSet);
                            notRanTestHashSet.Remove(fullyQualifiedName);

                            var passedTestHashSet = new HashSet<string>(inState.PassedTestHashSet);
                            passedTestHashSet.Remove(fullyQualifiedName);

                            return inState with
                            {
                                FailedTestHashSet = failedTestHashSet,
                                NotRanTestHashSet = notRanTestHashSet,
                                PassedTestHashSet = passedTestHashSet,
                            };
                        });
                    }
                }

                IdeService.TextEditorService.CommonService.TreeView_ReRenderNodeAction(TestExplorerState.TreeViewTestExplorerKey, treeViewStringFragment);
                return Task.CompletedTask;
            }
        };

        treeViewStringFragment.Item.TerminalCommandRequest = terminalCommandRequest;
        IdeService.GetTerminalState().TerminalMap[IdeFacts.EXECUTION_KEY].EnqueueCommand(terminalCommandRequest);
    }

    #region DotNetSolutionIdeApi
    private async ValueTask Do_SetDotNetSolution(AbsolutePath inSolutionAbsolutePath)
    {
        var dotNetSolutionAbsolutePathString = inSolutionAbsolutePath.Value;

        var content = IdeService.TextEditorService.CommonService.FileSystemProvider.File.ReadAllText(dotNetSolutionAbsolutePathString);

        var tokenBuilder = new StringBuilder();
        var formattedBuilder = new StringBuilder();

        var solutionAbsolutePath = IdeService.TextEditorService.CommonService.EnvironmentProvider.AbsolutePathFactory(
            dotNetSolutionAbsolutePathString,
            false,
            tokenBuilder,
            formattedBuilder,
            AbsolutePathNameKind.NameWithExtension);

        var resourceUri = new ResourceUri(solutionAbsolutePath.Value);

        if (IdeService.TextEditorService.Model_GetOrDefault(resourceUri) is null)
        {
            IdeService.TextEditorService.WorkerArbitrary.PostUnique(editContext =>
            {
                var extension = ExtensionNoPeriodFacts.DOT_NET_SOLUTION;

                if (dotNetSolutionAbsolutePathString.EndsWith(ExtensionNoPeriodFacts.DOT_NET_SOLUTION_X))
                    extension = ExtensionNoPeriodFacts.DOT_NET_SOLUTION_X;

                IdeService.TextEditorService.Model_RegisterTemplated(
                    editContext,
                    extension,
                    resourceUri,
                    DateTime.UtcNow,
                    content);

                IdeService.TextEditorService
                    .GetCompilerService(extension)
                    .RegisterResource(
                        resourceUri,
                        shouldTriggerResourceWasModified: true);

                return ValueTask.CompletedTask;
            });
        }

        DotNetSolutionModel dotNetSolutionModel;

        if (dotNetSolutionAbsolutePathString.EndsWith(ExtensionNoPeriodFacts.DOT_NET_SOLUTION_X))
            dotNetSolutionModel = ParseSlnx(solutionAbsolutePath, resourceUri, content, tokenBuilder, formattedBuilder);
        else
            dotNetSolutionModel = ParseSln(solutionAbsolutePath, resourceUri, content);

        dotNetSolutionModel.DotNetProjectList = SortProjectReferences(dotNetSolutionModel, tokenBuilder, formattedBuilder);

        /*    
        // FindAllReferences
        var pathGroupList = new List<(string Name, string Path)>();
        foreach (var project in sortedByProjectReferenceDependenciesDotNetProjectList)
        {
            if (project.AbsolutePath.ParentDirectory is not null)
            {
                pathGroupList.Add((project.DisplayName, project.AbsolutePath.ParentDirectory));
            }
        }
        _findAllReferencesService.PathGroupList = pathGroupList;
        */

        // TODO: If somehow model was registered already this won't write the state
        ReduceRegisterAction(dotNetSolutionModel);

        ReduceWithAction(new WithAction(
            inDotNetSolutionState => inDotNetSolutionState with
            {
                DotNetSolutionModelKey = dotNetSolutionModel.Key
            }));

        // TODO: Putting a hack for now to overwrite if somehow model was registered already
        ReduceWithAction(ConstructModelReplacement(
            dotNetSolutionModel.Key,
            dotNetSolutionModel));

        var dotNetSolutionCompilerService = (DotNetSolutionCompilerService)IdeService.TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.DOT_NET_SOLUTION);

        dotNetSolutionCompilerService.ResourceWasModified(
            new ResourceUri(solutionAbsolutePath.Value),
            Array.Empty<TextEditorTextSpan>());

        var parentDirectory = solutionAbsolutePath.CreateSubstringParentDirectory();
        if (parentDirectory is not null)
        {
            IdeService.TextEditorService.CommonService.EnvironmentProvider.DeletionPermittedRegister(new(parentDirectory, true), tokenBuilder, formattedBuilder);

            IdeService.TextEditorService.SetStartingDirectoryPath(parentDirectory);

            IdeService.CodeSearch_With(inState => inState with
            {
                StartingAbsolutePathForSearch = parentDirectory
            });

            TerminalCommandRequest terminalCommandRequest;

            var slnFoundString = $"sln found: {solutionAbsolutePath.Value}";
            var prefix = TerminalInteractive.RESERVED_TARGET_FILENAME_PREFIX + nameof(DotNetService);

            // Set 'generalTerminal' working directory
            terminalCommandRequest = new TerminalCommandRequest(
                prefix + "_General",
                parentDirectory)
            {
                BeginWithFunc = parsedCommand =>
                {
                    IdeService.GetTerminalState().TerminalMap[IdeFacts.GENERAL_KEY].TerminalOutput.WriteOutput(
                        parsedCommand,
                        // If newlines are added to this make sure to use '.ReplaceLineEndings("\n")' because the syntax highlighting and text editor are expecting this line ending.
                        new StandardOutputCommandEvent(slnFoundString));
                    return Task.CompletedTask;
                }
            };
            IdeService.GetTerminalState().TerminalMap[IdeFacts.GENERAL_KEY].EnqueueCommand(terminalCommandRequest);

            // Set 'executionTerminal' working directory
            terminalCommandRequest = new TerminalCommandRequest(
                prefix + "_Execution",
                parentDirectory)
            {
                BeginWithFunc = parsedCommand =>
                {
                    IdeService.GetTerminalState().TerminalMap[IdeFacts.EXECUTION_KEY].TerminalOutput.WriteOutput(
                        parsedCommand,
                        // If newlines are added to this make sure to use '.ReplaceLineEndings("\n")' because the syntax highlighting and text editor are expecting this line ending.
                        new StandardOutputCommandEvent(slnFoundString));
                    return Task.CompletedTask;
                }
            };
            IdeService.GetTerminalState().TerminalMap[IdeFacts.EXECUTION_KEY].EnqueueCommand(terminalCommandRequest);
        }

        try
        {
            await AppDataService.WriteAppDataAsync(new DotNetAppData
            {
                SolutionMostRecent = solutionAbsolutePath.Value
            });
        }
        catch (Exception e)
        {
            NotificationHelper.DispatchError(
                $"ERROR: nameof(_appDataService.WriteAppDataAsync)",
                e.ToString(),
                IdeService.TextEditorService.CommonService,
                TimeSpan.FromSeconds(5));
        }

        IdeService.TextEditorService.WorkerArbitrary.EnqueueUniqueTextEditorWork(
            new UniqueTextEditorWork(IdeService.TextEditorService, editContext =>
            {
                var compilerService = IdeService.TextEditorService.GetCompilerService(ExtensionNoPeriodFacts.C_SHARP_CLASS);
                var cSharpCompilerService = compilerService as CSharpCompilerService;

                try
                {
                    if (cSharpCompilerService is not null)
                    {
                        cSharpCompilerService.__CSharpBinder.ClearAllCompilationUnits();
                    }

                    ParseSolution(editContext, dotNetSolutionModel.Key, CompilationUnitKind.SolutionWide_DefinitionsOnly, tokenBuilder, formattedBuilder);



                    // Get the indices that the first 'ParseSolution' resides at.
                    // Then after the second 'ParseSolution' clear the first 'ParseSolution'.
                    // Then iterate over every compilationUnit and modify their indices by the amount removed.
                    //

                    int countDiagnosticList;
                    int countSymbolList;
                    int countFunctionInvocationParameterMetadataList;
                    int countCodeBlockOwnerList;
                    int countNodeList;

                    if (cSharpCompilerService is not null)
                    {
                        countDiagnosticList = cSharpCompilerService.__CSharpBinder.DiagnosticList.Count;
                        countSymbolList = cSharpCompilerService.__CSharpBinder.SymbolList.Count;
                        countFunctionInvocationParameterMetadataList = cSharpCompilerService.__CSharpBinder.FunctionInvocationParameterMetadataList.Count;
                        countCodeBlockOwnerList = cSharpCompilerService.__CSharpBinder.CodeBlockOwnerList.Count;
                        countNodeList = cSharpCompilerService.__CSharpBinder.NodeList.Count;
                    }
                    else
                    {
                        countDiagnosticList = 0;
                        countSymbolList = 0;
                        countFunctionInvocationParameterMetadataList = 0;
                        countCodeBlockOwnerList = 0;
                        countNodeList = 0;
                    }

                    ParseSolution(editContext, dotNetSolutionModel.Key, CompilationUnitKind.SolutionWide_MinimumLocalsData, tokenBuilder, formattedBuilder);

                    cSharpCompilerService.__CSharpBinder.DiagnosticList.RemoveRange(0, countDiagnosticList);
                    cSharpCompilerService.__CSharpBinder.SymbolList.RemoveRange(0, countSymbolList);
                    cSharpCompilerService.__CSharpBinder.FunctionInvocationParameterMetadataList.RemoveRange(0, countFunctionInvocationParameterMetadataList);
                    cSharpCompilerService.__CSharpBinder.CodeBlockOwnerList.RemoveRange(0, countCodeBlockOwnerList);
                    cSharpCompilerService.__CSharpBinder.NodeList.RemoveRange(0, countNodeList);

                    foreach (var compilationUnitKvp in cSharpCompilerService.__CSharpBinder.__CompilationUnitMap)
                    {
                        var compilationUnit = compilationUnitKvp.Value;

                        compilationUnit.IndexDiagnosticList -= countDiagnosticList;
                        compilationUnit.IndexSymbolList -= countSymbolList;
                        compilationUnit.IndexFunctionInvocationParameterMetadataList -= countFunctionInvocationParameterMetadataList;
                        compilationUnit.IndexCodeBlockOwnerList -= countCodeBlockOwnerList;
                        compilationUnit.IndexNodeList -= countNodeList;

                        cSharpCompilerService.__CSharpBinder.__CompilationUnitMap[compilationUnitKvp.Key] = compilationUnit;
                    }

                    IdeService.TextEditorService.EditContext_GetText_Clear();

                    
                }
                finally
                {
                    if (cSharpCompilerService is not null)
                    {
                        cSharpCompilerService.FastParseTuple = (null, null);

                        cSharpCompilerService.ClearStreamReaderTupleCache();
                    }
                }
                return ValueTask.CompletedTask;
            }));

        await Do_SetDotNetSolutionTreeView(dotNetSolutionModel.Key).ConfigureAwait(false);
    }

    public DotNetSolutionModel ParseSlnx(
        AbsolutePath solutionAbsolutePath,
        ResourceUri resourceUri,
        string content,
        StringBuilder tokenBuilder,
        StringBuilder formattedBuilder)
    {
        // (2025-08-16) breaking
        /*
        var htmlSyntaxUnit = HtmlSyntaxTree.ParseText(
            IdeService.TextEditorService,
            IdeService.TextEditorService.__StringWalker,
            new(solutionAbsolutePath.Value),
            content);

        var syntaxNodeRoot = htmlSyntaxUnit.RootTagSyntax;

        var cSharpProjectSyntaxWalker = new CSharpProjectSyntaxWalker();

        cSharpProjectSyntaxWalker.Visit(syntaxNodeRoot);

        var dotNetProjectList = new List<IDotNetProject>();
        var solutionFolderList = new List<SolutionFolder>();

        var folderTagList = cSharpProjectSyntaxWalker.TagNodes
            .Where(ts => (ts.OpenTagNameNode?.TextEditorTextSpan.GetText(content, IdeService.TextEditorService) ?? string.Empty) == "Folder")
            .ToList();

        var projectTagList = cSharpProjectSyntaxWalker.TagNodes
            .Where(ts => (ts.OpenTagNameNode?.TextEditorTextSpan.GetText(content, IdeService.TextEditorService) ?? string.Empty) == "Project")
            .ToList();

        var solutionFolderPathHashSet = new HashSet<string>();

        var stringNestedProjectEntryList = new List<StringNestedProjectEntry>();

        foreach (var folder in folderTagList)
        {
            var attributeNameValueTuples = folder
                .AttributeNodes
                .Select(x => (
                    x.AttributeNameSyntax.TextEditorTextSpan
                        .GetText(content, IdeService.TextEditorService)
                        .Trim(),
                    x.AttributeValueSyntax.TextEditorTextSpan
                        .GetText(content, IdeService.TextEditorService)
                        .Replace("\"", string.Empty)
                        .Replace("=", string.Empty)
                        .Trim()))
                .ToArray();

            var attribute = attributeNameValueTuples.FirstOrDefault(x => x.Item1 == "Name");
            if (attribute.Item2 is null)
                continue;

            var ancestorDirectoryList = new List<string>();

            var absolutePath = new AbsolutePath(
                attribute.Item2,
                isDirectory: true,
                IdeService.TextEditorService.CommonService.EnvironmentProvider,
                tokenBuilder,
                formattedBuilder,
                AbsolutePathNameKind.NameWithExtension,
                ancestorDirectoryList: ancestorDirectoryList);

            solutionFolderPathHashSet.Add(absolutePath.Value);

            for (int i = 0; i < ancestorDirectoryList.Count; i++)
            {
                if (i == 0)
                    continue;

                solutionFolderPathHashSet.Add(ancestorDirectoryList[i]);
            }

            foreach (var child in folder.ChildContent)
            {
                if (child.HtmlSyntaxKind == HtmlSyntaxKind.TagSelfClosingNode ||
                    child.HtmlSyntaxKind == HtmlSyntaxKind.TagClosingNode)
                {
                    var tagNode = (TagNode)child;

                    attributeNameValueTuples = tagNode
                        .AttributeNodes
                        .Select(x => (
                            x.AttributeNameSyntax.TextEditorTextSpan
                                .GetText(content, IdeService.TextEditorService)
                                .Trim(),
                            x.AttributeValueSyntax.TextEditorTextSpan
                                .GetText(content, IdeService.TextEditorService)
                                .Replace("\"", string.Empty)
                                .Replace("=", string.Empty)
                                .Trim()))
                        .ToArray();

                    attribute = attributeNameValueTuples.FirstOrDefault(x => x.Item1 == "Path");
                    if (attribute.Item2 is null)
                        continue;

                    stringNestedProjectEntryList.Add(new StringNestedProjectEntry(
                        ChildIsSolutionFolder: false,
                        attribute.Item2,
                        absolutePath.Value));
                }
            }
        }

        // I'm too tired to decide if enumerating a HashSet is safe
        var temporarySolutionFolderList = solutionFolderPathHashSet.ToList();

        foreach (var solutionFolderPath in temporarySolutionFolderList)
        {
            var absolutePath = new AbsolutePath(
                solutionFolderPath,
                isDirectory: true,
                IdeService.TextEditorService.CommonService.EnvironmentProvider,
                tokenBuilder,
                formattedBuilder,
                AbsolutePathNameKind.NameNoExtension);

            solutionFolderList.Add(new SolutionFolder(
                absolutePath.Name,
                solutionFolderPath));
        }

        foreach (var project in projectTagList)
        {
            var attributeNameValueTuples = project
                .AttributeNodes
                .Select(x => (
                    x.AttributeNameSyntax.TextEditorTextSpan
                        .GetText(content, IdeService.TextEditorService)
                        .Trim(),
                    x.AttributeValueSyntax.TextEditorTextSpan
                        .GetText(content, IdeService.TextEditorService)
                        .Replace("\"", string.Empty)
                        .Replace("=", string.Empty)
                        .Trim()))
                .ToArray();

            var attribute = attributeNameValueTuples.FirstOrDefault(x => x.Item1 == "Path");
            if (attribute.Item2 is null)
                continue;

            var relativePath = new RelativePath(attribute.Item2, isDirectory: false, IdeService.TextEditorService.CommonService.EnvironmentProvider);

            dotNetProjectList.Add(new CSharpProjectModel(
                relativePath.NameNoExtension,
                Guid.Empty,
                attribute.Item2,
                Guid.Empty,
                new(),
                new(),
                default(AbsolutePath)));
        }

        var dotNetSolutionHeader = new DotNetSolutionHeader();
        var dotNetSolutionGlobal = new DotNetSolutionGlobal();

        // You have to iterate in reverse so ascending will put longest words to shortest (when iterating reverse).
        var childSolutionFolderList = solutionFolderList.OrderBy(x => x.ActualName).ToList();
        var parentSolutionFolderList = new List<SolutionFolder>(childSolutionFolderList);

        for (int parentIndex = parentSolutionFolderList.Count - 1; parentIndex >= 0; parentIndex--)
        {
            var parentSolutionFolder = parentSolutionFolderList[parentIndex];

            for (int childIndex = childSolutionFolderList.Count - 1; childIndex >= 0; childIndex--)
            {
                var childSolutionFolder = childSolutionFolderList[childIndex];

                if (childSolutionFolder.ActualName != parentSolutionFolder.ActualName &&
                    childSolutionFolder.ActualName.StartsWith(parentSolutionFolder.ActualName))
                {
                    stringNestedProjectEntryList.Add(new StringNestedProjectEntry(
                        ChildIsSolutionFolder: true,
                        childSolutionFolder.ActualName,
                        parentSolutionFolder.ActualName));

                    childSolutionFolderList.RemoveAt(childIndex);
                }
            }
        }

        return new DotNetSolutionModel(
            solutionAbsolutePath,
            dotNetSolutionHeader,
            dotNetProjectList,
            solutionFolderList,
            guidNestedProjectEntryList: null,
            stringNestedProjectEntryList,
            dotNetSolutionGlobal,
            content);
        */
        throw new NotImplementedException();
    }

    public DotNetSolutionModel ParseSln(
        AbsolutePath solutionAbsolutePath,
        ResourceUri resourceUri,
        string content)
    {
        var lexer = new DotNetSolutionLexer(
            new StringWalker(),
            resourceUri,
            content);

        lexer.Lex();

        var parser = new DotNetSolutionParser(lexer);

        var compilationUnit = parser.Parse();

        return new DotNetSolutionModel(
            solutionAbsolutePath,
            parser.DotNetSolutionHeader,
            parser.DotNetProjectList,
            parser.SolutionFolderList,
            guidNestedProjectEntryList: parser.NestedProjectEntryList,
            null,
            parser.DotNetSolutionGlobal,
            content);
    }

    /// <summary>
    /// This solution is incomplete, the current code for this was just to get a "feel" for things.
    /// </summary>
    private List<IDotNetProject> SortProjectReferences(DotNetSolutionModel dotNetSolutionModel, StringBuilder tokenBuilder, StringBuilder formattedBuilder)
    {
        var moveUpDirectoryToken = $"..{IdeService.TextEditorService.CommonService.EnvironmentProvider.DirectorySeparatorChar}";
        // "./" is being called the 'sameDirectoryToken'
        var sameDirectoryToken = $".{IdeService.TextEditorService.CommonService.EnvironmentProvider.DirectorySeparatorChar}";
        
        var dotNetSolutionAncestorDirectoryList = dotNetSolutionModel.AbsolutePath.GetAncestorDirectoryList(
            IdeService.TextEditorService.CommonService.EnvironmentProvider,
            tokenBuilder,
            formattedBuilder,
            AbsolutePathNameKind.NameWithExtension);
    
        for (int i = dotNetSolutionModel.DotNetProjectList.Count - 1; i >= 0; i--)
        {
            var projectTuple = dotNetSolutionModel.DotNetProjectList[i];

            // Debugging Linux-Ubuntu (2024-04-28)
            // -----------------------------------
            // It is believed, that Linux-Ubuntu is not fully working correctly,
            // due to the directory separator character at the os level being '/',
            // meanwhile the .NET solution has as its directory separator character '\'.
            //
            // Will perform a string.Replace("\\", "/") here. And if it solves the issue,
            // then some standard way of doing this needs to be made available in the IEnvironmentProvider.
            //
            // Okay, this single replacement fixes 99% of the solution explorer issue.
            // And I say 99% instead of 100% just because I haven't tested every single part of it yet.
            var relativePathFromSolutionFileString = projectTuple.RelativePathFromSolutionFileString;
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                relativePathFromSolutionFileString = relativePathFromSolutionFileString.Replace("\\", "/");
            var absolutePathString = PathHelper.GetAbsoluteFromAbsoluteAndRelative(
                dotNetSolutionModel.AbsolutePath,
                relativePathFromSolutionFileString,
                IdeService.TextEditorService.CommonService.EnvironmentProvider,
                tokenBuilder,
                formattedBuilder,
                moveUpDirectoryToken: moveUpDirectoryToken,
                sameDirectoryToken: sameDirectoryToken,
                dotNetSolutionAncestorDirectoryList);
            projectTuple.AbsolutePath = IdeService.TextEditorService.CommonService.EnvironmentProvider.AbsolutePathFactory(absolutePathString, false, tokenBuilder, formattedBuilder, AbsolutePathNameKind.NameWithExtension);

            if (!IdeService.TextEditorService.CommonService.FileSystemProvider.File.Exists(projectTuple.AbsolutePath.Value))
            {
                dotNetSolutionModel.DotNetProjectList.RemoveAt(i);
                continue;
            }

            projectTuple.ReferencedAbsolutePathList = new List<AbsolutePath>();

            var innerParentDirectory = projectTuple.AbsolutePath.CreateSubstringParentDirectory();
            if (innerParentDirectory is not null)
            {
                IdeService.TextEditorService.CommonService.EnvironmentProvider.DeletionPermittedRegister(new(innerParentDirectory, true), tokenBuilder, formattedBuilder);
            }

            using StreamReader sr = new StreamReader(projectTuple.AbsolutePath.Value);
            var lexerOutput = XmlLexer.Lex(new StreamReaderWrap(sr));
            
            var stringBuilder = new StringBuilder();
            var getTextBuffer = new char[1];
            
            List<string> relativePathReferenceList = new();
            
            for (int indexTextSpan = 0; indexTextSpan < lexerOutput.TextSpanList.Count; indexTextSpan++)
            {
                var textSpan = lexerOutput.TextSpanList[indexTextSpan];
                var decorationKind = (XmlDecorationKind)textSpan.DecorationByte;
                
                if (decorationKind == XmlDecorationKind.TagNameOpen)
                {
                    sr.BaseStream.Seek(textSpan.ByteIndex, SeekOrigin.Begin);
                    sr.DiscardBufferedData();
                    stringBuilder.Clear();
                    for (int bufferRead = 0; bufferRead < textSpan.Length; bufferRead++)
                    {
                        sr.Read(getTextBuffer, 0, 1);
                        stringBuilder.Append(getTextBuffer[0]);
                    }
                    var tagNameOpenString = stringBuilder.ToString();
                
                    if (tagNameOpenString == "ProjectReference")
                    {
                        var includeValue = string.Empty;
                    
                        while (indexTextSpan < lexerOutput.TextSpanList.Count - 1)
                        {
                            if ((XmlDecorationKind)lexerOutput.TextSpanList[indexTextSpan + 1].DecorationByte == XmlDecorationKind.AttributeName)
                            {
                                var attributeNameTextSpan = lexerOutput.TextSpanList[indexTextSpan + 1];
                                ++indexTextSpan;
                                
                                sr.BaseStream.Seek(attributeNameTextSpan.ByteIndex, SeekOrigin.Begin);
                                sr.DiscardBufferedData();
                                stringBuilder.Clear();
                                for (int bufferRead = 0; bufferRead < attributeNameTextSpan.Length; bufferRead++)
                                {
                                    sr.Read(getTextBuffer, 0, 1);
                                    stringBuilder.Append(getTextBuffer[0]);
                                }
                                var attributeNameString = stringBuilder.ToString();
                                
                                while (indexTextSpan < lexerOutput.TextSpanList.Count - 1)
                                {
                                    var nextDecorationKind = (XmlDecorationKind)lexerOutput.TextSpanList[indexTextSpan + 1].DecorationByte;
                                    
                                    if (nextDecorationKind == XmlDecorationKind.AttributeOperator)
                                    {
                                        ++indexTextSpan;
                                    }
                                    else if (nextDecorationKind == XmlDecorationKind.AttributeDelimiter)
                                    {
                                        ++indexTextSpan;
                                    }
                                    else if (nextDecorationKind == XmlDecorationKind.AttributeValue)
                                    {
                                        var attributeValueTextSpan = lexerOutput.TextSpanList[indexTextSpan + 1];
                                        
                                        sr.BaseStream.Seek(attributeValueTextSpan.ByteIndex, SeekOrigin.Begin);
                                        sr.DiscardBufferedData();
                                        stringBuilder.Clear();
                                        for (int bufferRead = 0; bufferRead < attributeValueTextSpan.Length; bufferRead++)
                                        {
                                            sr.Read(getTextBuffer, 0, 1);
                                            stringBuilder.Append(getTextBuffer[0]);
                                        }
                                        
                                        if (attributeNameString == "Include")
                                        {
                                            if (includeValue == string.Empty)
                                                includeValue = stringBuilder.ToString();
                                        }
                                        
                                        ++indexTextSpan;
                                        break;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if ((XmlDecorationKind)lexerOutput.TextSpanList[indexTextSpan + 1].DecorationByte == XmlDecorationKind.AttributeDelimiter)
                                {
                                    ++indexTextSpan;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        
                        if (includeValue != string.Empty)
                            relativePathReferenceList.Add(includeValue);
                    }
                }
            }

            var projectAncestorDirectoryList = projectTuple.AbsolutePath.GetAncestorDirectoryList(
                IdeService.TextEditorService.CommonService.EnvironmentProvider,
                tokenBuilder,
                formattedBuilder,
                AbsolutePathNameKind.NameWithExtension);
            
            foreach (var projectReference in relativePathReferenceList)
            {
                var referenceProjectAbsolutePathString = PathHelper.GetAbsoluteFromAbsoluteAndRelative(
                    projectTuple.AbsolutePath,
                    projectReference,
                    IdeService.TextEditorService.CommonService.EnvironmentProvider,
                    tokenBuilder,
                    formattedBuilder,
                    moveUpDirectoryToken: moveUpDirectoryToken,
                    sameDirectoryToken: sameDirectoryToken,
                    projectAncestorDirectoryList);

                var referenceProjectAbsolutePath = IdeService.TextEditorService.CommonService.EnvironmentProvider.AbsolutePathFactory(
                    referenceProjectAbsolutePathString,
                    false,
                    tokenBuilder,
                    formattedBuilder,
                    AbsolutePathNameKind.NameWithExtension);

                projectTuple.ReferencedAbsolutePathList.Add(referenceProjectAbsolutePath);
            }
        }

        var upperLimit = dotNetSolutionModel.DotNetProjectList.Count > 4 // Extremely arbitrary number being used here.
            ? 4
            : dotNetSolutionModel.DotNetProjectList.Count;
        for (int outerIndex = 0; outerIndex < upperLimit; outerIndex++)
        {
            for (int i = 0; i < dotNetSolutionModel.DotNetProjectList.Count; i++)
            {
                var projectTuple = dotNetSolutionModel.DotNetProjectList[i];

                foreach (var referenceAbsolutePath in projectTuple.ReferencedAbsolutePathList)
                {
                    var referenceIndex = dotNetSolutionModel.DotNetProjectList
                        .FindIndex(x => x.AbsolutePath.Value == referenceAbsolutePath.Value);

                    if (referenceIndex > i)
                    {
                        var indexDestination = i - 1;
                        if (indexDestination == -1)
                            indexDestination = 0;

                        MoveAndShiftList(
                            dotNetSolutionModel.DotNetProjectList,
                            indexSource: referenceIndex,
                            indexDestination);
                    }
                }
            }
        }

        return dotNetSolutionModel.DotNetProjectList;
    }

    private void MoveAndShiftList(
        List<IDotNetProject> enumeratingProjectTupleList,
        int indexSource,
        int indexDestination)
    {
        if (indexSource == 1 && indexDestination == 0)
        {
            var otherTemporary = enumeratingProjectTupleList[indexDestination];
            enumeratingProjectTupleList[indexDestination] = enumeratingProjectTupleList[indexSource];
            enumeratingProjectTupleList[indexSource] = otherTemporary;
            return;
        }

        var temporary = enumeratingProjectTupleList[indexDestination];
        enumeratingProjectTupleList[indexDestination] = enumeratingProjectTupleList[indexSource];

        for (int i = indexSource; i > indexDestination; i--)
        {
            if (i - 1 == indexDestination)
                enumeratingProjectTupleList[i] = temporary;
            else
                enumeratingProjectTupleList[i] = enumeratingProjectTupleList[i - 1];
        }
    }

    private void ParseSolution(
        TextEditorEditContext editContext,
        Key<DotNetSolutionModel> dotNetSolutionModelKey,
        CompilationUnitKind compilationUnitKind,
        StringBuilder? tokenBuilder,
        StringBuilder? formattedBuilder)
    {
        var dotNetSolutionState = GetDotNetSolutionState();

        var dotNetSolutionModel = dotNetSolutionState.DotNetSolutionsList.FirstOrDefault(
            x => x.Key == dotNetSolutionModelKey);

        if (dotNetSolutionModel is null)
            return;

        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var progressBarModel = new ProgressBarModel(0, "parsing...")
        {
            OnCancelFunc = () =>
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                return Task.CompletedTask;
            }
        };

        NotificationHelper.DispatchProgress(
            $"Parse: {dotNetSolutionModel.AbsolutePath.Name}",
            progressBarModel,
            IdeService.TextEditorService.CommonService,
            TimeSpan.FromMilliseconds(-1));

        IStartupControlModel? originallyActiveStartupControl = null;
        
        try
        {
            var localStartupControlState = IdeService.GetIdeStartupControlState();
        	originallyActiveStartupControl = localStartupControlState.StartupControlList.FirstOrDefault(
        	    x => x.StartupProjectAbsolutePath.Value == localStartupControlState.ActiveStartupProjectAbsolutePathValue);
            IdeService.Ide_ClearAllStartupControls();
        
            foreach (var project in dotNetSolutionModel.DotNetProjectList)
            {
                RegisterStartupControl(project);

                var resourceUri = new ResourceUri(project.AbsolutePath.Value);

                if (!IdeService.TextEditorService.CommonService.FileSystemProvider.File.Exists(resourceUri.Value))
                    continue; // TODO: This can still cause a race condition exception if the file is removed before the next line runs.
            }

            var previousStageProgress = 0.05;
            var dotNetProjectListLength = dotNetSolutionModel.DotNetProjectList.Count;
            var projectsParsedCount = 0;
            foreach (var project in dotNetSolutionModel.DotNetProjectList)
            {
                // foreach project in solution
                //     foreach C# file in project
                //         EnqueueBackgroundTask(async () =>
                //         {
                //             ParseCSharpFile();
                //             UpdateProgressBar();
                //         });
                //
                // Treat every project as an equal weighting with relation to remaining percent to complete
                // on the progress bar.
                //
                // If the project were to be parsed, how much would it move the percent progress completed by?
                //
                // Then, in order to see progress while each C# file in the project gets parsed,
                // multiply the percent progress this project can provide by the proportion
                // of the project's C# files which have been parsed.
                var maximumProgressAvailableToProject = (1 - previousStageProgress) * ((double)1.0 / dotNetProjectListLength);
                var currentProgress = Math.Min(1.0, previousStageProgress + maximumProgressAvailableToProject * projectsParsedCount);

                // This 'SetProgress' is being kept out the throttle, since it sets message 1
                // whereas the per class progress updates set message 2.
                //
                // Otherwise an update to message 2 could result in this message 1 update never being written.
                progressBarModel.SetProgress(
                    currentProgress,
                    project.AbsolutePath.Name);

                cancellationToken.ThrowIfCancellationRequested();

                DiscoverClassesInProject(editContext, project, progressBarModel, currentProgress, maximumProgressAvailableToProject, compilationUnitKind, tokenBuilder, formattedBuilder);
                projectsParsedCount++;
            }

            progressBarModel.SetProgress(1, $"Finished parsing: {dotNetSolutionModel.AbsolutePath.Name}", string.Empty);
            progressBarModel.Dispose();
        }
        catch (Exception e)
        {
            if (e is OperationCanceledException)
                progressBarModel.IsCancelled = true;

            var currentProgress = progressBarModel.GetProgress();

            progressBarModel.SetProgress(currentProgress, e.ToString());
            progressBarModel.Dispose();
        }
        finally
        {
            if (originallyActiveStartupControl is not null)
            {
                var localStartupControlState = IdeService.GetIdeStartupControlState();
                
                foreach (var startupControl in localStartupControlState.StartupControlList)
                {
                    if (startupControl.Title == originallyActiveStartupControl.Title  &&
                        startupControl.StartupProjectAbsolutePath.Value == originallyActiveStartupControl.StartupProjectAbsolutePath.Value)
                    {
                        IdeService.Ide_SetActiveStartupControlKey(startupControl.StartupProjectAbsolutePath.Value);
                    }
                }
            }
        }
    }

    private void DiscoverClassesInProject(
        TextEditorEditContext editContext,
        IDotNetProject dotNetProject,
        ProgressBarModel progressBarModel,
        double currentProgress,
        double maximumProgressAvailableToProject,
        CompilationUnitKind compilationUnitKind,
        StringBuilder tokenBuilder,
        StringBuilder formattedBuilder)
    {
        if (!IdeService.TextEditorService.CommonService.FileSystemProvider.File.Exists(dotNetProject.AbsolutePath.Value))
            return; // TODO: This can still cause a race condition exception if the file is removed before the next line runs.

        var parentDirectory = dotNetProject.AbsolutePath.CreateSubstringParentDirectory();
        if (parentDirectory is null)
            return;

        var startingAbsolutePathForSearch = parentDirectory;
        var discoveredFileList = new List<string>();

        DiscoverFilesRecursively(startingAbsolutePathForSearch, discoveredFileList, true);

        ParseClassesInProject(
            editContext,
            dotNetProject,
            progressBarModel,
            currentProgress,
            maximumProgressAvailableToProject,
            discoveredFileList,
            compilationUnitKind,
            tokenBuilder,
            formattedBuilder);

        void DiscoverFilesRecursively(string directoryPathParent, List<string> discoveredFileList, bool isFirstInvocation)
        {
            var directoryPathChildList = IdeService.TextEditorService.CommonService.FileSystemProvider.Directory.GetDirectories(
                    directoryPathParent);

            var filePathChildList = IdeService.TextEditorService.CommonService.FileSystemProvider.Directory.GetFiles(
                    directoryPathParent);

            foreach (var filePathChild in filePathChildList)
            {
                if (filePathChild.EndsWith(".cs"))
                    discoveredFileList.Add(filePathChild);
            }

            foreach (var directoryPathChild in directoryPathChildList)
            {
                if (IFileSystemProvider.IsDirectoryIgnored(directoryPathChild))
                    continue;

                DiscoverFilesRecursively(directoryPathChild, discoveredFileList, isFirstInvocation: false);
            }
        }
    }

    private void ParseClassesInProject(
        TextEditorEditContext editContext,
        IDotNetProject dotNetProject,
        ProgressBarModel progressBarModel,
        double currentProgress,
        double maximumProgressAvailableToProject,
        List<string> discoveredFileList,
        CompilationUnitKind compilationUnitKind,
        StringBuilder? tokenBuilder,
        StringBuilder? formattedBuilder)
    {
        var fileParsedCount = 0;

        foreach (var file in discoveredFileList)
        {
            var progress = currentProgress + maximumProgressAvailableToProject * (fileParsedCount / (double)discoveredFileList.Count);
            var resourceUri = new ResourceUri(file);
            var compilerService = IdeService.TextEditorService.GetCompilerService("cs");

            compilerService.RegisterResource(
                resourceUri,
                shouldTriggerResourceWasModified: false);

            compilerService.FastParse(editContext, resourceUri, IdeService.TextEditorService.CommonService.FileSystemProvider, compilationUnitKind);
            fileParsedCount++;
        }
    }

    private async ValueTask Do_SetDotNetSolutionTreeView(Key<DotNetSolutionModel> dotNetSolutionModelKey)
    {
        var dotNetSolutionState = GetDotNetSolutionState();

        var dotNetSolutionModel = dotNetSolutionState.DotNetSolutionsList.FirstOrDefault(
            x => x.Key == dotNetSolutionModelKey);

        if (dotNetSolutionModel is null)
            return;

        var rootNode = new TreeViewSolution(
            dotNetSolutionModel,
            IdeService.TextEditorService.CommonService,
            true,
            true);

        await rootNode.LoadChildListAsync().ConfigureAwait(false);

        if (!IdeService.TextEditorService.CommonService.TryGetTreeViewContainer(DotNetSolutionState.TreeViewSolutionExplorerStateKey, out _))
        {
            IdeService.TextEditorService.CommonService.TreeView_RegisterContainerAction(new TreeViewContainer(
                DotNetSolutionState.TreeViewSolutionExplorerStateKey,
                rootNode,
                new List<TreeViewNoType> { rootNode }));
        }
        else
        {
            IdeService.TextEditorService.CommonService.TreeView_WithRootNodeAction(DotNetSolutionState.TreeViewSolutionExplorerStateKey, rootNode);

            IdeService.TextEditorService.CommonService.TreeView_SetActiveNodeAction(
                DotNetSolutionState.TreeViewSolutionExplorerStateKey,
                rootNode,
                true,
                false);
        }

        if (dotNetSolutionModel is null)
            return;

        ReduceWithAction(ConstructModelReplacement(
            dotNetSolutionModel.Key,
            dotNetSolutionModel));
    }

    private void RegisterStartupControl(IDotNetProject project)
    {
        IdeService.Ide_RegisterStartupControl(
            new StartupControlModel(
                project.DisplayName,
                project.AbsolutePath.Value,
                project.AbsolutePath,
                null,
                null,
                startupControlModel => StartButtonOnClick(startupControlModel, project),
                StopButtonOnClick));
    }

    private Task StartButtonOnClick(IStartupControlModel interfaceStartupControlModel, IDotNetProject project)
    {
        var startupControlModel = (StartupControlModel)interfaceStartupControlModel;

        var ancestorDirectory = project.AbsolutePath.CreateSubstringParentDirectory();
        if (ancestorDirectory is null)
        {
            return Task.CompletedTask;
        }

        var formattedCommand = DotNetCliCommandFormatter.FormatStartProjectWithoutDebugging(
            project.AbsolutePath);

        var terminalCommandRequest = new TerminalCommandRequest(
            formattedCommand.Value,
            ancestorDirectory,
            _newDotNetSolutionTerminalCommandRequestKey)
        {
            BeginWithFunc = parsedCommand =>
            {
                ParseOutputEntireDotNetRun(
                    string.Empty,
                    "Run-Project_started");

                return Task.CompletedTask;
            },
            ContinueWithFunc = parsedCommand =>
            {
                startupControlModel.ExecutingTerminalCommandRequest = null;
                IdeService.Ide_TriggerStartupControlStateChanged();

                ParseOutputEntireDotNetRun(
                    parsedCommand.OutputCache.ToString(),
                    "Run-Project_completed");

                return Task.CompletedTask;
            }
        };

        startupControlModel.ExecutingTerminalCommandRequest = terminalCommandRequest;

        IdeService.GetTerminalState().TerminalMap[IdeFacts.EXECUTION_KEY].EnqueueCommand(terminalCommandRequest);
        return Task.CompletedTask;
    }

    private Task StopButtonOnClick(IStartupControlModel interfaceStartupControlModel)
    {
        var startupControlModel = (StartupControlModel)interfaceStartupControlModel;

        IdeService.GetTerminalState().TerminalMap[IdeFacts.EXECUTION_KEY].KillProcess();
        startupControlModel.ExecutingTerminalCommandRequest = null;

        IdeService.Ide_TriggerStartupControlStateChanged();
        return Task.CompletedTask;
    }

    private ValueTask Do_Website_AddExistingProjectToSolution(
        Key<DotNetSolutionModel> dotNetSolutionModelKey,
        string projectTemplateShortName,
        string cSharpProjectName,
        AbsolutePath cSharpProjectAbsolutePath)
    {
        return ValueTask.CompletedTask;
    }

    /// <summary>Don't have the implementation <see cref="WithAction"/> as public scope.</summary>
    public interface IWithAction
    {
    }

    /// <summary>Don't have <see cref="WithAction"/> itself as public scope.</summary>
    public record WithAction(Func<DotNetSolutionState, DotNetSolutionState> WithFunc)
        : IWithAction;

    public static IWithAction ConstructModelReplacement(
            Key<DotNetSolutionModel> dotNetSolutionModelKey,
            DotNetSolutionModel outDotNetSolutionModel)
    {
        return new WithAction(dotNetSolutionState =>
        {
            var indexOfSln = dotNetSolutionState.DotNetSolutionsList.FindIndex(
                sln => sln.Key == dotNetSolutionModelKey);

            if (indexOfSln == -1)
                return dotNetSolutionState;

            var outDotNetSolutions = new List<DotNetSolutionModel>(dotNetSolutionState.DotNetSolutionsList);
            outDotNetSolutions[indexOfSln] = outDotNetSolutionModel;

            return dotNetSolutionState with
            {
                DotNetSolutionsList = outDotNetSolutions
            };
        });
    }
    #endregion
}
