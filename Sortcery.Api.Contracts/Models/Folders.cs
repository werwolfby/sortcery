namespace Sortcery.Api.Contracts.Models;

public class Folders
{
    public FolderData Source { get; set; }

    public FolderData[] DestinationFolders { get; set; }
}
