using Sortcery.Engine.Contracts;

namespace Sortcery.Engine;

public class Linker
{
    public IReadOnlyList<HardLinkData> FindLinks(FolderData source, IReadOnlyList<FolderData> destinations)
    {
        var sourceFiles = source.Traverse();
        var destinationFolderFiles = destinations
            .Select(x => (Folder: x, Files: x.Traverse()))
            .ToList();

        return FindLinks(sourceFiles, destinationFolderFiles);
    }

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
            .ToList();
    }

    public void Link(FileData sourceFile, FileData destinationFile) => sourceFile.Link(destinationFile);
}
