using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using GTranslate.Extensions;

namespace GTranslate;

#if NET8_0_OR_GREATER
using ReadOnlyLanguageDictionary = System.Collections.Frozen.FrozenDictionary<string, Language>;
#else
using ReadOnlyLanguageDictionary = System.Collections.ObjectModel.ReadOnlyDictionary<string, Language>;
#endif

/// <summary>
/// Represents the default language dictionary used in GTranslate. It contains all the supported languages across all the included translators.
/// </summary>
public sealed class LanguageDictionary : ILanguageDictionary<string, Language>
{
    private const int TotalLanguages = 269;
    private const int TotalAliases = 801;

    internal LanguageDictionary()
    {
        Aliases = BuildLanguageAliases().ToReadOnlyDictionary();
        Debug.Assert(Count == TotalLanguages, $"TotalLanguages is outdated (Count was {Count})");
        Debug.Assert(Aliases.Count == TotalAliases, $"TotalAliases is outdated (Count was {Aliases.Count})");
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
    /// <param name="code">The language name or code. It can be a ISO 639-1 code, a ISO 639-3 code, a language name, or a language alias.</param>
    /// <returns>The language, or an exception if the language was not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="code"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the language was not found.</exception>
    public Language GetLanguage(string code)
    {
        TranslatorGuards.NotNull(code);
        if (!TryGetLanguage(code, out var language))
            throw new ArgumentException($"Unknown language \"{code}\".", nameof(code));

        return language;
    }

    /// <summary>
    /// Tries to get a language from a language code, name or alias.
    /// </summary>
    /// <param name="code">The language name or code. It can be a ISO 639-1 code, a ISO 639-3 code, a language name, or a language alias.</param>
    /// <param name="language">The language, if found.</param>
    /// <returns><see langword="true"/> if the language was found, otherwise <see langword="false"/>.</returns>
    public bool TryGetLanguage(string code, [MaybeNullWhen(false)] out Language language)
    {
        language = null;

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
        var aliases = new Dictionary<string, string>(TotalAliases, StringComparer.InvariantCultureIgnoreCase)
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
            ["tw"] = "ak",
            ["fa-af"] = "prs", // Persian (Afghanistan)
            ["tamazight"] = "ber",
            ["bm-nkoo"] = "nqo", // Google uses "bm-Nkoo" for NKo
            ["ndc-zw"] = "ndc", // Ndau (Zimbabwe)
            ["sat-Latn"] = "sat"
        };

        foreach (var kvp in _languages)
        {
            aliases[kvp.Value.Name] = kvp.Key;
            aliases[kvp.Value.NativeName] = kvp.Key;
            aliases[kvp.Value.ISO6393] = kvp.Key;
        }

        return aliases;
    }

    private readonly ReadOnlyLanguageDictionary _languages = new Dictionary<string, Language>(TotalLanguages, StringComparer.OrdinalIgnoreCase)
    {
        ["aa"] = new("Afar", "Qafaraf", "aa", "aar", TranslationServices.Google),
        ["ab"] = new("Abkhaz", "Аԥсуа бызшәа", "ab", "abk", TranslationServices.Google),
        ["ace"] = new("Acehnese", "بهسا اچيه", "ace", "ace", TranslationServices.Google),
        ["ach"] = new("Acholi", "Lwo", "ach", "ach", TranslationServices.Google),
        ["af"] = new("Afrikaans", "Afrikaans", "af", "afr"),
        ["ak"] = new("Akan", "Ákán", "ak", "aka", TranslationServices.Google),
        ["alz"] = new("Alur", "Dho-Alur", "alz", "alz", TranslationServices.Google),
        ["am"] = new("Amharic", "አማርኛ", "am", "amh"),
        ["ar"] = new("Arabic", "العربية", "ar", "ara"),
        ["as"] = new("Assamese", "অসমীয়া", "as", "asm", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["av"] = new("Avar", "Магӏарул мацӏ", "av", "ava", TranslationServices.Google),
        ["awa"] = new("Awadhi", "अवधी", "awa", "awa", TranslationServices.Google),
        ["ay"] = new("Aymara", "Aymar aru", "ay", "aym", TranslationServices.Google),
        ["az"] = new("Azerbaijani", "Azərbaycan", "az", "aze"),
        ["ba"] = new("Bashkir", "Bashkir", "ba", "bak"),
        ["bal"] = new("Baluchi", "بلۏچی", "bal", "bal", TranslationServices.Google),
        ["ban"] = new("Balinese", "Basa Bali", "ban", "ban", TranslationServices.Google),
        ["bbc"] = new("Batak Toba", "Hata Batak Toba", "bbc", "bbc", TranslationServices.Google),
        ["bci"] = new("Baoulé", "Baule", "bci", "bci", TranslationServices.Google),
        ["be"] = new("Belarusian", "беларуская", "be", "bel", TranslationServices.Google | TranslationServices.Yandex),
        ["bem"] = new("Bemba", "Chibemba", "bem", "bem", TranslationServices.Google),
        ["ber"] = new("Berber", "ⵜⴰⵎⴰⵣⵉⵖⵜ", "ber", "ber", TranslationServices.Google),
        ["ber-Latn"] = new("Berber (Latin)", "Tamaziɣt (Talatinit)", "ber-Latn", "ber-Latn", TranslationServices.Google),
        ["bew"] = new("Betawi", "Bahasa Betawi", "bew", "bew", TranslationServices.Google),
        ["bg"] = new("Bulgarian", "Български", "bg", "bul"),
        ["bho"] = new("Bhojpuri", "भोजपुरी", "bho", "bho", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["bm"] = new("Bambara", "Bamanankan", "bm", "bam", TranslationServices.Google),
        ["bik"] = new("Bikol", "Bikol", "bik", "bik", TranslationServices.Google),
        ["bn"] = new("Bengali", "বাংলা", "bn", "ben"),
        ["bo"] = new("Tibetan", "བོད་སྐད་", "bo", "bod", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["br"] = new("Breton", "brezhoneg", "br", "bre", TranslationServices.Google),
        ["brx"] = new("Bodo", "बड़ो", "brx", "brx", TranslationServices.Bing | TranslationServices.Microsoft),
        ["bs"] = new("Bosnian", "bosanski", "bs", "bos"),
        ["bts"] = new("Batak Simalungun", "Sahap Simalungun", "bts", "bts", TranslationServices.Google),
        ["btx"] = new("Batak Karo", "Cakap Karo", "btx", "btx", TranslationServices.Google),
        ["bua"] = new("Buryat", "буряад хэлэн", "bua", "bua", TranslationServices.Google),
        ["ca"] = new("Catalan", "Català", "ca", "cat"),
        ["ce"] = new("Chechen", "Нохчийн мотт", "ce", "che", TranslationServices.Google),
        ["ceb"] = new("Cebuano", "Binisaya", "ceb", "ceb", TranslationServices.Google | TranslationServices.Yandex),
        ["cgg"] = new("Kiga", "Chiga", "cgg", "cgg", TranslationServices.Google),
        ["ch"] = new("Chamorro", "Finuʼ Chamoru", "ch", "cha", TranslationServices.Google),
        ["chk"] = new("Chuukese", "Chuuk", "chk", "chk", TranslationServices.Google),
        ["chm"] = new("Mari", "марий йылме", "chm", "chm", TranslationServices.Google),
        ["ckb"] = new("Kurdish (Central)", "کوردیی ناوەندی", "ckb", "ckb", TranslationServices.Google),
        ["cnh"] = new("Hakha Chin", "Laiholh", "cnh", "cnh", TranslationServices.Google),
        ["co"] = new("Corsican", "Corsu", "co", "cos", TranslationServices.Google),
        ["crh"] = new("Crimean Tatar", "Къырымтатар тили", "crh", "crh", TranslationServices.Google),
        ["crh-Latn"] = new("Crimean Tatar (Latin)", "qırımtatar tili", "crh-Latn", "crh-Latn", TranslationServices.Google),
        ["crs"] = new("Seychellois Creole", "Seselwa", "crs", "crs", TranslationServices.Google),
        ["cs"] = new("Czech", "Čeština", "cs", "ces"),
        ["cv"] = new("Chuvash", "Чӑвашла", "cv", "chv", TranslationServices.Google | TranslationServices.Yandex),
        ["cy"] = new("Welsh", "Cymraeg", "cy", "cym"),
        ["da"] = new("Danish", "Dansk", "da", "dan"),
        ["de"] = new("German", "Deutsch", "de", "deu"),
        ["din"] = new("Dinka", "Thuɔŋjäŋ", "din", "din", TranslationServices.Google),
        ["doi"] = new("Dogri", "डोगरी", "doi", "doi", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["dov"] = new("Dombe", "Dombe", "dov", "dov", TranslationServices.Google),
        ["dsb"] = new("Lower Sorbian", "Dolnoserbšćina", "dsb", "dsb", TranslationServices.Bing | TranslationServices.Microsoft),
        ["dv"] = new("Divehi", "ދިވެހިބަސް", "dv", "div", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["dyu"] = new("Dyula", "Julakan", "dyu", "dyu", TranslationServices.Google),
        ["dz"] = new("Dzongkha", "རྫོང་ཁ་", "dz", "dzo", TranslationServices.Google),
        ["ee"] = new("Ewe", "Eʋegbe", "ee", "ewe", TranslationServices.Google),
        ["el"] = new("Greek", "Ελληνικά", "el", "ell"),
        ["emj"] = new("Emoji", "Emoji", "emj", "emj", TranslationServices.Yandex), // Not present in Yandex.Cloud
        ["en"] = new("English", "English", "en", "eng"),
        ["eo"] = new("Esperanto", "Esperanto", "eo", "epo", TranslationServices.Google | TranslationServices.Yandex),
        ["es"] = new("Spanish", "Español", "es", "spa"),
        ["et"] = new("Estonian", "Eesti", "et", "est"),
        ["eu"] = new("Basque", "Euskara", "eu", "eus"),
        ["fa"] = new("Persian", "فارسی", "fa", "fas"),
        ["ff"] = new("Fula", "Fulfulde", "ff", "ful", TranslationServices.Google),
        ["fi"] = new("Finnish", "Suomi", "fi", "fin"),
        ["fil"] = new("Filipino", "Filipino", "fil", "fil", TranslationServices.Bing | TranslationServices.Microsoft),
        ["fj"] = new("Fijian", "Na Vosa Vakaviti", "fj", "fij", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["fo"] = new("Faroese", "føroyskt mál", "fo", "fao", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["fon"] = new("Fon", "Fɔngbè", "fon", "fon", TranslationServices.Google),
        ["fr"] = new("French", "Français", "fr", "fra"),
        ["fr-CA"] = new("French (Canada)", "Français (Canada)", "fr-CA", "fr-CA", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["fur"] = new("Friulian", "furlan", "fur", "fur", TranslationServices.Google),
        ["fy"] = new("Frisian", "Frysk", "fy", "fry", TranslationServices.Google),
        ["ga"] = new("Irish", "Gaeilge", "ga", "gle"),
        ["gaa"] = new("Ga", "Gã", "gaa", "gaa", TranslationServices.Google),
        ["gd"] = new("Scottish Gaelic", "Gàidhlig", "gd", "gla", TranslationServices.Google | TranslationServices.Yandex),
        ["gl"] = new("Galician", "Galego", "gl", "glg"),
        ["gn"] = new("Guarani", "avañeʼẽ", "gn", "grn", TranslationServices.Google),
        ["gom"] = new("Goan Konkani", "कोंकणी", "gom", "gom", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["gu"] = new("Gujarati", "ગુજરાતી", "gu", "guj"),
        ["gv"] = new("Manx", "Gaelg", "gv", "glv", TranslationServices.Google),
        ["ha"] = new("Hausa", "Hausa", "ha", "hau", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["haw"] = new("Hawaiian", "ʻŌlelo Hawaiʻi", "haw", "haw", TranslationServices.Google),
        ["he"] = new("Hebrew", "עברית", "he", "heb"),
        ["hi"] = new("Hindi", "हिन्दी", "hi", "hin"),
        ["hil"] = new("Hiligaynon", "Ilonggo", "hil", "hil", TranslationServices.Google),
        ["hmn"] = new("Hmong", "Hmong", "hmn", "hmn", TranslationServices.Google),
        ["hne"] = new("Chhattisgarhi", "छत्तीसगढ़ी", "hne", "hne", TranslationServices.Bing | TranslationServices.Microsoft),
        ["hr"] = new("Croatian", "Hrvatski", "hr", "hrv"),
        ["hrx"] = new("Hunsrik", "Hunsrik", "hrx", "hrx", TranslationServices.Google),
        ["hsb"] = new("Upper Sorbian", "Hornjoserbšćina", "hsb", "hsb", TranslationServices.Bing | TranslationServices.Microsoft),
        ["ht"] = new("Haitian Creole", "Kreyòl ayisyen", "ht", "hat"),
        ["hu"] = new("Hungarian", "Magyar", "hu", "hun"),
        ["hy"] = new("Armenian", "Հայերեն", "hy", "hye"),
        ["iba"] = new("Iban", "Jaku Iban", "iba", "iba", TranslationServices.Google),
        ["id"] = new("Indonesian", "Indonesia", "id", "ind"),
        ["ig"] = new("Igbo", "Igbo", "ig", "ibo", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["ikt"] = new("Inuinnaqtun", "Inuinnaqtun", "ikt", "ikt", TranslationServices.Bing | TranslationServices.Microsoft),
        ["ilo"] = new("Ilocano", "Iloko", "ilo", "ilo", TranslationServices.Google),
        ["is"] = new("Icelandic", "Íslenska", "is", "isl"),
        ["it"] = new("Italian", "Italiano", "it", "ita"),
        ["iu"] = new("Inuktitut", "ᐃᓄᒃᑎᑐᑦ", "iu", "iku", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["iu-Latn"] = new("Inuktitut (Latin)", "Inuktitut (Latin)", "iu-Latn", "iu-Latn", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["ja"] = new("Japanese", "日本語", "ja", "jpn"),
        ["jam"] = new("Jamaican Patois", "Patwah", "jam", "jam", TranslationServices.Google),
        ["jv"] = new("Javanese", "Jawa", "jv", "jav", TranslationServices.Google | TranslationServices.Yandex),
        ["ka"] = new("Georgian", "ქართული", "ka", "kat"),
        ["kac"] = new("Jingpo", "Jinghpaw ga", "kac", "kac", TranslationServices.Google),
        ["kazlat"] = new("Kazakh (Latin)", "qazaqşa", "kazlat", "kazlat", TranslationServices.Yandex),
        ["kek"] = new("Qʼeqchiʼ", "Kekchi", "kek", "kek", TranslationServices.Google),
        ["kg"] = new("Kikongo", "Kikongo", "kg", "kon", TranslationServices.Google),
        ["kha"] = new("Khasi", "Ka Ktien Khasi", "kha", "kha", TranslationServices.Google),
        ["kk"] = new("Kazakh", "Қазақ Тілі", "kk", "kaz"),
        ["kl"] = new("Greenlandic", "Kalaallisut", "kl", "kal", TranslationServices.Google),
        ["km"] = new("Khmer", "ខ្មែរ", "km", "khm"),
        ["kmr"] = new("Kurdish (Northern)", "Kurdî (Bakur)", "kmr", "kmr", TranslationServices.Bing | TranslationServices.Microsoft),
        ["kn"] = new("Kannada", "ಕನ್ನಡ", "kn", "kan"),
        ["ko"] = new("Korean", "한국어", "ko", "kor"),
        ["kr"] = new("Kanuri", "Kànùrí", "kr", "kau", TranslationServices.Google),
        ["kri"] = new("Krio", "Krio", "kri", "kri", TranslationServices.Google),
        ["ks"] = new("Kashmiri", "کٲشُر", "ks", "kas", TranslationServices.Bing | TranslationServices.Microsoft),
        ["ktu"] = new("Kituba", "Kikongo ya leta", "ktu", "ktu", TranslationServices.Google),
        ["ku"] = new("Kurdish", "Kurdî", "ku", "kur", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["kv"] = new("Komi", "Коми кыв", "kv", "kom", TranslationServices.Google | TranslationServices.Yandex),
        ["ky"] = new("Kyrgyz", "Kyrgyz", "ky", "kir"),
        ["la"] = new("Latin", "Latina", "la", "lat", TranslationServices.Google | TranslationServices.Yandex),
        ["lb"] = new("Luxembourgish", "Lëtzebuergesch", "lb", "ltz", TranslationServices.Google | TranslationServices.Yandex),
        ["lg"] = new("Luganda", "Oluganda", "lg", "lug", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["li"] = new("Limburgish", "Limburgs ", "li", "lim", TranslationServices.Google),
        ["lij"] = new("Ligurian", "Lìgure", "lij", "lij", TranslationServices.Google),
        ["lmo"] = new("Lombard", "Lombard", "lmo", "lmo", TranslationServices.Google),
        ["ln"] = new("Lingala", "Lingála", "ln", "lin", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["lo"] = new("Lao", "ລາວ", "lo", "lao"),
        ["lt"] = new("Lithuanian", "Lietuvių", "lt", "lit"),
        ["ltg"] = new("Latgalian", "latgalīšu volūda", "ltg", "ltg", TranslationServices.Google),
        ["lua"] = new("Tshiluba", "Tshiluba", "lua", "lua", TranslationServices.Google),
        ["luo"] = new("Luo", "Dholuo", "luo", "luo", TranslationServices.Google),
        ["lus"] = new("Mizo", "Mizo ṭawng", "lus", "lus", TranslationServices.Google),
        ["lv"] = new("Latvian", "Latviešu", "lv", "lav"),
        ["lzh"] = new("Chinese (Literary)", "中文 (文言文)", "lzh", "lzh", TranslationServices.Bing | TranslationServices.Microsoft),
        ["mad"] = new("Madurese", "Bhâsa Madhurâ", "mad", "mad", TranslationServices.Google),
        ["mai"] = new("Maithili", "मैथिली", "mai", "mai", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["mak"] = new("Makassarese", "Bahasa Makassar", "mak", "mak", TranslationServices.Google),
        ["mam"] = new("Mam", "Qyol Mam", "mam", "mam", TranslationServices.Google),
        ["mfe"] = new("Mauritian Creole", "Kreol Morisien", "mfe", "mfe", TranslationServices.Google),
        ["mg"] = new("Malagasy", "Malagasy", "mg", "mlg"),
        ["mh"] = new("Marshallese", "Kajin Majōl", "mh", "mah", TranslationServices.Google),
        ["mhr"] = new("Eastern Mari", "олык марий", "mhr", "mhr", TranslationServices.Yandex),
        ["mi"] = new("Maori", "Te Reo Māori", "mi", "mri"),
        ["min"] = new("Minangkabau", "Baso Minangkabau", "min", "min", TranslationServices.Google),
        ["mk"] = new("Macedonian", "Македонски", "mk", "mkd"),
        ["ml"] = new("Malayalam", "മലയാളം", "ml", "mal"),
        ["mn"] = new("Mongolian", "Монгол хэл", "mn", "mon"),
        ["mn-Mong"] = new("Mongolian (Traditional)", "ᠮᠣᠩᠭᠣᠯ ᠬᠡᠯᠡ", "mn-Mong", "mn-Mong", TranslationServices.Bing | TranslationServices.Microsoft),
        ["mni"] = new("Manipuri", "\uABC3\uABE9\uABC7\uABE9\uABC2\uABE3\uABDF", "mni", "mni", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["mr"] = new("Marathi", "मराठी", "mr", "mar"),
        ["mrj"] = new("Western Mari", "Мары йӹлмӹ", "mrj", "mrj", TranslationServices.Yandex),
        ["ms"] = new("Malay", "Melayu", "ms", "msa"),
        ["ms-Arab"] = new("Malay (Jawi)", "بهاس ملايو", "ms-Arab", "ms-Arab", TranslationServices.Google),
        ["mt"] = new("Maltese", "Malti", "mt", "mlt"),
        ["mwr"] = new("Marwari", "मारवाड़ी", "mwr", "mwr", TranslationServices.Google),
        ["mww"] = new("Hmong Daw", "Hmong Daw", "mww", "mww", TranslationServices.Bing | TranslationServices.Microsoft),
        ["my"] = new("Burmese", "မြန်မာ", "my", "mya"),
        ["ndc"] = new("Ndau", "Ndau", "ndc", "ndc", TranslationServices.Google),
        ["ne"] = new("Nepali", "नेपाली", "ne", "nep"),
        ["new"] = new("Newar", "नेपाल भाषा", "new", "new", TranslationServices.Google),
        ["nhe"] = new("Nahuatl", "Nawatlahtolli", "nhe", "nhe", TranslationServices.Google),
        ["nl"] = new("Dutch", "Nederlands", "nl", "nld"),
        ["no"] = new("Norwegian", "Norsk", "no", "nor"),
        ["nqo"] = new("NKo", "ߒߞߏ", "nqo", "nqo", TranslationServices.Google),
        ["nr"] = new("Ndebele (South)", "isiNdebele", "nr", "nbl", TranslationServices.Google),
        ["nso"] = new("Sepedi", "Sesotho sa Leboa", "nso", "nso", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["nus"] = new("Nuer", "Thok Naath", "nus", "nus", TranslationServices.Google),
        ["ny"] = new("Chichewa", "Nyanja", "ny", "nya", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["oc"] = new("Occitan", "Occitan", "oc", "oci", TranslationServices.Google),
        ["om"] = new("Oromo", "Afaan Oromoo", "om", "orm", TranslationServices.Google),
        ["or"] = new("Odia", "ଓଡ଼ିଆ", "or", "ori", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["os"] = new("Ossetian", "ирон ӕвзаг", "os", "oss", TranslationServices.Google | TranslationServices.Yandex),
        ["otq"] = new("Querétaro Otomi", "Hñähñu", "otq", "otq", TranslationServices.Bing | TranslationServices.Microsoft),
        ["pa"] = new("Punjabi", "ਪੰਜਾਬੀ", "pa", "pan"),
        ["pa-Arab"] = new("Punjabi (Shahmukhi)", "پنجابی", "pa-Arab", "pa-Arab", TranslationServices.Google),
        ["pag"] = new("Pangasinan", "Pangasinense", "pag", "pag", TranslationServices.Google),
        ["pam"] = new("Kapampangan", "Pampangan", "pam", "pam", TranslationServices.Google),
        ["pap"] = new("Papiamento", "Papiamento", "pap", "pap", TranslationServices.Google | TranslationServices.Yandex),
        ["pl"] = new("Polish", "Polski", "pl", "pol"),
        ["prs"] = new("Dari", "دری", "prs", "prs", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["ps"] = new("Pashto", "پښتو", "ps", "pus", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["pt"] = new("Portuguese", "Português", "pt", "por"),
        ["pt-PT"] = new("Portuguese (Portugal)", "Português (Portugal)", "pt-PT", "pt-PT"),
        ["qu"] = new("Quechua", "Runa simi", "qu", "que", TranslationServices.Google),
        ["rn"] = new("Rundi", "Ikirundi", "rn", "run", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["ro"] = new("Romanian", "Română", "ro", "ron"),
        ["rom"] = new("Romani", "romani ćhib", "rom", "rom", TranslationServices.Google),
        ["ru"] = new("Russian", "Русский", "ru", "rus"),
        ["rw"] = new("Kinyarwanda", "Kinyarwanda", "rw", "kin", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["sa"] = new("Sanskrit", "संस्कृत", "sa", "san", TranslationServices.Google),
        ["sah"] = new("Yakut", "Саха тыла", "sah", "sah", TranslationServices.Google | TranslationServices.Yandex),
        ["sat"] = new("Santali", "Santali", "sat", "sat", TranslationServices.Google),
        ["scn"] = new("Sicilian", "sicilianu", "scn", "scn", TranslationServices.Google),
        ["sd"] = new("Sindhi", "سنڌي", "sd", "snd", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["se"] = new("Northern Sámi", "davvisámegiella", "se", "sme", TranslationServices.Google),
        ["sg"] = new("Sango", "yângâ tî sängö", "sg", "sag", TranslationServices.Google),
        ["shn"] = new("Shan", "လိၵ်ႈတႆး", "shn", "shn", TranslationServices.Google),
        ["si"] = new("Sinhala", "සිංහල", "si", "sin"),
        ["sjn"] = new("Sindarin", "Eledhrim", "sjn", "sjn", TranslationServices.Yandex), // Not present in Yandex.Cloud
        ["sk"] = new("Slovak", "Slovenčina", "sk", "slk"),
        ["sl"] = new("Slovenian", "Slovenščina", "sl", "slv"),
        ["sm"] = new("Samoan", "Gagana Sāmoa", "sm", "smo", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["sn"] = new("Shona", "chiShona", "sn", "sna", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["so"] = new("Somali", "Af Soomaali", "so", "som", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["sq"] = new("Albanian", "Shqip", "sq", "sqi"),
        ["sr"] = new("Serbian (Cyrillic)", "Српски", "sr", "srp"),
        ["sr-Latn"] = new("Serbian (Latin)", "Srpski (latinica)", "sr-Latn", "srp-Latn", TranslationServices.Bing | TranslationServices.Yandex | TranslationServices.Microsoft),
        ["ss"] = new("Swati", "siSwati", "ss", "ssw", TranslationServices.Google),
        ["st"] = new("Sotho", "Sotho", "st", "sot", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["su"] = new("Sundanese", "Basa Sunda", "su", "sun", TranslationServices.Google | TranslationServices.Yandex),
        ["sus"] = new("Susu", "Sosoxui", "sus", "sus", TranslationServices.Google),
        ["sv"] = new("Swedish", "Svenska", "sv", "swe"),
        ["sw"] = new("Swahili", "Kiswahili", "sw", "swa"),
        ["szl"] = new("Silesian", "ślōnskŏ gŏdka", "szl", "szl", TranslationServices.Google),
        ["ta"] = new("Tamil", "தமிழ்", "ta", "tam"),
        ["tcy"] = new("Tulu", "ತುಳು", "tcy", "tcy", TranslationServices.Google),
        ["te"] = new("Telugu", "తెలుగు", "te", "tel"),
        ["tet"] = new("Tetum", "Tetun", "tet", "tet", TranslationServices.Google),
        ["tg"] = new("Tajik", "тоҷикӣ", "tg", "tgk", TranslationServices.Google | TranslationServices.Yandex),
        ["th"] = new("Thai", "ไทย", "th", "tha"),
        ["ti"] = new("Tigrinya", "ትግር", "ti", "tir", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["tiv"] = new("Tiv", "Tiv", "tiv", "tiv", TranslationServices.Google),
        ["tk"] = new("Turkmen", "Türkmen Dili", "tk", "tuk", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["tl"] = new("Tagalog", "Tagalog", "tl", "tgl", TranslationServices.Google | TranslationServices.Yandex),
        ["tlh"] = new("Klingon", "tlhIngan Hol", "tlh", "tlh", TranslationServices.Bing | TranslationServices.Microsoft),
        ["tlh-Piqd"] = new("Klingon (pIqaD)", "Klingon (pIqaD)", "tlh-Piqd", "tlh-Piqd", TranslationServices.Microsoft), // Not present in Bing
        ["tn"] = new("Tswana", "Setswana", "tn", "tn", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["to"] = new("Tongan", "Lea Fakatonga", "to", "ton", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["tpi"] = new("Tok Pisin", "Tok Pisin", "tpi", "tpi", TranslationServices.Google),
        ["tr"] = new("Turkish", "Türkçe", "tr", "tur"),
        ["trp"] = new("Kokborok", "Kokborok", "trp", "trp", TranslationServices.Google),
        ["ts"] = new("Tsonga", "Xitsonga", "ts", "tso", TranslationServices.Google),
        ["tt"] = new("Tatar", "Татар", "tt", "tat"),
        ["tum"] = new("Tumbuka", "chiTumbuka", "tum", "tum", TranslationServices.Google),
        ["ty"] = new("Tahitian", "Reo Tahiti", "ty", "tah", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["tyv"] = new("Tuvan", "тыва дыл", "tyv", "tyv", TranslationServices.Google | TranslationServices.Yandex),
        ["udm"] = new("Udmurt", "Удмурт кыл", "udm", "udm", TranslationServices.Google | TranslationServices.Yandex),
        ["ug"] = new("Uyghur", "ئۇيغۇرچە", "ug", "uig", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["uk"] = new("Ukrainian", "Українська", "uk", "ukr"),
        ["ur"] = new("Urdu", "اردو", "ur", "urd"),
        ["uz"] = new("Uzbek", "Uzbek", "uz", "uzb"),
        ["uzbcyr"] = new("Uzbek (Cyrillic)", "Ўзбекча", "uzbcyr", "uzbcyr", TranslationServices.Yandex),
        ["ve"] = new("Venda", "Tshivenḓa", "ve", "ven", TranslationServices.Google),
        ["vec"] = new("Venetian", "vèneto", "vec", "vec", TranslationServices.Google),
        ["vi"] = new("Vietnamese", "Tiếng Việt", "vi", "vie"),
        ["war"] = new("Waray", "Waray", "war", "war", TranslationServices.Google),
        ["wo"] = new("Wolof", "Wolof", "wo", "wol", TranslationServices.Google),
        ["xh"] = new("Xhosa", "isiXhosa", "xh", "xho"),
        ["yi"] = new("Yiddish", "ייִדיש", "yi", "yid", TranslationServices.Google | TranslationServices.Yandex),
        ["yo"] = new("Yoruba", "Èdè Yorùbá", "yo", "yor", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["yua"] = new("Yucatec Maya", "Yucatec Maya", "yua", "yua", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["yue"] = new("Cantonese", "粵語", "yue", "yue", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["zap"] = new("Zapotec", "Diidxazá", "zap", "zap", TranslationServices.Google),
        ["zh-CN"] = new("Chinese (Simplified)", "中文 (简体)", "zh-CN", "zho-CN"),
        ["zh-TW"] = new("Chinese (Traditional)", "繁體中文 (繁體)", "zh-TW", "zho-TW", TranslationServices.Google | TranslationServices.Bing | TranslationServices.Microsoft),
        ["zu"] = new("Zulu", "Isi-Zulu", "zu", "zul")
    }.ToReadOnlyDictionary();
}
