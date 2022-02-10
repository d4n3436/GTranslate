using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using GTranslate.Translators;

namespace GTranslate;

internal static class TranslatorGuards
{
    public static void NotNull<T>(T? obj, [CallerArgumentExpression("obj")] string? parameterName = null) where T : class
    {
        if (obj is null)
        {
            throw new ArgumentNullException(parameterName);
        }
    }

    public static void LanguageFound(string? language, out Language lang, string message = "Unknown language.",
        [CallerArgumentExpression("language")] string? parameterName = null)
    {
        Language temp = null!;
        if (language is not null && !Language.TryGetLanguage(language, out temp!))
        {
            throw new ArgumentException(message, parameterName);
        }

        lang = temp;
    }

    public static void LanguageSupported(ITranslator translator, ILanguage toLanguage, ILanguage? fromLanguage)
    {
        if (!translator.IsLanguageSupported(toLanguage))
        {
            throw new TranslatorException($"Target language \"{toLanguage.ISO6391}\" not supported for this service.", translator.Name);
        }

        if (fromLanguage != null && !translator.IsLanguageSupported(fromLanguage))
        {
            throw new TranslatorException($"Source language \"{fromLanguage.ISO6391}\" not supported for this service.", translator.Name);
        }
    }

    public static void LanguageSupported(AggregateTranslator translator, string toLanguage, string? fromLanguage)
    {
        if (!translator.IsLanguageSupported(toLanguage))
        {
            throw new TranslatorException($"No available translator supports the target language \"{toLanguage}\".", translator.Name);
        }

        if (!string.IsNullOrEmpty(fromLanguage) && !translator.IsLanguageSupported(fromLanguage!))
        {
            throw new TranslatorException($"No available translator supports the source language \"{fromLanguage}\".", translator.Name);
        }
    }

    public static void LanguageSupported(AggregateTranslator translator, ILanguage toLanguage, ILanguage? fromLanguage)
    {
        if (!translator.IsLanguageSupported(toLanguage))
        {
            throw new TranslatorException($"No available translator supports the target language \"{toLanguage.ISO6391}\".", translator.Name);
        }

        if (fromLanguage != null && !translator.IsLanguageSupported(fromLanguage))
        {
            throw new TranslatorException($"No available translator supports the source language \"{fromLanguage.ISO6391}\".", translator.Name);
        }
    }

    public static void ObjectNotDisposed<T>(T obj, [DoesNotReturnIf(true)] bool disposed, string? objectName = null) where T : IDisposable
    {
        if (disposed)
        {
            throw new ObjectDisposedException(objectName ?? obj.GetType().Name);
        }
    }
}