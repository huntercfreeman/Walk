using System.Text;

namespace Walk.Common.RazorLib.FileSystems.Models;

/// <summary>
/// Verify that 'ExactInput is not null' to know this struct was constructed, not defaulted.
/// </summary>
public struct AbsolutePath
{
    private string? _nameWithExtension;
    private List<string>? _ancestorDirectoryList;
    
	public AbsolutePath(
        string absolutePathString,
        bool isDirectory,
        IEnvironmentProvider environmentProvider,
        List<string>? ancestorDirectoryList = null)
    {
    	ExactInput = absolutePathString;
        IsDirectory = isDirectory;
        _ancestorDirectoryList = ancestorDirectoryList;

        var lengthAbsolutePathString = absolutePathString.Length;
        
        if (IsDirectory && lengthAbsolutePathString > 1)
        {
            // Strip the last character if this is a directory, where the exact input ended in a directory separator char.
            // Reasoning: This standardizes what a directory looks like within the scope of this method.
            //
            if (environmentProvider.IsDirectorySeparator(absolutePathString[^1]))
                lengthAbsolutePathString--;
        }
        
        var tokenBuilder = new StringBuilder();
        var formattedBuilder = new StringBuilder();
        
        int position = 0;
        int parentDirectoryEndExclusiveIndex = -1;

        while (position < lengthAbsolutePathString)
        {
            char currentCharacter = absolutePathString[position++];

            if (environmentProvider.IsDirectorySeparator(currentCharacter))
            {
                // ConsumeTokenAsDirectory
	            formattedBuilder
	            	.Append(tokenBuilder.ToString())
	            	.Append(environmentProvider.DirectorySeparatorChar);
	            
	            tokenBuilder.Clear();
	            
	            parentDirectoryEndExclusiveIndex = formattedBuilder.Length;
	            
	            if (ancestorDirectoryList is not null)
	            	ancestorDirectoryList.Add(formattedBuilder.ToString());
            }
            else if (currentCharacter == ':' && RootDrive.DriveNameAsIdentifier is null && ParentDirectory is null)
            {
            	// ConsumeTokenAsRootDrive
            	RootDrive = new FileSystemDrive(tokenBuilder.ToString());
        		tokenBuilder.Clear();
            }
            else
            {
                tokenBuilder.Append(currentCharacter);
            }
        }

        var fileNameAmbiguous = tokenBuilder.ToString();

        if (!IsDirectory)
        {
            var splitFileNameAmbiguous = fileNameAmbiguous.Split('.');

            if (splitFileNameAmbiguous.Length == 2)
            {
                NameNoExtension = splitFileNameAmbiguous[0];
                ExtensionNoPeriod = splitFileNameAmbiguous[1];
            }
            else if (splitFileNameAmbiguous.Length == 1)
            {
                NameNoExtension = splitFileNameAmbiguous[0];
                ExtensionNoPeriod = string.Empty;
            }
            else
            {
                var fileNameBuilder = new StringBuilder();

                foreach (var split in splitFileNameAmbiguous.SkipLast(1))
                {
                    fileNameBuilder.Append($"{split}.");
                }

                fileNameBuilder.Remove(fileNameBuilder.Length - 1, 1);

                NameNoExtension = fileNameBuilder.ToString();
                ExtensionNoPeriod = splitFileNameAmbiguous.Last();
            }
        }
        else
        {
            NameNoExtension = fileNameAmbiguous;
            ExtensionNoPeriod = environmentProvider.DirectorySeparatorChar.ToString();
        }
        
        if (IsDirectory)
        {
        	formattedBuilder
        		.Append(NameNoExtension)
        		.Append(ExtensionNoPeriod);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(ExtensionNoPeriod))
            {
                formattedBuilder.Append(NameNoExtension);
            }
            else
            {
                formattedBuilder
                	.Append(NameNoExtension)
                	.Append('.')
                	.Append(ExtensionNoPeriod);
            }
        }

        var formattedString = formattedBuilder.ToString();

        if (formattedString.Length == 2)
        {
        	// If two directory separators chars are one after another and that is the only text in the string.
        	if ((formattedString[0] == environmentProvider.DirectorySeparatorChar && formattedString[1] == environmentProvider.DirectorySeparatorChar) ||
        	    (formattedString[0] == environmentProvider.AltDirectorySeparatorChar && formattedString[1] == environmentProvider.AltDirectorySeparatorChar))
        	{
        		Value = environmentProvider.DirectorySeparatorChar.ToString();
        		return;
        	}
        }

		if (parentDirectoryEndExclusiveIndex != -1)
			ParentDirectory = formattedString[..parentDirectoryEndExclusiveIndex];
        
        Value = formattedString;
    }

    public string? ParentDirectory { get; private set; }
    public string? ExactInput { get; }
    public PathType PathType { get; } = PathType.AbsolutePath;
    public bool IsDirectory { get; private set; }
    /// <summary>
    /// The <see cref="NameNoExtension"/> for a directory does NOT end with a directory separator char.
    /// </summary>
    public string NameNoExtension { get; private set; }
    /// <summary>
    /// The <see cref="ExtensionNoPeriod"/> for a directory is the primary directory separator char.
    /// </summary>
    public string ExtensionNoPeriod { get; private set; }
    public FileSystemDrive RootDrive { get; private set; }

    public string Value { get; }
    public string NameWithExtension => _nameWithExtension ??= PathHelper.CalculateNameWithExtension(NameNoExtension, ExtensionNoPeriod, IsDirectory);
    public bool IsRootDirectory => ParentDirectory is null;
    
    public List<string> GetAncestorDirectoryList(IEnvironmentProvider environmentProvider)
    {
    	return _ancestorDirectoryList ??= new AbsolutePath(
        		Value,
	            IsDirectory,
	            environmentProvider,
	            ancestorDirectoryList: new())
            ._ancestorDirectoryList;
    }
}