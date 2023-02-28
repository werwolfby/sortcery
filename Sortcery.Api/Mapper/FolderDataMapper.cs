using Sortcery.Engine.Contracts;

namespace Sortcery.Api.Mapper;

public static class FolderDataMapper
{
    public static Contracts.Models.FolderData ToApi(this FolderData folderData)
    {
        return new()
        {
            Name = folderData.Name,
            Path = folderData.Path,
            Type = folderData.Type.ToString()
        };
    }
}
