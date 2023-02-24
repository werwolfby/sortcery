namespace Sortcery.Engine.Contracts;

public class FolderData
{
    public FolderData(FolderType type, string fullName)
    {
        if (!System.IO.Path.IsPathRooted(fullName))
            throw new ArgumentException("Path must be rooted.", nameof(fullName));

        fullName = System.IO.Path.TrimEndingDirectorySeparator(fullName);
        var path = fullName.Split(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
#if !_WINDOWS
        // On non-Windows platforms, the root directory is a disk name + colon.
        // And it will be split in first element
        // On Unix, it will be empty string
        // And it will create relative path on Unix in FullName property
        // So we need to fix it, and set as root directory
        path[0] = "/";
#endif
        Type = type;
        Path = path[..^1];
        Name = path[^1];
    }

    public FolderType Type { get; }

    public IReadOnlyList<string> Path { get; }

    public string Name { get; }

    public string FullName
    {
        get
        {
            var paths = new string[Path.Count + 1];
            for (var i = 0; i < Path.Count; i++)
            {
                paths[i] = Path[i];
            }
            paths[^1] = Name;
            return System.IO.Path.Join(paths);
        }
    }

    public void Deconstruct(out FolderType type, out IReadOnlyList<string> path, out string name)
    {
        type = Type;
        path = Path;
        name = Name;
    }

    public void Deconstruct(out IReadOnlyList<string> path, out string name)
    {
        path = Path;
        name = Name;
    }
}
