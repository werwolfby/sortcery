namespace Sortcery.Api.Contracts.Models;

public class HardLinkData
{
    public FileData? Source { get; set; }

    public List<FileData> Targets { get; set; } = default!;
}
