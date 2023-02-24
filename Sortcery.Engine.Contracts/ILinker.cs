namespace Sortcery.Engine.Contracts;

public interface ILinker
{
    IReadOnlyList<HardLinkData> Links { get; }

    void Update();

    void Link(FileData sourceFile, FileData destinationFile);
}
