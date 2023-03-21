using Moq;
using Sortcery.Engine.Contracts;

namespace Sortcery.Engine.UnitTests;

public abstract class GuesserTestCaseDataBase<T>
    where T: GuesserTestCaseDataBase<T>
{
    private readonly List<(string filename, Guess guess)> _guesses;
    private readonly string _name;
    private readonly FolderData _sourceDir;
    private readonly Dictionary<FolderType, FolderData> _targetDirs;

    public GuesserTestCaseDataBase(string name, string sourceDir, params (FolderType type, string path)[] targetDirs)
    {
        _name = name;
        _sourceDir = new FolderData(sourceDir.FixPath());
        _targetDirs = targetDirs.ToDictionary(d => d.type, d => new FolderData(d.path.FixPath()));
        _guesses = new List<(string filename, Guess guess)>();
    }

    public T AddSource(string path, int hardLinkId)
    {
        path = path.FixPath();
        var parts = path.Split(Path.DirectorySeparatorChar);
        _sourceDir.EnsureFolder(parts[..^1]).AddFile(parts[^1], Utils.NewHardLinkId(hardLinkId));
        return (T)this;
    }

    public T SetSourceProps(string path, params (string key, object value)[] props)
    {
        path = path.FixPath();
        var parts = path.Split(Path.DirectorySeparatorChar);
        var file = _sourceDir.FindFolder(parts) ?? throw new InvalidOperationException("Source folder not found");
        foreach (var (key, value) in props)
        {
            file.AddProperty(key, value);
        }
        return (T)this;
    }

    public T AddTarget(FolderType type, string path, int hardLinkId)
    {
        path = path.FixPath();
        var parts = path.Split(Path.DirectorySeparatorChar);
        _targetDirs[type].EnsureFolder(parts[..^1]).AddFile(parts[^1], Utils.NewHardLinkId(hardLinkId));
        return (T)this;
    }

    public T AddGuess(string filename, Guess guess)
    {
        _guesses.Add((filename, guess));
        return (T)this;
    }

    protected (Mock<IGuessItApi> guessItApiMock, Mock<IFoldersProvider> foldersProviderMock, IReadOnlyList<HardLinkData> links, FileData source, FileData? destination) PrepareTestCaseData(string source, (FolderType type, string destination)? destination)
    {
        source = source.FixPath();
        var sourceParts = source.Split(Path.DirectorySeparatorChar);
        var sourceFile = _sourceDir.FindFile(sourceParts) ??
                         throw new InvalidOperationException("Source file not found");

        FileData? destinationFile;
        if (destination == null)
        {
            destinationFile = null;
        }
        else
        {
            var destinationPath = destination.Value.destination.FixPath();
            var destinationParts = destinationPath.Split(Path.DirectorySeparatorChar);
            var destinationFolder = _targetDirs[destination.Value.type].FindOrFakeFolder(destinationParts[..^1]);
            destinationFile = new FileData(destinationFolder, HardLinkId.Empty, destinationParts[^1]);
        }

        var foldersProvider = new Mock<IFoldersProvider>();
        foldersProvider.CallBase = true;
        foldersProvider.Setup(p => p.Source).Returns(_sourceDir);
        foldersProvider.Setup(p => p.DestinationFolders).Returns(_targetDirs);

        var guessItApi = new Mock<IGuessItApi>();
        foreach (var (filename, guess) in _guesses)
        {
            guessItApi.Setup(g => g.GuessAsync(filename)).ReturnsAsync(guess);
        }

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

        return (guessItApi, foldersProvider, hardlinks.AsReadOnly(), sourceFile, destinationFile);
    }

    public TestCaseData CreateTestCaseData(string source, (FolderType type, string destination)? destination)
    {
        var (guessItApiMock, foldersProviderMock, links, sourceFile, destinationFile) = PrepareTestCaseData(source, destination);
        var testCaseData = CreateTestCaseData(guessItApiMock, foldersProviderMock, links, sourceFile, destinationFile);
        return testCaseData.SetName(_name);
    }

    protected abstract TestCaseData CreateTestCaseData(Mock<IGuessItApi> guessItApiMock,
        Mock<IFoldersProvider> foldersProviderMock, IReadOnlyList<HardLinkData> hardlinks,
        FileData sourceFile, FileData? destinationFile);
}
