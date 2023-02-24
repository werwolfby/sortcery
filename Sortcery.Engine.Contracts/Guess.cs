namespace Sortcery.Engine.Contracts;

public record class Guess(string Type, string Title)
{
    public string? Source { get; set; }
    
    public string? VideoCodec { get; set; }
    
    public string? VideoProfile { get; set; }
    
    public string? ScreenSize { get; set; }
    
    public string? ColorDepth { get; set; }
    
    public string? Edition { get; set; }
    
    public string? Container { get; set; }
    
    public string? ReleaseGroup { get; set; }
    
    public string? Mimetype { get; set; }
    
    public string? Other { get; set; }
    
    // Series specific
    public string? EpisodeTitle { get; set; }
    
    public int? Season { get; set; }
    
    public int? Episode { get; set; }
    
    // Movies specific
    public int? Year { get; set; }
}
