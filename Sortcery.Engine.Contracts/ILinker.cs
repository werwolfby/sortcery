namespace Sortcery.Engine.Contracts;

public interface ILinker
{
    IReadOnlyList<HardLinkData> Links { get; }

    void Update();

    Task<FileData> GuessAsync(FileData fileData);

    void Link(FileData sourceFile, FileData destinationFile);
}
