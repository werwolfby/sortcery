namespace Sortcery.Engine.Contracts;

public class FolderData
{
    public FolderData(FolderType type, string fullName)
    {
        if (!Path.IsPathRooted(fullName))
            throw new ArgumentException("Path must be rooted.", nameof(fullName));

        Type = type;
        FullName = Path.TrimEndingDirectorySeparator(fullName);
    }

    public string FullName { get; }

    public FolderType Type { get; }

    public string Name => Path.GetFileName(FullName);
}
