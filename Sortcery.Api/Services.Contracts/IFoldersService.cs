using Sortcery.Model;

namespace Sortcery.Api.Services.Contracts;

public interface IFoldersService
{
    FolderInfo SourceFolder { get; }
    
    IReadOnlyList<FolderInfo> DestinationFolders { get; }
    
    IReadOnlyDictionary<FolderInfo, string> FoldersMap { get; }
}
