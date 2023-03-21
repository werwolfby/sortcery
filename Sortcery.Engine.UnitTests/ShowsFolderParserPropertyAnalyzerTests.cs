using Moq;
using Sortcery.Engine.Contracts;

namespace Sortcery.Engine.UnitTests;

public class ShowsFolderParserPropertyAnalyzerTests
{
    [TestCaseSource(nameof(GetTestCases))]
    public void ShowsFolderParserPropertyAnalyzer_Analyze_WithValidShowsFolder_ReturnsExpectedFormatAndSeason(
        List<HardLinkData> links,
        IFoldersProvider foldersProvider,
        List<(string path, (string property, object value)[])> sourceExpectedProperties,
        List<(FolderType type, string path, (string property, object value)[] propertyValues)> targetExpectedProperties)
    {
        var analyzer = new ShowsFolderParserPropertyAnalyzer(foldersProvider);
        analyzer.Analyze(links);

        foreach (var (path, propertyValues) in sourceExpectedProperties)
        {
            var parts = path.FixPath().Split(Path.DirectorySeparatorChar);
            var folder = foldersProvider.Source.FindFolder(parts);
            Assert.That(folder, Is.Not.Null);

            foreach (var (property, value) in propertyValues)
            {
                Assert.That(folder.HasProperty(property, value), Is.True);
            }
        }

        foreach (var (type, path, propertyValues) in targetExpectedProperties)
        {
            var parts = path.FixPath().Split(Path.DirectorySeparatorChar);
            Assert.That(foldersProvider.TryGetDestinationFolder(type, out var target), Is.True);

            var folder = target!.FindFolder(parts);
            Assert.That(folder, Is.Not.Null);

            foreach (var (property, value) in propertyValues)
            {
                Assert.That(folder.HasProperty(property, value), Is.True);
            }
        }
    }

    public static IEnumerable<TestCaseData> GetTestCases()
    {
        yield return new ShowsTestCaseData("TBBT/S09 to The Big Bang Theory/Season 09","/Downloads", (FolderType.Shows, "/Shows"))
            .AddSource("TBBT/S09/TBBT.S09E01.mkv", 1)
            .AddSource("TBBT/S09/TBBT.S09E02.mkv", 2)
            .AddTarget(FolderType.Shows, "The Big Bang Theory/Season 09/TBBT.S09E01.mkv", 1)
            .ExpectSource("TBBT",
                ("Show", "the big bang theory"),
                ("ShowFolder", "The Big Bang Theory"),
                ("SeasonFolder", "Season 09"),
                ("SeasonFormat", "Season {0:D2}"),
                ("Season", 9))
            .ExpectSource("TBBT/S09",
                ("Show", "the big bang theory"),
                ("ShowFolder", "The Big Bang Theory"),
                ("SeasonFolder", "Season 09"),
                ("SeasonFormat", "Season {0:D2}"),
                ("Season", 9))
            .ExpectTarget(FolderType.Shows, "The Big Bang Theory/Season 09",
                ("SeasonFolder", "Season 09"),
                ("SeasonFormat", "Season {0:D2}"),
                ("Season", 9))
            .ExpectTarget(FolderType.Shows, "The Big Bang Theory",
                ("Show", "the big bang theory"),
                ("ShowFolder", "The Big Bang Theory"))
            .ToTestCaseData();

        yield return new ShowsTestCaseData("TBBT/S09 and S08 to The Big Bang Theory/Season 09 and Season 08","/Downloads", (FolderType.Shows, "/Shows"))
            .AddSource("TBBT/S09/TBBT.S09E01.mkv", 1)
            .AddSource("TBBT/S09/TBBT.S09E02.mkv", 2)
            .AddSource("TBBT/S08/TBBT.S08E01.mkv", 3)
            .AddTarget(FolderType.Shows, "The Big Bang Theory/Season 09/TBBT.S09E01.mkv", 1)
            .AddTarget(FolderType.Shows, "The Big Bang Theory/Season 08/TBBT.S08E01.mkv", 3)
            .ExpectSource("TBBT",
                ("Show", "the big bang theory"),
                ("ShowFolder", "The Big Bang Theory"),
                ("SeasonFolder", "Season 09"),
                ("SeasonFormat", "Season {0:D2}"),
                ("SeasonFolder", "Season 08"),
                ("Season", 9),
                ("Season", 8))
            .ExpectSource("TBBT/S09",
                ("Show", "the big bang theory"),
                ("ShowFolder", "The Big Bang Theory"),
                ("SeasonFolder", "Season 09"),
                ("SeasonFormat", "Season {0:D2}"),
                ("Season", 9))
            .ExpectSource("TBBT/S08",
                ("Show", "the big bang theory"),
                ("ShowFolder", "The Big Bang Theory"),
                ("SeasonFolder", "Season 08"),
                ("SeasonFormat", "Season {0:D2}"),
                ("Season", 8))
            .ExpectTarget(FolderType.Shows, "The Big Bang Theory/Season 09",
                ("SeasonFolder", "Season 09"),
                ("SeasonFormat", "Season {0:D2}"),
                ("Season", 9))
            .ExpectTarget(FolderType.Shows, "The Big Bang Theory/Season 08",
                ("SeasonFolder", "Season 08"),
                ("SeasonFormat", "Season {0:D2}"),
                ("Season", 8))
            .ExpectTarget(FolderType.Shows, "The Big Bang Theory",
                ("Show", "the big bang theory"),
                ("ShowFolder", "The Big Bang Theory"))
            .ToTestCaseData();
    }

    private class ShowsTestCaseData
    {
        private readonly string _name;
        private readonly FolderData _sourceDir;
        private readonly Dictionary<FolderType, FolderData> _targetDirs;
        private readonly Dictionary<HardLinkId, (FileData? source, List<FileData> targets)> _links = new();
        private readonly List<(string path, (string property, object value)[] propertyValues)> _sourceExpectedProperties = new();
        private readonly List<(FolderType type, string path, (string property, object value)[] propertyValues)> _targetExpectedProperties = new();

        public ShowsTestCaseData(string name, string sourceDir, params (FolderType type, string path)[] targetDirs)
        {
            _name = name;
            _sourceDir = new FolderData(sourceDir.FixPath());
            _targetDirs = targetDirs.ToDictionary(d => d.type, d => new FolderData(d.path.FixPath()));
        }

        public ShowsTestCaseData AddSource(string path, int hardLinkId)
        {
            path = path.FixPath();
            var parts = path.Split(Path.DirectorySeparatorChar);
            var sourceFileData = _sourceDir.EnsureFolder(parts[..^1]).AddFile(parts[^1], Utils.NewHardLinkId(hardLinkId));
            _links.Add(sourceFileData.HardLinkId, (sourceFileData, new List<FileData>()));
            return this;
        }

        public ShowsTestCaseData AddTarget(FolderType type, string path, int hardLinkId)
        {
            path = path.FixPath();
            var parts = path.Split(Path.DirectorySeparatorChar);
            var targetFileData = _targetDirs[type].EnsureFolder(parts[..^1]).AddFile(parts[^1], Utils.NewHardLinkId(hardLinkId));
            if (_links.TryGetValue(targetFileData.HardLinkId, out var link))
            {
                link.targets.Add(targetFileData);
            }
            else
            {
                _links.Add(targetFileData.HardLinkId, (null, new List<FileData> { targetFileData }));
            }
            return this;
        }

        public ShowsTestCaseData ExpectSource(string path, params (string property, object value)[] propertyValues)
        {
            _sourceExpectedProperties.Add((path, propertyValues));
            return this;
        }

        public ShowsTestCaseData ExpectTarget(FolderType type, string path, params (string property, object value)[] propertyValues)
        {
            _targetExpectedProperties.Add((type, path, propertyValues));
            return this;
        }

        public TestCaseData ToTestCaseData()
        {
            var links = _links.Values.Select(l => new HardLinkData(l.source, l.targets)).ToList();
            var foldersProvider = new Mock<IFoldersProvider>();
            foldersProvider.CallBase = true;
            foldersProvider.Setup(p => p.Source).Returns(_sourceDir);
            foldersProvider.Setup(p => p.DestinationFolders).Returns(_targetDirs);
            return new TestCaseData(links, foldersProvider.Object, _sourceExpectedProperties, _targetExpectedProperties).SetName(_name);
        }
    }
}
