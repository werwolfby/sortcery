using System.Collections;
using Sortcery.Engine.Contracts;

namespace Sortcery.Engine.UnitTests;

public class Tests
{
    public class FindLinksTestCase : IEnumerable
    {
        public (int inode, string filePath)[]? SourceFiles { get; set; }

        public (int inode, string filePath)[]? TargetFiles { get; set; }

        public (string? source, string[] targets)[]? ExpectedLinks { get; set; }

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
        var linker = new Linker(null, null);
        var sourceDir = new FolderData("/Downloads/Completed");
        var targetDir = new FolderData("/Video/Movies");
        var source = sourceFiles
            .ToDictionary(
                x => NewHardLinkId(x.inode),
                x => new FileData(sourceDir, NewHardLinkId(x.inode), x.filePath));
        var target1 = (IReadOnlyDictionary<HardLinkId, FileData>)targetFiles
            .ToDictionary(
                x => NewHardLinkId(x.inode),
                x => new FileData(targetDir, NewHardLinkId(x.inode), x.filePath));

        // Act
        var links = linker.FindLinks(source, new[] {(targetDir, target1)});

        // Assert
        Assert.That(links, Has.Count.EqualTo(links.Count));
        foreach (var (s, targets) in expectedLinks)
        {
            if (s != null)
            {
                var link = links.FirstOrDefault(x => x.Source?.Name == s);
                Assert.That(link.Source?.Name, Is.EqualTo(s));
                Assert.That(link.Targets.Select(x => x.Name), Is.EquivalentTo(targets));
            }
            else
            {
                var link = links.FirstOrDefault(x => x.Targets?.Any(t => targets.Contains(t.Name)) == true);
                Assert.That(link.Targets.Select(x => x.Name), Is.EquivalentTo(targets));
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
