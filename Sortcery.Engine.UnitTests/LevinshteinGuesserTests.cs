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
        yield return new GuesserTestCaseData("Flash to The Flash folder", "/Shows", (FolderType.Shows, "/Shows"))
            .AddSource("Flash.S09E01.1080p.rus.LostFilm.TV.mkv", 1)
            .AddSource("Flash.S09E02.1080p.rus.LostFilm.TV.mkv", 2)
            .AddTarget(FolderType.Shows, "The Flash/Season 09/Flash.S09E01.1080p.rus.LostFilm.TV.mkv", 1)
            .CreateTestCaseData("Flash.S09E02.1080p.rus.LostFilm.TV.mkv", FolderType.Shows, "The Flash/Season 09/Flash.S09E02.1080p.rus.LostFilm.TV.mkv");

        yield return new GuesserTestCaseData("Flash to The Flash folder with multiple similar", "/Shows", (FolderType.Shows, "/Shows"))
            .AddSource("Flash.S09E01.1080p.rus.LostFilm.TV.mkv", 1)
            .AddSource("Flash.S09E02.1080p.rus.LostFilm.TV.mkv", 2)
            .AddSource("Flash.S09E03.1080p.rus.LostFilm.TV.mkv", 3)
            .AddTarget(FolderType.Shows, "The Flash/Season 09/Flash.S09E01.1080p.rus.LostFilm.TV.mkv", 1)
            .AddTarget(FolderType.Shows, "The Flash/Season 09/Flash.S09E02.1080p.rus.LostFilm.TV.mkv", 2)
            .CreateTestCaseData("Flash.S09E03.1080p.rus.LostFilm.TV.mkv", FolderType.Shows, "The Flash/Season 09/Flash.S09E03.1080p.rus.LostFilm.TV.mkv");

        yield return new GuesserTestCaseData("Flash to The Flash folder with multiple seasons", "/Shows", (FolderType.Shows, "/Shows"))
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
            .CreateTestCaseData("Flash.S09E03.1080p.rus.LostFilm.TV.mkv", FolderType.Shows, "The Flash/Season 09/Flash.S09E03.1080p.rus.LostFilm.TV.mkv");
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

            var hardlinks = new List<HardLinkData>();
            var usedTargets = new HashSet<HardLinkId>();

            foreach (var fileData in _sourceDir.GetAllFilesRecursively())
            {
                var targets = new List<FileData>();
                foreach (var (_, folderData) in _targetDirs)
                {
                    foreach (var target in folderData.GetAllFilesRecursively())
                    {
                        if (target.HardLinkId == fileData.HardLinkId)
                        {
                            targets.Add(target);
                            usedTargets.Add(target.HardLinkId);
                        }
                    }
                }

                hardlinks.Add(new HardLinkData(fileData, targets.Count > 0 ? targets.AsReadOnly() : Array.Empty<FileData>()));
            }

            foreach (var folderData in _targetDirs)
            {
                foreach (var fileData in folderData.Value.GetAllFilesRecursively())
                {
                    if (!usedTargets.Contains(fileData.HardLinkId))
                    {
                        hardlinks.Add(new HardLinkData(null, new List<FileData> { fileData }.AsReadOnly()));
                    }
                }
            }

            return new TestCaseData(foldersProvider.Object, hardlinks, sourceFile, destinationFile).SetName(_name);
        }
    }

}
