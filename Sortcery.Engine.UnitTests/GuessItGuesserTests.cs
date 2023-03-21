using Moq;
using Sortcery.Engine.Contracts;

namespace Sortcery.Engine.UnitTests;

public class GuessItGuesserTests
{
    [TestCaseSource(nameof(GetTestCases))]
    public async Task GuessItGuesser_Guess(IGuessItApi guessItApi, IFoldersProvider foldersProvider, FileData sourceFile, FileData destinationFile)
    {
        var guesser = new GuessItGuesser(guessItApi, foldersProvider);
        var result = await guesser.GuessAsync(sourceFile, Array.Empty<HardLinkData>());

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Dir.FullName, Is.EqualTo(destinationFile.Dir.FullName));
        Assert.That(result.HardLinkId, Is.EqualTo(destinationFile.HardLinkId));
        Assert.That(result.Name, Is.EqualTo(destinationFile.Name));
    }

    public static IEnumerable<TestCaseData> GetTestCases()
    {
        yield return new GuesserTestCaseData("Flash to The Flash folder", "/Downloads", (FolderType.Shows, "/Shows"), (FolderType.Movies, "/Movies"))
            .AddSource("Flash.S09E01.1080p.rus.LostFilm.TV.mkv", 1)
            .AddGuess("Flash.S09E01.1080p.rus.LostFilm.TV.mkv", new Guess("episode", "Flash") { Season = 9, Episode = 1, ReleaseGroup = "LostFilm.TV", ScreenSize = "1080p"})
            .CreateTestCaseData("Flash.S09E01.1080p.rus.LostFilm.TV.mkv", (FolderType.Shows, "Flash/Season 9/Flash.S09E01.1080p.rus.LostFilm.TV.mkv"));

        yield return new GuesserTestCaseData("Flash to The Flash folder reuse show case", "/Downloads", (FolderType.Shows, "/Shows"), (FolderType.Movies, "/Movies"))
            .AddSource("Flash.S09E01.1080p.rus.LostFilm.TV.mkv", 1)
            .AddSource("Flash.S09E02.1080p.rus.LostFilm.TV.mkv", 2)
            .AddTarget(FolderType.Shows, "flash/Season 9/Flash.S09E01.1080p.rus.LostFilm.TV.mkv", 1)
            .AddGuess("Flash.S09E02.1080p.rus.LostFilm.TV.mkv", new Guess("episode", "Flash") { Season = 9, Episode = 2, ReleaseGroup = "LostFilm.TV", ScreenSize = "1080p"})
            .CreateTestCaseData("Flash.S09E02.1080p.rus.LostFilm.TV.mkv", (FolderType.Shows, "flash/Season 9/Flash.S09E02.1080p.rus.LostFilm.TV.mkv"));

        yield return new GuesserTestCaseData("Flash to The Flash folder reuse season case", "/Downloads", (FolderType.Shows, "/Shows"), (FolderType.Movies, "/Movies"))
            .AddSource("Flash.S09E01.1080p.rus.LostFilm.TV.mkv", 1)
            .AddSource("Flash.S09E02.1080p.rus.LostFilm.TV.mkv", 2)
            .AddTarget(FolderType.Shows, "Flash/season 9/Flash.S09E01.1080p.rus.LostFilm.TV.mkv", 1)
            .AddGuess("Flash.S09E02.1080p.rus.LostFilm.TV.mkv", new Guess("episode", "Flash") { Season = 9, Episode = 2, ReleaseGroup = "LostFilm.TV", ScreenSize = "1080p"})
            .CreateTestCaseData("Flash.S09E02.1080p.rus.LostFilm.TV.mkv", (FolderType.Shows, "Flash/season 9/Flash.S09E02.1080p.rus.LostFilm.TV.mkv"));

        yield return new GuesserTestCaseData("Flash to The Flash folder reuse case", "/Downloads", (FolderType.Shows, "/Shows"), (FolderType.Movies, "/Movies"))
            .AddSource("Flash.S09E01.1080p.rus.LostFilm.TV.mkv", 1)
            .AddSource("Flash.S09E02.1080p.rus.LostFilm.TV.mkv", 2)
            .AddTarget(FolderType.Shows, "flash/season 9/Flash.S09E01.1080p.rus.LostFilm.TV.mkv", 1)
            .AddGuess("Flash.S09E02.1080p.rus.LostFilm.TV.mkv", new Guess("episode", "Flash") { Season = 9, Episode = 2, ReleaseGroup = "LostFilm.TV", ScreenSize = "1080p"})
            .CreateTestCaseData("Flash.S09E02.1080p.rus.LostFilm.TV.mkv", (FolderType.Shows, "flash/season 9/Flash.S09E02.1080p.rus.LostFilm.TV.mkv"));

        yield return new GuesserTestCaseData("Flash to The Flash folder reuse season 0:D2 pattern case", "/Downloads", (FolderType.Shows, "/Shows"), (FolderType.Movies, "/Movies"))
            .AddSource("Flash.S09E01.1080p.rus.LostFilm.TV.mkv", 1)
            .AddSource("Flash.S09E02.1080p.rus.LostFilm.TV.mkv", 2)
            .AddTarget(FolderType.Shows, "Flash/season 09/Flash.S09E01.1080p.rus.LostFilm.TV.mkv", 1)
            .AddGuess("Flash.S09E02.1080p.rus.LostFilm.TV.mkv", new Guess("episode", "Flash") { Season = 9, Episode = 2, ReleaseGroup = "LostFilm.TV", ScreenSize = "1080p"})
            .CreateTestCaseData("Flash.S09E02.1080p.rus.LostFilm.TV.mkv", (FolderType.Shows, "Flash/season 09/Flash.S09E02.1080p.rus.LostFilm.TV.mkv"));

        yield return new GuesserTestCaseData("Flash to The Flash folder reuse SeaSON 0:D2 pattern case", "/Downloads", (FolderType.Shows, "/Shows"), (FolderType.Movies, "/Movies"))
            .AddSource("Flash.S09E01.1080p.rus.LostFilm.TV.mkv", 1)
            .AddSource("Flash.S09E02.1080p.rus.LostFilm.TV.mkv", 2)
            .AddTarget(FolderType.Shows, "Flash/SeaSON 09/Flash.S09E01.1080p.rus.LostFilm.TV.mkv", 1)
            .AddGuess("Flash.S09E02.1080p.rus.LostFilm.TV.mkv", new Guess("episode", "Flash") { Season = 9, Episode = 2, ReleaseGroup = "LostFilm.TV", ScreenSize = "1080p"})
            .CreateTestCaseData("Flash.S09E02.1080p.rus.LostFilm.TV.mkv", (FolderType.Shows, "Flash/SeaSON 09/Flash.S09E02.1080p.rus.LostFilm.TV.mkv"));

        yield return new GuesserTestCaseData("Flash to The Flash folder reuse S0 pattern case", "/Downloads", (FolderType.Shows, "/Shows"), (FolderType.Movies, "/Movies"))
            .AddSource("Flash.S09E01.1080p.rus.LostFilm.TV.mkv", 1)
            .AddSource("Flash.S09E02.1080p.rus.LostFilm.TV.mkv", 2)
            .AddTarget(FolderType.Shows, "Flash/S9/Flash.S09E01.1080p.rus.LostFilm.TV.mkv", 1)
            .AddGuess("Flash.S09E02.1080p.rus.LostFilm.TV.mkv", new Guess("episode", "Flash") { Season = 9, Episode = 2, ReleaseGroup = "LostFilm.TV", ScreenSize = "1080p"})
            .CreateTestCaseData("Flash.S09E02.1080p.rus.LostFilm.TV.mkv", (FolderType.Shows, "Flash/S9/Flash.S09E02.1080p.rus.LostFilm.TV.mkv"));

        yield return new GuesserTestCaseData("Flash to The Flash folder reuse pattern case from single case", "/Downloads", (FolderType.Shows, "/Shows"), (FolderType.Movies, "/Movies"))
            .AddSource("Flash.S06E01.1080p.rus.LostFilm.TV.mkv", 1)
            .AddSource("Flash.S07E01.1080p.rus.LostFilm.TV.mkv", 2)
            .AddSource("Flash.S08E01.1080p.rus.LostFilm.TV.mkv", 3)
            .AddSource("Flash.S09E01.1080p.rus.LostFilm.TV.mkv", 4)
            .AddTarget(FolderType.Shows, "Flash/s06/Flash.S06E01.1080p.rus.LostFilm.TV.mkv", 1)
            .AddTarget(FolderType.Shows, "Flash/s07/Flash.S07E01.1080p.rus.LostFilm.TV.mkv", 2)
            .AddTarget(FolderType.Shows, "Flash/s08/Flash.S08E01.1080p.rus.LostFilm.TV.mkv", 3)
            .AddGuess("Flash.S09E01.1080p.rus.LostFilm.TV.mkv", new Guess("episode", "Flash") { Season = 9, Episode = 1, ReleaseGroup = "LostFilm.TV", ScreenSize = "1080p"})
            .CreateTestCaseData("Flash.S09E01.1080p.rus.LostFilm.TV.mkv", (FolderType.Shows, "Flash/s09/Flash.S09E01.1080p.rus.LostFilm.TV.mkv"));

        yield return new GuesserTestCaseData("Flash to The Flash folder reuse max pattern case from multiple cases", "/Downloads", (FolderType.Shows, "/Shows"), (FolderType.Movies, "/Movies"))
            .AddSource("Flash.S06E01.1080p.rus.LostFilm.TV.mkv", 1)
            .AddSource("Flash.S07E01.1080p.rus.LostFilm.TV.mkv", 2)
            .AddSource("Flash.S08E01.1080p.rus.LostFilm.TV.mkv", 3)
            .AddSource("Flash.S09E01.1080p.rus.LostFilm.TV.mkv", 4)
            .AddTarget(FolderType.Shows, "Flash/Season Six/Flash.S06E01.1080p.rus.LostFilm.TV.mkv", 1)
            .AddTarget(FolderType.Shows, "Flash/S07/Flash.S07E01.1080p.rus.LostFilm.TV.mkv", 2)
            .AddTarget(FolderType.Shows, "Flash/Season 8/Flash.S08E01.1080p.rus.LostFilm.TV.mkv", 3)
            .AddGuess("Flash.S09E01.1080p.rus.LostFilm.TV.mkv", new Guess("episode", "Flash") { Season = 9, Episode = 1, ReleaseGroup = "LostFilm.TV", ScreenSize = "1080p"})
            .CreateTestCaseData("Flash.S09E01.1080p.rus.LostFilm.TV.mkv", (FolderType.Shows, "Flash/Season 9/Flash.S09E01.1080p.rus.LostFilm.TV.mkv"));

        yield return new GuesserTestCaseData("Mad Max 1 to Movies folder", "/Downloads", (FolderType.Shows, "/Shows"), (FolderType.Movies, "/Movies"))
            .AddSource("Mad Max 1.US.1979.DVDRip.mkv", 1)
            .AddGuess("Mad Max 1.US.1979.DVDRip.mkv", new Guess("movie", "Mad Max 1") { Year = 1979 })
            .CreateTestCaseData("Mad Max 1.US.1979.DVDRip.mkv", (FolderType.Movies, "Mad Max 1.US.1979.DVDRip.mkv"));

        yield return new GuesserTestCaseData("17 moments of Sprint to Shows folder without season", "/Downloads", (FolderType.Shows, "/Shows"), (FolderType.Movies, "/Movies"))
            .AddSource("17.Moments.of.Spring.01.Rus.1973.TVRip.avi", 1)
            .AddGuess("17.Moments.of.Spring.01.Rus.1973.TVRip.avi", new Guess("episode", "17 Moments of Spring") { Year = 1973 })
            .CreateTestCaseData("17.Moments.of.Spring.01.Rus.1973.TVRip.avi", (FolderType.Shows, "17 Moments of Spring/17.Moments.of.Spring.01.Rus.1973.TVRip.avi"));
    }

    private class GuesserTestCaseData : GuesserTestCaseDataBase<GuesserTestCaseData>
    {
        public GuesserTestCaseData(string name, string sourceDir, params (FolderType type, string path)[] targetDirs) : base(name, sourceDir, targetDirs)
        {
        }

        protected override TestCaseData CreateTestCaseData(Mock<IGuessItApi> guessItApiMock,
            Mock<IFoldersProvider> foldersProviderMock, IReadOnlyList<HardLinkData> links, FileData sourceFile,
            FileData? destinationFile) =>
            new(guessItApiMock.Object, foldersProviderMock.Object, sourceFile, destinationFile);
    }
}
