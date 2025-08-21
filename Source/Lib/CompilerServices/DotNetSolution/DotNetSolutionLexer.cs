using System.Text;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.CompilerServices.DotNetSolution.Models.Project;
using Walk.CompilerServices.Xml;

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
        
        Guid projectTypeGuid = default;
        string projectName = default;
        string projectPath = default;
        Guid projectIdGuid = default;
        
        var hasSeenGlobal = false;
        
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
                    if (context == DotNetSolutionLexerContextKind.Header)
                    {
                        if (streamReaderWrap.CurrentCharacter == 'P')
                        {
                            var startKeywordPosition = streamReaderWrap.PositionIndex;
                            
                            if (TrySkipProjectText(streamReaderWrap, output.TextSpanList))
                            {
                                output.TextSpanList.Add(new TextEditorTextSpan(
                                    startKeywordPosition,
                                    streamReaderWrap.PositionIndex,
                                    (byte)XmlDecorationKind.TagNameNone));
                                
                                context = DotNetSolutionLexerContextKind.ProjectListings_Expect_ProjectTypeGuid;
                                continue;
                            }
                        }
                    }
                    else if (context == DotNetSolutionLexerContextKind.ProjectListings_Expect_ProjectKeyword)
                    {
                        if (streamReaderWrap.CurrentCharacter == 'P')
                        {
                            var startKeywordPosition = streamReaderWrap.PositionIndex;
                                
                            if (TrySkipProjectText(streamReaderWrap, output.TextSpanList))
                            {
                                output.TextSpanList.Add(new TextEditorTextSpan(
                                    startKeywordPosition,
                                    streamReaderWrap.PositionIndex,
                                    (byte)XmlDecorationKind.TagNameNone));
                                    
                                context = DotNetSolutionLexerContextKind.ProjectListings_Expect_ProjectTypeGuid;
                                continue;
                            }
                        }
                    }
                    else if (context == DotNetSolutionLexerContextKind.ProjectListings_Expect_EndProjectKeyword)
                    {
                        if (streamReaderWrap.CurrentCharacter == 'E')
                        {
                            if (TrySkipEndProjectText(streamReaderWrap, output.TextSpanList))
                            {
                                context = DotNetSolutionLexerContextKind.ProjectListings_Expect_ProjectKeyword;
                                output.DotNetProjectList.Add(new CSharpProjectModel(
                                    projectName,
                                    projectTypeGuid,
                                    projectPath,
                                    projectIdGuid,
                                    absolutePath: default));
                                continue;
                            }
                        }
                    }
                    else if (context == DotNetSolutionLexerContextKind.ProjectListings_Expect_ProjectTypeGuid)
                    {
                        projectTypeGuid = LexGuid(streamReaderWrap, stringBuilder);
                        context = DotNetSolutionLexerContextKind.ProjectListings_Expect_ProjectName;
                    }
                    else if (context == DotNetSolutionLexerContextKind.ProjectListings_Expect_ProjectIdGuid)
                    {
                        projectIdGuid = LexGuid(streamReaderWrap, stringBuilder);
                        context = DotNetSolutionLexerContextKind.ProjectListings_Expect_EndProjectKeyword;
                    }
                    
                    if (!hasSeenGlobal && streamReaderWrap.CurrentCharacter == 'G')
                    {
                        if (TrySkipGlobalText(streamReaderWrap, output.TextSpanList))
                        {
                            hasSeenGlobal = true;
                            context = DotNetSolutionLexerContextKind.Global;
                            continue;
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
                        projectTypeGuid = LexGuid(streamReaderWrap, stringBuilder);
                        context = DotNetSolutionLexerContextKind.ProjectListings_Expect_ProjectName;
                    }
                    else if (context == DotNetSolutionLexerContextKind.ProjectListings_Expect_ProjectIdGuid)
                    {
                        projectIdGuid = LexGuid(streamReaderWrap, stringBuilder);
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
                    /*if (interpolatedExpressionUnmatchedBraceCount != -1)
                        ++interpolatedExpressionUnmatchedBraceCount;*/
                
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
    
    private static bool TrySkipProjectText(StreamReaderWrap streamReaderWrap, List<TextEditorTextSpan> textSpanList)
    {
        // "Project".Length == 7
        for (int i = 0; i < 7; i++)
        {
            if (i == 0 && streamReaderWrap.CurrentCharacter != 'P' ||
                i == 1 && streamReaderWrap.CurrentCharacter != 'r' ||
                i == 2 && streamReaderWrap.CurrentCharacter != 'o' ||
                i == 3 && streamReaderWrap.CurrentCharacter != 'j' ||
                i == 4 && streamReaderWrap.CurrentCharacter != 'e' ||
                i == 5 && streamReaderWrap.CurrentCharacter != 'c' ||
                i == 6 && streamReaderWrap.CurrentCharacter != 't')
            {
                return false;
            }
            
            _ = streamReaderWrap.ReadCharacter();
            
            if (i == 6)
                return true;
        }
        
        return false;
    }
    
    private static bool TrySkipEndProjectText(StreamReaderWrap streamReaderWrap, List<TextEditorTextSpan> textSpanList)
    {
        // "EndProject".Length == 10
        for (int i = 0; i < 10; i++)
        {
            if (i == 0 && streamReaderWrap.CurrentCharacter != 'E' ||
                i == 1 && streamReaderWrap.CurrentCharacter != 'n' ||
                i == 2 && streamReaderWrap.CurrentCharacter != 'd' ||
                i == 3 && streamReaderWrap.CurrentCharacter != 'P' ||
                i == 4 && streamReaderWrap.CurrentCharacter != 'r' ||
                i == 5 && streamReaderWrap.CurrentCharacter != 'o' ||
                i == 6 && streamReaderWrap.CurrentCharacter != 'j' ||
                i == 7 && streamReaderWrap.CurrentCharacter != 'e' ||
                i == 8 && streamReaderWrap.CurrentCharacter != 'c' ||
                i == 9 && streamReaderWrap.CurrentCharacter != 't')
            {
                return false;
            }

            _ = streamReaderWrap.ReadCharacter();
            
            if (i == 9)
                return true;
        }
        
        return false;
    }
    
    private static bool TrySkipGlobalText(StreamReaderWrap streamReaderWrap, List<TextEditorTextSpan> textSpanList)
    {
        // "Global".Length == 6
        for (int i = 0; i < 6; i++)
        {
            if (i == 0 && streamReaderWrap.CurrentCharacter != 'G' ||
                i == 1 && streamReaderWrap.CurrentCharacter != 'l' ||
                i == 2 && streamReaderWrap.CurrentCharacter != 'o' ||
                i == 3 && streamReaderWrap.CurrentCharacter != 'b' ||
                i == 4 && streamReaderWrap.CurrentCharacter != 'a' ||
                i == 5 && streamReaderWrap.CurrentCharacter != 'l')
            {
                return false;
            }

            _ = streamReaderWrap.ReadCharacter();
            
            if (i == 5)
                return true;
        }
        
        return false;
    }
    
    private static Guid LexGuid(StreamReaderWrap streamReaderWrap, StringBuilder stringBuilder)
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
        
        // Double quote wrapping the guids is causing a false
        // entry into the strings that get lexed.
        while (!streamReaderWrap.IsEof)
        {
            if (streamReaderWrap.CurrentCharacter == '"')
                break;
            _ = streamReaderWrap.ReadCharacter();
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
