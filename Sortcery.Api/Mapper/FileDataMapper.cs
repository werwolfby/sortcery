using Sortcery.Engine.Contracts;

namespace Sortcery.Api.Mapper;

public static class FileDataMapper
{
    public static Contracts.Models.FileData ToFileData(this FileData fileData, IReadOnlyDictionary<FolderData, string> foldersMap)
    {
        return new()
        {
            Dir = foldersMap[fileData.Dir],
            RelativePath = fileData.RelativePath
        };
    }
}
