namespace Sortcery.Engine.Contracts;

public class FileData
{
    public FileData(FolderData dir, HardLinkId hardLinkId, string name)
    {
        Dir = dir;
        HardLinkId = hardLinkId;
        Name = name;
    }

    public FolderData Dir { get; }

    public HardLinkId HardLinkId { get; }

    public string Name { get; }

    public string FullName => Path.Join(Dir.FullName, Name);

    public void Deconstruct(out FolderData dir, out string name)
    {
        dir = Dir;
        name = Name;
    }
}
