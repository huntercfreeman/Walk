using Walk.Common.RazorLib;
using Walk.Common.RazorLib.Dialogs.Models;
using Walk.Common.RazorLib.Dynamics.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Widgets.Models;
using Walk.Ide.RazorLib.CodeSearches.Displays;
using Walk.Ide.RazorLib.CodeSearches.Models;
using Walk.TextEditor.RazorLib;
using Walk.TextEditor.RazorLib.Cursors.Models;

namespace Walk.Ide.RazorLib;

public partial class IdeService
{
    private WidgetModel? _contextSwitchWidget;
    private WidgetModel? _commandBarWidget;

    public IDialog? CodeSearchDialog { get; set; }

    /*public void CommandFactory_Initialize()
    {
        ((TextEditorKeymapDefault)TextEditorFacts.Keymap_DefaultKeymap).AltF12Func = CommandFactory_PeekCodeSearchDialog;

        // FindAllReferences
        // ((TextEditorKeymapDefault)TextEditorKeymapFacts.DefaultKeymap).ShiftF12Func = ShowAllReferences;

        // ActiveContextsContext
        _ = CommonFacts.GlobalContext.Keymap.TryRegister(
            new KeymapArgs()
            {
                Key = "a",
                Code = "KeyA",
                LayerKey = -1,
                CtrlKey = true,
                AltKey = true,
            },
            ContextHelper.ConstructFocusContextElementCommand(
                CommonFacts.ActiveContextsContext, "Focus: ActiveContexts", "focus-active-contexts", CommonService.JsRuntimeCommonApi, CommonService));

        // BackgroundServicesContext
        _ = CommonFacts.GlobalContext.Keymap.TryRegister(
            new KeymapArgs()
            {
                Key = "b",
                Code = "KeyB",
                LayerKey = -1,

                CtrlKey = true,
                AltKey = true,
            },
            ContextHelper.ConstructFocusContextElementCommand(
                CommonFacts.BackgroundServicesContext, "Focus: BackgroundServices", "focus-background-services", CommonService.JsRuntimeCommonApi, CommonService));

        // CompilerServiceExplorerContext
        _ = CommonFacts.GlobalContext.Keymap.TryRegister(
            new KeymapArgs()
            {
                Key = "C",
                Code = "KeyC",
                LayerKey = -1,

                ShiftKey = true,
                CtrlKey = true,
                AltKey = true,
            },
            ContextHelper.ConstructFocusContextElementCommand(
                CommonFacts.CompilerServiceExplorerContext, "Focus: CompilerServiceExplorer", "focus-compiler-service-explorer", CommonService.JsRuntimeCommonApi, CommonService));

        // CompilerServiceEditorContext
        _ = CommonFacts.GlobalContext.Keymap.TryRegister(
            new KeymapArgs()
            {
                Key = "c",
                Code = "KeyC",
                LayerKey = -1,
                CtrlKey = true,
                AltKey = true,
            },
            ContextHelper.ConstructFocusContextElementCommand(
                CommonFacts.CompilerServiceEditorContext, "Focus: CompilerServiceEditor", "focus-compiler-service-editor", CommonService.JsRuntimeCommonApi, CommonService));

        // DialogDisplayContext
        _ = CommonFacts.GlobalContext.Keymap.TryRegister(
            new KeymapArgs()
            {
                Key = "d",
                Code = "KeyD",
                LayerKey = -1,
                CtrlKey = true,
                AltKey = true,
            },
            ContextHelper.ConstructFocusContextElementCommand(
                CommonFacts.DialogDisplayContext, "Focus: DialogDisplay", "focus-dialog-display", CommonService.JsRuntimeCommonApi, CommonService));

        // EditorContext
        _ = CommonFacts.GlobalContext.Keymap.TryRegister(
            new KeymapArgs()
            {
                Key = "E",
                Code = "KeyE",
                LayerKey = -1,
                ShiftKey = true,
                CtrlKey = true,
                AltKey = true,
            },
            ContextHelper.ConstructFocusContextElementCommand(
                CommonFacts.EditorContext, "Focus: Editor", "focus-editor", CommonService.JsRuntimeCommonApi, CommonService));

        // FolderExplorerContext
        _ = CommonFacts.GlobalContext.Keymap.TryRegister(
            new KeymapArgs()
            {
                Key = "f",
                Code = "KeyF",
                LayerKey = -1,
                CtrlKey = true,
                AltKey = true,
            },
            ContextHelper.ConstructFocusContextElementCommand(
                CommonFacts.FolderExplorerContext, "Focus: FolderExplorer", "focus-folder-explorer", CommonService.JsRuntimeCommonApi, CommonService));

        // GitContext
        _ = CommonFacts.GlobalContext.Keymap.TryRegister(
            new KeymapArgs()
            {
                Key = "g",
                Code = "KeyG",
                LayerKey = -1,
                CtrlKey = true,
                AltKey = true,
            },
            ContextHelper.ConstructFocusContextElementCommand(
                CommonFacts.GitContext, "Focus: Git", "focus-git", CommonService.JsRuntimeCommonApi, CommonService));

        // GlobalContext
        _ = CommonFacts.GlobalContext.Keymap.TryRegister(
            new KeymapArgs()
            {
                Key = "g",
                Code = "KeyG",
                LayerKey = -1,
                CtrlKey = true,
                AltKey = true,
            },
            ContextHelper.ConstructFocusContextElementCommand(
                CommonFacts.GlobalContext, "Focus: Global", "focus-global", CommonService.JsRuntimeCommonApi, CommonService));

        // MainLayoutFooterContext
        _ = CommonFacts.GlobalContext.Keymap.TryRegister(
            new KeymapArgs()
            {
                Key = "f",
                Code = "KeyF",
                LayerKey = -1,
                CtrlKey = true,
                AltKey = true,
            },
            ContextHelper.ConstructFocusContextElementCommand(
                CommonFacts.MainLayoutFooterContext, "Focus: Footer", "focus-footer", CommonService.JsRuntimeCommonApi, CommonService));

        // MainLayoutHeaderContext
        _ = CommonFacts.GlobalContext.Keymap.TryRegister(
            new KeymapArgs()
            {
                Key = "h",
                Code = "KeyH",
                LayerKey = -1,
                CtrlKey = true,
                AltKey = true,
            },
            ContextHelper.ConstructFocusContextElementCommand(
                CommonFacts.MainLayoutHeaderContext, "Focus: Header", "focus-header", CommonService.JsRuntimeCommonApi, CommonService));

        // ErrorListContext
        _ = CommonFacts.GlobalContext.Keymap.TryRegister(
            new KeymapArgs()
            {
                Key = "e",
                Code = "KeyE",
                LayerKey = -1,
                CtrlKey = true,
                AltKey = true,
            },
            ContextHelper.ConstructFocusContextElementCommand(
                CommonFacts.ErrorListContext, "Focus: Error List", "error-list", CommonService.JsRuntimeCommonApi, CommonService));

        // OutputContext
        _ = CommonFacts.GlobalContext.Keymap.TryRegister(
            new KeymapArgs()
            {
                Key = "o",
                Code = "KeyO",
                LayerKey = -1,
                CtrlKey = true,
                AltKey = true,
            },
            ContextHelper.ConstructFocusContextElementCommand(
                CommonFacts.OutputContext, "Focus: Output", "focus-output", CommonService.JsRuntimeCommonApi, CommonService));

        // TerminalContext
        _ = CommonFacts.GlobalContext.Keymap.TryRegister(
            new KeymapArgs()
            {
                Key = "t",
                Code = "KeyT",
                LayerKey = -1,
                CtrlKey = true,
                AltKey = true,
            },
            ContextHelper.ConstructFocusContextElementCommand(
                CommonFacts.TerminalContext, "Focus: Terminal", "focus-terminal", CommonService.JsRuntimeCommonApi, CommonService));

        // TestExplorerContext
        _ = CommonFacts.GlobalContext.Keymap.TryRegister(
            new KeymapArgs()
            {
                Key = "T",
                Code = "KeyT",
                LayerKey = -1,
                ShiftKey = true,
                CtrlKey = true,
                AltKey = true,
            },
            ContextHelper.ConstructFocusContextElementCommand(
                CommonFacts.TestExplorerContext, "Focus: Test Explorer", "focus-test-explorer", CommonService.JsRuntimeCommonApi, CommonService));

        // TextEditorContext
        _ = CommonFacts.GlobalContext.Keymap.TryRegister(
            new KeymapArgs()
            {
                Key = "t",
                Code = "KeyT",
                LayerKey = -1,
                CtrlKey = true,
                AltKey = true,
            },
            ContextHelper.ConstructFocusContextElementCommand(
                CommonFacts.TextEditorContext, "Focus: TextEditor", "focus-text-editor", CommonService.JsRuntimeCommonApi, CommonService));

        // Focus the text editor itself (as to allow for typing into the editor)
        var focusTextEditorCommand = new CommonCommand(
            "Focus: Text Editor", "focus-text-editor", false,
            async commandArgs =>
            {
                var group = TextEditorService.Group_GetOrDefault(EditorTextEditorGroupKey);
                if (group is null)
                    return;

                var activeViewModel = TextEditorService.ViewModel_GetOrDefault(group.ActiveViewModelKey);
                if (activeViewModel is null)
                    return;

                var componentData = activeViewModel.PersistentState.ComponentData;
                if (componentData is not null)
                {
                    await CommonService.JsRuntimeCommonApi
                        .FocusHtmlElementById(componentData.PrimaryCursorContentId)
                        .ConfigureAwait(false);
                }
            });

        _ = CommonFacts.GlobalContext.Keymap.TryRegister(
                new KeymapArgs()
                {
                    Key = "Escape",
                    Code = "Escape",
                    LayerKey = -1
                },
                focusTextEditorCommand);

        // Add command to bring up a FindAll dialog. Example: { Ctrl + Shift + f }
        var openFindDialogCommand = new CommonCommand(
            "Open: Find", "open-find", false,
            commandArgs =>
            {
                TextEditorService.Options_ShowFindAllDialog();
                return ValueTask.CompletedTask;
            });

        _ = CommonFacts.GlobalContext.Keymap.TryRegister(
                new KeymapArgs()
                {
                    Key = "F",
                    Code = "KeyF",
                    LayerKey = -1,
                    ShiftKey = true,
                    CtrlKey = true,
                },
                openFindDialogCommand);

        // Add command to bring up a CodeSearch dialog. Example: { Ctrl + , }
        // TODO: determine the actively focused element at time of invocation,
        //       then restore focus to that element when this dialog is closed.
        var openCodeSearchDialogCommand = new CommonCommand(
            "Open: Code Search", "open-code-search", false,
            commandArgs =>
            {
                return CommandFactory_OpenCodeSearchDialog();
            });

        _ = CommonFacts.GlobalContext.Keymap.TryRegister(
                new KeymapArgs()
                {
                    Key = ",",
                    Code = "Comma",
                    LayerKey = -1,
                    CtrlKey = true,
                },
                openCodeSearchDialogCommand);

        // Add command to bring up a Context Switch dialog. Example: { Ctrl + Tab }
        // TODO: determine the actively focused element at time of invocation,
        //       then restore focus to that element when this dialog is closed.
        var openContextSwitchDialogCommand = new CommonCommand(
            "Open: Context Switch", "open-context-switch", false,
            async commandArgs =>
            {
                var elementDimensions = await CommonService.JsRuntimeCommonApi
                    .MeasureElementById("di_ide_header-button-file")
                    .ConfigureAwait(false);

                var contextState = CommonService.GetContextState();

                var menuOptionList = new List<MenuOptionRecord>();

                foreach (var context in contextState.AllContextsList)
                {
                    menuOptionList.Add(new MenuOptionRecord(
                        "context.Name was here",
                        MenuOptionKind.Other));
                }

                MenuRecord menu;

                if (menuOptionList.Count == 0)
                    menu = new MenuRecord(MenuRecord.NoMenuOptionsExistList);
                else
                    menu = new MenuRecord(menuOptionList);

                var dropdownRecord = new DropdownRecord(
                    Key<DropdownRecord>.NewKey(),
                    elementDimensions.LeftInPixels,
                    elementDimensions.TopInPixels + elementDimensions.HeightInPixels,
                    typeof(MenuDisplay),
                    new Dictionary<string, object?>
                    {
                        {
                            nameof(MenuDisplay.MenuRecord),
                            menu
                        }
                    },
                    () => Task.CompletedTask);

                // _dispatcher.Dispatch(new DropdownState.RegisterAction(dropdownRecord));

                if (CommonService.GetContextState().FocusedContextKey == CommonFacts.TextEditorContext.ContextKey)
                {
                    CommonService.GetContextSwitchState().FocusInitiallyContextSwitchGroupKey = WalkTextEditorInitializer.ContextSwitchGroupKey;
                }
                else
                {
                    CommonService.GetContextSwitchState().FocusInitiallyContextSwitchGroupKey = WalkCommonInitializer.ContextSwitchGroupKey;
                }

                _contextSwitchWidget ??= new WidgetModel(
                    typeof(ContextSwitchDisplay),
                    componentParameterMap: null,
                    cssClass: null,
                    cssStyle: null);

                CommonService.SetWidget(_contextSwitchWidget);
            });

        _ = CommonFacts.GlobalContext.Keymap.TryRegister(
                new KeymapArgs()
                {
                    Key = "Tab",
                    Code = "Tab",
                    LayerKey = -1,
                    CtrlKey = true,
                },
                openContextSwitchDialogCommand);

        _ = CommonFacts.GlobalContext.Keymap.TryRegister(
                new KeymapArgs()
                {
                    Key = "/",
                    Code = "Slash",
                    LayerKey = -1,
                    CtrlKey = true,
                    AltKey = true,
                },
                openContextSwitchDialogCommand);

        // Command bar
        var openCommandBarCommand = new CommonCommand(
            "Open: Command Bar", "open-command-bar", false,
            commandArgs =>
            {
                _commandBarWidget ??= new WidgetModel(
                    typeof(Walk.Ide.RazorLib.CommandBars.Displays.CommandBarDisplay),
                    componentParameterMap: null,
                    cssClass: null,
                    cssStyle: "width: 80vw; height: 5em; left: 10vw; top: 0;");

                CommonService.SetWidget(_commandBarWidget);
                return ValueTask.CompletedTask;
            });

        _ = CommonFacts.GlobalContext.Keymap.TryRegister(
                new KeymapArgs()
                {
                    Key = "p",
                    Code = "KeyP",
                    LayerKey = -1,
                    CtrlKey = true,
                },
                openCommandBarCommand);
    }*/

    public ValueTask CommandFactory_OpenCodeSearchDialog()
    {
        // Duplicated Code: 'PeekCodeSearchDialog(...)'
        CodeSearchDialog ??= new DialogViewModel(
            Key<IDynamicViewModel>.NewKey(),
            "Code Search",
            typeof(CodeSearchDisplay),
            null,
            null,
            true,
            null);

        CommonService.Dialog_ReduceRegisterAction(CodeSearchDialog);

        TextEditorService.WorkerArbitrary.PostUnique(async editContext =>
        {
            var group = TextEditorService.Group_GetOrDefault(EditorTextEditorGroupKey);
            if (group is null)
                return;

            var activeViewModel = TextEditorService.ViewModel_GetOrDefault(group.ActiveViewModelKey);
            if (activeViewModel is null)
                return;

            var viewModelModifier = editContext.GetViewModelModifier(activeViewModel.PersistentState.ViewModelKey);
            if (viewModelModifier is null)
                return;

            // If the user has an active text selection,
            // then populate the code search with their selection.

            var modelModifier = editContext.GetModelModifier(viewModelModifier.PersistentState.ResourceUri);

            if (modelModifier is null)
                return;

            var selectedText = TextEditorSelectionHelper.GetSelectedText(viewModelModifier, modelModifier);
            if (selectedText is null)
                return;

            CodeSearch_With(inState => inState with
            {
                Query = selectedText,
            });

            CodeSearch_HandleSearchEffect();

            // I tried without the Yield and it works fine without it.
            // I'm gonna keep it though so I can sleep at night.
            //
            await Task.Yield();
            await Task.Delay(200).ConfigureAwait(false);

            CommonService.TreeView_MoveHomeAction(
                CodeSearchState.TreeViewCodeSearchContainerKey,
                false,
                false);
        });

        return ValueTask.CompletedTask;
    }

    public async ValueTask CommandFactory_PeekCodeSearchDialog(TextEditorEditContext editContext, string? resourceUriValue, int? indexInclusiveStart)
    {
        var absolutePath = CommonService.EnvironmentProvider.AbsolutePathFactory(resourceUriValue, isDirectory: false);

        // Duplicated Code: 'OpenCodeSearchDialog(...)'
        CodeSearchDialog ??= new DialogViewModel(
            Key<IDynamicViewModel>.NewKey(),
            "Code Search",
            typeof(CodeSearchDisplay),
            null,
            null,
            true,
            null);

        CommonService.Dialog_ReduceRegisterAction(CodeSearchDialog);

        CodeSearch_With(inState => inState with
        {
            Query = absolutePath.NameWithExtension,
        });

        await CodeSearch_HandleSearchEffect().ConfigureAwait(false);

        // I tried without the Yield and it works fine without it.
        // I'm gonna keep it though so I can sleep at night.
        //
        await Task.Yield();
        await Task.Delay(200).ConfigureAwait(false);

        CommonService.TreeView_MoveHomeAction(
            CodeSearchState.TreeViewCodeSearchContainerKey,
            false,
            false);
    }
}
