#if !NET8_0_OR_GREATER
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GTranslate.Common;

internal static class EmptyDictionary<TKey, TValue> where TKey : notnull
{
    public static ReadOnlyDictionary<TKey, TValue> Value { get; } = new(new Dictionary<TKey, TValue>());
}
#endif