namespace Sortcery.Engine.Contracts;

public interface ITraverser
{
    FolderData Traverse(string fullName);
}
