using Sortcery.Engine.Contracts;

namespace Sortcery.Engine;

public class ShowsGuesser : IGuesser
{
    private readonly IFoldersProvider _foldersProvider;

    public ShowsGuesser(IFoldersProvider foldersProvider)
    {
        _foldersProvider = foldersProvider;
    }

    public ValueTask<FileData?> GuessAsync(FileData source, IReadOnlyList<HardLinkData> links)
    {
        if (source.Dir.Root != _foldersProvider.Source) return ValueTask.FromResult<FileData?>(null);

        var shows = source.Dir.GetPropertyValues<string>("ShowFolder");
        if (shows.Count != 1) return ValueTask.FromResult<FileData?>(null);

        // Season folder is optional, but if it exists, it should be only one
        var seasons = source.Dir.GetPropertyValues<string>("SeasonFolder");
        if (seasons.Count > 1) return ValueTask.FromResult<FileData?>(null);

        var show = shows.First();
        var season = seasons.FirstOrDefault();

        if (!_foldersProvider.TryGetDestinationFolder(FolderType.Shows, out var destinationFolder))
        {
            // This is unexpected, if we have properties that we should have Shows folder
            throw new InvalidOperationException("Shows folder not found");
        }

        var showFolder = destinationFolder.GetFolder(show) ?? throw new InvalidOperationException("Show folder not found");
        var destinationFileFolder = showFolder;
        if (season != null)
        {
            destinationFileFolder = showFolder.GetFolder(season) ?? throw new InvalidOperationException("Season folder not found");
        }

        return ValueTask.FromResult<FileData?>(new FileData(destinationFileFolder, HardLinkId.Empty, source.Name));
    }
}
