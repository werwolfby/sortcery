namespace Sortcery.Engine;

public static class DictionaryCollectionExtensions
{
    public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory)
        where TKey : notnull
    {
        if (!dictionary.TryGetValue(key, out var value))
        {
            value = valueFactory(key);
            dictionary.Add(key, value);
        }

        return value;
    }

    public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        where TKey : notnull
        where TValue : new() =>
        dictionary.GetOrAdd(key, k => new TValue());

    public static void Add<TKey, TValue>(this Dictionary<TKey, List<TValue>> dictionary, TKey key, TValue value)
        where TKey : notnull =>
        dictionary.GetOrAdd(key).Add(value);
}
