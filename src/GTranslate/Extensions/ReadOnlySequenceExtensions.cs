using System;
using System.Buffers;
using System.IO;

namespace GTranslate.Extensions;

internal static class ReadOnlySequenceExtensions
{
    public static ReadOnlySequence<byte> AsReadOnlySequence(this ReadOnlyMemory<byte>[] chunks) => AsReadOnlySequence(chunks.AsSpan());

    public static ReadOnlySequence<byte> AsReadOnlySequence(this ReadOnlySpan<ReadOnlyMemory<byte>> chunks)
    {
        if (chunks.Length == 0)
        {
            throw new ArgumentException("Byte array is empty.", nameof(chunks));
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