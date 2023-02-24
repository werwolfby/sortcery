using Sortcery.Engine.Contracts;

namespace Sortcery.Api.Mapper;

public static class FileDataMapper
{
    public static Contracts.Models.FileData ToFileData(this FileData fileData)
    {
        return new()
        {
            Dir = fileData.Dir.Name,
            Path = fileData.Path.ToArray(),
            Name = fileData.Name
        };
    }
}
