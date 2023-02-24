namespace Sortcery.Engine.Contracts;

public record struct HardLinkInfo(FileInfo? Source, IReadOnlyList<FileInfo> Targets)
{
}
