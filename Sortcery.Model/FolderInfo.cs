using Mono.Unix;

namespace Sortcery.Model;

public class FolderInfo
{
    public FolderInfo(string fullName)
    {
        FullName = fullName;
    }
    
    public string FullName { get; }
    
    public IReadOnlyDictionary<HardLinkId, FileInfo> Traverse()
    {
        var result = new Dictionary<HardLinkId, FileInfo>();
        var path = new UnixDirectoryInfo(FullName);
        Traverse(result, path);
        return result;
    }
    
    private void Traverse(Dictionary<HardLinkId, FileInfo> result, UnixDirectoryInfo currentDir)
    {
        // Traverse all over GetSystemEntries() to get all files and directories
        foreach (var entry in currentDir.GetFileSystemEntries())
        {
            switch (entry)
            {
                // If it's a directory, traverse it
                case UnixDirectoryInfo subDir:
                    Traverse(result, subDir);
                    break;
                case UnixFileInfo file:
                    // If it's a file, add it to the result
                    var fileInfo = FileInfo.FromUnixFileInfo(this, file);
                    if (!result.ContainsKey(fileInfo.HardLinkId))
                    {
                        result.Add(fileInfo.HardLinkId, fileInfo);
                    }
                    break;
            }
        }
    }
}