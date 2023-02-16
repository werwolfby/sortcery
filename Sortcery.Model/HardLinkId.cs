using Mono.Unix;

namespace Sortcery.Model;

public record struct HardLinkId(long Inode, long Device)
{
    public static HardLinkId FromUnixFileInfo(UnixFileInfo unixFileInfo)
    {
        return new HardLinkId(unixFileInfo.Inode, unixFileInfo.Device);
    }
}