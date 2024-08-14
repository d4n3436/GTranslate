using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GTranslate.Extensions;

internal static class DictionaryExtensions
{
    public static ReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this Dictionary<TKey, TValue> dictionary) where TKey : notnull => new(dictionary);

    // Converts the dictionary into a frozen dictionary if possible

#if NET8_0_OR_GREATER
    public static System.Collections.Frozen.FrozenDictionary<TKey, TValue> ToReadOnlyDictionary<TKey, TValue>(this Dictionary<TKey, TValue> dictionary) where TKey : notnull
        => System.Collections.Frozen.FrozenDictionary.ToFrozenDictionary(dictionary);
#else
        public static ReadOnlyDictionary<TKey, TValue> ToReadOnlyDictionary<TKey, TValue>(this Dictionary<TKey, TValue> dictionary) where TKey : notnull => AsReadOnly(dictionary);
#endif
}