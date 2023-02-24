namespace Sortcery.Api;

public class FoldersOptions
{
    public const string Folders = "Folders";
    
    public string Source { get; set; } = string.Empty;
    
    public string Movies { get; set; } = string.Empty;
    
    public string Series { get; set; } = string.Empty;

    public bool IsValid
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Source))
            {
                return false;
            }
        
            if (string.IsNullOrWhiteSpace(Movies))
            {
                return false;
            }
        
            if (string.IsNullOrWhiteSpace(Series))
            {
                return false;
            }
        
            return true;
        }
    }
}
