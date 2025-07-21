using Walk.TextEditor.RazorLib.CompilerServices;
using Walk.TextEditor.RazorLib.Lexers.Models;

namespace Walk.Extensions.CompilerServices;

public static class DiagnosticHelper
{
    public static void ReportEndOfFileUnexpected(List<TextEditorDiagnostic> diagnosticList, TextEditorTextSpan textSpan)
    {
        Report(
            diagnosticList,
            TextEditorDiagnosticLevel.Error,
            "'End of file' was unexpected.",
            textSpan,
            0);
    }

    public static void ReportUnexpectedToken(
        List<TextEditorDiagnostic> diagnosticList,
        TextEditorTextSpan textSpan,
        string unexpectedToken,
        string expectedToken)
    {
        Report(
            diagnosticList,
            TextEditorDiagnosticLevel.Error,
            $"Unexpected token: '{unexpectedToken}' | expected: '{expectedToken}'",
            textSpan,
            1);
    }

    public static void ReportUnexpectedToken(
        List<TextEditorDiagnostic> diagnosticList,
        TextEditorTextSpan textSpan,
        string unexpectedToken)
    {
        Report(
            diagnosticList,
            TextEditorDiagnosticLevel.Error,
            $"Unexpected token: '{unexpectedToken}'",
            textSpan,
            2);
    }

    public static void ReportUndefinedClass(
        List<TextEditorDiagnostic> diagnosticList,
        TextEditorTextSpan textSpan,
        string undefinedClassIdentifier)
    {
        Report(
            diagnosticList,
            TextEditorDiagnosticLevel.Error,
            $"Undefined class: '{undefinedClassIdentifier}'",
            textSpan,
            3);
    }

    public static void ReportImplicitlyTypedVariablesMustBeInitialized(
        List<TextEditorDiagnostic> diagnosticList,
        TextEditorTextSpan textSpan)
    {
        Report(
            diagnosticList,
            TextEditorDiagnosticLevel.Error,
            $"Implicitly-typed variables must be initialized",
            textSpan,
            4);
    }

    public static void ReportUndefinedTypeOrNamespace(
        List<TextEditorDiagnostic> diagnosticList,
        TextEditorTextSpan textSpan,
        string undefinedTypeOrNamespaceIdentifier)
    {
        Report(
            diagnosticList,
            TextEditorDiagnosticLevel.Error,
            $"Undefined type or namespace: '{undefinedTypeOrNamespaceIdentifier}'",
            textSpan,
            5);
    }

    public static void ReportAlreadyDefinedType(
        List<TextEditorDiagnostic> diagnosticList,
        TextEditorTextSpan textSpan,
        string alreadyDefinedTypeIdentifier)
    {
        Report(
            diagnosticList,
            TextEditorDiagnosticLevel.Error,
            $"Already defined type: '{alreadyDefinedTypeIdentifier}'",
            textSpan,
            6);
    }

    public static void ReportUndefinedVariable(
        List<TextEditorDiagnostic> diagnosticList,
        TextEditorTextSpan textSpan,
        string undefinedVariableIdentifier)
    {
        Report(
            diagnosticList,
            TextEditorDiagnosticLevel.Error,
            $"Undefined variable: '{undefinedVariableIdentifier}'",
            textSpan,
            7);
    }

    /*public static void ReportNotDefinedInContext(
        List<TextEditorDiagnostic> diagnosticList,
        TextEditorTextSpan textSpan,
        string contextString)
    {
        Report(
            diagnosticList,
            TextEditorDiagnosticLevel.Error,
            $"'{textSpan.GetText()}' is not defined in the context '{contextString}'",
            textSpan,
            8);
    }*/

    public static void TheNameDoesNotExistInTheCurrentContext(
        List<TextEditorDiagnostic> diagnosticList,
        TextEditorTextSpan textSpan,
        string name)
    {
        Report(
            diagnosticList,
            TextEditorDiagnosticLevel.Error,
            $"The name '{name}' does not exist in the current context",
            textSpan,
            9);
    }

    public static void ReportAlreadyDefinedVariable(
        List<TextEditorDiagnostic> diagnosticList,
        TextEditorTextSpan textSpan,
        string alreadyDefinedVariableIdentifier)
    {
        Report(
            diagnosticList,
            TextEditorDiagnosticLevel.Error,
            $"Already defined variable: '{alreadyDefinedVariableIdentifier}'",
            textSpan,
            10);
    }
    
    public static void ReportAlreadyDefinedLabel(
        List<TextEditorDiagnostic> diagnosticList,
        TextEditorTextSpan textSpan,
        string alreadyDefinedLabelIdentifier)
    {
        Report(
            diagnosticList,
            TextEditorDiagnosticLevel.Error,
            $"Already defined variable: '{alreadyDefinedLabelIdentifier}'",
            textSpan,
            11);
    }

    public static void ReportAlreadyDefinedProperty(
        List<TextEditorDiagnostic> diagnosticList,
        TextEditorTextSpan textSpan,
        string alreadyDefinedPropertyIdentifier)
    {
        Report(
            diagnosticList,
            TextEditorDiagnosticLevel.Error,
            $"Already defined property: '{alreadyDefinedPropertyIdentifier}'",
            textSpan,
            12);
    }

    public static void ReportUndefinedFunction(
        List<TextEditorDiagnostic> diagnosticList,
        TextEditorTextSpan textSpan,
        string undefinedFunctionIdentifier)
    {
        Report(
            diagnosticList,
            TextEditorDiagnosticLevel.Error,
            $"Undefined function: '{undefinedFunctionIdentifier}'",
            textSpan,
            13);
    }

    public static void ReportAlreadyDefinedFunction(
        List<TextEditorDiagnostic> diagnosticList,
        TextEditorTextSpan textSpan,
        string alreadyDefinedFunctionIdentifier)
    {
        Report(
            diagnosticList,
            TextEditorDiagnosticLevel.Error,
            $"Already defined function: '{alreadyDefinedFunctionIdentifier}'",
            textSpan,
            14);
    }

    public static void ReportBadFunctionOptionalArgumentDueToMismatchInType(
        List<TextEditorDiagnostic> diagnosticList,
        TextEditorTextSpan optionalArgumentTextSpan,
        string optionalArgumentVariableIdentifier,
        string typeExpectedIdentifierString,
        string typeActualIdentifierString)
    {
        Report(
            diagnosticList,
            TextEditorDiagnosticLevel.Error,
            $"The optional argument: '{optionalArgumentVariableIdentifier}'" +
                $" expects the type: '{typeExpectedIdentifierString}'" +
                $", but received the type: '{typeActualIdentifierString}'",
            optionalArgumentTextSpan,
            15);
    }

    public static void ReportReturnStatementsAreStillBeingImplemented(
        List<TextEditorDiagnostic> diagnosticList,
        TextEditorTextSpan textSpan)
    {
        Report(
            diagnosticList,
            TextEditorDiagnosticLevel.Hint,
            $"Parsing of return statements is still being implemented.",
            textSpan,
            16);
    }

    public static void ReportTagNameMissing(List<TextEditorDiagnostic> diagnosticList, TextEditorTextSpan textSpan)
    {
        Report(
            diagnosticList,
            TextEditorDiagnosticLevel.Error,
            "Missing tag name.",
            textSpan,
            17);
    }

    public static void ReportOpenTagWithUnMatchedCloseTag(
        List<TextEditorDiagnostic> diagnosticList,
        string openTagName,
        string closeTagName,
        TextEditorTextSpan textSpan)
    {
        Report(
            diagnosticList,
            TextEditorDiagnosticLevel.Error,
            $"Open tag: '{openTagName}' has an unmatched close tag: {closeTagName}.",
            textSpan,
            18);
    }

    public static void ReportRazorExplicitExpressionPredicateWasExpected(
        List<TextEditorDiagnostic> diagnosticList,
        string transitionSubstring,
        string keywordText,
        TextEditorTextSpan textSpan)
    {
        Report(
            diagnosticList,
            TextEditorDiagnosticLevel.Error,
            $"An explicit expression predicate was expected to follow the {transitionSubstring}{keywordText} razor keyword.",
            textSpan,
            19);
    }

    public static void ReportRazorCodeBlockWasExpectedToFollowRazorKeyword(
        List<TextEditorDiagnostic> diagnosticList,
        string transitionSubstring,
        string keywordText,
        TextEditorTextSpan textSpan)
    {
        Report(
            diagnosticList,
            TextEditorDiagnosticLevel.Error,
            $"A code block was expected to follow the {transitionSubstring}{keywordText} razor keyword.",
            textSpan,
            20);
    }

    public static void ReportRazorWhitespaceImmediatelyFollowingTransitionCharacterIsUnexpected(
        List<TextEditorDiagnostic> diagnosticList,
        TextEditorTextSpan textSpan)
    {
        Report(
            diagnosticList,
            TextEditorDiagnosticLevel.Error,
            "Whitespace immediately following the Razor transition character is unexpected.",
            textSpan,
            21);
    }

    public static void ReportConstructorsNeedToBeWithinTypeDefinition(List<TextEditorDiagnostic> diagnosticList, TextEditorTextSpan textSpan)
    {
        Report(
            diagnosticList,
            TextEditorDiagnosticLevel.Error,
            "Constructors need to be within a type definition.",
            textSpan,
            22);
    }

    /// <summary>
    /// The text "TODO: " is pre-pended to the provided message.<br/><br/>
    /// Used when the C# Parser has not yet implemented the functionality that one would
    /// expect given the situation.
    /// <br/>
    /// // TODO: Find all references to this method and fix them. 
    /// Keep this comment so one can search // TODO: and arrive here
    /// </summary>
    public static void ReportTodoException(List<TextEditorDiagnostic> diagnosticList, TextEditorTextSpan textSpan, string message)
    {
        Report(
            diagnosticList,
            TextEditorDiagnosticLevel.Hint,
            $"TODO: {message}",
            textSpan,
            23);
    }

    private static void Report(
        List<TextEditorDiagnostic> diagnosticList,
        TextEditorDiagnosticLevel diagnosticLevel,
        string message,
        TextEditorTextSpan textSpan,
        int diagnosticId)
    {
        var compilerServiceDiagnosticDecorationKind = diagnosticLevel switch
        {
            TextEditorDiagnosticLevel.Hint => CompilerServiceDiagnosticDecorationKind.DiagnosticHint,
            TextEditorDiagnosticLevel.Suggestion => CompilerServiceDiagnosticDecorationKind.DiagnosticSuggestion,
            TextEditorDiagnosticLevel.Warning => CompilerServiceDiagnosticDecorationKind.DiagnosticWarning,
            TextEditorDiagnosticLevel.Error => CompilerServiceDiagnosticDecorationKind.DiagnosticError,
            TextEditorDiagnosticLevel.Other => CompilerServiceDiagnosticDecorationKind.DiagnosticOther,
            _ => CompilerServiceDiagnosticDecorationKind.DiagnosticOther,
        };

        textSpan = textSpan with
        {
            DecorationByte = (byte)compilerServiceDiagnosticDecorationKind
        };

        diagnosticList.Add(new TextEditorDiagnostic(
            diagnosticLevel,
            message,
            textSpan,
            diagnosticId));
    }

    public static void ClearByResourceUri(List<TextEditorDiagnostic> diagnosticList, ResourceUri resourceUri)
    {
        // var keep = diagnosticList.Where(x => x.TextSpan.ResourceUri != resourceUri);

        diagnosticList.Clear();
        // diagnosticList.AddRange(keep);
    }
}
