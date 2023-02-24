using FileInfo = Sortcery.Engine.Contracts.FileInfo;
#if _WINDOWS
using SortceryFileInfo = System.IO.FileInfo;
#else
using SortceryFileInfo = Mono.Unix.UnixFileInfo;
#endif

namespace Sortcery.Engine;

public static class FileInfoExtensions
{
    public static void Link(this FileInfo file, FileInfo target)
    {
        var sourcePath = Path.Join(file.Dir.FullName, file.RelativePath);
        var targetPath = Path.Join(target.Dir.FullName, target.RelativePath);
        #if _WINDOWS
        WinApi.CreateHardLink(targetPath, sourcePath);
        #else
        var sourceFileInfo = new SortceryFileInfo(sourcePath);
        sourceFileInfo.CreateLink(targetPath);
        #endif
    }
}
