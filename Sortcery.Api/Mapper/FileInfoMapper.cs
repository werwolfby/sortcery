using Sortcery.Model;
using FileInfo = Sortcery.Model.FileInfo;
using FileInfoContract = Sortcery.Api.Contracts.Models.FileInfo;

namespace Sortcery.Api.Mapper;

public static class FileInfoMapper
{
    public static FileInfoContract ToFileInfo(this FileInfo fileInfo, IReadOnlyDictionary<FolderInfo, string> foldersMap)
    {
        return new()
        {
            Dir = foldersMap[fileInfo.Dir],
            RelativePath = fileInfo.RelativePath
        };
    }
}