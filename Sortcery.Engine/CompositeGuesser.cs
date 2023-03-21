using Sortcery.Engine.Contracts;

namespace Sortcery.Engine;

public class CompositeGuesser : ISmartGuesser
{
    private readonly IEnumerable<IGuesser> _guessers;

    public CompositeGuesser(IEnumerable<IGuesser> guessers)
    {
        _guessers = guessers;
    }

    public async ValueTask<FileData?> GuessAsync(FileData source, IReadOnlyList<HardLinkData> links)
    {
        foreach (var guesser in _guessers)
        {
            var result = await guesser.GuessAsync(source, links);
            if (result != null) return result;
        }

        return null;
    }
}
