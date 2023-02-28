using Sortcery.Engine.Contracts;

namespace Sortcery.Api.Mapper;

public static class FileDataMapper
{
    public static Contracts.Models.FileData ToApi(this FileData fileData)
    {
        return new()
        {
            Dir = fileData.Dir.Root.Name,
            Path = fileData.Dir.RelativePath,
            Name = fileData.Name
        };
    }
}
