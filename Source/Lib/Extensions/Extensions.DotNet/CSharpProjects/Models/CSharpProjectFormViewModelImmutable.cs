using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.CompilerServices.DotNetSolution.Models;
using Walk.Ide.RazorLib.CommandLines.Models;
using Walk.Ide.RazorLib.Terminals.Models;

namespace Walk.Extensions.DotNet.CSharpProjects.Models;

public record CSharpProjectFormViewModelImmutable(
    DotNetSolutionModel DotNetSolutionModel,
    IEnvironmentProvider EnvironmentProvider,
    bool IsReadingProjectTemplates,
    string ProjectTemplateShortNameValue,
    string CSharpProjectNameValue,
    string OptionalParametersValue,
    string ParentDirectoryNameValue,
    List<ProjectTemplate> ProjectTemplateList,
    CSharpProjectFormPanelKind ActivePanelKind,
    string SearchInput,
    ProjectTemplate? SelectedProjectTemplate,
    bool IsValid,
    string ProjectTemplateShortNameDisplay,
    string CSharpProjectNameDisplay,
    string OptionalParametersDisplay,
    string ParentDirectoryNameDisplay,
    string FormattedNewCSharpProjectCommandValue,
    string FormattedAddExistingProjectToSolutionCommandValue,
    Key<TerminalCommandRequest> NewCSharpProjectTerminalCommandRequestKey,
    Key<TerminalCommandRequest> AddCSharpProjectToSolutionTerminalCommandRequestKey,
    Key<TerminalCommandRequest> LoadProjectTemplatesTerminalCommandRequestKey,
    CancellationTokenSource NewCSharpProjectCancellationTokenSource);
