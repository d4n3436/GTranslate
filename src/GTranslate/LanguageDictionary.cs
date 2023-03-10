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
    internal LanguageDictionary() => Aliases = new ReadOnlyDictionary<string, string>(BuildLanguageAliases());

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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="code"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the language was not found.</exception>
    public Language GetLanguage(string code)
    {
        TranslatorGuards.NotNull(code);

        if (TryGetValue(code, out var language))
        {
            return language;
        }

        return Aliases.TryGetValue(code, out var iso) ? _languages[iso] : throw new ArgumentException($"Unknown language \"{code}\".", nameof(code));
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

        if (!Aliases.TryGetValue(code, out string? iso))
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
        var aliases = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            ["bangla"] = "bn",
            ["myanmar"] = "my",
            ["in"] = "id",
            ["iw"] = "he",
            ["ji"] = "yi",
            ["jw"] = "jv",
            ["mo"] = "ro",
            ["nb"] = "no",
            ["nn"] = "no",
            ["portuguese"] = "pt",
            ["pt-BR"] = "pt",
            ["sh"] = "sr",
            ["sr"] = "sr",
            ["srp"] = "sr",
            ["serbian"] = "sr",
            ["zh"] = "zh-CN",
            ["zh-chs"] = "zh-CN",
            ["zh-cht"] = "zh-TW",
            ["zh-hans"] = "zh-CN",
            ["zh-hant"] = "zh-TW",
            ["zho"] = "zh-CN",
            ["chinese"] = "zh-CN",
            ["tlh-latn"] = "tlh",
            ["sr-cyrl"] = "sr",
            ["mn-Cyrl"] = "mn",
            ["konkani"] = "gom", // Google Translate uses "gom" (Goan Konkani) for Konkani instead of "kok"
            ["kok"] = "gom",
            ["sorani"] = "ckb", // Central Kurdish
            ["ganda"] = "ug", // Luganda
            ["mni-Mtei"] = "mni", // Google Translate uses mni-Mtei for Manipuri (Meitei)
            ["meitei"] = "mni",
            ["twi"] = "ak", // Google Translate uses "ak" (Akan) for Twi instead of "tw"
            ["tw"] = "ak"
        };

        foreach (var kvp in _languages)
        {
            aliases[kvp.Value.Name] = kvp.Key;
            aliases[kvp.Value.NativeName] = kvp.Key;
            aliases[kvp.Value.ISO6393] = kvp.Key;
        }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        aliases.TrimExcess();
#endif
        
        return aliases;
    }

    private readonly IReadOnlyDictionary<string, Language> _languages = new ReadOnlyDictionary<string, Language>(new Dictionary<string, Language>(StringComparer.OrdinalIgnoreCase)
    {
        ["af"] = new("Afrikaans", "Afrikaans", "af", "afr"),
        ["ak"] = new("Akan", "Ákán", "ak", "aka", TranslationServices.Google),
        ["am"] = new("Amharic", "አማርኛ", "am", "amh"),
        ["ar"] = new("Arabic", "العربية", "ar", "ara"),
        ["as"] = new("Assamese", "অসমীয়া", "as", "asm", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["ay"] = new("Aymara", "Aymar aru", "ay", "aym", TranslationServices.Google),
        ["az"] = new("Azerbaijani", "Azərbaycan", "az", "aze"),
        ["ba"] = new("Bashkir", "Bashkir", "ba", "bak", TranslationServices.Bing | TranslationServices.Yandex | TranslationServices.Microsoft),
        ["be"] = new("Belarusian", "беларуская", "be", "bel", TranslationServices.Google | TranslationServices.Yandex),
        ["bg"] = new("Bulgarian", "Български", "bg", "bul"),
        ["bho"] = new("Bhojpuri", "भोजपुरी", "bho", "bho", TranslationServices.Google),
        ["bm"] = new("Bambara", "ߓߊߡߊߣߊ߲ߞߊ߲", "bm", "bam", TranslationServices.Google),
        ["bn"] = new("Bengali", "বাংলা", "bn", "ben"),
        ["bo"] = new("Tibetan", "བོད་སྐད་", "bo", "bod", TranslationServices.Bing | TranslationServices.Microsoft),
        ["bs"] = new("Bosnian", "bosanski", "bs", "bos"),
        ["ca"] = new("Catalan", "Català", "ca", "cat"),
        ["ceb"] = new("Cebuano", "Binisaya", "ceb", "ceb", TranslationServices.Google | TranslationServices.Yandex),
        ["ckb"] = new("Kurdish (Central)", "کوردیی ناوەندی", "ckb", "ckb", TranslationServices.Google),
        ["co"] = new("Corsican", "Corsu", "co", "cos", TranslationServices.Google),
        ["cs"] = new("Czech", "Čeština", "cs", "ces"),
        ["cv"] = new("Chuvash", "Чӑвашла", "cv", "chv", TranslationServices.Yandex),
        ["cy"] = new("Welsh", "Cymraeg", "cy", "cym"),
        ["da"] = new("Danish", "Dansk", "da", "dan"),
        ["de"] = new("German", "Deutsch", "de", "deu"),
        ["doi"] = new("Dogri", "डोगरी", "doi", "doi", TranslationServices.Google),
        ["dv"] = new("Divehi", "ދިވެހިބަސް", "dv", "div", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["ee"] = new("Ewe", "Eʋegbe", "ee", "ewe", TranslationServices.Google),
        ["el"] = new("Greek", "Ελληνικά", "el", "ell"),
        ["emj"] = new("Emoji", "Emoji", "emj", "emj", TranslationServices.Yandex), // Not present in Yandex.Cloud
        ["en"] = new("English", "English", "en", "eng"),
        ["eo"] = new("Esperanto", "Esperanto", "eo", "epo", TranslationServices.Google | TranslationServices.Yandex),
        ["es"] = new("Spanish", "Español", "es", "spa"),
        ["et"] = new("Estonian", "Eesti", "et", "est"),
        ["eu"] = new("Basque", "Euskara", "eu", "eus"),
        ["fa"] = new("Persian", "فارسی", "fa", "fas"),
        ["fi"] = new("Finnish", "Suomi", "fi", "fin"),
        ["fil"] = new("Filipino", "Filipino", "fil", "fil", TranslationServices.Bing | TranslationServices.Microsoft),
        ["fj"] = new("Fijian", "Na Vosa Vakaviti", "fj", "fij", TranslationServices.Bing | TranslationServices.Microsoft),
        ["fo"] = new("Faroese", "føroyskt mál", "fo", "fao", TranslationServices.Bing | TranslationServices.Microsoft),
        ["fr"] = new("French", "Français", "fr", "fra"),
        ["fr-CA"] = new("French (Canada)", "Français (Canada)", "fr-CA", "fr-CA", TranslationServices.Bing | TranslationServices.Microsoft),
        ["fy"] = new("Frisian", "Frysk", "fy", "fry", TranslationServices.Google),
        ["ga"] = new("Irish", "Gaeilge", "ga", "gle"),
        ["gd"] = new("Scottish Gaelic", "Gàidhlig", "gd", "gla", TranslationServices.Google | TranslationServices.Yandex),
        ["gl"] = new("Galician", "Galego", "gl", "glg"),
        ["gn"] = new("Guarani", "avañeʼẽ", "gn", "grn", TranslationServices.Google),
        ["gom"] = new("Goan Konkani", "कोंकणी", "gom", "gom", TranslationServices.Google),
        ["gu"] = new("Gujarati", "ગુજરાતી", "gu", "guj"),
        ["ha"] = new("Hausa", "Hausa", "ha", "hau", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["haw"] = new("Hawaiian", "ʻŌlelo Hawaiʻi", "haw", "haw", TranslationServices.Google),
        ["he"] = new("Hebrew", "עברית", "he", "heb"),
        ["hi"] = new("Hindi", "हिन्दी", "hi", "hin"),
        ["hmn"] = new("Hmong", "Hmong", "hmn", "hmn", TranslationServices.Google),
        ["hr"] = new("Croatian", "Hrvatski", "hr", "hrv"),
        ["hsb"] = new("Upper Sorbian", "Hornjoserbšćina", "hsb", "hsb", TranslationServices.Bing | TranslationServices.Microsoft),
        ["ht"] = new("Haitian Creole", "Kreyòl ayisyen", "ht", "hat"),
        ["hu"] = new("Hungarian", "Magyar", "hu", "hun"),
        ["hy"] = new("Armenian", "Հայերեն", "hy", "hye"),
        ["id"] = new("Indonesian", "Indonesia", "id", "ind"),
        ["ig"] = new("Igbo", "Igbo", "ig", "ibo", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["ikt"] = new("Inuinnaqtun", "Inuinnaqtun", "ikt", "ikt", TranslationServices.Bing | TranslationServices.Microsoft),
        ["ilo"] = new("Ilocano", "Iloko", "ilo", "ilo", TranslationServices.Google),
        ["is"] = new("Icelandic", "Íslenska", "is", "isl"),
        ["it"] = new("Italian", "Italiano", "it", "ita"),
        ["iu"] = new("Inuktitut", "ᐃᓄᒃᑎᑐᑦ", "iu", "iku", TranslationServices.Bing | TranslationServices.Microsoft),
        ["iu-Latn"] = new("Inuktitut (Latin)", "Inuktitut (Latin)", "iu-Latn", "iu-Latn", TranslationServices.Bing | TranslationServices.Microsoft),
        ["ja"] = new("Japanese", "日本語", "ja", "jpn"),
        ["jv"] = new("Javanese", "Jawa", "jv", "jav", TranslationServices.Google | TranslationServices.Yandex),
        ["ka"] = new("Georgian", "ქართული", "ka", "kat"),
        ["kazlat"] = new("Kazakh (Latin)", "qazaqşa", "kazlat", "kazlat", TranslationServices.Yandex),
        ["kk"] = new("Kazakh", "Қазақ Тілі", "kk", "kaz"),
        ["km"] = new("Khmer", "ខ្មែរ", "km", "khm"),
        ["kmr"] = new("Kurdish (Northern)", "Kurdî (Bakur)", "kmr", "kmr", TranslationServices.Bing | TranslationServices.Microsoft),
        ["kn"] = new("Kannada", "ಕನ್ನಡ", "kn", "kan"),
        ["ko"] = new("Korean", "한국어", "ko", "kor"),
        ["kri"] = new("Krio", "Krio", "kri", "kri", TranslationServices.Google),
        ["ku"] = new("Kurdish", "Kurdî", "ku", "kur", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["ky"] = new("Kyrgyz", "Kyrgyz", "ky", "kir"),
        ["la"] = new("Latin", "Latina", "la", "lat", TranslationServices.Google | TranslationServices.Yandex),
        ["lb"] = new("Luxembourgish", "Lëtzebuergesch", "lb", "ltz", TranslationServices.Google | TranslationServices.Yandex),
        ["lg"] = new("Luganda", "Oluganda", "lg", "lug", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["ln"] = new("Lingala", "Lingála", "ln", "lin", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["lo"] = new("Lao", "ລາວ", "lo", "lao"),
        ["lt"] = new("Lithuanian", "Lietuvių", "lt", "lit"),
        ["lus"] = new("Mizo", "Mizo ṭawng", "lus", "lus", TranslationServices.Google),
        ["lv"] = new("Latvian", "Latviešu", "lv", "lav"),
        ["lzh"] = new("Chinese (Literary)", "中文 (文言文)", "lzh", "lzh", TranslationServices.Bing | TranslationServices.Microsoft),
        ["mai"] = new("Maithili", "मैथिली", "mai", "mai", TranslationServices.Google),
        ["mg"] = new("Malagasy", "Malagasy", "mg", "mlg"),
        ["mhr"] = new("Eastern Mari", "олык марий", "mhr", "mhr", TranslationServices.Yandex),
        ["mi"] = new("Maori", "Te Reo Māori", "mi", "mri"),
        ["mk"] = new("Macedonian", "Македонски", "mk", "mkd"),
        ["ml"] = new("Malayalam", "മലയാളം", "ml", "mal"),
        ["mn"] = new("Mongolian", "Монгол хэл", "mn", "mon"),
        ["mn-Mong"] = new("Mongolian (Traditional)", "ᠮᠣᠩᠭᠣᠯ ᠬᠡᠯᠡ", "mn-Mong", "mn-Mong", TranslationServices.Bing | TranslationServices.Microsoft),
        ["mni"] = new("Manipuri", "ꯃꯩꯇꯩꯂꯣꯟ", "mni", "mni", TranslationServices.Google),
        ["mr"] = new("Marathi", "मराठी", "mr", "mar"),
        ["mrj"] = new("Western Mari", "Мары йӹлмӹ", "mrj", "mrj", TranslationServices.Yandex),
        ["ms"] = new("Malay", "Melayu", "ms", "msa"),
        ["mt"] = new("Maltese", "Malti", "mt", "mlt"),
        ["mww"] = new("Hmong Daw", "Hmong Daw", "mww", "mww", TranslationServices.Bing | TranslationServices.Microsoft),
        ["my"] = new("Burmese", "မြန်မာ", "my", "mya"),
        ["ne"] = new("Nepali", "नेपाली", "ne", "nep"),
        ["nl"] = new("Dutch", "Nederlands", "nl", "nld"),
        ["no"] = new("Norwegian", "Norsk", "no", "nor"),
        ["nso"] = new("Sepedi", "Sesotho sa Leboa", "nso", "nso", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["ny"] = new("Chichewa", "Nyanja", "ny", "nya", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["om"] = new("Oromo", "Afaan Oromoo", "om", "orm", TranslationServices.Google),
        ["or"] = new("Odia", "ଓଡ଼ିଆ", "or", "ori", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["otq"] = new("Querétaro Otomi", "Hñähñu", "otq", "otq", TranslationServices.Bing | TranslationServices.Microsoft),
        ["pa"] = new("Punjabi", "ਪੰਜਾਬੀ", "pa", "pan"),
        ["pap"] = new("Papiamento", "Papiamento", "pap", "pap", TranslationServices.Yandex),
        ["pl"] = new("Polish", "Polski", "pl", "pol"),
        ["prs"] = new("Dari", "دری", "prs", "prs", TranslationServices.Bing | TranslationServices.Microsoft),
        ["ps"] = new("Pashto", "پښتو", "ps", "pus", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["pt"] = new("Portuguese", "Português", "pt", "por"),
        ["pt-PT"] = new("Portuguese (Portugal)", "Português (Portugal)", "pt-PT", "pt-PT", TranslationServices.Bing | TranslationServices.Yandex | TranslationServices.Microsoft),
        ["qu"] = new("Quechua", "Runa simi", "qu", "que", TranslationServices.Google),
        ["ro"] = new("Romanian", "Română", "ro", "ron"),
        ["ru"] = new("Russian", "Русский", "ru", "rus"),
        ["run"] = new("Rundi", "Ikirundi", "run", "run", TranslationServices.Bing | TranslationServices.Microsoft),
        ["rw"] = new("Kinyarwanda", "Kinyarwanda", "rw", "kin", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["sa"] = new("Sanskrit", "संस्कृत", "sa", "san", TranslationServices.Google),
        ["sah"] = new("Yakut", "Саха тыла", "sah", "sah", TranslationServices.Yandex),
        ["sd"] = new("Sindhi", "سنڌي", "sd", "snd", TranslationServices.Google),
        ["si"] = new("Sinhala", "සිංහල", "si", "sin", TranslationServices.Google | TranslationServices.Yandex),
        ["sjn"] = new("Sindarin", "Eledhrim", "sjn", "sjn", TranslationServices.Yandex), // Not present in Yandex.Cloud
        ["sk"] = new("Slovak", "Slovenčina", "sk", "slk"),
        ["sl"] = new("Slovenian", "Slovenščina", "sl", "slv"),
        ["sm"] = new("Samoan", "Gagana Sāmoa", "sm", "smo", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["sn"] = new("Shona", "chiShona", "sn", "sna", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["so"] = new("Somali", "Af Soomaali", "so", "som", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["sq"] = new("Albanian", "Shqip", "sq", "sqi"),
        ["sr"] = new("Serbian (Cyrillic)", "Српски", "sr", "srp"),
        ["sr-Latn"] = new("Serbian (Latin)", "Srpski (latinica)", "sr-Latn", "srp-Latn", TranslationServices.Bing | TranslationServices.Yandex | TranslationServices.Microsoft),
        ["st"] = new("Sotho", "Sotho", "st", "sot", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["su"] = new("Sundanese", "Basa Sunda", "su", "sun", TranslationServices.Google | TranslationServices.Yandex),
        ["sv"] = new("Swedish", "Svenska", "sv", "swe"),
        ["sw"] = new("Swahili", "Kiswahili", "sw", "swa"),
        ["ta"] = new("Tamil", "தமிழ்", "ta", "tam"),
        ["te"] = new("Telugu", "తెలుగు", "te", "tel"),
        ["tg"] = new("Tajik", "тоҷикӣ", "tg", "tgk", TranslationServices.Google | TranslationServices.Yandex),
        ["th"] = new("Thai", "ไทย", "th", "tha"),
        ["ti"] = new("Tigrinya", "ትግር", "ti", "tir", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["tk"] = new("Turkmen", "Türkmen Dili", "tk", "tuk", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["tl"] = new("Tagalog", "Tagalog", "tl", "tgl", TranslationServices.Google | TranslationServices.Yandex),
        ["tlh"] = new("Klingon", "tlhIngan Hol", "tlh", "tlh", TranslationServices.Bing | TranslationServices.Microsoft),
        ["tlh-Piqd"] = new("Klingon (pIqaD)", "Klingon (pIqaD)", "tlh-Piqd", "tlh-Piqd", TranslationServices.Microsoft), // For some reason Bing stopped supporting this language, ty Microsoft
        ["tn"] = new("Setswana", "Setswana", "tn", "tn", TranslationServices.Bing | TranslationServices.Microsoft),
        ["to"] = new("Tongan", "Lea Fakatonga", "to", "ton", TranslationServices.Bing | TranslationServices.Microsoft),
        ["tr"] = new("Turkish", "Türkçe", "tr", "tur"),
        ["ts"] = new("Tsonga", "Xitsonga", "ts", "tso", TranslationServices.Google),
        ["tt"] = new("Tatar", "Татар", "tt", "tat"),
        ["ty"] = new("Tahitian", "Reo Tahiti", "ty", "tah", TranslationServices.Bing | TranslationServices.Microsoft),
        ["udm"] = new("Udmurt", "Удмурт кыл", "udm", "udm", TranslationServices.Yandex),
        ["ug"] = new("Uyghur", "ئۇيغۇرچە", "ug", "uig", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["uk"] = new("Ukrainian", "Українська", "uk", "ukr"),
        ["ur"] = new("Urdu", "اردو", "ur", "urd"),
        ["uz"] = new("Uzbek", "Uzbek", "uz", "uzb"),
        ["uzbcyr"] = new("Uzbek (Cyrillic)", "Ўзбекча", "uzbcyr", "uzbcyr", TranslationServices.Yandex),
        ["vi"] = new("Vietnamese", "Tiếng Việt", "vi", "vie"),
        ["xh"] = new("Xhosa", "isiXhosa", "xh", "xho"),
        ["yi"] = new("Yiddish", "ייִדיש", "yi", "yid", TranslationServices.Google | TranslationServices.Yandex),
        ["yo"] = new("Yoruba", "Èdè Yorùbá", "yo", "yor", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["yua"] = new("Yucatec Maya", "Yucatec Maya", "yua", "yua", TranslationServices.Bing | TranslationServices.Microsoft),
        ["yue"] = new("Cantonese", "粵語", "yue", "yue", TranslationServices.Bing | TranslationServices.Microsoft),
        ["zh-CN"] = new("Chinese (Simplified)", "中文 (简体)", "zh-CN", "zho-CN"),
        ["zh-TW"] = new("Chinese (Traditional)", "繁體中文 (繁體)", "zh-TW", "zho-TW", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["zu"] = new("Zulu", "Isi-Zulu", "zu", "zul")
    });
}
