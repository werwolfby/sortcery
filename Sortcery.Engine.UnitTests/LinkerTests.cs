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

        public Dictionary<(FolderType, string), (int inode, string filePath)[]> TargetFiles { get; set; } = default!;

        public (string? source, (string targetDir, string file)[] targets)[] ExpectedLinks { get; set; } = default!;

        public TestCaseData GetTestCaseData(string sourcePath)
        {
            return new TestCaseData(
                SourceFiles,
                TargetFiles,
                ExpectedLinks
                    .Select(e => (
                        e.source != null
                            ? Path.Join(sourcePath, e.source).FixPath()
                            : null,
                        e.targets.Select(t => Path.Join(t.targetDir, t.file).FixPath()).ToArray()
                    )).ToArray()
            ).SetName("{m} - " + Name);
        }
    }

    [TestCaseSource(nameof(FindLinksTestCases))]
    public void Linker_Update(
        (int inode, string filePath)[] sourceFiles,
        Dictionary<(FolderType, string), (int inode, string filePath)[]> targetDirFiles,
        (string? source, string[] targets)[] expectedLinks)
    {
        // Arrange
        var sourceDir = new FolderData("/Downloads/Completed".FixPath());
        foreach (var (inode, filePath) in sourceFiles)
        {
            var fileParts = filePath.Split('/');
            var dir = fileParts.Length > 1
                ? sourceDir.EnsureFolder(fileParts[..^1])
                : sourceDir;
            dir.GetOrAddFile(fileParts[^1], Utils.NewHardLinkId(inode));
        }

        var destinationFolders = new Dictionary<FolderType, FolderData>();
        foreach (var ((folderType, targetDir), targetFiles) in targetDirFiles)
        {
            var targetFolder = new FolderData(targetDir.FixPath());
            destinationFolders.Add(folderType, targetFolder);
            foreach (var (inode, filePath) in targetFiles)
            {
                var fileParts = filePath.Split('/');
                var dir = fileParts.Length > 1
                    ? targetFolder.EnsureFolder(fileParts[..^1])
                    : targetFolder;
                dir.GetOrAddFile(fileParts[^1], Utils.NewHardLinkId(inode));
            }
        }

        var foldersProvider = new Mock<IFoldersProvider>();
        foldersProvider.Setup(x => x.Source).Returns(sourceDir).Verifiable();
        foldersProvider.Setup(x => x.DestinationFolders).Returns(destinationFolders);

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
            TargetFiles = new()
            {
                {
                    (FolderType.Shows, "/Video/Shows"),
                    new[]
                    {
                        (1, "a.txt"),
                        (2, "b.txt"),
                        (3, "c.txt"),
                    }
                }
            },
            ExpectedLinks = new (string? source, (string targetDir, string file)[] targets)[]
            {
                ("b.txt", new []{("/Video/Shows", "b.txt")}),
                ("c.txt", new []{("/Video/Shows", "c.txt")}),
            }
        }.GetTestCaseData("/Downloads/Completed");

        yield return new FindLinksTestCase
        {
            Name = "Stale Link",
            SourceFiles = new[]
            {
                (1, "a.txt"),
                (2, "b.txt"),
                (3, "c.txt"),
            },
            TargetFiles = new()
            {
                {
                    (FolderType.Shows, "/Video/Shows"),
                    new[]
                    {
                        (1, "a.txt"),
                        (2, "b.txt"),
                        (3, "c.txt"),
                        (4, "d.txt"),
                    }
                }
            },
            ExpectedLinks = new (string? source, (string targetDir, string file)[] targets)[]
            {
                ("a.txt", new []{("/Video/Shows", "a.txt") }),
                ("b.txt", new []{("/Video/Shows", "b.txt") }),
                ("c.txt", new []{("/Video/Shows", "c.txt") }),
                (null, new []{("/Video/Shows", "d.txt") }),
            }
        }.GetTestCaseData("/Downloads/Completed");

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
            TargetFiles = new()
            {
                {
                    (FolderType.Shows, "/Video/Shows"),
                    new[]
                    {
                        (1, "a.txt"),
                        (2, "b.txt"),
                        (3, "c.txt"),
                    }
                }
            },
            ExpectedLinks = new (string? source, (string targetDir, string file)[] targets)[]
            {
                ("a.txt", new []{("/Video/Shows", "a.txt") }),
                ("b.txt", new []{("/Video/Shows", "b.txt") }),
                ("c.txt", new []{("/Video/Shows", "c.txt") }),
                ("d.txt", Array.Empty<(string targetDir, string file)>()),
            }
        }.GetTestCaseData("/Downloads/Completed");

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
            TargetFiles = new()
            {
                {
                    (FolderType.Shows, "/Video/Shows"),
                    new[]
                    {
                        (1, "a/a.txt"),
                        (2, "a/b.txt"),
                        (3, "c.txt"),
                    }
                }
            },
            ExpectedLinks = new (string? source, (string targetDir, string file)[] targets)[]
            {
                ("a/a.txt", new []{("/Video/Shows", "a/a.txt")}),
                ("a/b.txt", new []{("/Video/Shows", "a/b.txt")}),
                ("c.txt", new []{("/Video/Shows", "c.txt")}),
                ("d.txt", Array.Empty<(string targetDir, string file)>()),
            }
        }.GetTestCaseData("/Downloads/Completed");

        yield return new FindLinksTestCase
        {
            Name = "Multiple Targets",
            SourceFiles = new[]
            {
                (1, "a/a.txt"),
                (2, "a/b.txt"),
            },
            TargetFiles = new()
            {
                {
                    (FolderType.Shows, "/Video/Shows"),
                    new[]
                    {
                        (1, "a/a.txt"),
                        (1, "b/a.txt"),
                        (2, "a/b.txt"),
                        (2, "b/b.txt"),
                    }
                }
            },
            ExpectedLinks = new (string? source, (string targetDir, string file)[] targets)[]
            {
                ("a/a.txt", new []{("/Video/Shows", "a/a.txt"), ("/Video/Shows", "b/a.txt")}),
                ("a/b.txt", new []{("/Video/Shows", "a/b.txt"), ("/Video/Shows", "b/b.txt")}),
            }
        }.GetTestCaseData("/Downloads/Completed");

        yield return new FindLinksTestCase
        {
            Name = "Multiple Targets In Single Destinations",
            SourceFiles = new[]
            {
                (1, "a/a.txt"),
                (2, "a/b.txt"),
            },
            TargetFiles = new()
            {
                {
                    (FolderType.Movies, "/Video/Movies"),
                    new[]
                    {
                        (1, "a/a.txt"),
                        (1, "b/a.txt"),
                        (2, "a/b.txt"),
                    }
                }
            },
            ExpectedLinks = new (string? source, (string targetDir, string file)[] targets)[]
            {
                ("a/a.txt", new[] { ("/Video/Movies", "a/a.txt"), ("/Video/Movies", "b/a.txt") }),
                ("a/b.txt", new[] { ("/Video/Movies", "a/b.txt") }),
            }
        }.GetTestCaseData("/Downloads/Completed");
        yield return new FindLinksTestCase
        {
            Name = "Multiple Targets In Multiple Destinations",
            SourceFiles = new[]
            {
                (1, "a/a.txt"),
                (2, "a/b.txt"),
            },
            TargetFiles = new()
            {
                {
                    (FolderType.Movies, "/Video/Movies"),
                    new[]
                    {
                        (1, "a/a.txt"),
                        (2, "a/b.txt"),
                    }
                },
                {
                    (FolderType.Shows, "/Video/Shows"),
                    new[]
                    {
                        (1, "b/a.txt"),
                        (2, "b/b.txt"),
                    }
                }
            },
            ExpectedLinks = new (string? source, (string targetDir, string file)[] targets)[]
            {
                ("a/a.txt", new[] { ("/Video/Movies", "a/a.txt"), ("/Video/Shows", "b/a.txt") }),
                ("a/b.txt", new[] { ("/Video/Movies", "a/b.txt"), ("/Video/Shows", "b/b.txt") }),
            }
        }.GetTestCaseData("/Downloads/Completed");

        yield return new FindLinksTestCase
        {
            Name = "Multiple Sources In Single Target",
            SourceFiles = new[]
            {
                (1, "a.txt"),
                (1, "b.txt"),
                (2, "c.txt"),
            },
            TargetFiles = new()
            {
                {
                    (FolderType.Shows, "/Video/Shows"),
                    new[]
                    {
                        (1, "a/a.txt"),
                        (2, "a/c.txt"),
                    }
                }
            },
            ExpectedLinks = new (string? source, (string targetDir, string file)[] targets)[]
            {
                ("a.txt", new []{("/Video/Shows", "a/a.txt")}),
                ("b.txt", new []{("/Video/Shows", "a/a.txt")}),
                ("c.txt", new []{("/Video/Shows", "a/c.txt")}),
            }
        }.GetTestCaseData("/Downloads/Completed");

        yield return new FindLinksTestCase
        {
            Name = "Multiple Sources In Multiple Target",
            SourceFiles = new[]
            {
                (1, "a.txt"),
                (1, "b.txt"),
                (2, "c.txt"),
            },
            TargetFiles = new()
            {
                {
                    (FolderType.Shows, "/Video/Shows"),
                    new[]
                    {
                        (1, "a/a.txt"),
                        (1, "a/b.txt"),
                        (2, "a/c.txt"),
                    }
                }
            },
            ExpectedLinks = new (string? source, (string targetDir, string file)[] targets)[]
            {
                ("a.txt", new []{("/Video/Shows", "a/a.txt"), ("/Video/Shows", "a/b.txt")}),
                ("b.txt", new []{("/Video/Shows", "a/a.txt"), ("/Video/Shows", "a/b.txt")}),
                ("c.txt", new []{("/Video/Shows", "a/c.txt")}),
            }
        }.GetTestCaseData("/Downloads/Completed");
    }
}
