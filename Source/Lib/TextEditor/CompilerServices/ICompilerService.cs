using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models.Internals;
using Walk.TextEditor.RazorLib.TextEditors.Displays.Internals;

namespace Walk.TextEditor.RazorLib.CompilerServices;

/// <summary>
/// See 'Walk.CompilerServices.CSharp.CompilerServiceCase.CSharpCompilerService'
/// for an example of an "implementation".
///
/// A lot of interfaces that relate to this one is not required to be implemented,
/// they instead act only as a guide on 'default' decisions if one desires it.
///
/// In order to avoid implementing the related interfaces you need to
/// implement ICompilerService.ParseAsync youself.
///
/// If you inherit 'CompilerService',
/// then you need to override 'CompilerService.ParseAsync'.
///
/// It is at this point that you now have full control
/// over how parsing is done.
/// </summary>
public interface ICompilerService
{
    public event Action? ResourceRegistered;
    public event Action? ResourceParsed;
    public event Action? ResourceDisposed;

    public IReadOnlyList<ICompilerServiceResource> CompilerServiceResources { get; }

    public void RegisterResource(ResourceUri resourceUri, bool shouldTriggerResourceWasModified);
    public void DisposeResource(ResourceUri resourceUri);

    public void ResourceWasModified(ResourceUri resourceUri, IReadOnlyList<TextEditorTextSpan> editTextSpansList);

    public ICompilerServiceResource? GetResource(ResourceUri resourceUri);

    public MenuRecord GetContextMenu(TextEditorVirtualizationResult virtualizationResult, ContextMenu contextMenu);

    public MenuRecord GetAutocompleteMenu(TextEditorVirtualizationResult virtualizationResult, AutocompleteMenu autocompleteMenu);

    public ValueTask<MenuRecord> GetQuickActionsSlashRefactorMenu(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModelModifier);
        
    public ValueTask OnInspect(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModelModifier,
        double clientX,
        double clientY,
        bool shiftKey,
        bool ctrlKey,
        bool altKey,
        TextEditorComponentData componentData,
        ResourceUri resourceUri);
    
    public ValueTask ShowCallingSignature(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModelModifier,
        int positionIndex,
        TextEditorComponentData componentData,
        ResourceUri resourceUri);
        
    public ValueTask GoToDefinition(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier,
        TextEditorViewModel viewModelModifier,
        Category category,
        int positionIndex);

    public ValueTask ParseAsync(TextEditorEditContext editContext, TextEditorModel modelModifier, bool shouldApplySyntaxHighlighting);
    
    public ValueTask FastParseAsync(
        TextEditorEditContext editContext,
        ResourceUri resourceUri,
        IFileSystemProvider fileSystemProvider,
        CompilationUnitKind compilationUnitKind);

    public void FastParse(
        TextEditorEditContext editContext,
        ResourceUri resourceUri,
        IFileSystemProvider fileSystemProvider,
        CompilationUnitKind compilationUnitKind);
}
