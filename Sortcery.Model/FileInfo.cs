using Mono.Unix;

namespace Sortcery.Model;

public record struct FileInfo(string Dir, string RelativePath, HardLinkId HardLinkId)
{
    public static FileInfo FromUnixFileInfo(UnixDirectoryInfo startDir, UnixFileInfo unixFileInfo)
    {
        var dir = startDir.FullName;
        var relativePath = unixFileInfo.FullName[(dir.Length + 1)..];
        var hardLinkId = HardLinkId.FromUnixFileInfo(unixFileInfo);
        return new FileInfo(dir, relativePath, hardLinkId);
    }
}
