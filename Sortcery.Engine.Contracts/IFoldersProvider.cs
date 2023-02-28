using System.Diagnostics.CodeAnalysis;

namespace Sortcery.Engine.Contracts;

public interface IFoldersProvider
{
    public FolderData Source { get; }

    IReadOnlyDictionary<FolderType, FolderData> DestinationFolders { get; }

    void Update();

    bool TryGetDestinationFolder(string dir, [MaybeNullWhen(false)]out FolderData folderData)
    {
        folderData = DestinationFolders.Values.FirstOrDefault(x => x.Name == dir);
        return folderData != null;
    }

    bool TryGetDestinationFolder(FolderType type, [MaybeNullWhen(false)]out FolderData folderData) =>
        DestinationFolders.TryGetValue(type, out folderData);
}
