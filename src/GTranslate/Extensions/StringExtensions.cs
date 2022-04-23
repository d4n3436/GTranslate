using System;
using System.Collections.Generic;

namespace GTranslate.Extensions;

internal static class StringExtensions
{
    private static readonly char[] _separators = { '\t', '\r', '\n', ' ' };

    // Splits a text into lines of max. 200 chars without breaking words (if possible)
    // This algorithm is not as accurate as the one Google uses, but it's good enough
    // Google prioritizes maintaining the structure of sentences rather than minimizing the number of requests
    public static IEnumerable<ReadOnlyMemory<char>> SplitWithoutWordBreaking(this string text, int maxLength = 200)
    {
        string[] split = text.Split(_separators, StringSplitOptions.RemoveEmptyEntries);
        var current = string.Join(" ", split).AsMemory();

        while (!current.IsEmpty)
        {
            int index = -1;
            int length;

            if (current.Length <= maxLength)
            {
                length = current.Length;
            }
            else
            {
                index = current.Slice(0, maxLength).Span.LastIndexOf(' ');
                length = index == -1 ? maxLength : index;
            }

            var line = current.Slice(0, length);
            // skip a single space if there's one
            if (index != -1)
            {
                length++;
            }

            current = current.Slice(length, current.Length - length);
            yield return line;
        }
    }
}