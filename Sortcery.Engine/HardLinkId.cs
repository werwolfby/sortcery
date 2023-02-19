namespace Sortcery.Engine;

#if _WINDOWS
#else
public record struct HardLinkId(long Inode, long Device)
{
}
#endif
