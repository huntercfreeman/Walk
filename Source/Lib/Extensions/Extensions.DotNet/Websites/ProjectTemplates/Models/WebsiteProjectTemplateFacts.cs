using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Options.Models;

namespace Walk.Extensions.DotNet.Websites.ProjectTemplates.Models;

public static class WebsiteProjectTemplateFacts
{
	static WebsiteProjectTemplateFacts()
	{
		BlazorWasmEmptyProjectTemplate = new ProjectTemplate(
			"Blazor WebAssembly App Empty",
			"blazorwasm-empty",
			"[C#]",
			"Web/Blazor/WebAssembly/PWA/Empty");

		ConsoleAppProjectTemplate = new ProjectTemplate(
			"Console App",
			"console",
			"[C#],F#,VB",
			"Common/Console");

		WebsiteProjectTemplatesContainer = new List<ProjectTemplate>
		{
			BlazorWasmEmptyProjectTemplate,
			ConsoleAppProjectTemplate,
		};
	}

	public static ProjectTemplate BlazorWasmEmptyProjectTemplate { get; }
	public static ProjectTemplate ConsoleAppProjectTemplate { get; }

	public static List<ProjectTemplate> WebsiteProjectTemplatesContainer { get; }

	public static async Task HandleNewCSharpProjectAsync(
		string projectTemplateShortName,
		string cSharpProjectAbsolutePathString,
		CommonUtilityService commonUtilityService)
	{
		if (projectTemplateShortName == BlazorWasmEmptyProjectTemplate.ShortName)
			await HandleBlazorWasmEmptyProjectTemplateAsync(cSharpProjectAbsolutePathString, commonUtilityService)
				.ConfigureAwait(false);
		else if (projectTemplateShortName == ConsoleAppProjectTemplate.ShortName)
			await HandleConsoleAppProjectTemplateAsync(cSharpProjectAbsolutePathString, commonUtilityService)
				.ConfigureAwait(false);
		else
			throw new NotImplementedException($"The {nameof(ProjectTemplate.ShortName)}: '{projectTemplateShortName}' was not recognized.");
	}

	private static async Task HandleBlazorWasmEmptyProjectTemplateAsync(
		string cSharpProjectAbsolutePathString,
		CommonUtilityService commonUtilityService)
	{
		var cSharpProjectAbsolutePath = commonUtilityService.EnvironmentProvider.AbsolutePathFactory(cSharpProjectAbsolutePathString, false);
		var parentDirectoryOfProject = cSharpProjectAbsolutePath.ParentDirectory;

		if (parentDirectoryOfProject is null)
			throw new NotImplementedException();

		var parentDirectoryOfProjectAbsolutePath = parentDirectoryOfProject;

		// AppCss
		{
			var absolutePath = commonUtilityService.EnvironmentProvider.JoinPaths(
				parentDirectoryOfProjectAbsolutePath,
				BlazorWasmEmptyFacts.APP_CSS_RELATIVE_FILE_PATH);

			await commonUtilityService.FileSystemProvider.File.WriteAllTextAsync(
					absolutePath,
					BlazorWasmEmptyFacts.GetAppCssContents(cSharpProjectAbsolutePath.NameNoExtension))
				.ConfigureAwait(false);
		}

		// Csproj
		{
			await commonUtilityService.FileSystemProvider.File.WriteAllTextAsync(
					cSharpProjectAbsolutePathString,
					BlazorWasmEmptyFacts.GetCsprojContents(cSharpProjectAbsolutePath.NameNoExtension))
				.ConfigureAwait(false);
		}

		// ImportsRazor
		{
			var absolutePath = commonUtilityService.EnvironmentProvider.JoinPaths(
				parentDirectoryOfProjectAbsolutePath,
				BlazorWasmEmptyFacts.IMPORTS_RAZOR_RELATIVE_FILE_PATH);

			await commonUtilityService.FileSystemProvider.File.WriteAllTextAsync(
					absolutePath,
					BlazorWasmEmptyFacts.GetImportsRazorContents(cSharpProjectAbsolutePath.NameNoExtension))
				.ConfigureAwait(false);
		}

		// IndexHtml
		{
			var absolutePath = commonUtilityService.EnvironmentProvider.JoinPaths(
				parentDirectoryOfProjectAbsolutePath,
				BlazorWasmEmptyFacts.INDEX_HTML_RELATIVE_FILE_PATH);

			await commonUtilityService.FileSystemProvider.File.WriteAllTextAsync(
					absolutePath,
					BlazorWasmEmptyFacts.GetIndexHtmlContents(cSharpProjectAbsolutePath.NameNoExtension))
				.ConfigureAwait(false);
		}

		// IndexRazor
		{
			var absolutePath = commonUtilityService.EnvironmentProvider.JoinPaths(
				parentDirectoryOfProjectAbsolutePath,
				BlazorWasmEmptyFacts.INDEX_RAZOR_RELATIVE_FILE_PATH);

			await commonUtilityService.FileSystemProvider.File.WriteAllTextAsync(
					absolutePath,
					BlazorWasmEmptyFacts.GetIndexRazorContents(cSharpProjectAbsolutePath.NameNoExtension))
				.ConfigureAwait(false);
		}

		// LaunchSettingsJson
		{
			var absolutePath = commonUtilityService.EnvironmentProvider.JoinPaths(
				parentDirectoryOfProjectAbsolutePath,
				BlazorWasmEmptyFacts.LAUNCH_SETTINGS_JSON_RELATIVE_FILE_PATH);

			await commonUtilityService.FileSystemProvider.File.WriteAllTextAsync(
					absolutePath,
					BlazorWasmEmptyFacts.GetLaunchSettingsJsonContents(cSharpProjectAbsolutePath.NameNoExtension))
				.ConfigureAwait(false);
		}

		// MainLayoutRazor
		{
			var absolutePath = commonUtilityService.EnvironmentProvider.JoinPaths(
				parentDirectoryOfProjectAbsolutePath,
				BlazorWasmEmptyFacts.MAIN_LAYOUT_RAZOR_RELATIVE_FILE_PATH);

			await commonUtilityService.FileSystemProvider.File.WriteAllTextAsync(
					absolutePath,
					BlazorWasmEmptyFacts.GetMainLayoutRazorContents(cSharpProjectAbsolutePath.NameNoExtension))
				.ConfigureAwait(false);
		}

		// ProgramCs
		{
			var absolutePath = commonUtilityService.EnvironmentProvider.JoinPaths(
				parentDirectoryOfProjectAbsolutePath,
				BlazorWasmEmptyFacts.PROGRAM_CS_RELATIVE_FILE_PATH);

			await commonUtilityService.FileSystemProvider.File.WriteAllTextAsync(
					absolutePath,
					BlazorWasmEmptyFacts.GetProgramCsContents(cSharpProjectAbsolutePath.NameNoExtension))
				.ConfigureAwait(false);
		}
	}
	
	private static async Task HandleConsoleAppProjectTemplateAsync(
		string cSharpProjectAbsolutePathString,
		CommonUtilityService commonUtilityService)
	{
		var cSharpProjectAbsolutePath = commonUtilityService.EnvironmentProvider.AbsolutePathFactory(cSharpProjectAbsolutePathString, false);
		var parentDirectoryOfProject = cSharpProjectAbsolutePath.ParentDirectory;

		if (parentDirectoryOfProject is null)
			throw new NotImplementedException();

		var ancestorDirectory = parentDirectoryOfProject;

		// ProgramCs
		{
			var absolutePath = commonUtilityService.EnvironmentProvider.JoinPaths(
				ancestorDirectory,
				ConsoleAppFacts.PROGRAM_CS_RELATIVE_FILE_PATH);

			await commonUtilityService.FileSystemProvider.File.WriteAllTextAsync(
					absolutePath,
					ConsoleAppFacts.GetProgramCsContents(cSharpProjectAbsolutePath.NameNoExtension))
				.ConfigureAwait(false);
		}

		// Csproj
		{
			await commonUtilityService.FileSystemProvider.File.WriteAllTextAsync(
					cSharpProjectAbsolutePathString,
					ConsoleAppFacts.GetCsprojContents(cSharpProjectAbsolutePath.NameNoExtension))
				.ConfigureAwait(false);
		}
	}

	public static Guid GetProjectTypeGuid(string projectTemplateShortName)
	{
		// I'm not going to DRY up the string "FAE04EC0-301F-11D3-BF4B-00C04F79EFBC" for now,
		// because I don't fully understand its purpose.

		if (projectTemplateShortName == BlazorWasmEmptyProjectTemplate.ShortName)
			return Guid.Parse("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC");
		else if (projectTemplateShortName == ConsoleAppProjectTemplate.ShortName)
			return Guid.Parse("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC");
		else
			throw new NotImplementedException($"The {nameof(ProjectTemplate.ShortName)}: '{projectTemplateShortName}' was not recognized.");
	}
}
