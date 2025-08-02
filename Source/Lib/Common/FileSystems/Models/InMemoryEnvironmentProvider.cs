using System.Text;

namespace Walk.Common.RazorLib.FileSystems.Models;

public class InMemoryEnvironmentProvider : IEnvironmentProvider
{
    public const string SafeRelativeDirectory = "Walk/";

    private readonly object _pathLock = new();

    public InMemoryEnvironmentProvider()
    {
        var tokenBuilder = new StringBuilder();
        var formattedBuilder = new StringBuilder();
    
        RootDirectoryAbsolutePath = new AbsolutePath("/", true, this, tokenBuilder, formattedBuilder);
        HomeDirectoryAbsolutePath = new AbsolutePath("/Repos/", true, this, tokenBuilder, formattedBuilder);
        ActualRoamingApplicationDataDirectoryAbsolutePath = new AbsolutePath("/AppData/Roaming/", true, this, tokenBuilder, formattedBuilder);
        ActualLocalApplicationDataDirectoryAbsolutePath = new AbsolutePath("/AppData/Local/", true, this, tokenBuilder, formattedBuilder);
        
        SafeRoamingApplicationDataDirectoryAbsolutePath = new AbsolutePath(
            JoinPaths(ActualRoamingApplicationDataDirectoryAbsolutePath.Value, SafeRelativeDirectory),
            true,
            this,
            tokenBuilder,
            formattedBuilder);
            
        SafeLocalApplicationDataDirectoryAbsolutePath = new AbsolutePath(
            JoinPaths(ActualLocalApplicationDataDirectoryAbsolutePath.Value, SafeRelativeDirectory),
            true,
            this,
            tokenBuilder,
            formattedBuilder);
        
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
        }
    }

    public AbsolutePath RootDirectoryAbsolutePath { get; }
    public AbsolutePath HomeDirectoryAbsolutePath { get; }
    public AbsolutePath ActualRoamingApplicationDataDirectoryAbsolutePath { get; }
    public AbsolutePath ActualLocalApplicationDataDirectoryAbsolutePath { get; }
    public AbsolutePath SafeRoamingApplicationDataDirectoryAbsolutePath { get; }
    public AbsolutePath SafeLocalApplicationDataDirectoryAbsolutePath { get; }
    public string DriveExecutingFromNoDirectorySeparator { get; } = string.Empty;
    public HashSet<SimplePath> DeletionPermittedPathList { get; private set; } = new();
    public HashSet<SimplePath> ProtectedPathList { get; private set; } = new();

    public char DirectorySeparatorChar => '/';
    public char AltDirectorySeparatorChar => '\\';

    public bool IsDirectorySeparator(char character) =>
        character == DirectorySeparatorChar || character == AltDirectorySeparatorChar;

    public string GetRandomFileName() => Guid.NewGuid().ToString();

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

    public void DeletionPermittedRegister(SimplePath simplePath, StringBuilder? tokenBuilder, StringBuilder? formattedBuilder)
    {
        lock (_pathLock)
        {
            var absolutePath = simplePath.AbsolutePath;

            if (absolutePath == "/" || absolutePath == "\\" || string.IsNullOrWhiteSpace(absolutePath))
                return;

            if (PermittanceChecker.IsRootOrHomeDirectory(simplePath, this, tokenBuilder, formattedBuilder))
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
    
    public void ProtectedPathsDispose(SimplePath simplePath, StringBuilder? tokenBuilder, StringBuilder? formattedBuilder)
    {
        lock (_pathLock)
        {
            var absolutePath = simplePath.AbsolutePath;

            if (absolutePath == "/" || absolutePath == "\\" || string.IsNullOrWhiteSpace(absolutePath))
                return;

            if (PermittanceChecker.IsRootOrHomeDirectory(simplePath, this, tokenBuilder, formattedBuilder))
                return;

            ProtectedPathList.Remove(simplePath);
        }
    }

    public AbsolutePath AbsolutePathFactory(string path, bool isDirectory, StringBuilder? tokenBuilder, StringBuilder? formattedBuilder)
    {
        return new AbsolutePath(path, isDirectory, this, tokenBuilder, formattedBuilder);
    }

    public RelativePath RelativePathFactory(string path, bool isDirectory)
    {
        return new RelativePath(path, isDirectory, this);
    }
}
