namespace Sortcery.Engine.Contracts;

public class FileData
{
    public FileData(FolderData dir, string relativeName)
    {
        Dir = dir;
        RelativeName = relativeName;
        Path = System.IO.Path.GetDirectoryName(relativeName)!;
        Name = System.IO.Path.GetFileName(relativeName);
    }

    public FileData(FolderData dir, string path, string name)
    {
        Dir = dir;
        RelativeName = System.IO.Path.Join(path, name);
        Path = path;
        Name = name;
    }

    public FolderData Dir { get; }

    public string Path { get; }

    public string Name { get; }

    public string RelativeName { get; }

    public string FullName => System.IO.Path.Join(Dir.FullName, RelativeName);

    public void Deconstruct(out FolderData dir, out string path, out string name)
    {
        dir = Dir;
        path = Path;
        name = Name;
    }
}
