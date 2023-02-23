using Sortcery.Engine.Contracts;

namespace Sortcery.Api.Services.Contracts;

public interface IFoldersService
{
    FolderData SourceFolder { get; }

    IReadOnlyList<FolderData> DestinationFolders { get; }

    IReadOnlyDictionary<FolderData, string> FoldersToNameMap { get; }

    IReadOnlyDictionary<string, FolderData> NameToFolderMap { get; }
}
