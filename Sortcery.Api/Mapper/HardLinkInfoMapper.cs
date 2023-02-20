using Sortcery.Model;
using HardLinkInfoContract = Sortcery.Api.Contracts.Models.HardLinkInfo;

namespace Sortcery.Api.Mapper;

public static class HardLinkInfoMapper
{
    public static HardLinkInfoContract ToHardLinkInfo(this HardLinkInfo hardLinkInfo, IReadOnlyDictionary<FolderInfo, string> foldersMap)
    {
        return new()
        {
            Source = hardLinkInfo.Source?.ToFileInfo(foldersMap),
            Targets = hardLinkInfo.Targets.Select(x => x.ToFileInfo(foldersMap)).ToList()
        };
    }
    
    public static List<HardLinkInfoContract> ToHardLinkInfo(this IReadOnlyList<HardLinkInfo> hardLinkInfos, IReadOnlyDictionary<FolderInfo, string> foldersMap)
    {
        return hardLinkInfos.Select(x => x.ToHardLinkInfo(foldersMap)).ToList();
    }
}
