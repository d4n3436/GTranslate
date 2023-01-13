using System;
using System.Buffers;
using System.IO;

namespace GTranslate.Extensions;

internal static class ReadOnlySequenceExtensions
{
    public static ReadOnlySequence<byte> AsReadOnlySequence(this Span<ReadOnlyMemory<byte>> chunks)
        => AsReadOnlySequence((ReadOnlySpan<ReadOnlyMemory<byte>>)chunks);

    public static ReadOnlySequence<byte> AsReadOnlySequence(this ReadOnlySpan<ReadOnlyMemory<byte>> chunks)
    {
        if (chunks.Length == 0)
        {
            return ReadOnlySequence<byte>.Empty;
        }

        if (chunks.Length == 1)
        {
            return new ReadOnlySequence<byte>(chunks[0]);
        }

        var start = new MemorySegment<byte>(chunks[0]);
        var end = start.Append(chunks[1]);

        for (int i = 2; i < chunks.Length; i++)
        {
            end = end.Append(chunks[i]);
        }

        return new ReadOnlySequence<byte>(start, 0, end, end.Memory.Length);
    }

    public static Stream AsStream(this ReadOnlySequence<byte> readOnlySequence) => new ReadOnlySequenceStream(readOnlySequence);
}