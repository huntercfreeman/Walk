using System.Text;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Ide.RazorLib.CommandLines.Models;

namespace Walk.Extensions.DotNet.CommandLines.Models;

/// <summary>
/// Any values given will be wrapped in quotes internally at this step.
/// </summary>
public static class DotNetCliCommandFormatter
{
    public const string DOT_NET_CLI_TARGET_FILE_NAME = "dotnet";

    public static string FormatStartProjectWithoutDebugging(AbsolutePath projectAbsolutePath)
    {
        return FormatStartProjectWithoutDebugging(projectAbsolutePath.Value);
    }

    public static string FormatStartProjectWithoutDebugging(string projectPath)
    {
        var stringBuilder = new StringBuilder();
        
        stringBuilder.Append(DOT_NET_CLI_TARGET_FILE_NAME);
        stringBuilder.Append(" run");
        stringBuilder.Append(" --project \"");
        stringBuilder.Append(projectPath);
        stringBuilder.Append("\"");
        
        return stringBuilder.ToString();
    }

    public static string FormatDotnetNewSln(string solutionName)
    {
        var stringBuilder = new StringBuilder();
    
        stringBuilder.Append(DOT_NET_CLI_TARGET_FILE_NAME);
        stringBuilder.Append(" new");
        stringBuilder.Append(" sln");
        stringBuilder.Append(" -o \"");
        stringBuilder.Append(solutionName);
        stringBuilder.Append("\"");
        
        return stringBuilder.ToString();
    }

    public static string FormatDotnetNewCSharpProject(
        string projectTemplateName,
        string cSharpProjectName,
        string manuallyDoubleQuotedOptionalParameters)
    {
        var stringBuilder = new StringBuilder();
        
        stringBuilder.Append(DOT_NET_CLI_TARGET_FILE_NAME);
        stringBuilder.Append(" new \"");
        stringBuilder.Append(projectTemplateName);
        stringBuilder.Append("\" -o \"");
        stringBuilder.Append(cSharpProjectName);
        stringBuilder.Append("\"");
    
        if (!string.IsNullOrWhiteSpace(manuallyDoubleQuotedOptionalParameters))
            stringBuilder.Append(manuallyDoubleQuotedOptionalParameters);

        return stringBuilder.ToString();
    }

    public static string FormatAddExistingProjectToSolution(
        string solutionAbsolutePathString,
        string cSharpProjectPath)
    {
        var stringBuilder = new StringBuilder();
        
        stringBuilder.Append(DOT_NET_CLI_TARGET_FILE_NAME);
        stringBuilder.Append(" sln \"");
        stringBuilder.Append(solutionAbsolutePathString);
        stringBuilder.Append("\" add \"");
        stringBuilder.Append(cSharpProjectPath);
        stringBuilder.Append("\"");
        
        return stringBuilder.ToString();
    }

    public static string FormatRemoveCSharpProjectReferenceFromSolutionAction(
        string solutionAbsolutePathString,
        string cSharpProjectAbsolutePathString)
    {
        var stringBuilder = new StringBuilder();
        
        stringBuilder.Append(DOT_NET_CLI_TARGET_FILE_NAME);
        stringBuilder.Append(" sln \"");
        stringBuilder.Append(solutionAbsolutePathString);
        stringBuilder.Append("\" remove \"");
        stringBuilder.Append(cSharpProjectAbsolutePathString);
        stringBuilder.Append("\"");
        
        return stringBuilder.ToString();
    }

    public static string FormatAddNugetPackageReferenceToProject(
        string cSharpProjectAbsolutePathString,
        string nugetPackageId,
        string nugetPackageVersion)
    {
        var stringBuilder = new StringBuilder();
        
        stringBuilder.Append(DOT_NET_CLI_TARGET_FILE_NAME);
        stringBuilder.Append(" add \"");
        stringBuilder.Append(cSharpProjectAbsolutePathString);
        stringBuilder.Append("\" package \"");
        stringBuilder.Append(nugetPackageId);
        stringBuilder.Append("\" --version \"");
        stringBuilder.Append(nugetPackageVersion);
        stringBuilder.Append("\"");
        
        return stringBuilder.ToString();
    }

    public static string FormatRemoveNugetPackageReferenceFromProject(
        string cSharpProjectAbsolutePathString,
        string nugetPackageId)
    {
        var stringBuilder = new StringBuilder();
        
        stringBuilder.Append(DOT_NET_CLI_TARGET_FILE_NAME);
        stringBuilder.Append(" remove \"");
        stringBuilder.Append(cSharpProjectAbsolutePathString);
        stringBuilder.Append("\" package \"");
        stringBuilder.Append(nugetPackageId);
        stringBuilder.Append("\"");
        
        return stringBuilder.ToString();
    }

    public static string FormatAddProjectToProjectReference(
        string receivingProjectAbsolutePathString,
        string referenceProjectAbsolutePathString)
    {
        var stringBuilder = new StringBuilder();
        
        stringBuilder.Append(DOT_NET_CLI_TARGET_FILE_NAME);
        stringBuilder.Append(" add \"");
        stringBuilder.Append(receivingProjectAbsolutePathString);
        stringBuilder.Append("\" reference \"");
        stringBuilder.Append(referenceProjectAbsolutePathString);
        stringBuilder.Append("\"");
    
        return stringBuilder.ToString();
    }

    public static string FormatRemoveProjectToProjectReference(
        string modifyProjectAbsolutePathString,
        string referenceProjectAbsolutePathString)
    {
        var stringBuilder = new StringBuilder();
    
        stringBuilder.Append(DOT_NET_CLI_TARGET_FILE_NAME);
        stringBuilder.Append(" remove \"");
        stringBuilder.Append(modifyProjectAbsolutePathString);
        stringBuilder.Append("\" reference \"");
        stringBuilder.Append(referenceProjectAbsolutePathString);
        stringBuilder.Append("\"");
    
        return stringBuilder.ToString();
    }

    public static string FormatMoveProjectToSolutionFolder(
        string solutionAbsolutePathString,
        string projectToMoveAbsolutePathString,
        string solutionFolderPath)
    {
        var stringBuilder = new StringBuilder();
        
        stringBuilder.Append(DOT_NET_CLI_TARGET_FILE_NAME);
        stringBuilder.Append(" sln \"");
        stringBuilder.Append(solutionAbsolutePathString);
        stringBuilder.Append("\" add \"");
        stringBuilder.Append(projectToMoveAbsolutePathString);
        stringBuilder.Append("\" --solution-folder \"");
        stringBuilder.Append(solutionFolderPath);
        stringBuilder.Append("\"");
        
        return stringBuilder.ToString();
    }

    public static string FormatDotnetNewList()
    {
        var stringBuilder = new StringBuilder();
        
        stringBuilder.Append(DOT_NET_CLI_TARGET_FILE_NAME);
        stringBuilder.Append(" new");
        stringBuilder.Append(" list");
        
        return stringBuilder.ToString();
    }
    
    public static string FormatDotnetNewListDeprecated()
    {
        var stringBuilder = new StringBuilder();
        
        stringBuilder.Append(DOT_NET_CLI_TARGET_FILE_NAME);
        stringBuilder.Append(" new");
        stringBuilder.Append(" --list");
        
        return stringBuilder.ToString();
    }
    
    public static string FormatDotNetTestListTests()
    {
        var stringBuilder = new StringBuilder();
        
        stringBuilder.Append(DOT_NET_CLI_TARGET_FILE_NAME);
        stringBuilder.Append(" test");
        stringBuilder.Append(" -t");
        
        return stringBuilder.ToString();
    }
    
    public static string FormatDotNetTestByFullyQualifiedName(string fullyQualifiedName)
    {
        var stringBuilder = new StringBuilder();
        
        stringBuilder.Append(DOT_NET_CLI_TARGET_FILE_NAME);
        stringBuilder.Append(" test");
        stringBuilder.Append(" --filter");
        stringBuilder.Append(" FullyQualifiedName=\"");
        stringBuilder.Append(fullyQualifiedName);
        stringBuilder.Append("\"");
        
        return stringBuilder.ToString();
    }
    
    public static string FormatDotnetBuildProject(string projectAbsolutePathString)
    {
        var stringBuilder = new StringBuilder();
        
        stringBuilder.Append(DOT_NET_CLI_TARGET_FILE_NAME);
        stringBuilder.Append(" build \"");
        stringBuilder.Append(projectAbsolutePathString);
        stringBuilder.Append("\"");
        
        return stringBuilder.ToString();
    }
    
    public static string FormatDotnetCleanProject(string projectAbsolutePathString)
    {
        var stringBuilder = new StringBuilder();
        
        stringBuilder.Append(DOT_NET_CLI_TARGET_FILE_NAME);
        stringBuilder.Append(" clean \"");
        stringBuilder.Append(projectAbsolutePathString);
        stringBuilder.Append("\"");
        
        return stringBuilder.ToString();
    }
    
    public static string FormatDotnetBuildSolution(string solutionAbsolutePathString)
    {
        var stringBuilder = new StringBuilder();
        
        stringBuilder.Append(DOT_NET_CLI_TARGET_FILE_NAME);
        stringBuilder.Append(" build \"");
        stringBuilder.Append(solutionAbsolutePathString);
        stringBuilder.Append("\"");
        
        return stringBuilder.ToString();
    }
    
    public static string FormatDotnetCleanSolution(string solutionAbsolutePathString)
    {
        var stringBuilder = new StringBuilder();
        
        stringBuilder.Append(DOT_NET_CLI_TARGET_FILE_NAME);
        stringBuilder.Append(" clean \"");
        stringBuilder.Append(solutionAbsolutePathString);
        stringBuilder.Append("\"");
        
        return stringBuilder.ToString();
    }
}
