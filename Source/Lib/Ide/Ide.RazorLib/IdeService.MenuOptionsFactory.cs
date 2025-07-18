﻿using Walk.Common.RazorLib;
using Walk.Common.RazorLib.FileSystems.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Namespaces.Models;
using Walk.Common.RazorLib.Notifications.Models;
using Walk.Ide.RazorLib.BackgroundTasks.Models;
using Walk.Ide.RazorLib.Clipboards.Models;
using Walk.Ide.RazorLib.ComponentRenderers.Models;
using Walk.Ide.RazorLib.FileSystems.Models;

namespace Walk.Ide.RazorLib;

public partial class IdeService
{
	public MenuOptionRecord NewEmptyFile(AbsolutePath parentDirectory, Func<Task> onAfterCompletion)
	{
		return new MenuOptionRecord("New Empty File", MenuOptionKind.Create,
			widgetRendererType: IdeComponentRenderers.FileFormRendererType,
			widgetParameterMap: new Dictionary<string, object?>
			{
				{ nameof(IFileFormRendererType.FileName), string.Empty },
				{ nameof(IFileFormRendererType.CheckForTemplates), false },
				{
					nameof(IFileFormRendererType.OnAfterSubmitFunc),
					new Func<string, IFileTemplate?, List<IFileTemplate>, Task>(
						(fileName, exactMatchFileTemplate, relatedMatchFileTemplates) =>
						{
							Enqueue_PerformNewFile(
								fileName,
								exactMatchFileTemplate,
								relatedMatchFileTemplates,
								new NamespacePath(string.Empty, parentDirectory),
								onAfterCompletion);

							return Task.CompletedTask;
						})
				},
			});
	}

	public MenuOptionRecord NewTemplatedFile(NamespacePath parentDirectory, Func<Task> onAfterCompletion)
	{
		return new MenuOptionRecord("New Templated File", MenuOptionKind.Create,
			widgetRendererType: IdeComponentRenderers.FileFormRendererType,
			widgetParameterMap: new Dictionary<string, object?>
			{
				{ nameof(IFileFormRendererType.FileName), string.Empty },
				{ nameof(IFileFormRendererType.CheckForTemplates), true },
				{
					nameof(IFileFormRendererType.OnAfterSubmitFunc),
					new Func<string, IFileTemplate?, List<IFileTemplate>, Task>(
						(fileName, exactMatchFileTemplate, relatedMatchFileTemplates) =>
						{
							Enqueue_PerformNewFile(
								fileName,
								exactMatchFileTemplate,
								relatedMatchFileTemplates,
								parentDirectory,
								onAfterCompletion);

							return Task.CompletedTask;
						})
				},
			});
	}

	public MenuOptionRecord NewDirectory(AbsolutePath parentDirectory, Func<Task> onAfterCompletion)
	{
		return new MenuOptionRecord("New Directory", MenuOptionKind.Create,
			widgetRendererType: IdeComponentRenderers.FileFormRendererType,
			widgetParameterMap: new Dictionary<string, object?>
			{
				{ nameof(IFileFormRendererType.FileName), string.Empty },
				{ nameof(IFileFormRendererType.IsDirectory), true },
				{
					nameof(IFileFormRendererType.OnAfterSubmitFunc),
					new Func<string, IFileTemplate?, List<IFileTemplate>, Task>(
						(directoryName, _, _) =>
						{
							Enqueue_PerformNewDirectory(directoryName, parentDirectory, onAfterCompletion);
							return Task.CompletedTask;
						})
				},
			});
	}

	public MenuOptionRecord DeleteFile(AbsolutePath absolutePath, Func<Task> onAfterCompletion)
	{
		return new MenuOptionRecord("Delete", MenuOptionKind.Delete,
			widgetRendererType: IdeComponentRenderers.DeleteFileFormRendererType,
			widgetParameterMap: new Dictionary<string, object?>
			{
				{ nameof(IDeleteFileFormRendererType.AbsolutePath), absolutePath },
				{ nameof(IDeleteFileFormRendererType.IsDirectory), true },
				{
					nameof(IDeleteFileFormRendererType.OnAfterSubmitFunc),
					new Func<AbsolutePath, Task>(
						x =>
						{
							Enqueue_PerformDeleteFile(x, onAfterCompletion);
							return Task.CompletedTask;
						})
				},
			});
	}

	public MenuOptionRecord RenameFile(AbsolutePath sourceAbsolutePath, CommonService commonService, Func<Task> onAfterCompletion)
	{
		return new MenuOptionRecord("Rename", MenuOptionKind.Update,
			widgetRendererType: IdeComponentRenderers.FileFormRendererType,
			widgetParameterMap: new Dictionary<string, object?>
			{
				{
					nameof(IFileFormRendererType.FileName),
					sourceAbsolutePath.IsDirectory
						? sourceAbsolutePath.NameNoExtension
						: sourceAbsolutePath.NameWithExtension
				},
				{ nameof(IFileFormRendererType.IsDirectory), sourceAbsolutePath.IsDirectory },
				{
					nameof(IFileFormRendererType.OnAfterSubmitFunc),
					new Func<string, IFileTemplate?, List<IFileTemplate>, Task>((nextName, _, _) =>
					{
						PerformRename(sourceAbsolutePath, nextName, commonService, onAfterCompletion);
						return Task.CompletedTask;
					})
				},
			});
	}

	public MenuOptionRecord CopyFile(AbsolutePath absolutePath, Func<Task> onAfterCompletion)
	{
		return new MenuOptionRecord("Copy", MenuOptionKind.Update,
			onClickFunc: () =>
			{
				Enqueue_PerformCopyFile(absolutePath, onAfterCompletion);
				return Task.CompletedTask;
			});
	}

	public MenuOptionRecord CutFile(AbsolutePath absolutePath, Func<Task> onAfterCompletion)
	{
		return new MenuOptionRecord("Cut", MenuOptionKind.Update,
			onClickFunc: () =>
			{
				Enqueue_PerformCutFile(absolutePath, onAfterCompletion);
				return Task.CompletedTask;
			});
	}

	public MenuOptionRecord PasteClipboard(AbsolutePath directoryAbsolutePath, Func<Task> onAfterCompletion)
	{
		return new MenuOptionRecord("Paste", MenuOptionKind.Update,
			onClickFunc: () =>
			{
				Enqueue_PerformPasteFile(directoryAbsolutePath, onAfterCompletion);
				return Task.CompletedTask;
			});
	}

	private void Enqueue_PerformNewFile(
		string fileName,
		IFileTemplate? exactMatchFileTemplate,
		List<IFileTemplate> relatedMatchFileTemplatesList,
		NamespacePath namespacePath,
		Func<Task> onAfterCompletion)
	{
		Enqueue(new IdeWorkArgs
		{
			WorkKind = IdeWorkKind.PerformNewFile,
			StringValue = fileName,
			ExactMatchFileTemplate = exactMatchFileTemplate,
			RelatedMatchFileTemplatesList = relatedMatchFileTemplatesList,
			NamespacePath = namespacePath,
			OnAfterCompletion = onAfterCompletion,
		});
	}

	private async ValueTask Do_PerformNewFile(
		string fileName,
		IFileTemplate? exactMatchFileTemplate,
		List<IFileTemplate> relatedMatchFileTemplatesList,
		NamespacePath namespacePath,
		Func<Task> onAfterCompletion)
	{
		if (exactMatchFileTemplate is null)
		{
			var emptyFileAbsolutePathString = namespacePath.AbsolutePath.Value + fileName;

			var emptyFileAbsolutePath = CommonService.EnvironmentProvider.AbsolutePathFactory(
				emptyFileAbsolutePathString,
				false);

			await CommonService.FileSystemProvider.File.WriteAllTextAsync(
					emptyFileAbsolutePath.Value,
					string.Empty,
					CancellationToken.None)
				.ConfigureAwait(false);
		}
		else
		{
			var allTemplates = new[] { exactMatchFileTemplate }
				.Union(relatedMatchFileTemplatesList)
				.ToArray();

			foreach (var fileTemplate in allTemplates)
			{
				var templateResult = fileTemplate.ConstructFileContents.Invoke(
					new FileTemplateParameter(fileName, namespacePath, CommonService.EnvironmentProvider));

				await CommonService.FileSystemProvider.File.WriteAllTextAsync(
						templateResult.FileNamespacePath.AbsolutePath.Value,
						templateResult.Contents,
						CancellationToken.None)
					.ConfigureAwait(false);
			}
		}

		await onAfterCompletion.Invoke().ConfigureAwait(false);
	}

	private void Enqueue_PerformNewDirectory(string directoryName, AbsolutePath parentDirectory, Func<Task> onAfterCompletion)
	{
		Enqueue(new IdeWorkArgs
		{
			WorkKind = IdeWorkKind.PerformNewDirectory,
			StringValue = directoryName,
			AbsolutePath = parentDirectory,
			OnAfterCompletion = onAfterCompletion,
		});
	}

	private async ValueTask Do_PerformNewDirectory(string directoryName, AbsolutePath parentDirectory, Func<Task> onAfterCompletion)
	{
		var directoryAbsolutePathString = parentDirectory.Value + directoryName;
		var directoryAbsolutePath = CommonService.EnvironmentProvider.AbsolutePathFactory(directoryAbsolutePathString, true);

		await CommonService.FileSystemProvider.Directory.CreateDirectoryAsync(
				directoryAbsolutePath.Value,
				CancellationToken.None)
			.ConfigureAwait(false);

		await onAfterCompletion.Invoke().ConfigureAwait(false);
	}

	private void Enqueue_PerformDeleteFile(AbsolutePath absolutePath, Func<Task> onAfterCompletion)
	{
		Enqueue(new IdeWorkArgs
		{
			WorkKind = IdeWorkKind.PerformDeleteFile,
			AbsolutePath = absolutePath,
			OnAfterCompletion = onAfterCompletion,
		});
	}

	private async ValueTask Do_PerformDeleteFile(AbsolutePath absolutePath, Func<Task> onAfterCompletion)
	{
		if (absolutePath.IsDirectory)
		{
			await CommonService.FileSystemProvider.Directory
				.DeleteAsync(absolutePath.Value, true, CancellationToken.None)
				.ConfigureAwait(false);
		}
		else
		{
			await CommonService.FileSystemProvider.File
				.DeleteAsync(absolutePath.Value)
				.ConfigureAwait(false);
		}

		await onAfterCompletion.Invoke().ConfigureAwait(false);
	}

	private void Enqueue_PerformCopyFile(AbsolutePath absolutePath, Func<Task> onAfterCompletion)
	{
		Enqueue(new IdeWorkArgs
		{
			WorkKind = IdeWorkKind.PerformCopyFile,
			AbsolutePath = absolutePath,
			OnAfterCompletion = onAfterCompletion,
		});
	}

	private async ValueTask Do_PerformCopyFile(AbsolutePath absolutePath, Func<Task> onAfterCompletion)
	{
		await CommonService.SetClipboard(ClipboardFacts.FormatPhrase(
				ClipboardFacts.CopyCommand,
				ClipboardFacts.AbsolutePathDataType,
				absolutePath.Value))
			.ConfigureAwait(false);

		await onAfterCompletion.Invoke().ConfigureAwait(false);
	}

	private void Enqueue_PerformCutFile(
		AbsolutePath absolutePath,
		Func<Task> onAfterCompletion)
	{
		Enqueue(new IdeWorkArgs
		{
			WorkKind = IdeWorkKind.PerformCutFile,
			AbsolutePath = absolutePath,
			OnAfterCompletion = onAfterCompletion,
		});
	}

	private async ValueTask Do_PerformCutFile(
		AbsolutePath absolutePath,
		Func<Task> onAfterCompletion)
	{
		await CommonService.SetClipboard(ClipboardFacts.FormatPhrase(
				ClipboardFacts.CutCommand,
				ClipboardFacts.AbsolutePathDataType,
				absolutePath.Value))
			.ConfigureAwait(false);

		await onAfterCompletion.Invoke().ConfigureAwait(false);
	}

	private void Enqueue_PerformPasteFile(AbsolutePath receivingDirectory, Func<Task> onAfterCompletion)
	{
		Enqueue(new IdeWorkArgs
		{
			WorkKind = IdeWorkKind.PerformPasteFile,
			AbsolutePath = receivingDirectory,
			OnAfterCompletion = onAfterCompletion,
		});
	}

	private async ValueTask Do_PerformPasteFile(AbsolutePath receivingDirectory, Func<Task> onAfterCompletion)
	{
		var clipboardContents = await CommonService.ReadClipboard().ConfigureAwait(false);

		if (ClipboardFacts.TryParseString(clipboardContents, out var clipboardPhrase))
		{
			if (clipboardPhrase is not null &&
				clipboardPhrase.DataType == ClipboardFacts.AbsolutePathDataType)
			{
				if (clipboardPhrase.Command == ClipboardFacts.CopyCommand ||
					clipboardPhrase.Command == ClipboardFacts.CutCommand)
				{
					AbsolutePath clipboardAbsolutePath = default;

					// Should the if and else if be kept as inline awaits?
					// If kept as inline awaits then the else if won't execute if the first one succeeds.
					if (await CommonService.FileSystemProvider.Directory.ExistsAsync(clipboardPhrase.Value).ConfigureAwait(false))
					{
						clipboardAbsolutePath = CommonService.EnvironmentProvider.AbsolutePathFactory(
							clipboardPhrase.Value,
							true);
					}
					else if (await CommonService.FileSystemProvider.File.ExistsAsync(clipboardPhrase.Value).ConfigureAwait(false))
					{
						clipboardAbsolutePath = CommonService.EnvironmentProvider.AbsolutePathFactory(
							clipboardPhrase.Value,
							false);
					}

					if (clipboardAbsolutePath.ExactInput is not null)
					{
						var successfullyPasted = true;

						try
						{
							if (clipboardAbsolutePath.IsDirectory)
							{
								var clipboardDirectoryInfo = new DirectoryInfo(clipboardAbsolutePath.Value);
								var receivingDirectoryInfo = new DirectoryInfo(receivingDirectory.Value);

								CopyFilesRecursively(clipboardDirectoryInfo, receivingDirectoryInfo);
							}
							else
							{
								var destinationAbsolutePathString = receivingDirectory.Value +
									clipboardAbsolutePath.NameWithExtension;

								var sourceAbsolutePathString = clipboardAbsolutePath.Value;

								await CommonService.FileSystemProvider.File.CopyAsync(
										sourceAbsolutePathString,
										destinationAbsolutePathString)
									.ConfigureAwait(false);
							}
						}
						catch (Exception)
						{
							successfullyPasted = false;
						}

						if (successfullyPasted && clipboardPhrase.Command == ClipboardFacts.CutCommand)
						{
							// TODO: Rerender the parent of the deleted due to cut file
							Enqueue_PerformDeleteFile(clipboardAbsolutePath, onAfterCompletion);
						}
						else
						{
							await onAfterCompletion.Invoke().ConfigureAwait(false);
						}
					}
				}
			}
		}
	}

	private AbsolutePath PerformRename(AbsolutePath sourceAbsolutePath, string nextName, CommonService commonService, Func<Task> onAfterCompletion)
	{
		// Check if the current and next name match when compared with case insensitivity
		if (0 == string.Compare(sourceAbsolutePath.NameWithExtension, nextName, StringComparison.OrdinalIgnoreCase))
		{
			var temporaryNextName = CommonService.EnvironmentProvider.GetRandomFileName();

			var temporaryRenameResult = PerformRename(
				sourceAbsolutePath,
				temporaryNextName,
				commonService,
				() => Task.CompletedTask);

			if (temporaryRenameResult.ExactInput is null)
			{
				onAfterCompletion.Invoke();
				return default;
			}
			else
			{
				sourceAbsolutePath = temporaryRenameResult;
			}
		}

		var sourceAbsolutePathString = sourceAbsolutePath.Value;
		var parentOfSource = sourceAbsolutePath.ParentDirectory;
		var destinationAbsolutePathString = parentOfSource + nextName;

		try
		{
			if (sourceAbsolutePath.IsDirectory)
				CommonService.FileSystemProvider.Directory.MoveAsync(sourceAbsolutePathString, destinationAbsolutePathString);
			else
				CommonService.FileSystemProvider.File.MoveAsync(sourceAbsolutePathString, destinationAbsolutePathString);
		}
		catch (Exception e)
		{
			NotificationHelper.DispatchError("Rename Action", e.Message, commonService, TimeSpan.FromSeconds(14));
			onAfterCompletion.Invoke();
			return default;
		}

		onAfterCompletion.Invoke();

		return CommonService.EnvironmentProvider.AbsolutePathFactory(destinationAbsolutePathString, sourceAbsolutePath.IsDirectory);
	}

	/// <summary>
	/// Looking into copying and pasting a directory
	/// https://stackoverflow.com/questions/58744/copy-the-entire-contents-of-a-directory-in-c-sharp
	/// </summary>
	public static DirectoryInfo CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
	{
		var newDirectoryInfo = target.CreateSubdirectory(source.Name);
		foreach (var fileInfo in source.GetFiles())
			fileInfo.CopyTo(Path.Combine(newDirectoryInfo.FullName, fileInfo.Name));

		foreach (var childDirectoryInfo in source.GetDirectories())
			CopyFilesRecursively(childDirectoryInfo, newDirectoryInfo);

		return newDirectoryInfo;
	}
}
