using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.CompilerServices.CSharp.BinderCase;
using Walk.CompilerServices.CSharp.CompilerServiceCase;

namespace Walk.CompilerServices.Razor.CompilerServiceCase;

public record SemanticResultRazor
{
    /// <summary>
    /// The goal is to take a '.razor' file and create a 'behind-the-scenes'
    /// C# class.
    /// <br/><br/>
    /// Any C# logic which exists in an '@code' or '@functions' block
    /// relates to a C# class's class level code.
    /// <br/><br/>
    /// Any C# logic which is NOT within an '@code' or '@functions' block
    /// is placed within a fabricated C# method.
    /// <br/><br/>
    /// This allows the C# Parser to look at the .razor markup
    /// as if it were just a C# class.
    /// <br/><br/>
    /// An issue arises however. One must map each character from the
    /// fabricated 'behind-the-scenes' C# class to the actual .razor file.
    /// This way the .razor file has semantic syntax highlighting,
    /// diagnostics, on hover tooltips, etc...
    /// </summary>
    public SemanticResultRazor(
        CSharpCompilationUnit compilationUnit,
        CSharpBinder binder,
        List<AdhocTextInsertion> codebehindClassInsertions,
        List<AdhocTextInsertion> codebehindRenderFunctionInsertions,
        AdhocTextInsertion adhocTextInsertionOfTheRenderFunctionItselfIntoTheCodebehindClass,
        string classContents)
    {
        CompilationUnit = compilationUnit;
        Binder = binder;
        CodebehindClassInsertions = codebehindClassInsertions;
        CodebehindRenderFunctionInsertions = codebehindRenderFunctionInsertions;
        AdhocTextInsertionOfTheRenderFunctionItselfIntoTheCodebehindClass = adhocTextInsertionOfTheRenderFunctionItselfIntoTheCodebehindClass;
        ClassContents = classContents;
    }

    public CSharpCompilationUnit CompilationUnit { get; }
    public CSharpBinder Binder { get; }
    public List<AdhocTextInsertion> CodebehindClassInsertions { get; }
    public List<AdhocTextInsertion> CodebehindRenderFunctionInsertions { get; }
    public AdhocTextInsertion AdhocTextInsertionOfTheRenderFunctionItselfIntoTheCodebehindClass { get; }
    /// <summary>
    /// After the <see cref="CodebehindClassInsertions"/> and the <see cref="CodebehindRenderFunctionInsertions"/> are combined into
    /// a single C# class. The resulting string is here.
    /// </summary>
    public string ClassContents { get; }

    public TextEditorTextSpan? MapAdhocCSharpTextSpanToSource(
        ResourceUri sourceResourceUri,
        string sourceText,
        TextEditorTextSpan textSpan)
    {
        var adhocTextInsertion = CodebehindClassInsertions
                .SingleOrDefault(x =>
                    textSpan.StartInclusiveIndex >= x.Insertion_StartInclusiveIndex &&
                    textSpan.EndExclusiveIndex <= x.Insertion_EndExclusiveIndex);

        // TODO: Fix for spans that go 2 adhocTextInsertions worth of length?
        if (adhocTextInsertion is null)
        {
            adhocTextInsertion = CodebehindRenderFunctionInsertions
                .SingleOrDefault(x =>
                    textSpan.StartInclusiveIndex >= x.Insertion_StartInclusiveIndex &&
                    textSpan.EndExclusiveIndex <= x.Insertion_EndExclusiveIndex);
        }

        if (adhocTextInsertion is null)
        {
            // Could not map the text span back to the source file  
            return null;
        }

        var symbolSourceText_StartInclusiveIndex =
                    adhocTextInsertion.SourceText_StartInclusiveIndex +
                    (textSpan.StartInclusiveIndex - adhocTextInsertion.Insertion_StartInclusiveIndex);

        var symbolSourceText_EndExclusiveIndex =
            symbolSourceText_StartInclusiveIndex +
            (textSpan.EndExclusiveIndex - textSpan.StartInclusiveIndex);

        return new TextEditorTextSpan(
        	symbolSourceText_StartInclusiveIndex,
        	symbolSourceText_EndExclusiveIndex,
        	textSpan.DecorationByte);
    }
}