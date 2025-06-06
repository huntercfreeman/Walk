namespace Walk.Common.RazorLib.FileSystems.Models;

public class LocalEnvironmentProvider : IEnvironmentProvider
{
	#if DEBUG
    public const string SafeRelativeDirectory = "Walk/Debug/";
	#else
    public const string SafeRelativeDirectory = "Walk/Release/";
	#endif
	
    private readonly object _pathLock = new();

    public LocalEnvironmentProvider()
    {
        RootDirectoryAbsolutePath = new AbsolutePath(
        	"/",
        	true,
        	this);

        HomeDirectoryAbsolutePath = new AbsolutePath(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            true,
            this);
            
        ActualRoamingApplicationDataDirectoryAbsolutePath = new AbsolutePath(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            true,
            this);
            
        SafeRoamingApplicationDataDirectoryAbsolutePath = new AbsolutePath(
            JoinPaths(ActualRoamingApplicationDataDirectoryAbsolutePath.Value, SafeRelativeDirectory),
            true,
            this);
            
        ActualLocalApplicationDataDirectoryAbsolutePath = new AbsolutePath(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            true,
            this);
        
        SafeLocalApplicationDataDirectoryAbsolutePath = new AbsolutePath(
            JoinPaths(ActualLocalApplicationDataDirectoryAbsolutePath.Value, SafeRelativeDirectory),
            true,
            this);

        ProtectedPathList.Add(new(
        	RootDirectoryAbsolutePath.Value,
            RootDirectoryAbsolutePath.IsDirectory));

		ProtectedPathList.Add(new(
        	HomeDirectoryAbsolutePath.Value,
            HomeDirectoryAbsolutePath.IsDirectory));
        
        ProtectedPathList.Add(new(
        	ActualRoamingApplicationDataDirectoryAbsolutePath.Value,
            ActualRoamingApplicationDataDirectoryAbsolutePath.IsDirectory));
            
        ProtectedPathList.Add(new(
        	ActualLocalApplicationDataDirectoryAbsolutePath.Value,
            ActualLocalApplicationDataDirectoryAbsolutePath.IsDirectory));
            
        ProtectedPathList.Add(new(
        	SafeRoamingApplicationDataDirectoryAbsolutePath.Value,
            SafeRoamingApplicationDataDirectoryAbsolutePath.IsDirectory));
            
        ProtectedPathList.Add(new(
        	SafeLocalApplicationDataDirectoryAbsolutePath.Value,
            SafeLocalApplicationDataDirectoryAbsolutePath.IsDirectory));

        // Redundantly hardcode some obvious cases for protection.
        {
            ProtectedPathList.Add(new SimplePath("/", true));
            ProtectedPathList.Add(new SimplePath("\\", true));
            ProtectedPathList.Add(new SimplePath("", true));

            try
            {
                var entryAssembly = System.Reflection.Assembly.GetEntryAssembly();

                if (entryAssembly is not null)
                {
                    var driveExecutingFrom = Path.GetPathRoot(entryAssembly.Location);

                    if (driveExecutingFrom is not null)
                    {
                        if (driveExecutingFrom.EndsWith('/') || driveExecutingFrom.EndsWith('\\'))
                        {
                            DriveExecutingFromNoDirectorySeparator = driveExecutingFrom[..^1];

                            ProtectedPathList.Add(
                                new SimplePath(DriveExecutingFromNoDirectorySeparator + '/',
                                true));
                            
                            ProtectedPathList.Add(
                                new SimplePath(DriveExecutingFromNoDirectorySeparator + '\\',
                                true));
                        }
                        else
                        {
                            DriveExecutingFromNoDirectorySeparator = driveExecutingFrom;
                            ProtectedPathList.Add(new SimplePath(driveExecutingFrom, true));
                        }
                    }
                }
            }
            catch (Exception)
            {
                // This code is intended to be an extra level of caution.
                // it trys to add the current drive to the protected path list if possible.
            }
        }
    }

    public AbsolutePath RootDirectoryAbsolutePath { get; }
    public AbsolutePath HomeDirectoryAbsolutePath { get; }
	public AbsolutePath SafeRoamingApplicationDataDirectoryAbsolutePath { get; }
	public AbsolutePath SafeLocalApplicationDataDirectoryAbsolutePath { get; }
	public AbsolutePath ActualRoamingApplicationDataDirectoryAbsolutePath { get; }
	public AbsolutePath ActualLocalApplicationDataDirectoryAbsolutePath { get; }

    public string DriveExecutingFromNoDirectorySeparator { get; }

    public char DirectorySeparatorChar => Path.DirectorySeparatorChar;
    public char AltDirectorySeparatorChar => Path.AltDirectorySeparatorChar;
    public HashSet<SimplePath> DeletionPermittedPathList { get; private set; } = [];
    public HashSet<SimplePath> ProtectedPathList { get; private set; } = [];

    public bool IsDirectorySeparator(char character) =>
        character == DirectorySeparatorChar || character == AltDirectorySeparatorChar;

    public string GetRandomFileName() => Path.GetRandomFileName();

    public string JoinPaths(string pathOne, string pathTwo)
    {
        if (IsDirectorySeparator(pathOne.LastOrDefault()))
            return pathOne + pathTwo;

        return string.Join(DirectorySeparatorChar, pathOne, pathTwo);
    }

    public void AssertDeletionPermitted(string path, bool isDirectory)
    {
        PermittanceChecker.AssertDeletionPermitted(this, path, isDirectory);
    }

    public void DeletionPermittedRegister(SimplePath simplePath)
    {
        lock (_pathLock)
        {
            var absolutePath = simplePath.AbsolutePath;

            if (absolutePath == "/" || absolutePath == "\\" || string.IsNullOrWhiteSpace(absolutePath))
                return;

            if (PermittanceChecker.IsRootOrHomeDirectory(simplePath, this))
                return;

            DeletionPermittedPathList.Add(simplePath);
        }
    }

    public void DeletionPermittedDispose(SimplePath simplePath)
    {
        lock (_pathLock)
        {
            DeletionPermittedPathList.Remove(simplePath);
        }
    }

    public void ProtectedPathsRegister(SimplePath simplePath)
    {
        lock (_pathLock)
        {
            ProtectedPathList.Add(simplePath);
        }
    }

    public void ProtectedPathsDispose(SimplePath simplePath)
    {
        lock (_pathLock)
        {
            var absolutePath = simplePath.AbsolutePath;

            if (absolutePath == "/" || absolutePath == "\\" || string.IsNullOrWhiteSpace(absolutePath))
                return;

            if (PermittanceChecker.IsRootOrHomeDirectory(simplePath, this))
                return;

            ProtectedPathList.Remove(simplePath);
        }
    }

    public AbsolutePath AbsolutePathFactory(string path, bool isDirectory)
    {
        return new AbsolutePath(path, isDirectory, this);
    }

    public RelativePath RelativePathFactory(string path, bool isDirectory)
    {
        return new RelativePath(path, isDirectory, this);
    }
}