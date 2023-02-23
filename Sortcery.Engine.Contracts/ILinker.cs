namespace Sortcery.Engine.Contracts;

public interface ILinker
{
    IReadOnlyList<HardLinkData> FindLinks();

    void Link(FileData sourceFile, FileData destinationFile);
}
