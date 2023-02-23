using Sortcery.Engine.Contracts;

namespace Sortcery.Api.Services.Contracts;

public interface IFoldersService
{
    FolderData SourceFolder { get; }

    IReadOnlyList<FolderData> DestinationFolders { get; }

    bool TryGetDestinationFolder(string dir, out FolderData? folderData)
    {
        folderData = DestinationFolders.FirstOrDefault(x => x.Name == dir);
        return folderData != null;
    }
}
