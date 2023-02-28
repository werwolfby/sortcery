using Sortcery.Engine.Contracts;

namespace Sortcery.Engine;

public class FoldersProvider : IFoldersProvider
{
    private readonly ITraverser _traverser;

    public FoldersProvider(ITraverser traverser, string source, params (FolderType type, string folder)[] destination)
    {
        _traverser = traverser;

        Source = new FolderData(source);
        DestinationFolders = destination.ToDictionary(
            x => x.type,
            x => new FolderData(x.folder));
    }

    public void Update()
    {
        _traverser.Traverse(Source);
        foreach (var (_, folder) in DestinationFolders)
        {
            _traverser.Traverse(folder);
        }
    }

    public FolderData Source { get; }

    public IReadOnlyDictionary<FolderType, FolderData> DestinationFolders { get; }
}
