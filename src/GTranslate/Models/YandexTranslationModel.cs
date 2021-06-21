using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GTranslate.Models
{
    /// <summary>
    /// Represents the translation model for the Yandex.Translate API
    /// </summary>
    internal class YandexTranslationModel
    {
        /// <summary>
        /// Gets the response code.
        /// </summary>
        [JsonProperty("code")]
        public int Code { get; set; }

        /// <summary>
        /// Gets the source and target language codes.
        /// </summary>
        [JsonProperty("lang")]
        public string Lang { get; set; }

        /// <summary>
        /// Gets a read-only list containing the translated text.
        /// </summary>
        [JsonProperty("text")]
        public IReadOnlyList<string> Text { get; set; } = Array.Empty<string>();
    }
}