using System.Text.RegularExpressions;

namespace Sortcery.Engine;

public static class SeasonFolderParser
{
    public static bool TryParse(string name, out string format, out int season)
    {
        var matches = Regex.Matches(name, @"\d{1,2}");
        if (matches.Count != 1)
        {
            format = null;
            season = 0;
            return false;
        }

        var seasonValue = matches[0].Value;
        var formatArg = seasonValue.StartsWith('0') && seasonValue.Length > 1 ? "{0:D2}" : "{0}";
        format = name.Replace(seasonValue, formatArg);
        season = int.Parse(seasonValue);

        return true;
    }
}
