using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GTranslate.Models
{
    /// <summary>
    /// Represents the translation (plus transliteration and language detection) model for the Bing Translator API.
    /// </summary>
    internal class BingTranslationModel
    {
        /// <summary>
        /// Gets the language detection info.
        /// </summary>
        [JsonProperty("detectedLanguage")]
        public DetectedLanguage DetectedLanguage { get; set; } = new DetectedLanguage();

        /// <summary>
        /// Gets a read-only list of translations.
        /// </summary>
        [JsonProperty("translations")]
        public IReadOnlyList<BingTranslation> Translations { get; set; } = Array.Empty<BingTranslation>();

        /// <summary>
        /// Gets the transliteration of the text.
        /// </summary>
        [JsonProperty("inputTransliteration")]
        public string InputTransliteration { get; set; }
    }

    internal class DetectedLanguage
    {
        /// <summary>
        /// Gets the detected language.
        /// </summary>
        [JsonProperty("language")]
        public string Language { get; set; }

        /// <summary>
        /// Gets the detection score.
        /// </summary>
        [JsonProperty("score")]
        public float Score { get; set; }
    }

    internal class BingTranslation
    {
        /// <summary>
        /// Gets the translated text.
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets the transliteration info.
        /// </summary>
        [JsonProperty("transliteration")]
        public BingTransliteration Transliteration { get; set; } = new BingTransliteration();

        /// <summary>
        /// Gets the target language.
        /// </summary>
        [JsonProperty("to")]
        public string To { get; set; }

        /// <summary>
        /// Gets the info about the sent text line lengths.
        /// </summary>
        [JsonProperty("sentLen")]
        public SentLen SentLen { get; set; } = new SentLen();
    }

    internal class BingTransliteration
    {
        /// <summary>
        /// Gets the transliteration of the text.
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets the language script of the text.
        /// </summary>
        [JsonProperty("script")]
        public string Script { get; }
    }

    internal class SentLen
    {
        /// <summary>
        /// Gets a read-only list containing the length of every line in the source text.
        /// </summary>
        [JsonProperty("srcSentLen")]
        public IReadOnlyList<int> SrcSentLen { get; set; } = Array.Empty<int>();

        /// <summary>
        /// Gets a read-only list containing the length of every line in the translated text.
        /// </summary>
        [JsonProperty("transSentLen")]
        public IReadOnlyList<int> TransSentLen { get; set; } = Array.Empty<int>();
    }
}