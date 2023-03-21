using Sortcery.Engine.Contracts;

namespace Sortcery.Engine;

public class Linker : ILinker
{
    private readonly IFoldersProvider _foldersProvider;
    private readonly IPropertyAnalyzer _propertyAnalyzer;
    private readonly ISmartGuesser _guesser;

    public Linker(IFoldersProvider foldersProvider, IPropertyAnalyzer propertyAnalyzer, ISmartGuesser guesser)
    {
        _foldersProvider = foldersProvider;
        _propertyAnalyzer = propertyAnalyzer;
        _guesser = guesser;

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
            .Select(x => new HardLinkData(x.Source, x.Targets?.AsReadOnly() ?? (IReadOnlyList<FileData>)Array.Empty<FileData>()))
            .ToList()
            .AsReadOnly();
    }

    public async Task<FileData> GuessAsync(FileData fileData)
    {
        _foldersProvider.Source.ClearPropertiesRecursively();
        _propertyAnalyzer.Analyze(Links);
        var result = await _guesser.GuessAsync(fileData, Links);
        return result!;
    }

    public bool Link(FileData sourceFile, FileData destinationFile)
    {
        if (!sourceFile.Link(destinationFile)) return false;
        destinationFile.Dir.AddFile(destinationFile);
        return true;
    }
}
