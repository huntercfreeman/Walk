using Microsoft.AspNetCore.Components;
using Walk.Common.RazorLib.Installations.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.CompilerServices.DotNetSolution.Models;
using Walk.Ide.RazorLib;
using Walk.Ide.RazorLib.Terminals.Models;
using Walk.Ide.RazorLib.BackgroundTasks.Models;
using Walk.Ide.RazorLib.InputFiles.Models;
using Walk.Extensions.DotNet.CSharpProjects.Models;
using Walk.Extensions.DotNet.CommandLines.Models;

namespace Walk.Extensions.DotNet.CSharpProjects.Displays;

public partial class CSharpProjectFormDisplay : ComponentBase, IDisposable
{
    [Inject]
    private DotNetService DotNetService { get; set; } = null!;

    [CascadingParameter]
    public IDialog DialogRecord { get; set; } = null!;

    [Parameter]
    public Key<DotNetSolutionModel> DotNetSolutionModelKey { get; set; }

    private CSharpProjectFormViewModel _viewModel = null!;

    private DotNetSolutionModel? DotNetSolutionModel => DotNetService.GetDotNetSolutionState().DotNetSolutionsList.FirstOrDefault(
        x => x.Key == DotNetSolutionModelKey);

    protected override void OnInitialized()
    {
        _viewModel = new(DotNetSolutionModel, DotNetService.IdeService.TextEditorService.CommonService.EnvironmentProvider);
        
        DotNetService.DotNetSolutionStateChanged += OnDotNetSolutionStateChanged;
        DotNetService.IdeService.IdeStateChanged += OnTerminalStateChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await ReadProjectTemplates().ConfigureAwait(false);
        }
    }

    private string GetIsActiveCssClassString(CSharpProjectFormPanelKind panelKind) =>
        _viewModel.ActivePanelKind == panelKind ? "di_active" : string.Empty;

    private void RequestInputFileForParentDirectory(string message)
    {
        DotNetService.IdeService.Enqueue(new IdeWorkArgs
        {
            WorkKind = IdeWorkKind.RequestInputFileStateForm,
            StringValue = message,
            OnAfterSubmitFunc = async absolutePath =>
            {
                if (absolutePath.ExactInput is null)
                    return;

                _viewModel.ParentDirectoryNameValue = absolutePath.Value;
                await InvokeAsync(StateHasChanged);
            },
            SelectionIsValidFunc = absolutePath =>
            {
                if (absolutePath.ExactInput is null || !absolutePath.IsDirectory)
                    return Task.FromResult(false);

                return Task.FromResult(true);
            },
            InputFilePatterns = new()
            {
                new InputFilePattern("Directory", absolutePath => absolutePath.IsDirectory)
            }
        });
    }

    private async Task ReadProjectTemplates()
    {
        if (DotNetService.IdeService.TextEditorService.CommonService.WalkHostingInformation.WalkHostingKind != WalkHostingKind.Photino)
        {
            _viewModel.ProjectTemplateList = new();
            await InvokeAsync(StateHasChanged);
        }
        else
        {
            await EnqueueDotNetNewListAsync().ConfigureAwait(false);
        }
    }

    private async Task EnqueueDotNetNewListAsync()
    {
        try
        {
            // Render UI loading icon
            _viewModel.IsReadingProjectTemplates = true;
            await InvokeAsync(StateHasChanged);

            var formattedCommand = DotNetCliCommandFormatter.FormatDotnetNewList();
            var generalTerminal = DotNetService.IdeService.GetTerminalState().TerminalMap[IdeFacts.GENERAL_KEY];
                
            var terminalCommandRequest = new TerminalCommandRequest(
                formattedCommand.Value,
                DotNetService.IdeService.CommonService.EnvironmentProvider.HomeDirectoryAbsolutePath.Value,
                new Key<TerminalCommandRequest>(_viewModel.LoadProjectTemplatesTerminalCommandRequestKey.Guid))
            {
                ContinueWithFunc = parsedTerminalCommand =>
                {
                    DotNetService.ParseOutputLineDotNetNewList(parsedTerminalCommand.OutputCache.ToString());
                    _viewModel.ProjectTemplateList = DotNetService.ProjectTemplateList ?? new();
                    return InvokeAsync(StateHasChanged);
                }
            };

            generalTerminal.EnqueueCommand(terminalCommandRequest);
        }
        finally
        {
            // UI loading message
            _viewModel.IsReadingProjectTemplates = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    /// <summary>If the non-deprecated version of the command fails, then try the deprecated one.</summary>
    private async Task EnqueueDotnetNewListDeprecatedAsync()
    {
        try
        {
            // UI loading message
            _viewModel.IsReadingProjectTemplates = true;
            await InvokeAsync(StateHasChanged);

            var formattedCommand = DotNetCliCommandFormatter.FormatDotnetNewListDeprecated();
            var generalTerminal = DotNetService.IdeService.GetTerminalState().TerminalMap[IdeFacts.GENERAL_KEY];

            var terminalCommandRequest = new TerminalCommandRequest(
                formattedCommand.Value,
                DotNetService.IdeService.TextEditorService.CommonService.EnvironmentProvider.HomeDirectoryAbsolutePath.Value,
                new Key<TerminalCommandRequest>(_viewModel.LoadProjectTemplatesTerminalCommandRequestKey.Guid))
            {
                ContinueWithFunc = parsedCommand =>
                {
                    DotNetService.ParseOutputLineDotNetNewList(parsedCommand.OutputCache.ToString());
                    _viewModel.ProjectTemplateList = DotNetService.ProjectTemplateList ?? new();
                    return InvokeAsync(StateHasChanged);
                }
            };
                
            DotNetService.IdeService.GetTerminalState().TerminalMap[IdeFacts.GENERAL_KEY].EnqueueCommand(terminalCommandRequest);
        }
        finally
        {
            // UI loading message
            _viewModel.IsReadingProjectTemplates = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private string GetCssClassForActivePanelKind(CSharpProjectFormPanelKind localActivePanelKind)
    {
        return localActivePanelKind switch
        {
            CSharpProjectFormPanelKind.Graphical => "di_ide_c-sharp-project-form-graphical-panel",
            CSharpProjectFormPanelKind.Manual => "di_ide_c-sharp-project-form-manual-panel",
            _ => throw new NotImplementedException($"The {nameof(CSharpProjectFormPanelKind)}: '{localActivePanelKind}' was unrecognized."),
        };
    }

    private async Task StartNewCSharpProjectCommandOnClick()
    {
        if (!_viewModel.TryTakeSnapshot(out var immutableView) ||
            immutableView is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(immutableView.ProjectTemplateShortNameValue) ||
            string.IsNullOrWhiteSpace(immutableView.CSharpProjectNameValue) ||
            string.IsNullOrWhiteSpace(immutableView.ParentDirectoryNameValue))
        {
            return;
        }

        if (DotNetService.IdeService.TextEditorService.CommonService.WalkHostingInformation.WalkHostingKind == WalkHostingKind.Photino)
        {
            var generalTerminal = DotNetService.IdeService.GetTerminalState().TerminalMap[IdeFacts.GENERAL_KEY];

            var terminalCommandRequest = new TerminalCommandRequest(
                immutableView.FormattedNewCSharpProjectCommand.Value,
                immutableView.ParentDirectoryNameValue,
                new Key<TerminalCommandRequest>(immutableView.NewCSharpProjectTerminalCommandRequestKey.Guid))
            {
                ContinueWithFunc = parsedCommand =>
                {
                    var terminalCommandRequest = new TerminalCommandRequest(
                        immutableView.FormattedAddExistingProjectToSolutionCommand.Value,
                        immutableView.ParentDirectoryNameValue,
                        new Key<TerminalCommandRequest>(immutableView.AddCSharpProjectToSolutionTerminalCommandRequestKey.Guid))
                    {
                        ContinueWithFunc = parsedCommand =>
                        {
                            DotNetService.IdeService.TextEditorService.CommonService.Dialog_ReduceDisposeAction(DialogRecord.DynamicViewModelKey);
    
                            DotNetService.Enqueue(new DotNetWorkArgs
                            {
                                WorkKind = DotNetWorkKind.SetDotNetSolution,
                                DotNetSolutionAbsolutePath = immutableView.DotNetSolutionModel.NamespacePath.AbsolutePath,
                            });
                            return Task.CompletedTask;
                        }
                    };
                        
                    generalTerminal.EnqueueCommand(terminalCommandRequest);
                    return Task.CompletedTask;
                }
            };
                
            generalTerminal.EnqueueCommand(terminalCommandRequest);
        }
    }
    
    public async void OnDotNetSolutionStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }
    
    public async void OnTerminalStateChanged(IdeStateChangedKind ideStateChangedKind)
    {
        if (ideStateChangedKind == IdeStateChangedKind.TerminalStateChanged)
        {
            await InvokeAsync(StateHasChanged);
        }
    }
    
    public void Dispose()
    {
        DotNetService.DotNetSolutionStateChanged -= OnDotNetSolutionStateChanged;
        DotNetService.IdeService.IdeStateChanged -= OnTerminalStateChanged;
    }
}
