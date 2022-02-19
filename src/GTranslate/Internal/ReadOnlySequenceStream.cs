/*
The MIT License (MIT)

Copyright (c) Andrew Arnott

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace GTranslate;

using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

internal class ReadOnlySequenceStream : Stream
{
    private readonly ReadOnlySequence<byte> _readOnlySequence;
    private bool _isDisposed;
    private SequencePosition _position;

    internal ReadOnlySequenceStream(ReadOnlySequence<byte> readOnlySequence)
    {
        _readOnlySequence = readOnlySequence;
        _position = readOnlySequence.Start;
    }

    /// <inheritdoc/>
    public override bool CanRead => !_isDisposed;

    /// <inheritdoc/>
    public override bool CanSeek => !_isDisposed;

    /// <inheritdoc/>
    public override bool CanWrite => false;

    /// <inheritdoc/>
    public override long Length => ReturnOrThrowDisposed(_readOnlySequence.Length);

    /// <inheritdoc/>
    public override long Position
    {
        get => _readOnlySequence.Slice(0, _position).Length;
        set => _position = _readOnlySequence.GetPosition(value, _readOnlySequence.Start);
    }

    /// <inheritdoc/>
    public override void Flush() => ThrowDisposedOr(new NotSupportedException());

    /// <inheritdoc/>
    public override Task FlushAsync(CancellationToken cancellationToken) => throw ThrowDisposedOr(new NotSupportedException());

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count)
    {
        var remaining = _readOnlySequence.Slice(_position);
        var toCopy = remaining.Slice(0, Math.Min(count, remaining.Length));
        _position = toCopy.End;
        toCopy.CopyTo(buffer.AsSpan(offset, count));
        return (int)toCopy.Length;
    }

    /// <inheritdoc/>
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        int bytesRead = Read(buffer, offset, count);

        return Task.FromResult(bytesRead);
    }

    /// <inheritdoc/>
    public override int ReadByte()
    {
        var remaining = _readOnlySequence.Slice(_position);
        if (remaining.Length > 0)
        {
            byte result = remaining.First.Span[0];
            _position = _readOnlySequence.GetPosition(1, _position);
            return result;
        }

        return -1;
    }

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin)
    {
        TranslatorGuards.ObjectNotDisposed(this, _isDisposed);

        SequencePosition relativeTo;
        switch (origin)
        {
            case SeekOrigin.Begin:
                relativeTo = _readOnlySequence.Start;
                break;
            case SeekOrigin.Current:
                if (offset >= 0)
                {
                    relativeTo = _position;
                }
                else
                {
                    relativeTo = _readOnlySequence.Start;
                    offset += Position;
                }

                break;
            case SeekOrigin.End:
                if (offset >= 0)
                {
                    relativeTo = _readOnlySequence.End;
                }
                else
                {
                    relativeTo = _readOnlySequence.Start;
                    offset += Position;
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(origin));
        }

        _position = _readOnlySequence.GetPosition(offset, relativeTo);
        return Position;
    }

    /// <inheritdoc/>
    public override void SetLength(long value) => ThrowDisposedOr(new NotSupportedException());

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count) => ThrowDisposedOr(new NotSupportedException());

    /// <inheritdoc/>
    public override void WriteByte(byte value) => ThrowDisposedOr(new NotSupportedException());

    /// <inheritdoc/>
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => throw ThrowDisposedOr(new NotSupportedException());

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
    /// <inheritdoc/>
    public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
        foreach (var segment in _readOnlySequence)
        {
            await destination.WriteAsync(segment, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public override int Read(Span<byte> buffer)
    {
        var remaining = _readOnlySequence.Slice(_position);
        var toCopy = remaining.Slice(0, Math.Min(buffer.Length, remaining.Length));
        _position = toCopy.End;
        toCopy.CopyTo(buffer);
        return (int)toCopy.Length;
    }

    /// <inheritdoc/>
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return new ValueTask<int>(Read(buffer.Span));
    }

    /// <inheritdoc/>
    public override void Write(ReadOnlySpan<byte> buffer) => throw ThrowDisposedOr(new NotSupportedException());

    /// <inheritdoc/>
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => throw ThrowDisposedOr(new NotSupportedException());
#endif

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            base.Dispose(disposing);
        }
    }

    private T ReturnOrThrowDisposed<T>(T value)
    {
        TranslatorGuards.ObjectNotDisposed(this, _isDisposed);
        return value;
    }

    private Exception ThrowDisposedOr(Exception ex)
    {
        TranslatorGuards.ObjectNotDisposed(this, _isDisposed);
        throw ex;
    }
}