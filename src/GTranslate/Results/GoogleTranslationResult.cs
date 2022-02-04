using System;
using System.Collections.Generic;

namespace GTranslate.Results
{
    /// <summary>
    /// Represents a translation result from Google Translate.
    /// </summary>
    public class GoogleTranslationResult : ITranslationResult<Language>, ITranslationResult
    {
        internal GoogleTranslationResult(string translation, string source, Language targetLanguage, Language sourceLanguage)
        {
            Translation = translation;
            Source = source;
            TargetLanguage = targetLanguage;
            SourceLanguage = sourceLanguage;
        }

        internal GoogleTranslationResult(string translation, string source, Language targetLanguage, Language sourceLanguage,
            string transliteration, double confidence, IReadOnlyList<GoogleAlternativeTranslation> alternativeTranslations,
            IReadOnlyList<GoogleLanguageDetection> languageDetections) : this(translation, source, targetLanguage, sourceLanguage)
        {
            Transliteration = transliteration;
            Confidence = confidence;
            AlternativeTranslations = alternativeTranslations;
            LanguageDetections = languageDetections;
        }

        /// <inheritdoc/>
        public string Translation { get; }

        /// <inheritdoc/>
        public string Source { get; }

        /// <inheritdoc/>
        public string Service => "GoogleTranslator";

        /// <inheritdoc/>
        public Language TargetLanguage { get; }

        /// <inheritdoc/>
        public Language SourceLanguage { get; }

        /// <summary>
        /// Gets the transliteration of the text.
        /// </summary>
        public string Transliteration { get; }

        /// <summary>
        /// Gets the translation confidence.
        /// </summary>
        public double Confidence { get; }

        /// <summary>
        /// Gets the alternative translations.
        /// </summary>
        public IReadOnlyList<GoogleAlternativeTranslation> AlternativeTranslations { get; } = Array.Empty<GoogleAlternativeTranslation>();

        /// <summary>
        /// Gets the language detections.
        /// </summary>
        public IReadOnlyList<GoogleLanguageDetection> LanguageDetections { get; } = Array.Empty<GoogleLanguageDetection>();

        /// <inheritdoc />
        ILanguage ITranslationResult<ILanguage>.SourceLanguage => SourceLanguage;

        /// <inheritdoc />
        ILanguage ITranslationResult<ILanguage>.TargetLanguage => TargetLanguage;
    }

    /// <summary>
    /// Represents an alternative translation from Google Translate.
    /// </summary>
    public class GoogleAlternativeTranslation
    {
        internal GoogleAlternativeTranslation(string translation, int score)
        {
            Translation = translation;
            Score = score;
        }

        /// <summary>
        /// Gets the translation.
        /// </summary>
        public string Translation { get; }

        /// <summary>
        /// Gets the score.
        /// </summary>
        public int Score { get; }
    }

    /// <summary>
    /// Represents a language detection from Google Translate.
    /// </summary>
    public class GoogleLanguageDetection
    {
        internal GoogleLanguageDetection(string sourceLanguage, double confidence)
        {
            SourceLanguage = sourceLanguage;
            Confidence = confidence;
        }

        /// <summary>
        /// Gets the source language.
        /// </summary>
        public string SourceLanguage { get; }

        /// <summary>
        /// Gets the confidence.
        /// </summary>
        public double Confidence { get; }
    }
}