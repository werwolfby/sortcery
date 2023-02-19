namespace Sortcery.Model;

public record struct HardLinkInfo(FileInfo? Source, IReadOnlyList<FileInfo> Targets)
{
}
