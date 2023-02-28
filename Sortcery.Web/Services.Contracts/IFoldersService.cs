using Sortcery.Api.Contracts.Models;

namespace Sortcery.Web.Services.Contracts;

public interface IFoldersService
{
    public string DirectorySeparator { get; }

    public FolderData Source { get; }

    IReadOnlyList<FolderData> DestinationFolders { get; }

    Task InitializeAsync();
}
