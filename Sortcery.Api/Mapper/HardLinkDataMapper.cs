using Sortcery.Engine.Contracts;

namespace Sortcery.Api.Mapper;

public static class HardLinkDataMapper
{
    public static Contracts.Models.HardLinkData ToHardLinkData(this HardLinkData hardLinkData, IReadOnlyDictionary<FolderData, string> foldersMap)
    {
        return new()
        {
            Source = hardLinkData.Source?.ToFileData(foldersMap),
            Targets = hardLinkData.Targets.Select(x => x.ToFileData(foldersMap)).ToList()
        };
    }

    public static List<Contracts.Models.HardLinkData> ToHardLinkData(this IReadOnlyList<HardLinkData> hardLinks, IReadOnlyDictionary<FolderData, string> foldersMap)
    {
        return hardLinks.Select(x => x.ToHardLinkData(foldersMap)).ToList();
    }
}
