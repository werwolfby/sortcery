#if _WINDOWS
using SortceryFileInfo = System.IO.FileInfo;
#else
using SortceryFileInfo = Mono.Unix.UnixFileInfo;
#endif
using Sortcery.Engine.Contracts;

namespace Sortcery.Engine;

internal static class FileDataExtensions
{
    internal static void Link(this FileData file, FileData target)
    {
        var sourcePath = file.FullName;
        var targetPath = target.FullName;
        var targetFileInfo = new SortceryFileInfo(targetPath);
        targetFileInfo.Directory!.Create();
        #if _WINDOWS
        WinApi.CreateHardLink(targetPath, sourcePath);
        #else
        var sourceFileInfo = new SortceryFileInfo(sourcePath);
        sourceFileInfo.CreateLink(targetPath);
        #endif
    }
}
