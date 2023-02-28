using Sortcery.Engine.Contracts;
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

internal static class FolderDataExtensions
{
    internal static IReadOnlyDictionary<HardLinkId, FileData> Traverse(this FolderData dir)
    {
        var files = new Dictionary<HardLinkId, FileData>();
        var unixDir = new SortceryDirectoryInfo(dir.FullName);
        Traverse(dir, unixDir, files);
        return files;
    }

    private static void Traverse(FolderData dir, SortceryDirectoryInfo currentDir, Dictionary<HardLinkId,FileData> result)
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
                    var (fileData, hardLinkId) = FromSortceryFileInfo(dir, file);
                    if (!result.ContainsKey(hardLinkId))
                    {
                        result.Add(hardLinkId, fileData);
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

    private static (FileData, HardLinkId) FromSortceryFileInfo(FolderData dir, SortceryFileInfo fileInfo)
    {
        var hardLinkId = FromSortceryFileInfo(fileInfo);
        return (new FileData(dir, hardLinkId, fileInfo.Name), hardLinkId);
    }

    private static HardLinkId FromSortceryFileInfo(SortceryFileInfo fileInfo)
    {
        #if _WINDOWS
        return WinApi.FromFileInfo(fileInfo);
        #else
        return new HardLinkId(fileInfo.Inode, fileInfo.Device);
        #endif
    }
}
