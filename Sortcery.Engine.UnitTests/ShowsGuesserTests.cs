using Moq;
using Sortcery.Engine.Contracts;

namespace Sortcery.Engine.UnitTests;

public class ShowsGuesserTests
{
    [TestCaseSource(nameof(GetTestCases))]
    public void ShowsGuesser_Guess(IFoldersProvider foldersProvider, FileData sourceFile, FileData? destinationFile)
    {
        var guesser = new ShowsGuesser(foldersProvider);
        var result = guesser.Guess(sourceFile);

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
            .CreateTestCaseData("TBBT/S09/TBBT.S09E02.mkv", FolderType.Shows, "The Big Bang Theory/Season 09/TBBT.S09E02.mkv");

        yield return new GuesserTestCaseData("17 мгновений весны to 17 Мгновений Весны", "/Downloads",
                (FolderType.Shows, "/Shows"))
            .AddSource("17 мгновений весны/17 мгновений весны 1.mkv", 1)
            .SetSourceProps("17 мгновений весны",
                ("Show", "17 мгновений весны"),
                ("ShowFolder", "17 Мгновений Весны"))
            .CreateTestCaseData("17 мгновений весны/17 мгновений весны 1.mkv", FolderType.Shows, "17 Мгновений Весны/17 мгновений весны 1.mkv");
    }

    private class GuesserTestCaseData
    {
        private readonly string _name;
        private readonly FolderData _sourceDir;
        private readonly Dictionary<FolderType, FolderData> _targetDirs;

        public GuesserTestCaseData(string name, string sourceDir, params (FolderType type, string path)[] targetDirs)
        {
            _name = name;
            _sourceDir = new FolderData(sourceDir.FixPath());
            _targetDirs = targetDirs.ToDictionary(d => d.type, d => new FolderData(d.path.FixPath()));
        }

        public GuesserTestCaseData AddSource(string path, int hardLinkId)
        {
            path = path.FixPath();
            var parts = path.Split(Path.DirectorySeparatorChar);
            _sourceDir.EnsureFolder(parts[..^1]).AddFile(parts[^1], Utils.NewHardLinkId(hardLinkId));
            return this;
        }

        public GuesserTestCaseData SetSourceProps(string path, params (string key, object value)[] props)
        {
            path = path.FixPath();
            var parts = path.Split(Path.DirectorySeparatorChar);
            var file = _sourceDir.FindFolder(parts) ?? throw new InvalidOperationException("Source folder not found");
            foreach (var (key, value) in props)
            {
                file.AddProperty(key, value);
            }
            return this;
        }

        public GuesserTestCaseData AddTarget(FolderType type, string path, int hardLinkId)
        {
            path = path.FixPath();
            var parts = path.Split(Path.DirectorySeparatorChar);
            _targetDirs[type].EnsureFolder(parts[..^1]).AddFile(parts[^1], Utils.NewHardLinkId(hardLinkId));
            return this;
        }

        public TestCaseData CreateTestCaseData(string source, FolderType type, string destination)
        {
            source = source.FixPath();
            var sourceParts = source.Split(Path.DirectorySeparatorChar);
            var sourceFile = _sourceDir.FindFile(sourceParts) ?? throw new InvalidOperationException("Source file not found");

            destination = destination.FixPath();
            var destinationParts = destination.Split(Path.DirectorySeparatorChar);
            var destinationFolder = _targetDirs[type].EnsureFolder(destinationParts[..^1]);
            var destinationFile = new FileData(destinationFolder, HardLinkId.Empty, destinationParts[^1]);

            var foldersProvider = new Mock<IFoldersProvider>();
            foldersProvider.CallBase = true;
            foldersProvider.Setup(p => p.Source).Returns(_sourceDir);
            foldersProvider.Setup(p => p.DestinationFolders).Returns(_targetDirs);
            return new TestCaseData(foldersProvider.Object, sourceFile, destinationFile).SetName(_name);
        }
    }
}
