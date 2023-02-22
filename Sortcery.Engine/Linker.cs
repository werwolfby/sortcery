using Sortcery.Engine.Contracts;
using FileInfo = Sortcery.Engine.Contracts.FileInfo;

namespace Sortcery.Engine;

public class Linker
{
    public IReadOnlyList<HardLinkInfo> FindLinks(FolderInfo source, IReadOnlyList<FolderInfo> destinations)
    {
        var sourceFiles = source.Traverse();
        var destinationFolderFiles = destinations
            .Select(x => (Folder: x, Files: x.Traverse()))
            .ToList();
        
        return FindLinks(sourceFiles, destinationFolderFiles);
    }
    
    public IReadOnlyList<HardLinkInfo> FindLinks(
        IReadOnlyDictionary<HardLinkId, FileInfo> sourceFiles, 
        IReadOnlyList<(FolderInfo Folder, IReadOnlyDictionary<HardLinkId, FileInfo> Files)> destinationFolderFiles)
    {
        var maxCapacity = Math.Max(
            sourceFiles.Count, 
            destinationFolderFiles.Sum(d => d.Files.Count));
        var result = new Dictionary<HardLinkId,(FileInfo? Source, List<FileInfo>? Targets)>(maxCapacity);
        
        // Find all hardlinks in the source folder
        foreach (var (hardLinkId, fileInfo) in sourceFiles)
        {
            var exists = false;
            foreach (var (_, destinationFiles) in destinationFolderFiles)
            {
                if (!destinationFiles.TryGetValue(hardLinkId, out var destinationFileInfo)) continue;
                
                if (!result.TryGetValue(hardLinkId, out var hardLinkInfo))
                {
                    hardLinkInfo = (fileInfo, new List<FileInfo>());
                    result.Add(hardLinkId, hardLinkInfo);
                }
                hardLinkInfo.Targets!.Add(destinationFileInfo);
                exists = true;
            }
            if (!exists)
            {
                result.Add(hardLinkId, (fileInfo, null));
            }
        }
        
        // Find all hardlinks that exists only in at any destination folder but not in the source folder
        foreach (var (_, destinationFiles) in destinationFolderFiles)
        {
            foreach (var (hardLinkId, fileInfo) in destinationFiles)
            {
                if (result.ContainsKey(hardLinkId)) continue;
                result.Add(hardLinkId, (null, new List<FileInfo> {fileInfo}));
            }
        }
        
        return result
            .Select(x => 
                new HardLinkInfo(x.Value.Source, 
                    x.Value.Targets 
                    ?? (IReadOnlyList<FileInfo>)Array.Empty<FileInfo>()))
            .ToList();
    }
}
