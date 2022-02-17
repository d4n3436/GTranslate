using System;
using System.Collections.Generic;

namespace GTranslate.Extensions
{
    internal static class StringExtensions
    {
        private static readonly char[] _separators = { '\t', '\r', '\n', ' ' };

        // Splits a text into lines of max. 200 chars without breaking words (if possible)
        // This algorithm is not as accurate as the one Google uses, but it's good enough
        // Google prioritizes maintaining the structure of sentences rather than minimizing the number of requests
        public static IEnumerable<ReadOnlyMemory<char>> SplitWithoutWordBreaking(this string text, int maxLength = 200)
        {
            int offset = 0;
            var split = text.Split(_separators, StringSplitOptions.RemoveEmptyEntries);
            text = string.Join(" ", split);

            while (offset < text.Length)
            {
                int length;
                int index = -1;
                if (text.Length - 1 <= offset + maxLength)
                {
                    length = text.Length - offset;
                }
                else
                {
                    index = text.LastIndexOf(' ', offset + maxLength);
                    length = (index - offset <= 0 ? Math.Min(text.Length, maxLength) : index) - offset;
                }

                offset += length;
                if (index != -1)
                    offset++;

                yield return text.AsMemory(offset, length);
            }
        }
    }
}