using Sortcery.Engine.Contracts;

namespace Sortcery.Engine.UnitTests;

public static class Utils
{
    public static string FixPath(this string path) =>
        path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

    public static HardLinkId NewHardLinkId(int inode)
    {
#if _WINDOWS
        return new HardLinkId((uint)inode, 0, 0);
#else
        return new HardLinkId(inode, 0);
#endif
    }
}
