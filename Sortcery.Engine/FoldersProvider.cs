using Sortcery.Engine.Contracts;

namespace Sortcery.Engine;

public class FoldersProvider : IFoldersProvider
{
    public FoldersProvider(FolderData source, params FolderData[] destination) :
        this(source, destination.ToList().AsReadOnly())
    {
    }

    public FoldersProvider(FolderData source, IReadOnlyList<FolderData> destinationFolders)
    {
        Source = source;
        DestinationFolders = destinationFolders;
    }

    public FolderData Source { get; }

    public IReadOnlyList<FolderData> DestinationFolders { get; }
}
