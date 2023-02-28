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

    public void Traverse(FolderData folder)
    {
        var existingFolders = new HashSet<string>(folder.Folders.Select(x => x.Name));
        var existingFiles = new HashSet<string>(folder.Files.Select(x => x.Name));

        foreach (var entry in new SortceryDirectoryInfo(folder.FullName).GetFileSystemEntries())
        {
            switch (entry)
            {
                case SortceryDirectoryInfo subDir:
                    var subFolder = folder.GetOrAddFolder(subDir.Name);
                    existingFolders.Remove(subFolder.Name);
                    Traverse(subFolder);
                    break;
                case SortceryFileInfo fileInfo:
                    var hardLinkId = fileInfo.GetHardLinkId();
                    var file = folder.GetOrAddFile(fileInfo.Name, hardLinkId);
                    file.HardLinkId = hardLinkId;
                    existingFiles.Remove(fileInfo.Name);
                    break;
            }
        }

        foreach (var folderToRemove in existingFolders)
        {
            folder.RemoveFolder(folderToRemove);
        }

        foreach (var fileToRemove in existingFiles)
        {
            folder.RemoveFile(fileToRemove);
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


