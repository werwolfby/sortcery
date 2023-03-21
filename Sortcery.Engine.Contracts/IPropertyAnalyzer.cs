namespace Sortcery.Engine.Contracts;

public interface IPropertyAnalyzer
{
    void Analyze(IReadOnlyList<HardLinkData> links);
}
