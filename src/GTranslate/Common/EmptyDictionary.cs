using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GTranslate.Common;

internal static class EmptyDictionary<TKey, TValue> where TKey : notnull
{
    public static IReadOnlyDictionary<TKey, TValue> Value { get; } =

#if NET8_0_OR_GREATER
        ReadOnlyDictionary<TKey, TValue>.Empty;
#else
        new ReadOnlyDictionary<TKey, TValue>(new Dictionary<TKey, TValue>());
#endif
}