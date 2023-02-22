using Sortcery.Engine.Contracts;
using FileInfo = Sortcery.Engine.Contracts.FileInfo;
#if _WINDOWS
using SortceryFileSystemInfo = System.IO.FileSystemInfo;
using SortceryDirectoryInfo = System.IO.DirectoryInfo;
using SortceryFileInfo = System.IO.FileInfo;
#else
using SortceryFileSystemInfo = Mono.Unix.UnixFileSystemInfo;
using SortceryDirectoryInfo = Mono.Unix.UnixDirectoryInfo;
using SortceryFileInfo = Mono.Unix.UnixFileInfo;
#endif

namespace Sortcery.Engine;

public static class FolderInfoExtensions
{
    public static IReadOnlyDictionary<HardLinkId, FileInfo> Traverse(this FolderInfo dir)
    {
        var files = new Dictionary<HardLinkId, FileInfo>();
        var unixDir = new SortceryDirectoryInfo(dir.FullName);
        Traverse(dir, unixDir, files);
        return files;
    }

    private static void Traverse(FolderInfo dir, SortceryDirectoryInfo currentDir, Dictionary<HardLinkId,FileInfo> result)
    {
        // Traverse all over GetSystemEntries() to get all files and directories
        foreach (var entry in currentDir.GetFileSystemEntries())
        {
            switch (entry)
            {
                // If it's a directory, traverse it
                case SortceryDirectoryInfo subDir:
                    Traverse(dir, subDir, result);
                    break;
                case SortceryFileInfo file:
                    // If it's a file, add it to the result
                    var (fileInfo, hardLinkId) = FromUnixFileInfo(dir, file);
                    if (!result.ContainsKey(hardLinkId))
                    {
                        result.Add(hardLinkId, fileInfo);
                    }
                    break;
            }
        }
    }
    
    #if _WINDOWS
    private static IEnumerable<SortceryFileSystemInfo> GetFileSystemEntries(this SortceryDirectoryInfo dir)
    {
        return dir.EnumerateFileSystemInfos();
    }
    #endif

    private static (FileInfo, HardLinkId) FromUnixFileInfo(FolderInfo dir, SortceryFileInfo fileInfo)
    {
        var relativePath = fileInfo.FullName[(dir.FullName.Length + 1)..];
        var hardLinkId = FromUnixFileInfo(fileInfo);
        return (new FileInfo(dir, relativePath), hardLinkId);
    }

    private static HardLinkId FromUnixFileInfo(SortceryFileInfo fileInfo)
    {
        #if _WINDOWS
        return WinApi.FromFileInfo(fileInfo);
        #else
        return new HardLinkId(fileInfo.Inode, fileInfo.Device);
        #endif
    }
}
