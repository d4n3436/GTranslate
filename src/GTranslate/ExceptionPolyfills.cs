using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>Provides downlevel polyfills for static methods on Exception-derived types.</summary>
internal static class ExceptionPolyfills
{
    extension(ArgumentNullException)
    {
        public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (argument is null)
            {
                ThrowArgumentNullException(paramName);
            }
        }
    }

    [DoesNotReturn]
    private static void ThrowArgumentNullException(string? paramName) => throw new ArgumentNullException(paramName);

    extension(ObjectDisposedException)
    {
        public static void ThrowIf([DoesNotReturnIf(true)] bool condition, object instance)
        {
            if (condition)
            {
                ThrowObjectDisposedException(instance);
            }
        }
    }

    [DoesNotReturn]
    private static void ThrowObjectDisposedException(object? instance)
    {
        throw new ObjectDisposedException(instance?.GetType().FullName);
    }
}