using System.Text;
using Walk.Common.RazorLib;
using Walk.Extensions.DotNet;
using Walk.Ide.Wasm.Facts;

namespace Walk.Website.RazorLib.Websites.ProjectTemplates.Models;

public static class WebsiteProjectTemplateFacts
{
    static WebsiteProjectTemplateFacts()
    {
        ConsoleAppProjectTemplate = new ProjectTemplate(
            "Console App",
            "console",
            "[C#],F#,VB",
            "Common/Console");

        WebsiteProjectTemplatesContainer = new List<ProjectTemplate>
        {
            ConsoleAppProjectTemplate,
        };
    }

    public static ProjectTemplate ConsoleAppProjectTemplate { get; }

    public static List<ProjectTemplate> WebsiteProjectTemplatesContainer { get; }

    public static async Task HandleNewCSharpProjectAsync(
        string projectTemplateShortName,
        string cSharpProjectAbsolutePathString,
        CommonService commonService)
    {
        if (projectTemplateShortName == ConsoleAppProjectTemplate.ShortName)
            await HandleConsoleAppProjectTemplateAsync(cSharpProjectAbsolutePathString, commonService)
                .ConfigureAwait(false);
        else
            throw new NotImplementedException($"The {nameof(ProjectTemplate.ShortName)}: '{projectTemplateShortName}' was not recognized.");
    }

    private static async Task HandleConsoleAppProjectTemplateAsync(
        string cSharpProjectAbsolutePathString,
        CommonService commonService)
    {
        var cSharpProjectAbsolutePath = commonService.EnvironmentProvider.AbsolutePathFactory(cSharpProjectAbsolutePathString, false, tokenBuilder: new StringBuilder(), formattedBuilder: new StringBuilder());
        var parentDirectoryOfProject = cSharpProjectAbsolutePath.ParentDirectory;

        if (parentDirectoryOfProject is null)
            throw new NotImplementedException();

        var ancestorDirectory = parentDirectoryOfProject;

        // ProgramCs
        {
            var absolutePath = commonService.EnvironmentProvider.JoinPaths(
                ancestorDirectory,
                ConsoleAppFacts.PROGRAM_CS_RELATIVE_FILE_PATH);

            await commonService.FileSystemProvider.File.WriteAllTextAsync(
                    absolutePath,
                    InitialSolutionFacts.PERSON_CS_CONTENTS)
                .ConfigureAwait(false);
        }

        // Csproj
        {
            await commonService.FileSystemProvider.File.WriteAllTextAsync(
                    cSharpProjectAbsolutePathString,
                    ConsoleAppFacts.GetCsprojContents(cSharpProjectAbsolutePath.NameNoExtension))
                .ConfigureAwait(false);
        }
    }

    public static Guid GetProjectTypeGuid(string projectTemplateShortName)
    {
        // I'm not going to DRY up the string "FAE04EC0-301F-11D3-BF4B-00C04F79EFBC" for now,
        // because I don't fully understand its purpose.

        if (projectTemplateShortName == ConsoleAppProjectTemplate.ShortName)
            return Guid.Parse("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC");
        else
            throw new NotImplementedException($"The {nameof(ProjectTemplate.ShortName)}: '{projectTemplateShortName}' was not recognized.");
    }
}
