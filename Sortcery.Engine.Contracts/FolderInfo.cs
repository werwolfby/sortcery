namespace Sortcery.Engine.Contracts;

public class FolderInfo
{
    public FolderInfo(string fullName)
    {
        FullName = fullName;
    }
    
    public string FullName { get; }
}
