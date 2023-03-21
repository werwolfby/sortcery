#if _WINDOWS
using SortceryFileInfo = System.IO.FileInfo;
#else
using SortceryFileInfo = Mono.Unix.UnixFileInfo;
#endif
using Sortcery.Engine.Contracts;

namespace Sortcery.Engine;

internal static class FileDataExtensions
{
    internal static bool Link(this FileData file, FileData target)
    {
        var sourcePath = file.FullName;
        var targetPath = target.FullName;
        var targetFileInfo = new SortceryFileInfo(targetPath);
        if (!targetFileInfo.Directory!.Exists)
        {
            targetFileInfo.Directory!.Create();
        }
        #if _WINDOWS
        return WinApi.CreateHardLink(targetPath, sourcePath);
        #else
        var sourceFileInfo = new SortceryFileInfo(sourcePath);
        try
        {
            sourceFileInfo.CreateLink(targetPath);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
        #endif
    }
}
