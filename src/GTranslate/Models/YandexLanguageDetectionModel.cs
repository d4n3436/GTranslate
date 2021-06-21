using Newtonsoft.Json;

namespace GTranslate.Models
{
    /// <summary>
    /// Represents the language detection model for the Yandex.Translate API.
    /// </summary>
    internal class YandexLanguageDetectionModel
    {
        /// <summary>
        /// Gets the response code.
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; }

        /// <summary>
        /// Gets the detected language.
        /// </summary>
        [JsonProperty("lang")]
        public string Lang { get; set; }
    }
}