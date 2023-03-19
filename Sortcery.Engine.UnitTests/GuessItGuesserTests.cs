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
        Assert.That(result!.Dir, Is.EqualTo(destinationFile.Dir));
        Assert.That(result.HardLinkId, Is.EqualTo(destinationFile.HardLinkId));
        Assert.That(result.Name, Is.EqualTo(destinationFile.Name));
    }

    public static IEnumerable<TestCaseData> GetTestCases()
    {
        yield return new GuesserTestCaseData("Flash to The Flash folder", "/Downloads", (FolderType.Shows, "/Shows"), (FolderType.Movies, "/Movies"))
            .AddSource("Flash.S09E01.1080p.rus.LostFilm.TV.mkv", 1)
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

    private class GuesserTestCaseData
    {
        private readonly string _name;
        private readonly FolderData _sourceDir;
        private readonly Dictionary<FolderType, FolderData> _targetDirs;
        private readonly List<(string filename, Guess guess)> _guesses;

        public GuesserTestCaseData(string name, string sourceDir, params (FolderType type, string path)[] targetDirs)
        {
            _name = name;
            _sourceDir = new FolderData(sourceDir.FixPath());
            _targetDirs = targetDirs.ToDictionary(d => d.type, d => new FolderData(d.path.FixPath()));
            _guesses = new List<(string filename, Guess guess)>();
        }

        public GuesserTestCaseData AddSource(string path, int hardLinkId)
        {
            path = path.FixPath();
            var parts = path.Split(Path.DirectorySeparatorChar);
            _sourceDir.EnsureFolder(parts[..^1]).AddFile(parts[^1], Utils.NewHardLinkId(hardLinkId));
            return this;
        }

        public GuesserTestCaseData AddTarget(FolderType type, string path, int hardLinkId)
        {
            path = path.FixPath();
            var parts = path.Split(Path.DirectorySeparatorChar);
            _targetDirs[type].EnsureFolder(parts[..^1]).AddFile(parts[^1], Utils.NewHardLinkId(hardLinkId));
            return this;
        }

        public GuesserTestCaseData AddGuess(string filename, Guess guess)
        {
            _guesses.Add((filename, guess));
            return this;
        }

        public TestCaseData CreateTestCaseData(string source, (FolderType type, string destination)? destination)
        {
            source = source.FixPath();
            var sourceParts = source.Split(Path.DirectorySeparatorChar);
            var sourceFile = _sourceDir.FindFile(sourceParts) ?? throw new InvalidOperationException("Source file not found");

            FileData? destinationFile;
            if (destination == null)
            {
                destinationFile = null;
            }
            else
            {
                var destinationPath = destination.Value.destination.FixPath();
                var destinationParts = destinationPath.Split(Path.DirectorySeparatorChar);
                var destinationFolder = _targetDirs[destination.Value.type].EnsureFolder(destinationParts[..^1]);
                destinationFile = new FileData(destinationFolder, HardLinkId.Empty, destinationParts[^1]);
            }

            var foldersProvider = new Mock<IFoldersProvider>();
            foldersProvider.CallBase = true;
            foldersProvider.Setup(p => p.Source).Returns(_sourceDir);
            foldersProvider.Setup(p => p.DestinationFolders).Returns(_targetDirs);

            var guessItApi = new Mock<IGuessItApi>();
            foreach (var (filename, guess) in _guesses)
            {
                guessItApi.Setup(g => g.GuessAsync(filename)).ReturnsAsync(guess, TimeSpan.FromMilliseconds(1));
            }

            return new TestCaseData(guessItApi.Object, foldersProvider.Object, sourceFile, destinationFile).SetName(_name);
        }
    }

}
