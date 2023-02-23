namespace Sortcery.Engine.Contracts;

public interface IFoldersProvider
{
    public FolderData Source { get; }

    IReadOnlyList<FolderData> DestinationFolders { get; }

    bool TryGetDestinationFolder(string dir, out FolderData? folderData)
    {
        folderData = DestinationFolders.FirstOrDefault(x => x.Name == dir);
        return folderData != null;
    }
}
