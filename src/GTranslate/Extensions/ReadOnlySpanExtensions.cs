using System.Buffers;
using System.Text.Json;
using System.Text;
using System;

namespace GTranslate.Extensions;

internal static class ReadOnlySpanExtensions
{
    public static JsonEncodedText SafeJsonTextEncode(this ReadOnlySpan<char> utf16Chars)
    {
        // Convert UTF16 chars to UTF8 bytes to avoid encoding exceptions on JsonEncodedText.Encode()

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        byte[] utf8Bytes = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetByteCount(utf16Chars));
        int written = Encoding.UTF8.GetBytes(utf16Chars, utf8Bytes);
#else
        char[] utf16Arr = ArrayPool<char>.Shared.Rent(utf16Chars.Length);
        utf16Chars.CopyTo(utf16Arr);
        byte[] utf8Bytes = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetByteCount(utf16Arr, 0, utf16Chars.Length));
        int written = Encoding.UTF8.GetBytes(utf16Arr, 0, utf16Chars.Length, utf8Bytes, 0);
        ArrayPool<char>.Shared.Return(utf16Arr);
#endif

        try
        {
            return JsonEncodedText.Encode(utf8Bytes.AsSpan(0, written));
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(utf8Bytes);
        }
    }
}