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
        var sourceFiles = _foldersProvider.Source.GetAllFilesRecursively().ToDictionary(x => x.HardLinkId);
        var destinationFolderFiles = _foldersProvider.DestinationFolders.Values
            .Select(x => (Folder: x, Files: (IReadOnlyDictionary<HardLinkId, FileData>)x.GetAllFilesRecursively().ToDictionary(x => x.HardLinkId).AsReadOnly()))
            .ToList();

        Links = FindLinks(sourceFiles, destinationFolderFiles);
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

    public void Link(FileData sourceFile, FileData destinationFile) => sourceFile.Link(destinationFile);

    internal IReadOnlyList<HardLinkData> FindLinks(
        IReadOnlyDictionary<HardLinkId, FileData> sourceFiles,
        IReadOnlyList<(FolderData Folder, IReadOnlyDictionary<HardLinkId, FileData> Files)> destinationFolderFiles)
    {
        var maxCapacity = Math.Max(
            sourceFiles.Count,
            destinationFolderFiles.Sum(d => d.Files.Count));
        var result = new Dictionary<HardLinkId,(FileData? Source, List<FileData>? Targets)>(maxCapacity);

        // Find all hardlinks in the source folder
        foreach (var (hardLinkId, fileData) in sourceFiles)
        {
            var exists = false;
            foreach (var (_, destinationFiles) in destinationFolderFiles)
            {
                if (!destinationFiles.TryGetValue(hardLinkId, out var destinationFileData)) continue;

                if (!result.TryGetValue(hardLinkId, out var hardLinkData))
                {
                    hardLinkData = (fileData, new List<FileData>());
                    result.Add(hardLinkId, hardLinkData);
                }
                hardLinkData.Targets!.Add(destinationFileData);
                exists = true;
            }
            if (!exists)
            {
                result.Add(hardLinkId, (fileData, null));
            }
        }

        // Find all hardlinks that exists only in at any destination folder but not in the source folder
        foreach (var (_, destinationFiles) in destinationFolderFiles)
        {
            foreach (var (hardLinkId, fileData) in destinationFiles)
            {
                if (result.ContainsKey(hardLinkId)) continue;
                result.Add(hardLinkId, (null, new List<FileData> {fileData}));
            }
        }

        return result
            .Select(x =>
                new HardLinkData(x.Value.Source,
                    x.Value.Targets
                    ?? (IReadOnlyList<FileData>)Array.Empty<FileData>()))
            .ToList()
            .AsReadOnly();
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

        var showFolder = destinationFolder.GetOrAddFolder(guess.Title);
        var seasonFolder = showFolder.GetOrAddFolder($"Season {guess.Season}");

        return new FileData(seasonFolder, HardLinkId.Empty, filename);
    }
}
