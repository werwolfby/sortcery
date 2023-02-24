using System.Collections;
using Sortcery.Engine.Contracts;
using FileInfo = Sortcery.Engine.Contracts.FileInfo;

namespace Sortcery.Engine.UnitTests;

public class Tests
{
    public class FindLinksTestCase : IEnumerable
    {
        public (int inode, string filePath)[] SourceFiles { get; set; }

        public (int inode, string filePath)[] TargetFiles { get; set; }

        public (string? source, string[] targets)[] ExpectedLinks { get; set; }

        public IEnumerator GetEnumerator()
        {
            yield return SourceFiles;
            yield return TargetFiles;
            yield return ExpectedLinks;
        }
    }

    [TestCaseSource(nameof(FindLinksTestCases))]
    public void Linker_FindLinks(
        (int inode, string filePath)[] sourceFiles,
        (int inode, string filePath)[] targetFiles,
        (string? source, string[] targets)[] expectedLinks)
    {
        // Arrange
        var linker = new Linker();
        var sourceDir = new FolderInfo("D:\\Downloads\\Sources");
        var targetDir = new FolderInfo("D:\\Video\\Movies");
        var source = sourceFiles
            .ToDictionary(
                x => NewHardLinkId(x.inode),
                x => new FileInfo(sourceDir, x.filePath));
        var target1 = (IReadOnlyDictionary<HardLinkId, FileInfo>)targetFiles
            .ToDictionary(
                x => NewHardLinkId(x.inode),
                x => new FileInfo(targetDir, x.filePath));

        // Act
        var links = linker.FindLinks(source, new[] {(targetDir, target1)});

        // Assert
        Assert.That(links, Has.Count.EqualTo(links.Count));
        foreach (var (s, targets) in expectedLinks)
        {
            if (s != null)
            {
                var link = links.FirstOrDefault(x => x.Source?.RelativePath == s);
                Assert.That(link.Source?.RelativePath, Is.EqualTo(s));
                Assert.That(link.Targets.Select(x => x.RelativePath), Is.EquivalentTo(targets));
            }
            else
            {
                var link = links.FirstOrDefault(x => x.Targets?.Any(t => targets.Contains(t.RelativePath)) == true);
                Assert.That(link.Targets.Select(x => x.RelativePath), Is.EquivalentTo(targets));
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


    public static IEnumerable<object[]> FindLinksTestCases()
    {
        yield return new FindLinksTestCase
        {
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
        }.Cast<object>().ToArray();

        yield return new FindLinksTestCase
        {
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
        }.Cast<object>().ToArray();

        yield return new FindLinksTestCase
        {
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
        }.Cast<object>().ToArray();
    }
}
