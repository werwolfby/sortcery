#if _WINDOWS
using SortceryFileInfo = System.IO.FileInfo;
#else
using SortceryFileInfo = Mono.Unix.UnixFileInfo;
#endif
using Sortcery.Engine.Contracts;

namespace Sortcery.Engine;

public static class FileDataExtensions
{
    public static void Link(this FileData file, FileData target)
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
