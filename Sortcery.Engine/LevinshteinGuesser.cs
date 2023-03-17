using Sortcery.Engine.Contracts;
using Zeroshtein;

namespace Sortcery.Engine;

public class LevinshteinGuesser
{
    private readonly IFoldersProvider _foldersProvider;

    public LevinshteinGuesser(IFoldersProvider foldersProvider)
    {
        _foldersProvider = foldersProvider;
    }

    public float SimilarityRatioThreshold { get; set; } = 0.8f;

    public FileData? Guess(FileData source, IReadOnlyList<HardLinkData> links)
    {
        // It makes sense only for root files
        if (source.Dir != _foldersProvider.Source) return null;

        (HardLinkData source, float similarityRatio)? maxSimilar = default;

        foreach (var link in links)
        {
            if (link.Source is null || link.Targets.Count <= 0 || link.Source.Dir != _foldersProvider.Source) continue;
            if (link.Source.HardLinkId == source.HardLinkId || source == link.Source) continue;

            var sourceName = source.Name;
            var targetName = link.Targets[0].Name;
            var similarityRatio = GetSimilarityRatio(sourceName, targetName);
            // If current similarity ratio is less than current minimum, then update minimum
            // comparison is reversed because nullable float comparison will be always false for null
            if (!maxSimilar.HasValue || similarityRatio > maxSimilar.Value.similarityRatio)
            {
                maxSimilar = (link, similarityRatio);
            }
        }

        if (maxSimilar is null) return null;

        // If similarity ratio is less than threshold, then we don't consider it as a match
        if (maxSimilar.Value.similarityRatio < SimilarityRatioThreshold) return null;

        return new FileData(maxSimilar.Value.source.Targets[0].Dir, HardLinkId.Empty, source.Name);
    }

    private float GetSimilarityRatio(string sourceName, string targetName)
    {
        var distance = Levenshtein.Distance(sourceName, targetName);
        return 1.0f - distance / (float)Math.Max(sourceName.Length, targetName.Length);
    }
}
