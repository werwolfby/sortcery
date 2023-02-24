using System.Diagnostics.CodeAnalysis;

namespace Sortcery.Engine.Contracts;

public interface IFoldersProvider
{
    public FolderData Source { get; }

    IReadOnlyList<FolderData> DestinationFolders { get; }

    bool TryGetDestinationFolder(string dir, [MaybeNullWhen(false)]out FolderData folderData)
    {
        folderData = DestinationFolders.FirstOrDefault(x => x.Name == dir);
        return folderData != null;
    }

    bool TryGetDestinationFolder(FolderType type, [MaybeNullWhen(false)]out FolderData folderData)
    {
        folderData = DestinationFolders.FirstOrDefault(x => x.Type == type);
        return folderData != null;
    }
}
