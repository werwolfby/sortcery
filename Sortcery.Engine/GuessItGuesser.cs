using Sortcery.Engine.Contracts;

namespace Sortcery.Engine;

public class GuessItGuesser
{
    private readonly IGuessItApi _guessItApi;
    private readonly IFoldersProvider _foldersProvider;

    public GuessItGuesser(IGuessItApi guessItApi, IFoldersProvider foldersProvider)
    {
        _guessItApi = guessItApi;
        _foldersProvider = foldersProvider;
    }

    public async ValueTask<FileData?> GuessAsync(FileData source, IReadOnlyList<HardLinkData> links)
    {
        var guess = await _guessItApi.GuessAsync(source.Name);
        return guess.Type switch
        {
            "movie" => GuessMovie(guess, source.Name),
            "episode" => GuessEpisode(guess, source.Name),
            _ => throw new NotSupportedException($"Unknown guess type: {guess.Type}")
        };
    }

    private FileData GuessMovie(Guess guess, string filename)
    {
        if (!_foldersProvider.TryGetDestinationFolder(FolderType.Movies, out var destinationFolder))
        {
            throw new InvalidOperationException("Unknown destination folder: Movies");
        }

        return new FileData(destinationFolder, HardLinkId.Empty, filename);
    }

    private FileData GuessEpisode(Guess guess, string filename)
    {
        if (!_foldersProvider.TryGetDestinationFolder(FolderType.Shows, out var destinationFolder))
        {
            throw new InvalidOperationException("Unknown destination folder: Shows");
        }

        // Create temporary/virtual folder structure if it doesn't exist
        var showFolder = destinationFolder.GetFolder(guess.Title)
                         ?? new FolderData(Path.Join(destinationFolder.FullName, guess.Title), destinationFolder);
        var resultFolder = showFolder;
        if (guess.Season.HasValue)
        {
            resultFolder = GetOrCreateSeasonFolderData(showFolder, guess.Season.Value);
        }

        return new FileData(resultFolder, HardLinkId.Empty, filename);
    }

    private FolderData GetOrCreateSeasonFolderData(FolderData showFolder, int season)
    {
        var seasonFormats = new Dictionary<string, List<int>>();
        foreach (var folder in showFolder.Folders)
        {
            if (!SeasonFolderParser.TryParse(folder.Name, out var format, out var seasonNumber))
            {
                continue;
            }

            if (seasonNumber == season)
            {
                return folder;
            }

            seasonFormats.Add(format, seasonNumber);
        }

        var seasonFormat = seasonFormats.Count switch
        {
            0 => "Season {0}",
            1 => seasonFormats.Single().Key,
            _ => seasonFormats.MaxBy(e => e.Value.Max()).Key
        };

        var seasonFolderName = string.Format(seasonFormat, season);
        var seasonFolder = showFolder.GetFolder(seasonFolderName)
                           ?? new FolderData(Path.Join(showFolder.FullName, seasonFolderName), showFolder);
        return seasonFolder;
    }
}
