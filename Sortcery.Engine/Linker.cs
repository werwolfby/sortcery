using Sortcery.Engine.Contracts;

namespace Sortcery.Engine;

public class Linker : ILinker
{
    private readonly IFoldersProvider _foldersProvider;
    private readonly IGuessItApi _guessItApi;

    public Linker(IFoldersProvider foldersProvider, IGuessItApi guessItApi)
    {
        _foldersProvider = foldersProvider;
        _guessItApi = guessItApi;

        Links = Array.Empty<HardLinkData>();
    }

    public IReadOnlyList<HardLinkData> Links { get; private set; }

    public void Update()
    {
        _foldersProvider.Update();

        var destinationFolderFiles = _foldersProvider.DestinationFolders.Values
            .Select(x => x.GetAllFilesRecursively().ToDictionaryAggregated(x => x.HardLinkId))
            .ToList();

        var maxCapacity = Math.Max(
            _foldersProvider.Source.GetAllFilesCount(),
            destinationFolderFiles.Sum(d => d.Count));
        var result = new List<(FileData? Source, List<FileData>? Targets)>(maxCapacity);

        // Find all hardlinks in the source folder
        var existingHardLinks = new HashSet<HardLinkId>();
        foreach (var fileData in _foldersProvider.Source.GetAllFilesRecursively())
        {
            var hardLinkId = fileData.HardLinkId;
            List<FileData>? targets = null;
            foreach (var destinationFiles in destinationFolderFiles)
            {
                if (destinationFiles.TryGetValue(hardLinkId, out var destinationFileDataList))
                {
                    targets ??= new List<FileData>();
                    targets.AddRange(destinationFileDataList);
                    existingHardLinks.Add(hardLinkId);
                }
            }
            result.Add((fileData, targets));
        }

        // Find all hardlinks that exists only in at any destination folder but not in the source folder
        foreach (var destinationFiles in destinationFolderFiles)
        {
            foreach (var (hardLinkId, fileDataList) in destinationFiles)
            {
                if (existingHardLinks.Contains(hardLinkId)) continue;
                result.Add((null, fileDataList));
            }
        }

        Links = result
            .Select(x => new HardLinkData(x.Source, x.Targets ?? (IReadOnlyList<FileData>)Array.Empty<FileData>()))
            .ToList()
            .AsReadOnly();
    }

    public async Task<FileData> GuessAsync(FileData fileData)
    {
        var guess = await _guessItApi.GuessAsync(fileData.Name);
        return guess.Type switch
        {
            "movie" => GuessMovie(guess, fileData.Name),
            "episode" => GuessEpisode(guess, fileData.Name),
            _ => throw new NotSupportedException($"Unknown guess type: {guess.Type}")
        };
    }

    public bool Link(FileData sourceFile, FileData destinationFile)
    {
        if (!sourceFile.Link(destinationFile)) return false;
        destinationFile.Dir.AddFile(destinationFile);
        return true;
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
            throw new InvalidOperationException("Unknown destination folder: Series");
        }

        // Create temporary/virtual folder structure if it doesn't exist
        var showFolder = destinationFolder.GetFolder(guess.Title)
                         ?? new FolderData(Path.Join(destinationFolder.FullName, guess.Title), destinationFolder);
        var seasonFolderName = $"Season {guess.Season}";
        var seasonFolder = showFolder.GetFolder(seasonFolderName)
                           ?? new FolderData(Path.Join(showFolder.FullName, seasonFolderName), showFolder);;

        return new FileData(seasonFolder, HardLinkId.Empty, filename);
    }
}
