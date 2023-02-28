using Sortcery.Engine.Contracts;

namespace Sortcery.Engine;

public class FoldersProvider : IFoldersProvider
{
    public FoldersProvider(FolderData source, params (FolderType type, FolderData folder)[] destination) :
        this(source, destination.ToDictionary(x => x.type, x => x.folder).AsReadOnly())
    {
    }

    public FoldersProvider(FolderData source, IReadOnlyDictionary<FolderType, FolderData> destinationFolders)
    {
        Source = source;
        DestinationFolders = destinationFolders;
    }

    public FolderData Source { get; }

    public IReadOnlyDictionary<FolderType, FolderData> DestinationFolders { get; }
}
