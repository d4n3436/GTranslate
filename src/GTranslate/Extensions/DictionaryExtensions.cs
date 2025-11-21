using System.Collections.Generic;
using System.Collections.ObjectModel;
using GTranslate.Common;

namespace GTranslate.Extensions;

internal static class DictionaryExtensions
{
    extension<TKey, TValue>(Dictionary<TKey, TValue> dictionary) where TKey : notnull
    {
        public ReadOnlyDictionary<TKey, TValue> AsReadOnly() => new(dictionary);

        // Converts the dictionary into a frozen dictionary if possible
#if NET8_0_OR_GREATER
        public System.Collections.Frozen.FrozenDictionary<TKey, TValue> ToReadOnlyDictionary()
        => System.Collections.Frozen.FrozenDictionary.ToFrozenDictionary(dictionary, dictionary.Comparer);
#else
        public ReadOnlyDictionary<TKey, TValue> ToReadOnlyDictionary() => AsReadOnly(dictionary);
#endif
    }

#if !NET8_0_OR_GREATER
    extension<TKey, TValue>(ReadOnlyDictionary<TKey, TValue>) where TKey : notnull
    {
        public static ReadOnlyDictionary<TKey, TValue> Empty => EmptyDictionary<TKey, TValue>.Value;
    }
#endif
}