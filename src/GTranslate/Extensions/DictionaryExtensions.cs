using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GTranslate.Extensions;

internal static class DictionaryExtensions
{
    public static ReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this Dictionary<TKey, TValue> dictionary) where TKey : notnull => new(dictionary);
}