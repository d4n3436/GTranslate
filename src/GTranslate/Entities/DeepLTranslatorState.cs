using System;
using System.Threading;
using GTranslate.Translators;

namespace GTranslate;

/// <summary>
/// Holds state information for <see cref="DeepLTranslator"/>.
/// </summary>
public class DeepLTranslatorState
{
    private int _id = GenerateInitialId();

    /// <summary>
    /// Gets the next request ID.
    /// </summary>
    /// <remarks>The request ID is automatically incremented on every use.</remarks>
    public int RequestId => Interlocked.Increment(ref _id) - 1;

    private static int GenerateInitialId()
    {
#if NET6_0_OR_GREATER
        var random = Random.Shared;
#else
        var random = new Random();
#endif
        return random.Next(0, int.MaxValue);
    }

    /// <summary>
    /// Returns the last request ID.
    /// </summary>
    /// <returns>The last request ID.</returns>
    public override string ToString() => $"{nameof(RequestId)} = {_id}";
}