namespace Sortcery.Engine.Contracts;

public class FileData
{
    public FileData(FolderData dir, string relativePath)
    {
        var path = relativePath.Split(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);

        Dir = dir;
        Path = path[..^1];
        Name = path[^1];
    }

    public FileData(FolderData dir, IReadOnlyList<string> path, string name)
    {
        Dir = dir;
        Path = path;
        Name = name;
    }

    public FolderData Dir { get; }

    public IReadOnlyList<string> Path { get; }

    public string Name { get; }

    public string RelativePath
    {
        get
        {
            var paths = new string[Path.Count + 1];
            for (var i = 0; i < Path.Count; i++)
            {
                paths[i] = Path[i];
            }
            paths[^1] = Name;
            return System.IO.Path.Combine(paths);
        }
    }

    public string FullName
    {
        get
        {
            var paths = new string[Dir.Path.Count + 1 + Path.Count + 1];
            for (var i = 0; i < Dir.Path.Count; i++)
            {
                paths[i] = Dir.Path[i];
            }
            paths[Dir.Path.Count] = Dir.Name;
            for (var i = 0; i < Path.Count; i++)
            {
                paths[Dir.Path.Count + 1 + i] = Path[i];
            }
            paths[^1] = Name;
            return System.IO.Path.Combine(paths);
        }
    }

    public void Deconstruct(out FolderData dir, out IReadOnlyList<string> path, out string name)
    {
        dir = Dir;
        path = Path;
        name = Name;
    }
}
