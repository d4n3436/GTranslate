using System.Buffers;
using System.IO;

namespace GTranslate.Extensions;

internal static class ReadOnlySequenceExtensions
{
    public static Stream AsStream(this ReadOnlySequence<byte> readOnlySequence) => new ReadOnlySequenceStream(readOnlySequence);
}