using System;
using GTranslate.Translators;

namespace GTranslate
{
    internal static class TranslatorGuards
    {
        public static void ArgumentNotNull(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentNullException(nameof(text));
            }
        }

        public static void ArgumentNotNull(string text, string toLanguage)
        {
            ArgumentNotNull(text);

            if (string.IsNullOrEmpty(toLanguage))
            {
                throw new ArgumentNullException(nameof(toLanguage));
            }
        }

        public static void ArgumentNotNull(string text, ILanguage toLanguage)
        {
            ArgumentNotNull(text);

            if (toLanguage == null)
            {
                throw new ArgumentNullException(nameof(toLanguage));
            }
        }

        public static void LanguageFound(string toLanguage, string fromLanguage, out Language toLang, out Language fromLang)
        {
            if (!Language.TryGetLanguage(toLanguage, out var toLng))
            {
                throw new ArgumentException("Unknown target language.", nameof(toLanguage));
            }

            toLang = toLng;

            fromLang = null;
            if (!string.IsNullOrEmpty(fromLanguage) && !Language.TryGetLanguage(fromLanguage, out fromLang))
            {
                throw new ArgumentException("Unknown source language.", nameof(fromLanguage));
            }
        }

        public static void LanguageSupported(ITranslator translator, ILanguage toLanguage, ILanguage fromLanguage)
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

        public static void LanguageSupported(AggregateTranslator translator, string toLanguage, string fromLanguage)
        {
            if (!translator.IsLanguageSupported(toLanguage))
            {
                throw new TranslatorException($"No available translator supports the target language \"{toLanguage}\".", translator.Name);
            }

            if (!string.IsNullOrEmpty(fromLanguage) && !translator.IsLanguageSupported(fromLanguage))
            {
                throw new TranslatorException($"No available translator supports the source language \"{fromLanguage}\".", translator.Name);
            }
        }

        public static void LanguageSupported(AggregateTranslator translator, ILanguage toLanguage, ILanguage fromLanguage)
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

        public static void ObjectNotDisposed(ITranslator translator, bool disposed)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(translator.GetType().Name);
            }
        }
    }
}