using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GTranslate.Models
{
    /// <summary>
    /// Represents the translation (plus transliteration and language detection) model for one of the internal Google Translate APIs
    /// </summary>
    internal class GoogleTranslationModel
    {
        [JsonProperty("sentences")]
        public IList<Sentence> Sentences { get; set; } = Array.Empty<Sentence>();

        [JsonProperty("src")]
        public string Source { get; set; }

        [JsonProperty("alternative_translations")]
        public IList<AltTranslation> AlternativeTranslations { get; set; } = Array.Empty<AltTranslation>();

        [JsonProperty("confidence")]
        public double Confidence { get; set; }

        [JsonProperty("ld_result")]
        public LdResult LanguageDetection { get; set; }

        [JsonProperty("dict", NullValueHandling = NullValueHandling.Ignore)]
        public IList<Dict> Dict { get; set; } = Array.Empty<Dict>();

        [JsonProperty("query_inflections", NullValueHandling = NullValueHandling.Ignore)]
        public IList<QueryInflection> QueryInflections { get; set; } = Array.Empty<QueryInflection>();
    }

    internal class AltTranslation
    {
        [JsonProperty("src_phrase")]
        public string SrcPhrase { get; set; }

        [JsonProperty("alternative")]
        public IList<Alternative> Alternative { get; set; } = Array.Empty<Alternative>();

        [JsonProperty("srcunicodeoffsets")]
        public IList<SourceUnicodeOffset> SourceUnicodeOffsets { get; set; } = Array.Empty<SourceUnicodeOffset>();

        [JsonProperty("raw_src_segment")]
        public string RawSrcSegment { get; set; }

        [JsonProperty("start_pos")]
        public int StartPos { get; set; }

        [JsonProperty("end_pos")]
        public int EndPos { get; set; }
    }

    internal class Alternative
    {
        [JsonProperty("word_postproc")]
        public string WordPostProcess { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("has_preceding_space")]
        public bool HasPrecedingSpace { get; set; }

        [JsonProperty("attach_to_next_token")]
        public bool AttachToNextToken { get; set; }
    }

    internal class SourceUnicodeOffset
    {
        [JsonProperty("begin")]
        public int Begin { get; set; }

        [JsonProperty("end")]
        public int End { get; set; }
    }

    internal class Dict
    {
        [JsonProperty("pos")]
        public string Position { get; set; }

        [JsonProperty("terms")]
        public IList<string> Terms { get; set; } = Array.Empty<string>();

        [JsonProperty("entry")]
        public IList<Entry> Entry { get; set; } = Array.Empty<Entry>();

        [JsonProperty("base_form")]
        public string BaseForm { get; set; }

        [JsonProperty("pos_enum")]
        public int PosEnum { get; set; }
    }

    internal class Entry
    {
        [JsonProperty("word")]
        public string Word { get; set; }

        [JsonProperty("reverse_translation")]
        public IList<string> ReverseTranslation { get; set; } = Array.Empty<string>();

        [JsonProperty("score", NullValueHandling = NullValueHandling.Ignore)]
        public double? Score { get; set; }
    }

    internal class LdResult
    {
        [JsonProperty("srclangs")]
        public IList<string> SourceLanguages { get; set; } = Array.Empty<string>();

        [JsonProperty("srclangs_confidences")]
        public IList<double> SourceLanguageConfidences { get; set; } = Array.Empty<double>();

        [JsonProperty("extended_srclangs")]
        public IList<string> ExtendedSourceLanguages { get; set; } = Array.Empty<string>();
    }

    internal class QueryInflection
    {
        [JsonProperty("written_form")]
        public string WrittenForm { get; set; }

        [JsonProperty("features")]
        public Feature Features { get; set; }
    }

    internal class Feature
    {
        [JsonProperty("gender")]
        public int? Gender { get; set; }

        [JsonProperty("number")]
        public int Number { get; set; }
    }

    internal class Sentence
    {
        [JsonProperty("trans")]
        public string Translation { get; set; }

        [JsonProperty("orig")]
        public string Origin { get; set; }

        [JsonProperty("backend")]
        public int? Backend { get; set; }

        [JsonProperty("translit")]
        public string Transliteration { get; set; }
    }
}