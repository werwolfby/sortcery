namespace Sortcery.Api.Contracts.Models;

public class HardLinkInfo
{
    public FileInfo? Source { get; set; }

    public List<FileInfo> Targets { get; set; } = default!;
}
