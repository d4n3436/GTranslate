using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTranslate.Results;

namespace GTranslate.Translators
{
    /// <summary>
    /// Represents an aggregate translator. This class groups all translation services into a single class for ease of use.
    /// </summary>
    public sealed class Translator : ITranslator, IDisposable
    {
        /// <inheritdoc/>
        public string Name => "Translator";

        private readonly IReadOnlyCollection<ITranslator> _translators;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Translator"/> class.
        /// </summary>
        public Translator()
        {
            _translators = new ITranslator[]
            {
                new GoogleTranslator(),
                new BingTranslator(),
                new YandexTranslator()
            };

            /*
            _translators = typeof(ITranslator).GetTypeInfo().Assembly.DefinedTypes
                .Where(x => !x.IsInterface && !x.IsAbstract && !Equals(x, GetType().GetTypeInfo()) && x.ImplementedInterfaces.Contains(typeof(ITranslator)))
                .Select(x => (ITranslator)Activator.CreateInstance(x.AsType()))
                .ToArray();
            */
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Translator"/> class with the specified translators.
        /// </summary>
        public Translator(IReadOnlyCollection<ITranslator> translators)
        {
            if (translators == null)
            {
                throw new ArgumentNullException(nameof(translators));
            }

            if (translators.Count == 0)
            {
                throw new InvalidOperationException("Collection must not be empty.");
            }

            if (translators.Any(x => x is Translator))
            {
                throw new ArgumentException("Collection must not contain an instance of Translator.");
            }

            _translators = translators;
        }

        /// <summary>
        /// Translates a text using the available translation services.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="toLanguage">The target language.</param>
        /// <param name="fromLanguage">The source language.</param>
        /// <returns>A task containing the translation result.</returns>
        /// <remarks>This method will attempt to use all the translation services passed in the constructor, one by one.</remarks>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="TranslatorException"/>
        /// <exception cref="AggregateException"/>
        public async Task<ITranslationResult> TranslateAsync(string text, string toLanguage, string fromLanguage = null)
        {
            TranslatorGuards.ObjectNotDisposed(this, _disposed);
            TranslatorGuards.ArgumentNotNull(text, toLanguage);
            TranslatorGuards.LanguageSupported(this, toLanguage, fromLanguage);

            List<Exception> exceptions = null;
            foreach (var translator in _translators)
            {
                if (!translator.IsLanguageSupported(toLanguage) || fromLanguage != null && !translator.IsLanguageSupported(fromLanguage))
                {
                    continue;
                }

                try
                {
                    return await translator.TranslateAsync(text, toLanguage, fromLanguage).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    exceptions = exceptions ?? new List<Exception>();
                    exceptions.Add(e);
                }
            }

            throw new AggregateException("No translator provided a valid result.", exceptions);
        }

        /// <inheritdoc cref="TranslateAsync(string, string, string)"/>
        public async Task<ITranslationResult> TranslateAsync(string text, ILanguage toLanguage, ILanguage fromLanguage = null)
        {
            TranslatorGuards.ObjectNotDisposed(this, _disposed);
            TranslatorGuards.ArgumentNotNull(text, toLanguage);
            TranslatorGuards.LanguageSupported(this, toLanguage, fromLanguage);

            List<Exception> exceptions = null;
            foreach (var translator in _translators)
            {
                if (!translator.IsLanguageSupported(toLanguage) || fromLanguage != null && !translator.IsLanguageSupported(fromLanguage))
                {
                    continue;
                }

                try
                {
                    return await translator.TranslateAsync(text, toLanguage, fromLanguage).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    exceptions = exceptions ?? new List<Exception>();
                    exceptions.Add(e);
                }
            }

            throw new AggregateException("No translator provided a valid result.", exceptions);
        }

        /// <summary>
        /// Transliterates a text using the available translation services.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="toLanguage">The target language.</param>
        /// <param name="fromLanguage">The source language.</param>
        /// <returns>A task containing the transliteration result.</returns>
        /// <remarks>This method will attempt to use all the translation services passed in the constructor, one by one.</remarks>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="TranslatorException"/>
        /// <exception cref="AggregateException"/>
        public async Task<ITransliterationResult> TransliterateAsync(string text, string toLanguage, string fromLanguage = null)
        {
            TranslatorGuards.ObjectNotDisposed(this, _disposed);
            TranslatorGuards.ArgumentNotNull(text, toLanguage);
            TranslatorGuards.LanguageFound(toLanguage, fromLanguage, out var toLang, out var fromLang);

            return await TransliterateAsync(text, toLang, fromLang).ConfigureAwait(false);
        }

        /// <inheritdoc cref="TransliterateAsync(string, string, string)"/>
        public async Task<ITransliterationResult> TransliterateAsync(string text, ILanguage toLanguage, ILanguage fromLanguage = null)
        {
            TranslatorGuards.ObjectNotDisposed(this, _disposed);
            TranslatorGuards.ArgumentNotNull(text, toLanguage);
            TranslatorGuards.LanguageSupported(this, toLanguage, fromLanguage);

            List<Exception> exceptions = null;
            foreach (var translator in _translators)
            {
                if (!translator.IsLanguageSupported(toLanguage) || fromLanguage != null && !translator.IsLanguageSupported(fromLanguage))
                {
                    continue;
                }

                try
                {
                    return await translator.TransliterateAsync(text, toLanguage, fromLanguage).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    exceptions = exceptions ?? new List<Exception>();
                    exceptions.Add(e);
                }
            }

            throw new AggregateException("No translator provided a valid result.", exceptions);
        }

        /// <summary>
        /// Detects the language of a text using the available translation services.
        /// </summary>
        /// <param name="text">The text to detect its language.</param>
        /// <returns>A task that represents the asynchronous language detection operation. The task contains the detected language.</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="AggregateException"/>
        public async Task<ILanguage> DetectLanguageAsync(string text)
        {
            TranslatorGuards.ObjectNotDisposed(this, _disposed);
            TranslatorGuards.ArgumentNotNull(text);

            List<Exception> exceptions = null;
            foreach (var translator in _translators)
            {
                try
                {
                    return await translator.DetectLanguageAsync(text).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    exceptions = exceptions ?? new List<Exception>();
                    exceptions.Add(e);
                }
            }

            throw new AggregateException("No translator provided a valid result.", exceptions);
        }

        /// <summary>
        /// Returns whether at least one translator supports the specified language.
        /// </summary>
        /// <param name="language">The language.</param>
        /// <returns><see langword="true"/> if the language is supported by at least one translator, otherwise <see langword="false"/>.</returns>
        public bool IsLanguageSupported(string language)
        {
            foreach (var translator in _translators)
            {
                if (translator.IsLanguageSupported(language))
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc cref="IsLanguageSupported(string)"/>
        public bool IsLanguageSupported(ILanguage language)
        {
            foreach (var translator in _translators)
            {
                if (translator.IsLanguageSupported(language))
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public void Dispose() => Dispose(true);

        /// <inheritdoc cref="Dispose()"/>
        private void Dispose(bool disposing)
        {
            if (!disposing || _disposed) return;

            foreach (var translator in _translators)
            {
                (translator as IDisposable)?.Dispose();
            }

            _disposed = true;
        }
    }
}