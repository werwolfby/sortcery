using Mono.Unix;
using Sortcery.Model;
using FileInfo = Sortcery.Model.FileInfo;

namespace Sortcery.Engine;

public class Traverser
{
    public IReadOnlyDictionary<HardLinkId, FileInfo> Traverse(FolderInfo dir)
    {
        var files = new Dictionary<HardLinkId, FileInfo>();
        var unixDir = new UnixDirectoryInfo(dir.FullName);
        Traverse(dir, unixDir, files);
        return files;
    }

    private void Traverse(FolderInfo dir, UnixDirectoryInfo currentDir, Dictionary<HardLinkId,FileInfo> result)
    {
        // Traverse all over GetSystemEntries() to get all files and directories
        foreach (var entry in currentDir.GetFileSystemEntries())
        {
            switch (entry)
            {
                // If it's a directory, traverse it
                case UnixDirectoryInfo subDir:
                    Traverse(dir, subDir, result);
                    break;
                case UnixFileInfo file:
                    // If it's a file, add it to the result
                    var fileInfo = FromUnixFileInfo(dir, file);
                    if (!result.ContainsKey(fileInfo.HardLinkId))
                    {
                        result.Add(fileInfo.HardLinkId, fileInfo);
                    }
                    break;
            }
        }
    }

    private static FileInfo FromUnixFileInfo(FolderInfo dir, UnixFileInfo unixFileInfo)
    {
        var relativePath = unixFileInfo.FullName[(dir.FullName.Length + 1)..];
        var hardLinkId = FromUnixFileInfo(unixFileInfo);
        return new FileInfo(dir, relativePath, hardLinkId);
    }

    private static HardLinkId FromUnixFileInfo(UnixFileInfo unixFileInfo)
    {
        return new HardLinkId(unixFileInfo.Inode, unixFileInfo.Device);
    }
}
