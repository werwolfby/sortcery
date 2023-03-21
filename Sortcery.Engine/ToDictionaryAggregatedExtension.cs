namespace Sortcery.Engine;

public static class ToDictionaryAggregatedExtension
{
    public static Dictionary<TKey, List<T>> ToDictionaryAggregated<T, TKey>(this IEnumerable<T> items,  Func<T, TKey> keySelector)
        where TKey : notnull
    {
        var result = new Dictionary<TKey, List<T>>();
        foreach (var item in items)
        {
            var key = keySelector(item);
            if (!result.TryGetValue(key, out var list))
            {
                list = new List<T>();
                result.Add(key, list);
            }
            list.Add(item);
        }
        return result.ToDictionary(x => x.Key, x => x.Value);
    }
}
