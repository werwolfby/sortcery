using System.Text.Json;

namespace Sortcery.Engine;

public class SnakeCaseJsonNamingPolicy : JsonNamingPolicy
{
    public static readonly SnakeCaseJsonNamingPolicy Instance = new();
    
    public override string ConvertName(string name)
    {
        var countUpper = 0;
        for (var i = 1; i < name.Length; i++)
        {
            if (char.IsUpper(name[i]))
            {
                countUpper++;
            }
        }
        Span<char> newStringSpan = stackalloc char[name.Length + countUpper];
        var j = 0;
        for (var i = 0; i < name.Length; i++)
        {
            if (i > 0 && char.IsUpper(name[i]))
            {
                newStringSpan[j++] = '_';
            }

            newStringSpan[j++] = char.ToLowerInvariant(name[i]);
        }

        return new string(newStringSpan);
    }
}