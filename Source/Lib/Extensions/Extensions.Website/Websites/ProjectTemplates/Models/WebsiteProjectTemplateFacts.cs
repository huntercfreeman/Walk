using Walk.Extensions.DotNet;

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
