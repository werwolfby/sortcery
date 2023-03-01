using System.Collections;
using Moq;
using Sortcery.Engine.Contracts;

namespace Sortcery.Engine.UnitTests;

public class Tests
{
    public class FindLinksTestCase
    {
        public string Name { get; set; } = default!;

        public (int inode, string filePath)[] SourceFiles { get; set; } = default!;

        public (int inode, string filePath)[] TargetFiles { get; set; } = default!;

        public (string? source, string[] targets)[] ExpectedLinks { get; set; } = default!;

        public TestCaseData GetTestCaseData(string sourcePath, string targetPath)
        {
            return new TestCaseData(
                SourceFiles,
                TargetFiles,
                ExpectedLinks.Select(
                    e => (
                        e.source != null
                            ? Path.Join(sourcePath, e.source).Replace(Path.AltDirectorySeparatorChar,
                                Path.DirectorySeparatorChar)
                            : null,
                        e.targets.Select(t =>
                            Path.Join(targetPath, t)
                                .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)).ToArray()
                    )).ToArray()
            ).SetName("{m} - " + Name);
        }
    }

    [TestCaseSource(nameof(FindLinksTestCases))]
    public void Linker_Update(
        (int inode, string filePath)[] sourceFiles,
        (int inode, string filePath)[] targetFiles,
        (string? source, string[] targets)[] expectedLinks)
    {
        // Arrange
        var sourceDir = new FolderData("/Downloads/Completed".Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
        var targetDir = new FolderData("/Video/Movies".Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
        foreach (var (inode, filePath) in sourceFiles)
        {
            var fileParts = filePath.Split('/');
            var dir = fileParts.Length > 1
                ? sourceDir.EnsureFolder(fileParts[..^1])
                : sourceDir;
            dir.GetOrAddFile(fileParts[^1], NewHardLinkId(inode));
        }

        foreach (var (inode, filePath) in targetFiles)
        {
            var fileParts = filePath.Split('/');
            var dir = fileParts.Length > 1
                ? targetDir.EnsureFolder(fileParts[..^1])
                : targetDir;
            dir.GetOrAddFile(fileParts[^1], NewHardLinkId(inode));
        }

        var foldersProvider = new Mock<IFoldersProvider>();
        foldersProvider.Setup(x => x.Source).Returns(sourceDir).Verifiable();
        foldersProvider.Setup(x => x.DestinationFolders).Returns(new Dictionary<FolderType, FolderData>
        {
            {FolderType.Movies, targetDir}
        }).Verifiable();

        var linker = new Linker(foldersProvider.Object, null);

        // Act
        linker.Update();
        var links = linker.Links;

        // Assert
        Assert.That(links, Has.Count.EqualTo(links.Count));
        foreach (var (s, targets) in expectedLinks)
        {
            if (s != null)
            {
                var link = links.FirstOrDefault(x => x.Source?.FullName == s);
                Assert.That(link.Source?.FullName, Is.EqualTo(s));
                Assert.That(link.Targets.Select(x => x.FullName), Is.EquivalentTo(targets));
            }
            else
            {
                var link = links.FirstOrDefault(x => x.Targets.Any(t => targets.Contains(t.FullName)));
                Assert.That(link.Targets.Select(x => x.FullName), Is.EquivalentTo(targets));
            }
        }
    }

    private static HardLinkId NewHardLinkId(int inode)
    {
        #if _WINDOWS
        return new HardLinkId((uint)inode, 0, 0);
        #else
        return new HardLinkId(inode, 0);
        #endif
    }


    public static IEnumerable<TestCaseData> FindLinksTestCases()
    {
        yield return new FindLinksTestCase
        {
            Name = "All Match",
            SourceFiles = new[]
            {
                (1, "a.txt"),
                (2, "b.txt"),
                (3, "c.txt"),
            },
            TargetFiles = new[]
            {
                (1, "a.txt"),
                (2, "b.txt"),
                (3, "c.txt"),
            },
            ExpectedLinks = new (string? source, string[] targets)[]
            {
                ("a.txt", new[] { "a.txt" }),
                ("b.txt", new[] { "b.txt" }),
                ("c.txt", new[] { "c.txt" }),
            }
        }.GetTestCaseData("/Downloads/Completed", "/Video/Movies");

        yield return new FindLinksTestCase
        {
            Name = "Stale Link",
            SourceFiles = new[]
            {
                (1, "a.txt"),
                (2, "b.txt"),
                (3, "c.txt"),
            },
            TargetFiles = new[]
            {
                (1, "a.txt"),
                (2, "b.txt"),
                (3, "c.txt"),
                (4, "d.txt"),
            },
            ExpectedLinks = new[]
            {
                ("a.txt", new[] { "a.txt" }),
                ("b.txt", new[] { "b.txt" }),
                ("c.txt", new[] { "c.txt" }),
                (null, new[] { "d.txt" }),
            }
        }.GetTestCaseData("/Downloads/Completed", "/Video/Movies");

        yield return new FindLinksTestCase
        {
            Name = "Missing Link",
            SourceFiles = new[]
            {
                (1, "a.txt"),
                (2, "b.txt"),
                (3, "c.txt"),
                (4, "d.txt"),
            },
            TargetFiles = new[]
            {
                (1, "a.txt"),
                (2, "b.txt"),
                (3, "c.txt"),
            },
            ExpectedLinks = new (string? source, string[] targets)[]
            {
                ("a.txt", new[] { "a.txt" }),
                ("b.txt", new[] { "b.txt" }),
                ("c.txt", new[] { "c.txt" }),
                ("d.txt", Array.Empty<string>()),
            }
        }.GetTestCaseData("/Downloads/Completed", "/Video/Movies");

        yield return new FindLinksTestCase
        {
            Name = "Missing Link - Nested",
            SourceFiles = new[]
            {
                (1, "a/a.txt"),
                (2, "a/b.txt"),
                (3, "c.txt"),
                (4, "d.txt"),
            },
            TargetFiles = new[]
            {
                (1, "a/a.txt"),
                (2, "a/b.txt"),
                (3, "c.txt"),
            },
            ExpectedLinks = new (string? source, string[] targets)[]
            {
                ("a/a.txt", new[] { "a/a.txt" }),
                ("a/b.txt", new[] { "a/b.txt" }),
                ("c.txt", new[] { "c.txt" }),
                ("d.txt", Array.Empty<string>()),
            }
        }.GetTestCaseData("/Downloads/Completed", "/Video/Movies");
    }
}
