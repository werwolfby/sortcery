using Sortcery.Engine.Contracts;

namespace Sortcery.Api.Mapper;

public static class HardLinkDataMapper
{
    public static Contracts.Models.HardLinkData ToHardLinkData(this HardLinkData hardLinkData)
    {
        return new()
        {
            Source = hardLinkData.Source?.ToFileData(),
            Targets = hardLinkData.Targets.Select(x => x.ToFileData()).ToList()
        };
    }

    public static List<Contracts.Models.HardLinkData> ToHardLinkData(this IReadOnlyList<HardLinkData> hardLinks)
    {
        return hardLinks.Select(x => x.ToHardLinkData()).ToList();
    }
}
