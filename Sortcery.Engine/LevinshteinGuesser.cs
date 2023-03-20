using Sortcery.Engine.Contracts;
using Zeroshtein;

namespace Sortcery.Engine;

public readonly record struct SimilarityRatio(int Distance, int Length)
{
    public float Ratio => 1.0f - Distance / (float)Length;

    public static bool operator > (SimilarityRatio left, SimilarityRatio right) => left.Ratio > right.Ratio;

    public static bool operator < (SimilarityRatio left, SimilarityRatio right) => left.Ratio < right.Ratio;
}

public class SimilarityRatios
{
    private readonly List<SimilarityRatio> _rations = new();

    public float Average => _rations.Average(x => x.Ratio);

    public void Add(SimilarityRatio ratio) => _rations.Add(ratio);
}

public class LevinshteinGuesser : IGuesser
{
    private readonly IFoldersProvider _foldersProvider;

    public LevinshteinGuesser(IFoldersProvider foldersProvider)
    {
        _foldersProvider = foldersProvider;
    }

    public float SimilarityRatioThreshold { get; set; } = 0.8f;

    public ValueTask<FileData?> GuessAsync(FileData source, IReadOnlyList<HardLinkData> links)
    {
        // It makes sense only for root files
        if (source.Dir != _foldersProvider.Source) return ValueTask.FromResult<FileData?>(null);

        var similarities =  new Dictionary<SimilarityRatio, List<HardLinkData>>();

        foreach (var link in links)
        {
            if (link.Source is null || link.Targets.Count <= 0 || link.Source.Dir != _foldersProvider.Source) continue;
            if (link.Source.HardLinkId == source.HardLinkId || source == link.Source) continue;

            var similarityRatio = GetSimilarityRatio(source.Name, link.Source.Name);
            // If current similarity ratio is less than current minimum, then update minimum
            // comparison is reversed because nullable float comparison will be always false for null
            if (similarityRatio.Ratio < SimilarityRatioThreshold) continue;

            similarities.Add(similarityRatio, link);
        }

        if (similarities.Count <= 0) return ValueTask.FromResult<FileData?>(null);

        var destinations = new Dictionary<FolderData, SimilarityRatios>();

        foreach (var (similarityRatio, similarLinks) in similarities)
        {
            foreach (var similarLink in similarLinks)
            {
                foreach (var target in similarLink.Targets)
                {
                    destinations.GetOrAdd(target.Dir).Add(similarityRatio);
                }
            }
        }

        var maxSimilarity = destinations.MaxBy(x => x.Value.Average);

        return ValueTask.FromResult<FileData?>(new FileData(maxSimilarity.Key, HardLinkId.Empty, source.Name));
    }

    private SimilarityRatio GetSimilarityRatio(string sourceName, string targetName)
    {
        var distance = Levenshtein.Distance(sourceName, targetName);
        return new SimilarityRatio(distance, Math.Max(sourceName.Length, targetName.Length));
    }
}
