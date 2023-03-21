namespace Sortcery.Engine.UnitTests;

public class SeasonFolderParserTests
{
    [TestCase("Season 1", "Season {0}", 1, true, TestName = "Season 1")]
    [TestCase("Season 2", "Season {0}", 2, true, TestName = "Season 2")]
    [TestCase("Season 01", "Season {0:D2}", 1, true, TestName = "Season 01")]
    [TestCase("1", "{0}", 1, true, TestName = "1")]
    [TestCase("2", "{0}", 2, true, TestName = "2")]
    [TestCase("02", "{0:D2}", 2, true, TestName = "02")]
    [TestCase("S1", "S{0}", 1, true, TestName = "S1")]
    [TestCase("S01", "S{0:D2}", 1, true, TestName = "S01")]
    [TestCase("S2", "S{0}", 2, true, TestName = "S2")]
    [TestCase("S 1", "S {0}", 1, true, TestName = "S 1")]
    [TestCase("S 01", "S {0:D2}", 1, true, TestName = "S 01")]
    [TestCase("S 2", "S {0}", 2, true, TestName = "S 2")]
    [TestCase("Season One", null, 0, false, TestName = "Season One")]
    public void Parse_WithValidSeasonFolder_ReturnsExpectedFormatAndSeason(string name, string expectedFormat, int expectedSeason, bool success)
    {
        Assert.That(SeasonFolderParser.TryParse(name, out var format, out var season), Is.EqualTo(success));
        Assert.That(format, Is.EqualTo(expectedFormat));
        Assert.That(season, Is.EqualTo(expectedSeason));
    }
}
