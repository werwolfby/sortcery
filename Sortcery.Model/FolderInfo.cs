using Mono.Unix;

namespace Sortcery.Model;

public class FolderInfo
{
    public FolderInfo(string path)
    {
        Path = path;
    }
    
    public string Path { get; }
    
    public IReadOnlyDictionary<HardLinkId, FileInfo> Traverse()
    {
        var result = new Dictionary<HardLinkId, FileInfo>();
        var path = new UnixDirectoryInfo(Path);
        Traverse(result, path, path);
        return result;
    }
    
    private void Traverse(Dictionary<HardLinkId, FileInfo> result, UnixDirectoryInfo initialDir, UnixDirectoryInfo currentDir)
    {
        // Traverse all over GetSystemEntries() to get all files and directories
        foreach (var entry in currentDir.GetFileSystemEntries())
        {
            switch (entry)
            {
                // If it's a directory, traverse it
                case UnixDirectoryInfo subDir:
                    Traverse(result, initialDir, subDir);
                    break;
                case UnixFileInfo file:
                    // If it's a file, add it to the result
                    var fileInfo = FileInfo.FromUnixFileInfo(initialDir, file);
                    if (!result.ContainsKey(fileInfo.HardLinkId))
                    {
                        result.Add(fileInfo.HardLinkId, fileInfo);
                    }
                    break;
            }
        }
    }
}