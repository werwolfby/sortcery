namespace Sortcery.Engine.Contracts;

public interface IGuesser
{
    public ValueTask<FileData?> GuessAsync(FileData source, IReadOnlyList<HardLinkData> links);
}
