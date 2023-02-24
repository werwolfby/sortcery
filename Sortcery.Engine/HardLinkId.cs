namespace Sortcery.Engine;

#if _WINDOWS
public record struct HardLinkId(uint FileIndexLow, uint FileIndexHigh, uint VolumeSerialNumber)
{
}
#else
public record struct HardLinkId(long Inode, long Device)
{
}
#endif
