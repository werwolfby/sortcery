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

public class Traverser : ITraverser
{
    public FolderData Traverse(string fullName)
    {
        var folderData = new FolderData(fullName);
        Traverse(folderData);
        return folderData;
    }

    private static void Traverse(FolderData folder)
    {
        foreach (var entry in new SortceryDirectoryInfo(folder.FullName).GetFileSystemEntries())
        {
            switch (entry)
            {
                case SortceryDirectoryInfo subDir:
                    var subFolder = folder.AddFolder(subDir.FullName);
                    Traverse(subFolder);
                    break;
                case SortceryFileInfo fileInfo:
                    folder.AddFile(fileInfo.Name, fileInfo.GetHardLinkId());
                    break;
            }
        }
    }
}

internal static class SortceryFileSystemInfoExtensions
{
#if _WINDOWS
    public static IEnumerable<SortceryFileSystemInfo> GetFileSystemEntries(this SortceryDirectoryInfo dir)
    {
        return dir.EnumerateFileSystemInfos();
    }
#endif

    public static HardLinkId GetHardLinkId(this SortceryFileInfo fileInfo)
    {
#if _WINDOWS
        return WinApi.FromFileInfo(fileInfo);
#else
        return new HardLinkId(fileInfo.Inode, fileInfo.Device);
#endif
    }
}


