using System.Text.Json.Serialization;

namespace Sortcery.Api.Contracts.Models;

public class FileData
{
    public string Dir { get; set; }

    public string[] Path { get; set; }

    public string Name { get; set; }

    [JsonIgnore]
    public string RelativePath
    {
        get
        {
            var paths = new string[Path.Length + 1];
            Path.CopyTo(paths, 0);
            paths[^1] = Name;

            return System.IO.Path.Combine(paths);
        }
    }

    [JsonIgnore]
    public string FullName
    {
        get
        {
            var paths = new string[Path.Length + 2];
            paths[0] = Dir;
            Path.CopyTo(paths, 1);
            paths[^1] = Name;

            return System.IO.Path.Combine(paths);
        }
    }
}
