using Microsoft.Extensions.Options;
using Sortcery.Api.Services.Contracts;
using Sortcery.Engine.Contracts;

namespace Sortcery.Api.Services;

public class FoldersService : IFoldersService
{
    public FoldersService(IOptions<FoldersOptions> foldersOptions)
    {
        SourceFolder = new FolderData(FolderType.Source, foldersOptions.Value.Source);
        DestinationFolders = new []
        {
            new FolderData(FolderType.Movies, foldersOptions.Value.Movies),
            new FolderData(FolderType.Shows, foldersOptions.Value.Series)
        };
    }

    public FolderData SourceFolder { get; }

    public IReadOnlyList<FolderData> DestinationFolders { get; }
}
