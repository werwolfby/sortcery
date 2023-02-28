using System.Text.Json.Serialization;

namespace Sortcery.Api.Contracts.Models;

public class FileData
{
    public string Dir { get; set; }

    public string Path { get; set; }

    public string Name { get; set; }

    [JsonIgnore]
    public string RelativeName => Path + Name;
}
