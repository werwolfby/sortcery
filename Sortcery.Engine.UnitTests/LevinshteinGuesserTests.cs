using Moq;
using Sortcery.Engine.Contracts;

namespace Sortcery.Engine.UnitTests;

public class LevinshteinGuesserTests
{
    [TestCaseSource(nameof(GetTestCases))]
    public void LevinshteinGuesser_Guess(IFoldersProvider foldersProvider, IReadOnlyList<HardLinkData> hardlinks, FileData sourceFile, FileData? destinationFile)
    {
        var guesser = new LevinshteinGuesser(foldersProvider);
        var result = guesser.Guess(sourceFile, hardlinks);

        if (destinationFile == null)
        {
            Assert.That(result, Is.Null);
        }
        else
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Dir, Is.EqualTo(destinationFile.Dir));
            Assert.That(result.HardLinkId, Is.EqualTo(destinationFile.HardLinkId));
            Assert.That(result.Name, Is.EqualTo(destinationFile.Name));
        }
    }

    public static IEnumerable<TestCaseData> GetTestCases()
    {
        yield return new GuesserTestCaseData("Flash to The Flash folder", "/Downloads", (FolderType.Shows, "/Shows"))
            .AddSource("Flash.S09E01.1080p.rus.LostFilm.TV.mkv", 1)
            .AddSource("Flash.S09E02.1080p.rus.LostFilm.TV.mkv", 2)
            .AddTarget(FolderType.Shows, "The Flash/Season 09/Flash.S09E01.1080p.rus.LostFilm.TV.mkv", 1)
            .CreateTestCaseData("Flash.S09E02.1080p.rus.LostFilm.TV.mkv", (FolderType.Shows, "The Flash/Season 09/Flash.S09E02.1080p.rus.LostFilm.TV.mkv"));

        yield return new GuesserTestCaseData("Flash to The Flash folder with multiple similar", "/Downloads", (FolderType.Shows, "/Shows"))
            .AddSource("Flash.S09E01.1080p.rus.LostFilm.TV.mkv", 1)
            .AddSource("Flash.S09E02.1080p.rus.LostFilm.TV.mkv", 2)
            .AddSource("Flash.S09E03.1080p.rus.LostFilm.TV.mkv", 3)
            .AddTarget(FolderType.Shows, "The Flash/Season 09/Flash.S09E01.1080p.rus.LostFilm.TV.mkv", 1)
            .AddTarget(FolderType.Shows, "The Flash/Season 09/Flash.S09E02.1080p.rus.LostFilm.TV.mkv", 2)
            .CreateTestCaseData("Flash.S09E03.1080p.rus.LostFilm.TV.mkv", (FolderType.Shows, "The Flash/Season 09/Flash.S09E03.1080p.rus.LostFilm.TV.mkv"));

        yield return new GuesserTestCaseData("Flash to The Flash folder with multiple seasons", "/Downloads", (FolderType.Shows, "/Shows"))
            .AddSource("Flash.S08E01.1080p.rus.LostFilm.TV.mkv", 1)
            .AddSource("Flash.S08E02.1080p.rus.LostFilm.TV.mkv", 2)
            .AddSource("Flash.S08E03.1080p.rus.LostFilm.TV.mkv", 3)
            .AddSource("Flash.S09E01.1080p.rus.LostFilm.TV.mkv", 4)
            .AddSource("Flash.S09E02.1080p.rus.LostFilm.TV.mkv", 5)
            .AddSource("Flash.S09E03.1080p.rus.LostFilm.TV.mkv", 6)
            .AddTarget(FolderType.Shows, "The Flash/Season 08/Flash.S08E01.1080p.rus.LostFilm.TV.mkv", 1)
            .AddTarget(FolderType.Shows, "The Flash/Season 08/Flash.S08E02.1080p.rus.LostFilm.TV.mkv", 2)
            .AddTarget(FolderType.Shows, "The Flash/Season 08/Flash.S08E03.1080p.rus.LostFilm.TV.mkv", 3)
            .AddTarget(FolderType.Shows, "The Flash/Season 09/Flash.S09E01.1080p.rus.LostFilm.TV.mkv", 4)
            .AddTarget(FolderType.Shows, "The Flash/Season 09/Flash.S09E02.1080p.rus.LostFilm.TV.mkv", 5)
            .CreateTestCaseData("Flash.S09E03.1080p.rus.LostFilm.TV.mkv", (FolderType.Shows, "The Flash/Season 09/Flash.S09E03.1080p.rus.LostFilm.TV.mkv"));

        yield return new GuesserTestCaseData("Mad Max Trilogy into Mad Max movies folder", "/Downloads", (FolderType.Movies, "/Movies"))
            .AddSource("Mad Max 1.US.1979.DVDRip.mkv", 1)
            .AddSource("Mad Max 2.US.1981.DVDRip.mkv", 2)
            .AddSource("Mad Max 3.US.1985.DVDRip.mkv", 3)
            .AddTarget(FolderType.Movies, "Mad Max/Mad Max 1.US.1979.DVDRip.mkv", 1)
            .AddTarget(FolderType.Movies, "Mad Max/Mad Max 2.US.1981.DVDRip.mkv", 2)
            .CreateTestCaseData("Mad Max 3.US.1985.DVDRip.mkv", (FolderType.Movies, "Mad Max/Mad Max 3.US.1985.DVDRip.mkv"));

        yield return new GuesserTestCaseData("Completely different movies will not be guessed", "/Downloads", (FolderType.Movies, "/Movies"))
            .AddSource("Mad Max 1.US.1979.DVDRip.mkv", 1)
            .AddSource("Avengers.Infinity War.2019.BDRip.mkv", 2)
            .AddSource("Captain Marvel.2018.BDRip.mkv", 3)
            .AddTarget(FolderType.Movies, "Mad Max/Mad Max 1.US.1979.DVDRip.mkv", 1)
            .AddTarget(FolderType.Movies, "Avengers.Infinity War.2019.BDRip.mkv", 2)
            .CreateTestCaseData("Captain Marvel.2018.BDRip.mkv", null);
    }

    private class GuesserTestCaseData : GuesserTestCaseDataBase<GuesserTestCaseData>
    {
        public GuesserTestCaseData(string name, string sourceDir, params (FolderType type, string path)[] targetDirs) : base(name, sourceDir, targetDirs)
        {
        }

        protected override TestCaseData CreateTestCaseData(Mock<IGuessItApi> guessItApiMock,
            Mock<IFoldersProvider> foldersProviderMock, IReadOnlyList<HardLinkData> hardlinks, FileData sourceFile,
            FileData? destinationFile) => new(foldersProviderMock.Object, hardlinks, sourceFile, destinationFile);
    }
}
