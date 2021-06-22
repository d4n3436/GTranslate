using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GTranslate
{
    /// <summary>
    /// Represents the default language dictionary used in GTranslate. It contains all the supported languages across all the included translators.
    /// </summary>
    public sealed class LanguageDictionary : ILanguageDictionary<string, Language>
    {
        internal LanguageDictionary()
        {
            Aliases = new ReadOnlyDictionary<string, string>(BuildLanguageAliases());
        }

        /// <inheritdoc />
        IEnumerator<KeyValuePair<string, Language>> IEnumerable<KeyValuePair<string, Language>>.GetEnumerator()
            => _languages.GetEnumerator();

        /// <inheritdoc />
        public IEnumerator GetEnumerator() => _languages.GetEnumerator();

        /// <inheritdoc />
        public int Count => _languages.Count;

        /// <inheritdoc />
        public Language this[string key] => _languages[key];

        /// <inheritdoc />
        public IEnumerable<string> Keys => _languages.Keys;

        /// <inheritdoc />
        public IEnumerable<Language> Values => _languages.Values;

        /// <inheritdoc />
        public bool ContainsKey(string key) => _languages.ContainsKey(key);

        /// <inheritdoc/>
        public bool TryGetValue(string key, out Language value) => _languages.TryGetValue(key, out value);

        /// <summary>
        /// Gets a language from a language code, name or alias.
        /// </summary>
        /// <param name="code">The language name or code. It can be a ISO 639-1 code, a ISO 639-3 code, a language name or a language alias.</param>
        /// <returns>The language, or null if the language was not found.</returns>
        public Language GetLanguage(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            if (TryGetValue(code, out var language))
            {
                return language;
            }

            return Aliases.TryGetValue(code.ToLowerInvariant(), out string iso) ? _languages[iso] : null;
        }

        /// <summary>
        /// Tries to get a language from a language code, name or alias.
        /// </summary>
        /// <param name="code">The language name or code. It can be a ISO 639-1 code, a ISO 639-3 code, a language name or a language alias.</param>
        /// <param name="language">The language, if found.</param>
        /// <returns><see langword="true"/> if the language was found, otherwise <see langword="false"/>.</returns>
        public bool TryGetLanguage(string code, out Language language)
        {
            language = null;
            if (string.IsNullOrEmpty(code))
            {
                return false;
            }

            language = GetLanguage(code);
            return language != null;
        }

        /// <summary>
        /// Gets a read-only dictionary containing language aliases and their corresponding ISO 639-1 codes.
        /// </summary>
        public IReadOnlyDictionary<string, string> Aliases { get; }

        private Dictionary<string, string> BuildLanguageAliases()
        {
            var aliases = new Dictionary<string, string>();

            foreach (var kvp in _languages)
            {
                aliases[kvp.Value.ISO6393.ToLowerInvariant()] = kvp.Key;
                aliases[kvp.Value.Name.ToLowerInvariant()] = kvp.Key;
            }

            aliases["myanmar"] = "my";
            aliases["in"] = "id";
            aliases["iw"] = "he";
            aliases["ji"] = "yi";
            aliases["jw"] = "jv";
            aliases["kurdish"] = "ku";
            aliases["mo"] = "ro";
            aliases["nb"] = "no";
            aliases["nn"] = "no";
            aliases["portuguese"] = "pt";
            aliases["sh"] = "sr-Cyrl";
            aliases["sr"] = "sr-Cyrl";
            aliases["srp"] = "sr-Cyrl";
            aliases["serbian"] = "sr-Cyrl";
            aliases["zh"] = "zh-CN";
            aliases["zh-chs"] = "zh-CN";
            aliases["zh-cht"] = "zh-TW";
            aliases["zh-hans"] = "zh-CN";
            aliases["zh-hant"] = "zh-TW";
            aliases["zho"] = "zh-CN";
            aliases["chinese"] = "zh-CN";
            aliases["tlh-latn"] = "tlh";
            aliases["tlh-piqd"] = "tlh-Qaak";
            aliases["sr-cyrl"] = "sr";

            return aliases;
        }

        private readonly IReadOnlyDictionary<string, Language> _languages = new ReadOnlyDictionary<string, Language>(new Dictionary<string, Language>
        {
            ["af"] = new Language("Afrikaans", "af", "afr"),
            ["sq"] = new Language("Albanian", "sq", "sqi"),
            ["am"] = new Language("Amharic", "am", "amh"),
            ["ar"] = new Language("Arabic", "ar", "ara"),
            ["hy"] = new Language("Armenian", "hy", "hye"),
            ["az"] = new Language("Azerbaijani", "az", "aze"),
            ["eu"] = new Language("Basque", "eu", "eus", TranslationServices.Google | TranslationServices.Yandex),
            ["be"] = new Language("Belarusian", "be", "bel", TranslationServices.Google | TranslationServices.Yandex),
            ["bn"] = new Language("Bengali", "bn", "ben"),
            ["bs"] = new Language("Bosnian", "bs", "bos"),
            ["bg"] = new Language("Bulgarian", "bg", "bul"),
            ["my"] = new Language("Burmese", "my", "mya"),
            ["ca"] = new Language("Catalan", "ca", "cat"),
            ["ceb"] = new Language("Cebuano", "ceb", "ceb", TranslationServices.Google | TranslationServices.Yandex),
            ["ny"] = new Language("Chichewa", "ny", "nya", TranslationServices.Google),
            ["zh-CN"] = new Language("Chinese Simplified", "zh-CN", "zho-CN"),
            ["zh-TW"] = new Language("Chinese Traditional", "zh-TW", "zho-TW", TranslationServices.Google | TranslationServices.Bing),
            ["co"] = new Language("Corsican", "co", "cos", TranslationServices.Google),
            ["hr"] = new Language("Croatian", "hr", "hrv"),
            ["cs"] = new Language("Czech", "cs", "ces"),
            ["da"] = new Language("Danish", "da", "dan"),
            ["nl"] = new Language("Dutch", "nl", "nld"),
            ["en"] = new Language("English", "en", "eng"),
            ["eo"] = new Language("Esperanto", "eo", "epo", TranslationServices.Google | TranslationServices.Yandex),
            ["et"] = new Language("Estonian", "et", "est"),
            ["fi"] = new Language("Finnish", "fi", "fin"),
            ["fr"] = new Language("French", "fr", "fra"),
            ["fy"] = new Language("Frisian", "fy", "fry", TranslationServices.Google),
            ["gl"] = new Language("Galician", "gl", "glg", TranslationServices.Google | TranslationServices.Yandex),
            ["ka"] = new Language("Georgian", "ka", "kat", TranslationServices.Google | TranslationServices.Yandex),
            ["de"] = new Language("German", "de", "deu"),
            ["el"] = new Language("Greek", "el", "ell"),
            ["gu"] = new Language("Gujarati", "gu", "guj"),
            ["ht"] = new Language("Haitian Creole", "ht", "hat"),
            ["ha"] = new Language("Hausa", "ha", "hau", TranslationServices.Google),
            ["haw"] = new Language("Hawaiian", "haw", "haw", TranslationServices.Google),
            ["he"] = new Language("Hebrew", "he", "heb"),
            ["hi"] = new Language("Hindi", "hi", "hin"),
            ["hmn"] = new Language("Hmong", "hmn", "hmn", TranslationServices.Google),
            ["hu"] = new Language("Hungarian", "hu", "hun"),
            ["is"] = new Language("Icelandic", "is", "isl"),
            ["ig"] = new Language("Igbo", "ig", "ibo", TranslationServices.Google),
            ["id"] = new Language("Indonesian", "id", "ind"),
            ["ga"] = new Language("Irish", "ga", "gle"),
            ["it"] = new Language("Italian", "it", "ita"),
            ["ja"] = new Language("Japanese", "ja", "jpn"),
            ["jv"] = new Language("Javanese", "jv", "jav", TranslationServices.Google | TranslationServices.Yandex),
            ["kn"] = new Language("Kannada", "kn", "kan"),
            ["kk"] = new Language("Kazakh", "kk", "kaz"),
            ["km"] = new Language("Khmer", "km", "khm"),
            ["rw"] = new Language("Kinyarwanda", "rw", "kin", TranslationServices.Google),
            ["ko"] = new Language("Korean", "ko", "kor"),
            ["ku"] = new Language("Kurdish (Central)", "ku", "kur", TranslationServices.Google | TranslationServices.Bing),
            ["ky"] = new Language("Kyrgyz", "ky", "kir", TranslationServices.Google | TranslationServices.Yandex),
            ["lo"] = new Language("Lao", "lo", "lao"),
            ["la"] = new Language("Latin", "la", "lat", TranslationServices.Google | TranslationServices.Yandex),
            ["lv"] = new Language("Latvian", "lv", "lav"),
            ["lt"] = new Language("Lithuanian", "lt", "lit"),
            ["lb"] = new Language("Luxembourgish", "lb", "ltz", TranslationServices.Google | TranslationServices.Yandex),
            ["mk"] = new Language("Macedonian", "mk", "mkd", TranslationServices.Google | TranslationServices.Yandex),
            ["mg"] = new Language("Malagasy", "mg", "mlg"),
            ["ms"] = new Language("Malay", "ms", "msa"),
            ["ml"] = new Language("Malayalam", "ml", "mal"),
            ["mt"] = new Language("Maltese", "mt", "mlt"),
            ["mi"] = new Language("Maori", "mi", "mri"),
            ["mr"] = new Language("Marathi", "mr", "mar"),
            ["mn"] = new Language("Mongolian", "mn", "mon", TranslationServices.Google | TranslationServices.Yandex),
            ["ne"] = new Language("Nepali", "ne", "nep"),
            ["no"] = new Language("Norwegian", "no", "nor"),
            ["or"] = new Language("Odia", "or", "ori", TranslationServices.Google | TranslationServices.Bing),
            ["ps"] = new Language("Pashto", "ps", "pus", TranslationServices.Google | TranslationServices.Bing),
            ["fa"] = new Language("Persian", "fa", "fas"),
            ["pl"] = new Language("Polish", "pl", "pol"),
            ["pt"] = new Language("Portuguese", "pt", "por"),
            ["pa"] = new Language("Punjabi", "pa", "pan"),
            ["ro"] = new Language("Romanian", "ro", "ron"),
            ["ru"] = new Language("Russian", "ru", "rus"),
            ["sm"] = new Language("Samoan", "sm", "smo", TranslationServices.Google | TranslationServices.Bing),
            ["gd"] = new Language("Scottish Gaelic", "gd", "gla", TranslationServices.Google | TranslationServices.Yandex),
            ["sr"] = new Language("Serbian (Cyrillic)", "sr", "srp"),
            ["st"] = new Language("Sotho", "st", "sot", TranslationServices.Google),
            ["sn"] = new Language("Shona", "sn", "sna", TranslationServices.Google),
            ["sd"] = new Language("Sindhi", "sd", "snd", TranslationServices.Google),
            ["si"] = new Language("Sinhala", "si", "sin", TranslationServices.Google | TranslationServices.Yandex),
            ["sk"] = new Language("Slovak", "sk", "slk"),
            ["sl"] = new Language("Slovenian", "sl", "slv"),
            ["so"] = new Language("Somali", "so", "som", TranslationServices.Google),
            ["es"] = new Language("Spanish", "es", "spa"),
            ["su"] = new Language("Sundanese", "su", "sun", TranslationServices.Google | TranslationServices.Yandex),
            ["sw"] = new Language("Swahili", "sw", "swa"),
            ["sv"] = new Language("Swedish", "sv", "swe"),
            ["tl"] = new Language("Tagalog", "tl", "tgl", TranslationServices.Google | TranslationServices.Yandex),
            ["tg"] = new Language("Tajik", "tg", "tgk", TranslationServices.Google | TranslationServices.Yandex),
            ["ta"] = new Language("Tamil", "ta", "tam"),
            ["tt"] = new Language("Tatar", "tt", "tat", TranslationServices.Google | TranslationServices.Yandex),
            ["te"] = new Language("Telugu", "te", "tel"),
            ["th"] = new Language("Thai", "th", "tha"),
            ["tr"] = new Language("Turkish", "tr", "tur"),
            ["tk"] = new Language("Turkmen", "tk", "tuk", TranslationServices.Google),
            ["uk"] = new Language("Ukrainian", "uk", "ukr"),
            ["ur"] = new Language("Urdu", "ur", "urd"),
            ["ug"] = new Language("Uyghur", "ug", "uig", TranslationServices.Google),
            ["uz"] = new Language("Uzbek", "uz", "uzb", TranslationServices.Google | TranslationServices.Yandex),
            ["vi"] = new Language("Vietnamese", "vi", "vie"),
            ["cy"] = new Language("Welsh", "cy", "cym"),
            ["xh"] = new Language("Xhosa", "xh", "xho", TranslationServices.Google | TranslationServices.Yandex),
            ["yi"] = new Language("Yiddish", "yi", "yid", TranslationServices.Google | TranslationServices.Yandex),
            ["yo"] = new Language("Yoruba", "yo", "yor", TranslationServices.Google),
            ["zu"] = new Language("Zulu", "zu", "zul", TranslationServices.Google | TranslationServices.Yandex),

            ["as"] = new Language("Assamese", "as", "asm", TranslationServices.Bing),
            ["yue"] = new Language("Cantonese", "yue", "yue", TranslationServices.Bing),
            ["prs"] = new Language("Dari", "prs", "prs", TranslationServices.Bing),
            ["fj"] = new Language("Fijian", "fj", "fij", TranslationServices.Bing),
            ["fil"] = new Language("Filipino", "fil", "fil", TranslationServices.Bing),
            ["fr-CA"] = new Language("French (Canada)", "fr-CA", "fr-CA", TranslationServices.Bing),
            ["mww"] = new Language("Hmong Daw", "mww", "mww", TranslationServices.Bing),
            ["iu"] = new Language("Inuktitut", "iu", "iku", TranslationServices.Bing),
            ["pt-PT"] = new Language("Portuguese (Portugal)", "pt-PT", "pt-PT", TranslationServices.Bing),
            ["otq"] = new Language("Querétaro Otomi", "otq", "otq", TranslationServices.Bing),
            ["sr-Latn"] = new Language("Serbian (Latin)", "sr-Latn", "srp-Latn", TranslationServices.Bing),
            ["ty"] = new Language("Tahitian", "ty", "tah", TranslationServices.Bing),
            ["ti"] = new Language("Tigrinya", "ti", "tir", TranslationServices.Bing),
            ["to"] = new Language("Tongan", "to", "ton", TranslationServices.Bing),
            ["tlh"] = new Language("Klingon", "tlh", "tlh", TranslationServices.Bing),
            ["tlh-Piqd"] = new Language("Klingon (pIqaD)", "tlh-Piqd", "tlh-Piqd", TranslationServices.Bing),
            ["kmr"] = new Language("Kurdish (Northern)", "kmr", "kmr", TranslationServices.Bing),
            ["yua"] = new Language("Yucatec Maya", "yua", "yua", TranslationServices.Bing),

            ["ba"] = new Language("Bashkir", "ba", "bak", TranslationServices.Yandex),
            ["cv"] = new Language("Chuvash", "cv", "chv", TranslationServices.Yandex),
            ["mhr"] = new Language("Eastern Mari", "mhr", "mhr", TranslationServices.Yandex),
            ["emj"] = new Language("Emoji", "emj", "emj", TranslationServices.Yandex),
            ["kazlat"] = new Language("Kazakh (Latin)", "kazlat", "kazlat", TranslationServices.Yandex),
            ["pap"] = new Language("Papiamento", "pap", "pap", TranslationServices.Yandex),
            ["sjn"] = new Language("Sindarin", "sjn", "sjn", TranslationServices.Yandex),
            ["udm"] = new Language("Udmurt", "udm", "udm", TranslationServices.Yandex),
            ["uzbcyr"] = new Language("Uzbek (Cyrillic)", "uzbcyr", "uzbcyr", TranslationServices.Yandex),
            ["mrj"] = new Language("Western Mari", "mrj", "mrj", TranslationServices.Yandex),
            ["sah"] = new Language("Yakut", "sah", "sah", TranslationServices.Yandex)
        });
    }
}