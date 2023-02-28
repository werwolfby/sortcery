namespace Sortcery.Api.Contracts.Models;

public class Folders
{
    public string DirectorySeparator { get; set; }

    public FolderData Source { get; set; }

    public FolderData[] DestinationFolders { get; set; }
}
