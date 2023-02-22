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
        
        SourceFolder = new FolderInfo(_foldersOptions.Value.Source);
        DestinationFolders = new []
        {
            new FolderInfo(_foldersOptions.Value.Movies),
            new FolderInfo(_foldersOptions.Value.Series)
        };
        
        FoldersMap = new Dictionary<FolderInfo, string>
        {
            {SourceFolder, "Source"},
            {DestinationFolders[0], "Movies"},
            {DestinationFolders[1], "Series"}
        };
    }
    
    public FolderInfo SourceFolder { get; }

    public IReadOnlyList<FolderInfo> DestinationFolders { get; }

    public IReadOnlyDictionary<FolderInfo, string> FoldersMap { get; }
}