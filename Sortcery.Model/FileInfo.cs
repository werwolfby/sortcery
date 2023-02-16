using Mono.Unix;

namespace Sortcery.Model;

public record struct FileInfo(FolderInfo Dir, string RelativePath, HardLinkId HardLinkId)
{
    public static FileInfo FromUnixFileInfo(FolderInfo dir, UnixFileInfo unixFileInfo)
    {
        var relativePath = unixFileInfo.FullName[(dir.FullName.Length + 1)..];
        var hardLinkId = HardLinkId.FromUnixFileInfo(unixFileInfo);
        return new FileInfo(dir, relativePath, hardLinkId);
    }
}
