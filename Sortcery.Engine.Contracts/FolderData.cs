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
        GetFolder(name)
        ?? AddFolder(System.IO.Path.Combine(FullName, name));

    public FolderData? GetFolder(string name) =>
        _folders.FirstOrDefault(x => x.Name == name)
        ?? _folders.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

    public FolderData? FindFolder(string[] folderPath)
    {
        if (folderPath.Length == 0)
            throw new ArgumentException("Folder path must not be empty.", nameof(folderPath));

        var folder = this;
        for (var i = 0; i < folderPath.Length; i++)
        {
            folder = folder.GetFolder(folderPath[i]);
            if (folder == null)
                return null;
        }

        return folder;
    }

    public FolderData EnsureFolder(string[] folderPath)
    {
        if (folderPath.Length == 0)
            throw new ArgumentException("Folder path must not be empty.", nameof(folderPath));

        var folder = this;
        for (var i = 0; i < folderPath.Length; i++)
        {
            folder = folder.GetOrAddFolder(folderPath[i]);
        }

        return folder;
    }

    public void RemoveFolder(string name)
    {
        var folder = GetFolder(name);
        if (folder == null)
            throw new ArgumentException($"Folder '{name}' not found.", nameof(name));

        _folders.Remove(folder);
    }

    public FileData AddFile(string name, HardLinkId hardLinkId)
    {
        var fileData = new FileData(this, hardLinkId, name);
        _files.Add(fileData);
        return fileData;
    }

    public void AddFile(FileData fileData)
    {
        if (fileData.Dir != this)
            throw new ArgumentException("File must be in this folder.", nameof(fileData));

        _files.Add(fileData);
    }

    public FileData GetOrAddFile(string name, HardLinkId hardLinkId) =>
        GetFile(name) ?? AddFile(name, hardLinkId);

    public FileData? GetFile(string name) =>
        _files.FirstOrDefault(x => x.Name == name)
        ?? _files.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

    public void RemoveFile(string name)
    {
        var file = GetFile(name);
        if (file == null)
            throw new ArgumentException($"File '{name}' not found.", nameof(name));

        _files.Remove(file);
    }

    public FileData? FindFile(string[] filePath)
    {
        if (filePath.Length == 0)
            throw new ArgumentException("File path must not be empty.", nameof(filePath));

        return FindFolder(filePath[..^1])?.GetFile(filePath[^1]);
    }

    public IEnumerable<FileData> GetAllFilesRecursively()
    {
        foreach (var folder in Folders)
        {
            foreach (var file in folder.GetAllFilesRecursively())
                yield return file;
        }

        foreach (var file in Files)
            yield return file;
    }

    public void Deconstruct(out string path, out string name)
    {
        path = Path;
        name = Name;
    }
}
