using Microsoft.Extensions.Options;
using Sortcery.Api.Services.Contracts;
using Sortcery.Engine.Contracts;

namespace Sortcery.Api.Services;

public class FoldersService : IFoldersService
{
    private readonly IOptions<FoldersOptions> _foldersOptions;

    public FoldersService(IOptions<FoldersOptions> foldersOptions)
    {
        _foldersOptions = foldersOptions;

        SourceFolder = new FolderData(_foldersOptions.Value.Source);
        DestinationFolders = new []
        {
            new FolderData(_foldersOptions.Value.Movies),
            new FolderData(_foldersOptions.Value.Series)
        };

        FoldersToNameMap = new Dictionary<FolderData, string>
        {
            {SourceFolder, "Source"},
            {DestinationFolders[0], "Movies"},
            {DestinationFolders[1], "Series"}
        };

        NameToFolderMap = FoldersToNameMap
            .ToDictionary(x => x.Value, x => x.Key);
    }

    public FolderData SourceFolder { get; }

    public IReadOnlyList<FolderData> DestinationFolders { get; }

    public IReadOnlyDictionary<FolderData, string> FoldersToNameMap { get; }

    public IReadOnlyDictionary<string, FolderData> NameToFolderMap { get; }
}
