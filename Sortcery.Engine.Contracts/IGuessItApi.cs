namespace Sortcery.Engine.Contracts;

public interface IGuessItApi
{
    Task<Guess> GuessAsync(string filename);
}