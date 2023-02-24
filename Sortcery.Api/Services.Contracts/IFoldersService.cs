using Sortcery.Engine.Contracts;

namespace Sortcery.Api.Services.Contracts;

public interface IFoldersService
{
    FolderInfo SourceFolder { get; }

    IReadOnlyList<FolderInfo> DestinationFolders { get; }

    IReadOnlyDictionary<FolderInfo, string> FoldersToNameMap { get; }

    IReadOnlyDictionary<string, FolderInfo> NameToFolderMap { get; }
}
