using System.Text;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.Decorations.Models;
using Walk.CompilerServices.DotNetSolution.Models.Project;

namespace Walk.CompilerServices.DotNetSolution;

public static class DotNetSolutionLexer
{
    public enum DotNetSolutionLexerContextKind
    {
        Header,
        ProjectListings_Expect_ProjectKeyword,
        ProjectListings_Expect_ProjectTypeGuid,
        ProjectListings_Expect_ProjectName,
        ProjectListings_Expect_ProjectPath,
        ProjectListings_Expect_ProjectIdGuid,
        ProjectListings_Expect_EndProjectKeyword,
        Global,
        GlobalSectionNestedProjects_Expect_ChildGuid,
        GlobalSectionNestedProjects_Expect_ParentGuid,
        Finish
    }

    public static DotNetSolutionLexerOutput Lex(StreamReaderWrap streamReaderWrap)
    {
        var stringBuilder = new StringBuilder();
    
        var context = DotNetSolutionLexerContextKind.Header;
        var output = new DotNetSolutionLexerOutput();
        
        // This gets updated throughout the loop
        var startPosition = streamReaderWrap.PositionIndex;
        var startByte = streamReaderWrap.ByteIndex;
        
        var indexOfMostRecentTagOpen = -1;
        
        Guid projectTypeGuid = Guid.Empty;
        string projectName = default;
        string projectPath = default;
        Guid projectIdGuid = Guid.Empty;
        
        var hasSeenGlobal = false;
        var hasSeenGlobalSectionNestedProjects = false;
        
        var childGuid = Guid.Empty;
        var parentGuid = Guid.Empty;
        
        while (!streamReaderWrap.IsEof)
        {
            switch (streamReaderWrap.CurrentCharacter)
            {
                /* Lowercase Letters */
                case 'a':
                case 'b':
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                case 'g':
                case 'h':
                case 'i':
                case 'j':
                case 'k':
                case 'l':
                case 'm':
                case 'n':
                case 'o':
                case 'p':
                case 'q':
                case 'r':
                case 's':
                case 't':
                case 'u':
                case 'v':
                case 'w':
                case 'x':
                case 'y':
                case 'z':
                /* Uppercase Letters */
                case 'A':
                case 'B':
                case 'C':
                case 'D':
                case 'E':
                case 'F':
                case 'G':
                case 'H':
                case 'I':
                case 'J':
                case 'K':
                case 'L':
                case 'M':
                case 'N':
                case 'O':
                case 'P':
                case 'Q':
                case 'R':
                case 'S':
                case 'T':
                case 'U':
                case 'V':
                case 'W':
                case 'X':
                case 'Y':
                case 'Z':
                /* Underscore */
                case '_':
                    switch (context)
                    {
                        case DotNetSolutionLexerContextKind.Header:
                        {
                            if (streamReaderWrap.CurrentCharacter == 'P')
                            {
                                var startKeywordPosition = streamReaderWrap.PositionIndex;
                                
                                if (TrySkipProjectText(streamReaderWrap))
                                {
                                    output.TextSpanList.Add(new TextEditorTextSpan(
                                        startKeywordPosition,
                                        streamReaderWrap.PositionIndex,
                                        (byte)GenericDecorationKind.Xml_TagNameNone));
                                    
                                    context = DotNetSolutionLexerContextKind.ProjectListings_Expect_ProjectTypeGuid;
                                    continue;
                                }
                            }
                            break;
                        }
                        case DotNetSolutionLexerContextKind.ProjectListings_Expect_ProjectKeyword:
                        {
                            if (streamReaderWrap.CurrentCharacter == 'P')
                            {
                                if (TrySkipProjectText(streamReaderWrap))
                                {
                                    context = DotNetSolutionLexerContextKind.ProjectListings_Expect_ProjectTypeGuid;
                                    continue;
                                }
                            }
                            break;
                        }
                        case DotNetSolutionLexerContextKind.ProjectListings_Expect_EndProjectKeyword:
                        {
                            if (streamReaderWrap.CurrentCharacter == 'E')
                            {
                                if (TrySkipEndProjectText(streamReaderWrap))
                                {
                                    context = DotNetSolutionLexerContextKind.ProjectListings_Expect_ProjectKeyword;
                                    
                                    if (projectTypeGuid == SolutionFolder.SolutionFolderProjectTypeGuid)
                                    {
                                        output.SolutionFolderList.Add(new SolutionFolder(
                                            projectName,
                                            projectTypeGuid,
                                            projectPath,
                                            projectIdGuid));
                                    }
                                    else
                                    {
                                        output.DotNetProjectList.Add(new CSharpProjectModel(
                                            projectName,
                                            projectTypeGuid,
                                            projectPath,
                                            projectIdGuid,
                                            absolutePath: default));
                                    }
                                    
                                    continue;
                                }
                            }
                            break;
                        }
                        case DotNetSolutionLexerContextKind.ProjectListings_Expect_ProjectTypeGuid:
                        {
                            projectTypeGuid = LexGuid(streamReaderWrap, stringBuilder, true);
                            context = DotNetSolutionLexerContextKind.ProjectListings_Expect_ProjectName;
                            break;
                        }
                        case DotNetSolutionLexerContextKind.ProjectListings_Expect_ProjectIdGuid:
                        {
                            projectIdGuid = LexGuid(streamReaderWrap, stringBuilder, true);
                            context = DotNetSolutionLexerContextKind.ProjectListings_Expect_EndProjectKeyword;
                            break;
                        }
                        case DotNetSolutionLexerContextKind.GlobalSectionNestedProjects_Expect_ChildGuid:
                        case DotNetSolutionLexerContextKind.GlobalSectionNestedProjects_Expect_ParentGuid:
                        {
                            if (streamReaderWrap.CurrentCharacter == 'E')
                            {
                                var startKeywordPosition = streamReaderWrap.PositionIndex;
                            
                                if (TrySkipEndGlobalSectionText(streamReaderWrap))
                                {
                                    output.TextSpanList.Add(new TextEditorTextSpan(
                                        startKeywordPosition,
                                        streamReaderWrap.PositionIndex,
                                        (byte)GenericDecorationKind.Xml_TagNameNone));
                                
                                    context = DotNetSolutionLexerContextKind.Finish;
                                    continue;
                                }
                            }
                            break;
                        }
                    }
                    
                    if (!hasSeenGlobal)
                    {
                        if (streamReaderWrap.CurrentCharacter == 'G')
                        {
                            if (TrySkipGlobalText(streamReaderWrap))
                            {
                                hasSeenGlobal = true;
                                context = DotNetSolutionLexerContextKind.Global;
                                continue;
                            }
                        }
                    }
                    else if (!hasSeenGlobalSectionNestedProjects)
                    {
                        if (streamReaderWrap.CurrentCharacter == 'G')
                        {
                            var startKeywordPosition = streamReaderWrap.PositionIndex;
                            
                            if (TrySkipGlobalSectionNestedProjectsText(streamReaderWrap))
                            {
                                output.TextSpanList.Add(new TextEditorTextSpan(
                                    startKeywordPosition,
                                    streamReaderWrap.PositionIndex,
                                    (byte)GenericDecorationKind.Xml_TagNameNone));
                            
                                hasSeenGlobalSectionNestedProjects = true;
                                context = DotNetSolutionLexerContextKind.GlobalSectionNestedProjects_Expect_ChildGuid;
                                continue;
                            }
                        }
                    }

                    goto default;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    if (context == DotNetSolutionLexerContextKind.ProjectListings_Expect_ProjectTypeGuid)
                    {
                        projectTypeGuid = LexGuid(streamReaderWrap, stringBuilder, true);
                        context = DotNetSolutionLexerContextKind.ProjectListings_Expect_ProjectName;
                    }
                    else if (context == DotNetSolutionLexerContextKind.ProjectListings_Expect_ProjectIdGuid)
                    {
                        projectIdGuid = LexGuid(streamReaderWrap, stringBuilder, true);
                        context = DotNetSolutionLexerContextKind.ProjectListings_Expect_EndProjectKeyword;
                    }
                    goto default;
                case '\'':
                    goto default;
                case '"':
                    if (context == DotNetSolutionLexerContextKind.ProjectListings_Expect_ProjectName)
                    {
                        projectName = LexString(streamReaderWrap, stringBuilder);
                        context = DotNetSolutionLexerContextKind.ProjectListings_Expect_ProjectPath;
                    }
                    else if (context == DotNetSolutionLexerContextKind.ProjectListings_Expect_ProjectPath)
                    {
                        projectPath = LexString(streamReaderWrap, stringBuilder);
                        context = DotNetSolutionLexerContextKind.ProjectListings_Expect_ProjectIdGuid;
                    }
                    goto default;
                case '/':
                    if (streamReaderWrap.PeekCharacter(1) == '/')
                    {
                        goto default;
                    }
                    else if (streamReaderWrap.PeekCharacter(1) == '*')
                    {
                        goto default;
                    }
                    else
                    {
                        goto default;
                    }
                    break;
                case '+':
                    if (streamReaderWrap.PeekCharacter(1) == '+')
                    {
                        goto default;
                    }
                    else
                    {
                        goto default;
                    }
                    break;
                case '-':
                    if (streamReaderWrap.PeekCharacter(1) == '-')
                    {
                        goto default;
                    }
                    else
                    {
                        goto default;
                    }
                    break;
                case '=':
                    if (streamReaderWrap.PeekCharacter(1) == '=')
                    {
                        goto default;
                    }
                    else if (streamReaderWrap.PeekCharacter(1) == '>')
                    {
                        goto default;
                    }
                    else
                    {
                        goto default;
                    }
                    break;
                case '?':
                    if (streamReaderWrap.PeekCharacter(1) == '?')
                    {
                        goto default;
                    }
                    else
                    {
                        goto default;
                    }
                    break;
                case '|':
                    if (streamReaderWrap.PeekCharacter(1) == '|')
                    {
                        goto default;
                    }
                    else
                    {
                        goto default;
                    }
                case '&':
                    if (streamReaderWrap.PeekCharacter(1) == '&')
                    {
                        goto default;
                    }
                    else
                    {
                        goto default;
                    }
                case '*':
                {
                    goto default;
                }
                case '!':
                {
                    if (streamReaderWrap.PeekCharacter(1) == '=')
                    {
                        goto default;
                    }
                    else
                    {
                        goto default;
                    }
                }
                case ';':
                {
                    goto default;
                }
                case '(':
                {
                    goto default;
                }
                case ')':
                {
                    goto default;
                }
                case '{':
                {
                    if (context == DotNetSolutionLexerContextKind.GlobalSectionNestedProjects_Expect_ChildGuid)
                    {
                        _ = streamReaderWrap.ReadCharacter();
                        
                        // var startGuidPosition = streamReaderWrap.PositionIndex;
                            
                        childGuid = LexGuid(streamReaderWrap, stringBuilder);
                        
                        /*output.TextSpanList.Add(new TextEditorTextSpan(
                            startGuidPosition,
                            streamReaderWrap.PositionIndex,
                            (byte)XmlDecorationKind.AttributeName));*/
                        
                        context = DotNetSolutionLexerContextKind.GlobalSectionNestedProjects_Expect_ParentGuid;
                    }
                    else if (context == DotNetSolutionLexerContextKind.GlobalSectionNestedProjects_Expect_ParentGuid)
                    {
                        _ = streamReaderWrap.ReadCharacter();
                        
                        // var startGuidPosition = streamReaderWrap.PositionIndex;
                        
                        parentGuid = LexGuid(streamReaderWrap, stringBuilder);
                        
                        /*output.TextSpanList.Add(new TextEditorTextSpan(
                            startGuidPosition,
                            streamReaderWrap.PositionIndex,
                            (byte)XmlDecorationKind.AttributeValue));*/
                        
                        context = DotNetSolutionLexerContextKind.GlobalSectionNestedProjects_Expect_ChildGuid;
                        
                        output.GuidNestedProjectEntryList.Add(new GuidNestedProjectEntry(
                            childGuid,
                            parentGuid));
                    }
                    // TODO: Highlight the end section thing
                    
                    goto default;
                }
                case '}':
                {
                    /*if (interpolatedExpressionUnmatchedBraceCount != -1)
                    {
                        if (--interpolatedExpressionUnmatchedBraceCount <= 0)
                            goto forceExit;
                    }*/
                
                    goto default;
                }
                case '<':
                {
                    if (streamReaderWrap.PeekCharacter(1) == '=')
                    {
                        goto default;
                    }
                    else
                    {
                        goto default;
                    }
                }
                case '>':
                {
                    if (streamReaderWrap.PeekCharacter(1) == '=')
                    {
                        goto default;
                    }
                    else
                    {
                        goto default;
                    }
                }
                case '[':
                {
                    goto default;
                }
                case ']':
                {
                    goto default;
                }
                case '$':
                    if (streamReaderWrap.NextCharacter == '"')
                    {
                        goto default;
                    }
                    else if (streamReaderWrap.PeekCharacter(1) == '@' && streamReaderWrap.PeekCharacter(2) == '"')
                    {
                        goto default;
                    }
                    else if (streamReaderWrap.NextCharacter == '$')
                    {
                        /*var entryPositionIndex = streamReaderWrap.PositionIndex;
                        var byteEntryIndex = streamReaderWrap.ByteIndex;
    
                        // The while loop starts counting from and including the first dollar sign.
                        var countDollarSign = 0;
                    
                        while (!streamReaderWrap.IsEof)
                        {
                            if (streamReaderWrap.CurrentCharacter != '$')
                                break;
                            
                            ++countDollarSign;
                            _ = streamReaderWrap.ReadCharacter();
                        }*/
                        
                        goto default;
                        
                        /*if (streamReaderWrap.NextCharacter == '"')
                            LexString(binder, ref lexerOutput, streamReaderWrap, ref previousEscapeCharacterTextSpan, countDollarSign: countDollarSign, useVerbatim: false);*/
                    }
                    else
                    {
                        goto default;
                    }
                    break;
                case '@':
                    if (streamReaderWrap.NextCharacter == '"')
                    {
                        goto default;
                    }
                    else if (streamReaderWrap.PeekCharacter(1) == '$' && streamReaderWrap.PeekCharacter(2) == '"')
                    {
                        goto default;
                    }
                    else
                    {
                        goto default;
                    }
                case ':':
                {
                    goto default;
                }
                case '.':
                {
                    goto default;
                }
                case ',':
                {
                    goto default;
                }
                case '#':
                    goto default;
                default:
                    _ = streamReaderWrap.ReadCharacter();
                    break;
            }
        }

        forceExit:
        return output;
    }
    
    private static bool TrySkipProjectText(StreamReaderWrap streamReaderWrap)
    {
        // "Project".Length == 7
        for (int i = 0; i < 7; i++)
        {
            switch (i)
            {
                case 0:
                    if (streamReaderWrap.CurrentCharacter != 'P')
                        return false;
                    break;
                case 1:
                    if (streamReaderWrap.CurrentCharacter != 'r')
                        return false;
                    break;
                case 2:
                    if (streamReaderWrap.CurrentCharacter != 'o')
                        return false;
                    break;
                case 3:
                    if (streamReaderWrap.CurrentCharacter != 'j')
                        return false;
                    break;
                case 4:
                    if (streamReaderWrap.CurrentCharacter != 'e')
                        return false;
                    break;
                case 5:
                    if (streamReaderWrap.CurrentCharacter != 'c')
                        return false;
                    break;
                case 6:
                    if (streamReaderWrap.CurrentCharacter != 't')
                        return false;
                    break;
            }
            
            _ = streamReaderWrap.ReadCharacter();
        }
        
        return true;
    }
    
    private static bool TrySkipEndProjectText(StreamReaderWrap streamReaderWrap)
    {
        // "End".Length == 3
        for (int i = 0; i < 3; i++)
        {
            if (i == 0)
            {
                if (streamReaderWrap.CurrentCharacter != 'E')
                    return false;
            }
            else if (i == 1)
            {
                if (streamReaderWrap.CurrentCharacter != 'n')
                    return false;
            }
            else if (i == 2)
            {
                if (streamReaderWrap.CurrentCharacter != 'd')
                    return false;
            }

            _ = streamReaderWrap.ReadCharacter();
        }
        
        return TrySkipProjectText(streamReaderWrap);
    }
    
    private static bool TrySkipEndGlobalSectionText(StreamReaderWrap streamReaderWrap)
    {
        // "End".Length == 3
        for (int i = 0; i < 3; i++)
        {
            if (i == 0)
            {
                if (streamReaderWrap.CurrentCharacter != 'E')
                    return false;
            }
            else if (i == 1)
            {
                if (streamReaderWrap.CurrentCharacter != 'n')
                    return false;
            }
            else if (i == 2)
            {
                if (streamReaderWrap.CurrentCharacter != 'd')
                    return false;
            }

            _ = streamReaderWrap.ReadCharacter();
        }
        
        if (!TrySkipGlobalText(streamReaderWrap))
            return false;
            
        // "Section".Length == 7
        for (int i = 0; i < 7; i++)
        {
            switch (i)
            {
                case 0:
                    if (streamReaderWrap.CurrentCharacter != 'S')
                        return false;
                    break;
                case 1:
                    if (streamReaderWrap.CurrentCharacter != 'e')
                        return false;
                    break;
                case 2:
                    if (streamReaderWrap.CurrentCharacter != 'c')
                        return false;
                    break;
                case 3:
                    if (streamReaderWrap.CurrentCharacter != 't')
                        return false;
                    break;
                case 4:
                    if (streamReaderWrap.CurrentCharacter != 'i')
                        return false;
                    break;
                case 5:
                    if (streamReaderWrap.CurrentCharacter != 'o')
                        return false;
                    break;
                case 6:
                    if (streamReaderWrap.CurrentCharacter != 'n')
                        return false;
                    break;
            }

            _ = streamReaderWrap.ReadCharacter();
        }
        
        return true;
    }
    
    private static bool TrySkipGlobalText(StreamReaderWrap streamReaderWrap)
    {
        // "Global".Length == 6
        for (int i = 0; i < 6; i++)
        {
            switch (i)
            {
                case 0:
                    if (streamReaderWrap.CurrentCharacter != 'G')
                        return false;
                    break;
                case 1:
                    if (streamReaderWrap.CurrentCharacter != 'l')
                        return false;
                    break;
                case 2:
                    if (streamReaderWrap.CurrentCharacter != 'o')
                        return false;
                    break;
                case 3:
                    if (streamReaderWrap.CurrentCharacter != 'b')
                        return false;
                    break;
                case 4:
                    if (streamReaderWrap.CurrentCharacter != 'a')
                        return false;
                    break;
                case 5:
                    if (streamReaderWrap.CurrentCharacter != 'l')
                        return false;
                    break;
            }

            _ = streamReaderWrap.ReadCharacter();
        }
        
        return true;
    }
    
    private static bool TrySkipGlobalSectionNestedProjectsText(StreamReaderWrap streamReaderWrap)
    {
        if (!TrySkipGlobalText(streamReaderWrap))
            return false;
        
        // "Section".Length == 7
        for (int i = 0; i < 7; i++)
        {
            switch (i)
            {
                case 0:
                    if (streamReaderWrap.CurrentCharacter != 'S')
                        return false;
                    break;
                case 1:
                    if (streamReaderWrap.CurrentCharacter != 'e')
                        return false;
                    break;
                case 2:
                    if (streamReaderWrap.CurrentCharacter != 'c')
                        return false;
                    break;
                case 3:
                    if (streamReaderWrap.CurrentCharacter != 't')
                        return false;
                    break;
                case 4:
                    if (streamReaderWrap.CurrentCharacter != 'i')
                        return false;
                    break;
                case 5:
                    if (streamReaderWrap.CurrentCharacter != 'o')
                        return false;
                    break;
                case 6:
                    if (streamReaderWrap.CurrentCharacter != 'n')
                        return false;
                    break;
            }

            _ = streamReaderWrap.ReadCharacter();
        }
        
        if (streamReaderWrap.CurrentCharacter != '(')
        {
            return false;
        }
        else
        {
            _ = streamReaderWrap.ReadCharacter();
        }
        
        // "NestedProjects".Length == 14
        for (int i = 0; i < 14; i++)
        {
            switch (i)
            {
                case 0:
                    if (streamReaderWrap.CurrentCharacter != 'N')
                        return false;
                    break;
                case 1:
                    if (streamReaderWrap.CurrentCharacter != 'e')
                        return false;
                    break;
                case 2:
                    if (streamReaderWrap.CurrentCharacter != 's')
                        return false;
                    break;
                case 3:
                    if (streamReaderWrap.CurrentCharacter != 't')
                        return false;
                    break;
                case 4:
                    if (streamReaderWrap.CurrentCharacter != 'e')
                        return false;
                    break;
                case 5:
                    if (streamReaderWrap.CurrentCharacter != 'd')
                        return false;
                    break;
                case 6:
                    if (streamReaderWrap.CurrentCharacter != 'P')
                        return false;
                    break;
                case 7:
                    if (streamReaderWrap.CurrentCharacter != 'r')
                        return false;
                    break;
                case 8:
                    if (streamReaderWrap.CurrentCharacter != 'o')
                        return false;
                    break;
                case 9:
                    if (streamReaderWrap.CurrentCharacter != 'j')
                        return false;
                    break;
                case 10:
                    if (streamReaderWrap.CurrentCharacter != 'e')
                        return false;
                    break;
                case 11:
                    if (streamReaderWrap.CurrentCharacter != 'c')
                        return false;
                    break;
                case 12:
                    if (streamReaderWrap.CurrentCharacter != 't')
                        return false;
                    break;
                case 13:
                    if (streamReaderWrap.CurrentCharacter != 's')
                        return false;
                    break;
            }

            _ = streamReaderWrap.ReadCharacter();
        }
        
        if (streamReaderWrap.CurrentCharacter != ')')
        {
            return false;
        }
        else
        {
            _ = streamReaderWrap.ReadCharacter();
        }
        
        return true;
    }
    
    private static Guid LexGuid(StreamReaderWrap streamReaderWrap, StringBuilder stringBuilder, bool shouldFindDoubleQuote = false)
    {
        stringBuilder.Clear();
        
        // Guids when formatted with a hyphen are 36 characters long...
        // but it still feels more sensible to use the while loop instead of reading exactly 36 characters.
        while (!streamReaderWrap.IsEof)
        {
            if (streamReaderWrap.CurrentCharacter == ' ' ||
                streamReaderWrap.CurrentCharacter == '}')
            {
                break;
            }
            
            stringBuilder.Append(streamReaderWrap.CurrentCharacter);
            _ = streamReaderWrap.ReadCharacter();
        }
        
        // Double quote wrapping the guids is causing a false entry into the strings that get lexed.
        if (shouldFindDoubleQuote)
        {
            while (!streamReaderWrap.IsEof)
            {
                if (streamReaderWrap.CurrentCharacter == '"')
                    break;
                _ = streamReaderWrap.ReadCharacter();
            }
        }
        
        // TODO: Use the Guid.Parse(...) overload that takes a span
        return Guid.Parse(stringBuilder.ToString());
    }
    
    private static string LexString(StreamReaderWrap streamReaderWrap, StringBuilder stringBuilder)
    {
        _ = streamReaderWrap.ReadCharacter(); // Skip opening double quote
        
        stringBuilder.Clear();
        
        while (!streamReaderWrap.IsEof)
        {
            if (streamReaderWrap.CurrentCharacter == '"')
                break;
            
            stringBuilder.Append(streamReaderWrap.CurrentCharacter);
            _ = streamReaderWrap.ReadCharacter();
        }
        
        return stringBuilder.ToString();
    }
}
