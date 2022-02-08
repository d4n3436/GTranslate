using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace GTranslate;

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
    public IEnumerator<KeyValuePair<string, Language>> GetEnumerator()
        => _languages.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => _languages.GetEnumerator();

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
#if NETCOREAPP3_0_OR_GREATER
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out Language value) => _languages.TryGetValue(key, out value);
#else
    public bool TryGetValue(string key, out Language value) => _languages.TryGetValue(key, out value!);
#endif

    /// <summary>
    /// Gets a language from a language code, name or alias.
    /// </summary>
    /// <param name="code">The language name or code. It can be a ISO 639-1 code, a ISO 639-3 code, a language name or a language alias.</param>
    /// <returns>The language, or an exception if the language was not found.</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="ArgumentException"/>
    public Language GetLanguage(string code)
    {
        TranslatorGuards.NotNull(code);

        if (TryGetValue(code, out var language))
        {
            return language;
        }

        return Aliases.TryGetValue(code.ToLowerInvariant(), out var iso) ? _languages[iso] : throw new ArgumentException($"Unknown language \"{code}\".", nameof(code));
    }

    /// <summary>
    /// Tries to get a language from a language code, name or alias.
    /// </summary>
    /// <param name="code">The language name or code. It can be a ISO 639-1 code, a ISO 639-3 code, a language name or a language alias.</param>
    /// <param name="language">The language, if found.</param>
    /// <returns><see langword="true"/> if the language was found, otherwise <see langword="false"/>.</returns>
    public bool TryGetLanguage(string code, [MaybeNullWhen(false)] out Language language)
    {
        language = default;

        if (string.IsNullOrEmpty(code))
        {
            return false;
        }

        if (TryGetValue(code, out language))
        {
            return true;
        }

        if (!Aliases.TryGetValue(code.ToLowerInvariant(), out var iso))
        {
            return false;
        }

        language = _languages[iso];
        return true;
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
        aliases["sh"] = "sr";
        aliases["sr"] = "sr";
        aliases["srp"] = "sr";
        aliases["serbian"] = "sr";
        aliases["zh"] = "zh-CN";
        aliases["zh-chs"] = "zh-CN";
        aliases["zh-cht"] = "zh-TW";
        aliases["zh-hans"] = "zh-CN";
        aliases["zh-hant"] = "zh-TW";
        aliases["zho"] = "zh-CN";
        aliases["chinese"] = "zh-CN";
        aliases["tlh-latn"] = "tlh";
        aliases["sr-cyrl"] = "sr";

        return aliases;
    }

    private readonly IReadOnlyDictionary<string, Language> _languages = new ReadOnlyDictionary<string, Language>(new Dictionary<string, Language>
    {
        ["af"] = new("Afrikaans", "af", "afr"),
        ["sq"] = new("Albanian", "sq", "sqi"),
        ["am"] = new("Amharic", "am", "amh"),
        ["ar"] = new("Arabic", "ar", "ara"),
        ["hy"] = new("Armenian", "hy", "hye"),
        ["az"] = new("Azerbaijani", "az", "aze"),
        ["eu"] = new("Basque", "eu", "eus", TranslationServices.Google | TranslationServices.Yandex),
        ["be"] = new("Belarusian", "be", "bel", TranslationServices.Google | TranslationServices.Yandex),
        ["bn"] = new("Bengali", "bn", "ben"),
        ["bs"] = new("Bosnian", "bs", "bos"),
        ["bg"] = new("Bulgarian", "bg", "bul"),
        ["my"] = new("Burmese", "my", "mya"),
        ["ca"] = new("Catalan", "ca", "cat"),
        ["ceb"] = new("Cebuano", "ceb", "ceb", TranslationServices.Google | TranslationServices.Yandex),
        ["ny"] = new("Chichewa", "ny", "nya", TranslationServices.Google),
        ["zh-CN"] = new("Chinese Simplified", "zh-CN", "zho-CN"),
        ["zh-TW"] = new("Chinese Traditional", "zh-TW", "zho-TW", TranslationServices.Google | TranslationServices.Bing),
        ["co"] = new("Corsican", "co", "cos", TranslationServices.Google),
        ["hr"] = new("Croatian", "hr", "hrv"),
        ["cs"] = new("Czech", "cs", "ces"),
        ["da"] = new("Danish", "da", "dan"),
        ["nl"] = new("Dutch", "nl", "nld"),
        ["en"] = new("English", "en", "eng"),
        ["eo"] = new("Esperanto", "eo", "epo", TranslationServices.Google | TranslationServices.Yandex),
        ["et"] = new("Estonian", "et", "est"),
        ["fi"] = new("Finnish", "fi", "fin"),
        ["fr"] = new("French", "fr", "fra"),
        ["fy"] = new("Frisian", "fy", "fry", TranslationServices.Google),
        ["gl"] = new("Galician", "gl", "glg", TranslationServices.Google | TranslationServices.Yandex),
        ["ka"] = new("Georgian", "ka", "kat", TranslationServices.Google | TranslationServices.Yandex),
        ["de"] = new("German", "de", "deu"),
        ["el"] = new("Greek", "el", "ell"),
        ["gu"] = new("Gujarati", "gu", "guj"),
        ["ht"] = new("Haitian Creole", "ht", "hat"),
        ["ha"] = new("Hausa", "ha", "hau", TranslationServices.Google),
        ["haw"] = new("Hawaiian", "haw", "haw", TranslationServices.Google),
        ["he"] = new("Hebrew", "he", "heb"),
        ["hi"] = new("Hindi", "hi", "hin"),
        ["hmn"] = new("Hmong", "hmn", "hmn", TranslationServices.Google),
        ["hu"] = new("Hungarian", "hu", "hun"),
        ["is"] = new("Icelandic", "is", "isl"),
        ["ig"] = new("Igbo", "ig", "ibo", TranslationServices.Google),
        ["id"] = new("Indonesian", "id", "ind"),
        ["ga"] = new("Irish", "ga", "gle"),
        ["it"] = new("Italian", "it", "ita"),
        ["ja"] = new("Japanese", "ja", "jpn"),
        ["jv"] = new("Javanese", "jv", "jav", TranslationServices.Google | TranslationServices.Yandex),
        ["kn"] = new("Kannada", "kn", "kan"),
        ["kk"] = new("Kazakh", "kk", "kaz"),
        ["km"] = new("Khmer", "km", "khm"),
        ["rw"] = new("Kinyarwanda", "rw", "kin", TranslationServices.Google),
        ["ko"] = new("Korean", "ko", "kor"),
        ["ku"] = new("Kurdish (Central)", "ku", "kur", TranslationServices.Google | TranslationServices.Bing),
        ["ky"] = new("Kyrgyz", "ky", "kir", TranslationServices.Google | TranslationServices.Yandex),
        ["lo"] = new("Lao", "lo", "lao"),
        ["la"] = new("Latin", "la", "lat", TranslationServices.Google | TranslationServices.Yandex),
        ["lv"] = new("Latvian", "lv", "lav"),
        ["lt"] = new("Lithuanian", "lt", "lit"),
        ["lb"] = new("Luxembourgish", "lb", "ltz", TranslationServices.Google | TranslationServices.Yandex),
        ["mk"] = new("Macedonian", "mk", "mkd", TranslationServices.Google | TranslationServices.Yandex),
        ["mg"] = new("Malagasy", "mg", "mlg"),
        ["ms"] = new("Malay", "ms", "msa"),
        ["ml"] = new("Malayalam", "ml", "mal"),
        ["mt"] = new("Maltese", "mt", "mlt"),
        ["mi"] = new("Maori", "mi", "mri"),
        ["mr"] = new("Marathi", "mr", "mar"),
        ["mn"] = new("Mongolian", "mn", "mon", TranslationServices.Google | TranslationServices.Yandex),
        ["ne"] = new("Nepali", "ne", "nep"),
        ["no"] = new("Norwegian", "no", "nor"),
        ["or"] = new("Odia", "or", "ori", TranslationServices.Google | TranslationServices.Bing),
        ["ps"] = new("Pashto", "ps", "pus", TranslationServices.Google | TranslationServices.Bing),
        ["fa"] = new("Persian", "fa", "fas"),
        ["pl"] = new("Polish", "pl", "pol"),
        ["pt"] = new("Portuguese", "pt", "por"),
        ["pa"] = new("Punjabi", "pa", "pan"),
        ["ro"] = new("Romanian", "ro", "ron"),
        ["ru"] = new("Russian", "ru", "rus"),
        ["sm"] = new("Samoan", "sm", "smo", TranslationServices.Google | TranslationServices.Bing),
        ["gd"] = new("Scottish Gaelic", "gd", "gla", TranslationServices.Google | TranslationServices.Yandex),
        ["sr"] = new("Serbian (Cyrillic)", "sr", "srp"),
        ["st"] = new("Sotho", "st", "sot", TranslationServices.Google),
        ["sn"] = new("Shona", "sn", "sna", TranslationServices.Google),
        ["sd"] = new("Sindhi", "sd", "snd", TranslationServices.Google),
        ["si"] = new("Sinhala", "si", "sin", TranslationServices.Google | TranslationServices.Yandex),
        ["sk"] = new("Slovak", "sk", "slk"),
        ["sl"] = new("Slovenian", "sl", "slv"),
        ["so"] = new("Somali", "so", "som", TranslationServices.Google),
        ["es"] = new("Spanish", "es", "spa"),
        ["su"] = new("Sundanese", "su", "sun", TranslationServices.Google | TranslationServices.Yandex),
        ["sw"] = new("Swahili", "sw", "swa"),
        ["sv"] = new("Swedish", "sv", "swe"),
        ["tl"] = new("Tagalog", "tl", "tgl", TranslationServices.Google | TranslationServices.Yandex),
        ["tg"] = new("Tajik", "tg", "tgk", TranslationServices.Google | TranslationServices.Yandex),
        ["ta"] = new("Tamil", "ta", "tam"),
        ["tt"] = new("Tatar", "tt", "tat", TranslationServices.Google | TranslationServices.Yandex),
        ["te"] = new("Telugu", "te", "tel"),
        ["th"] = new("Thai", "th", "tha"),
        ["tr"] = new("Turkish", "tr", "tur"),
        ["tk"] = new("Turkmen", "tk", "tuk", TranslationServices.Google),
        ["uk"] = new("Ukrainian", "uk", "ukr"),
        ["ur"] = new("Urdu", "ur", "urd"),
        ["ug"] = new("Uyghur", "ug", "uig", TranslationServices.Google),
        ["uz"] = new("Uzbek", "uz", "uzb", TranslationServices.Google | TranslationServices.Yandex),
        ["vi"] = new("Vietnamese", "vi", "vie"),
        ["cy"] = new("Welsh", "cy", "cym"),
        ["xh"] = new("Xhosa", "xh", "xho", TranslationServices.Google | TranslationServices.Yandex),
        ["yi"] = new("Yiddish", "yi", "yid", TranslationServices.Google | TranslationServices.Yandex),
        ["yo"] = new("Yoruba", "yo", "yor", TranslationServices.Google),
        ["zu"] = new("Zulu", "zu", "zul", TranslationServices.Google | TranslationServices.Yandex),

        ["as"] = new("Assamese", "as", "asm", TranslationServices.Bing),
        ["yue"] = new("Cantonese", "yue", "yue", TranslationServices.Bing),
        ["prs"] = new("Dari", "prs", "prs", TranslationServices.Bing),
        ["fj"] = new("Fijian", "fj", "fij", TranslationServices.Bing),
        ["fil"] = new("Filipino", "fil", "fil", TranslationServices.Bing),
        ["fr-CA"] = new("French (Canada)", "fr-CA", "fr-CA", TranslationServices.Bing),
        ["mww"] = new("Hmong Daw", "mww", "mww", TranslationServices.Bing),
        ["iu"] = new("Inuktitut", "iu", "iku", TranslationServices.Bing),
        ["pt-PT"] = new("Portuguese (Portugal)", "pt-PT", "pt-PT", TranslationServices.Bing),
        ["otq"] = new("Querétaro Otomi", "otq", "otq", TranslationServices.Bing),
        ["sr-Latn"] = new("Serbian (Latin)", "sr-Latn", "srp-Latn", TranslationServices.Bing),
        ["ty"] = new("Tahitian", "ty", "tah", TranslationServices.Bing),
        ["ti"] = new("Tigrinya", "ti", "tir", TranslationServices.Bing),
        ["to"] = new("Tongan", "to", "ton", TranslationServices.Bing),
        ["tlh"] = new("Klingon", "tlh", "tlh", TranslationServices.Bing),
        //["tlh-Piqd"] = new Language("Klingon (pIqaD)", "tlh-Piqd", "tlh-Piqd", TranslationServices.Bing),
        ["kmr"] = new("Kurdish (Northern)", "kmr", "kmr", TranslationServices.Bing),
        ["yua"] = new("Yucatec Maya", "yua", "yua", TranslationServices.Bing),

        ["ba"] = new("Bashkir", "ba", "bak", TranslationServices.Yandex),
        ["cv"] = new("Chuvash", "cv", "chv", TranslationServices.Yandex),
        ["mhr"] = new("Eastern Mari", "mhr", "mhr", TranslationServices.Yandex),
        ["emj"] = new("Emoji", "emj", "emj", TranslationServices.Yandex),
        ["kazlat"] = new("Kazakh (Latin)", "kazlat", "kazlat", TranslationServices.Yandex),
        ["pap"] = new("Papiamento", "pap", "pap", TranslationServices.Yandex),
        ["sjn"] = new("Sindarin", "sjn", "sjn", TranslationServices.Yandex),
        ["udm"] = new("Udmurt", "udm", "udm", TranslationServices.Yandex),
        ["uzbcyr"] = new("Uzbek (Cyrillic)", "uzbcyr", "uzbcyr", TranslationServices.Yandex),
        ["mrj"] = new("Western Mari", "mrj", "mrj", TranslationServices.Yandex),
        ["sah"] = new("Yakut", "sah", "sah", TranslationServices.Yandex)
    });
}