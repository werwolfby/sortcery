namespace Sortcery.Engine.Contracts;

public class FolderData
{
    private readonly List<FolderData> _folders = new();
    private readonly List<FileData> _files = new();

    public FolderData(string fullName, FolderData? parent = null)
    {
        if (!System.IO.Path.IsPathRooted(fullName))
            throw new ArgumentException("Path must be rooted.", nameof(fullName));

        fullName = System.IO.Path.TrimEndingDirectorySeparator(fullName);

        Parent = parent;
        FullName = fullName + System.IO.Path.DirectorySeparatorChar;

        Files = _files.AsReadOnly();
        Folders = _folders.AsReadOnly();

        Path = System.IO.Path.GetDirectoryName(fullName)!;
        Name = System.IO.Path.GetFileName(fullName);
    }

    public FolderData? Parent { get; }

    public FolderData Root => Parent?.Root ?? this;

    public string FullName { get; }

    public string Path { get; }

    public string Name { get; }

    public string RelativePath => FullName[Root.FullName.Length..];

    public IReadOnlyList<FolderData> Folders { get; }

    public IReadOnlyList<FileData> Files { get; }

    public FolderData AddFolder(string fullName)
    {
        var folderData = new FolderData(fullName, this);
        _folders.Add(folderData);
        return folderData;
    }

    public FolderData GetOrAddFolder(string name) =>
        _folders.FirstOrDefault(x => x.Name == name)
        ?? _folders.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase))
        ?? AddFolder(System.IO.Path.Combine(FullName, name));

    public FileData AddFile(string name, HardLinkId hardLinkId)
    {
        var fileData = new FileData(this, hardLinkId, name);
        _files.Add(fileData);
        return fileData;
    }

    public void Deconstruct(out string path, out string name)
    {
        path = Path;
        name = Name;
    }
}
