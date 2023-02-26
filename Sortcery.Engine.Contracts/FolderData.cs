namespace Sortcery.Engine.Contracts;

public class FolderData
{
    public FolderData(FolderType type, string fullName)
    {
        if (!System.IO.Path.IsPathRooted(fullName))
            throw new ArgumentException("Path must be rooted.", nameof(fullName));

        Type = type;
        FullName = System.IO.Path.TrimEndingDirectorySeparator(fullName);

        Path = System.IO.Path.GetDirectoryName(fullName)!;
        Name = System.IO.Path.GetFileName(fullName);
    }

    public FolderType Type { get; }

    public string Path { get; }

    public string Name { get; }

    public string FullName { get; }

    public void Deconstruct(out FolderType type, out string path, out string name)
    {
        type = Type;
        path = Path;
        name = Name;
    }

    public void Deconstruct(out string path, out string name)
    {
        path = Path;
        name = Name;
    }
}
