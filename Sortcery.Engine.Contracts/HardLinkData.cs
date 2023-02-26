namespace Sortcery.Engine.Contracts;

public record struct HardLinkData(FileData? Source, IReadOnlyList<FileData> Targets)
{
}
