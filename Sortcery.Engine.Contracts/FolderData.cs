namespace Sortcery.Engine.Contracts;

public class FolderData
{
    public FolderData(string fullName)
    {
        FullName = fullName;
    }

    public string FullName { get; }
}
