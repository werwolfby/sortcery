using System.Text.Json.Serialization;

namespace Sortcery.Api.Contracts.Models;

public class FileData
{
    public string Dir { get; set; }

    public string Path { get; set; }

    public string Name { get; set; }

    [JsonIgnore]
    public string RelativeName => System.IO.Path.Join(Path, Name);

    [JsonIgnore]
    public string FullName => System.IO.Path.Join(Dir, RelativeName);
}
