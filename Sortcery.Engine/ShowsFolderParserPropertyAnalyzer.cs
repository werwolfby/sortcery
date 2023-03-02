using Sortcery.Engine.Contracts;

namespace Sortcery.Engine;

public class ShowsFolderParserPropertyAnalyzer : IPropertyAnalyzer
{
    private readonly IFoldersProvider _foldersProvider;

    public ShowsFolderParserPropertyAnalyzer(IFoldersProvider foldersProvider)
    {
        _foldersProvider = foldersProvider;
    }

    public void Analyze(IReadOnlyList<HardLinkData> links)
    {
        foreach (var (sourceFileData, targetFileData) in links
                     .SelectMany(l => l.Targets.Select(t => (source: l.Source, target: t)))
                     .Where(t => _foldersProvider.GetDestinationFolderType(t.target.Dir.Root) == FolderType.Shows))
        {
            // Skip links in root shows folder
            var showsFolder = targetFileData.Dir.Root;
            if (targetFileData.Dir == showsFolder) continue;

            FolderData? seasonFolder = null;
            FolderData? showFolder = null;
            // File path structure is <Show>
            if (targetFileData.Dir.Parent == showsFolder)
            {
                showFolder = targetFileData.Dir;
            }
            // File path structure is <Show>/<Season>
            else if (targetFileData.Dir.Parent?.Parent == showsFolder)
            {
                seasonFolder = targetFileData.Dir;
                showFolder = targetFileData.Dir.Parent;
            }
            else
            {
                continue;
            }

            var sourceProperties = new Dictionary<string, object>();

            var showName = showFolder.Name.ToLowerInvariant();
            showFolder.AddProperty("Show", showName);
            sourceProperties.Add("Show", showName);

            showFolder.AddProperty("ShowFolder", showFolder.Name);
            sourceProperties.Add("ShowFolder", showFolder.Name);

            if (seasonFolder != null && SeasonFolderParser.TryParse(seasonFolder.Name, out var format, out var season))
            {
                seasonFolder.AddProperty("SeasonFolder", seasonFolder.Name);
                sourceProperties.Add("SeasonFolder", seasonFolder.Name);

                seasonFolder.AddProperty("SeasonFormat", format);
                sourceProperties.Add("SeasonFormat", format);

                seasonFolder.AddProperty("Season", season);
                sourceProperties.Add("Season", season);
            }

            // Add properties to all parent source folders
            if (sourceFileData != null)
            {
                var sourceParent = sourceFileData.Dir;
                while (sourceParent != null && sourceParent != _foldersProvider.Source)
                {
                    foreach (var (key, value) in sourceProperties)
                    {
                        sourceParent.AddProperty(key, value);
                    }
                    sourceParent = sourceParent.Parent;
                }
            }
        }
    }
}
