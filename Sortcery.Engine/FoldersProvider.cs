using Sortcery.Engine.Contracts;

namespace Sortcery.Engine;

public class FoldersProvider : IFoldersProvider
{
    public FoldersProvider(ITraverser traverser, string source, params (FolderType type, string folder)[] destination)
    {
        Source = traverser.Traverse(source);
        DestinationFolders = destination.ToDictionary(
            x => x.type,
            x => traverser.Traverse(x.folder));
    }

    public FolderData Source { get; }

    public IReadOnlyDictionary<FolderType, FolderData> DestinationFolders { get; }
}
