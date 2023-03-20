using Moq;
using Sortcery.Engine.Contracts;

namespace Sortcery.Engine.UnitTests;

public class ShowsGuesserTests
{
    [TestCaseSource(nameof(GetTestCases))]
    public async Task ShowsGuesser_Guess(IFoldersProvider foldersProvider, FileData sourceFile, FileData? destinationFile)
    {
        var guesser = new ShowsGuesser(foldersProvider);
        var result = await guesser.GuessAsync(sourceFile, Array.Empty<HardLinkData>());

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Dir, Is.EqualTo(destinationFile.Dir));
        Assert.That(result.HardLinkId, Is.EqualTo(destinationFile.HardLinkId));
        Assert.That(result.Name, Is.EqualTo(destinationFile.Name));
    }

    public static IEnumerable<TestCaseData> GetTestCases()
    {
        yield return new GuesserTestCaseData("TBBT/S09 to The Big Bang Theory/Season 09", "/Downloads",
                (FolderType.Shows, "/Shows"))
            .AddSource("TBBT/S09/TBBT.S09E01.mkv", 1)
            .AddSource("TBBT/S09/TBBT.S09E02.mkv", 2)
            .SetSourceProps("TBBT/S09",
                ("Show", "the big bang theory"),
                ("ShowFolder", "The Big Bang Theory"),
                ("SeasonFolder", "Season 09"),
                ("SeasonFormat", "Season {0:D2}"),
                ("Season", 9))
            .SetSourceProps("TBBT",
                ("Show", "the big bang theory"),
                ("ShowFolder", "The Big Bang Theory"),
                ("SeasonFolder", "Season 09"),
                ("SeasonFormat", "Season {0:D2}"),
                ("Season", 9))
            .AddTarget(FolderType.Shows, "The Big Bang Theory/Season 09/TBBT.S09E01.mkv", 1)
            .CreateTestCaseData("TBBT/S09/TBBT.S09E02.mkv", (FolderType.Shows, "The Big Bang Theory/Season 09/TBBT.S09E02.mkv"));

        yield return new GuesserTestCaseData("17 мгновений весны to 17 Мгновений Весны", "/Downloads",
                (FolderType.Shows, "/Shows"))
            .AddSource("17 мгновений весны/17 мгновений весны 1.mkv", 1)
            .AddSource("17 мгновений весны/17 мгновений весны 2.mkv", 2)
            .AddTarget(FolderType.Shows, "17 Мгновений Весны/17 мгновений весны 1.mkv", 1)
            .SetSourceProps("17 мгновений весны",
                ("Show", "17 Мгновений Весны"),
                ("ShowFolder", "17 Мгновений Весны"))
            .CreateTestCaseData("17 мгновений весны/17 мгновений весны 2.mkv", (FolderType.Shows, "17 Мгновений Весны/17 мгновений весны 2.mkv"));
    }

    private class GuesserTestCaseData : GuesserTestCaseDataBase<GuesserTestCaseData>
    {
        public GuesserTestCaseData(string name, string sourceDir, params (FolderType type, string path)[] targetDirs) : base(name, sourceDir, targetDirs)
        {
        }

        protected override TestCaseData CreateTestCaseData(Mock<IGuessItApi> guessItApiMock,
            Mock<IFoldersProvider> foldersProviderMock, IReadOnlyList<HardLinkData> hardlinks, FileData sourceFile,
            FileData? destinationFile) => new(foldersProviderMock.Object, sourceFile, destinationFile);
    }
}
