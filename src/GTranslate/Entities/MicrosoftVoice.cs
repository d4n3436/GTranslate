﻿using System.Diagnostics;
using System.Text.Json.Serialization;

namespace GTranslate;

/// <summary>
/// Represents a TTS voice in Microsoft Translator.
/// </summary>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
public class MicrosoftVoice
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MicrosoftVoice"/> class.
    /// </summary>
    /// <param name="displayName">The display name of the voice.</param>
    /// <param name="shortName">The short name of the voice.</param>
    /// <param name="gender">The gender of the voice.</param>
    /// <param name="locale">The locale of the voice.</param>
    public MicrosoftVoice(string displayName, string shortName, string gender, string locale)
    {
        DisplayName = displayName;
        Gender = gender;
        ShortName = shortName;
        Locale = locale;
    }

    /// <summary>
    /// Gets the display name of this voice.
    /// </summary>
    [JsonPropertyName("DisplayName")]
    public string DisplayName { get; }

    /// <summary>
    /// Gets the short name of this voice.
    /// </summary>
    [JsonPropertyName("ShortName")]
    public string ShortName { get; }

    /// <summary>
    /// Gets the gender of this voice.
    /// </summary>
    [JsonPropertyName("Gender")]
    public string Gender { get; }

    /// <summary>
    /// Gets the locale of this voice.
    /// </summary>
    [JsonPropertyName("Locale")]
    public string Locale { get; }

    /// <inheritdoc/>
    public override string ToString() => $"{nameof(DisplayName)}: '{DisplayName}', {nameof(Locale)}: '{Locale}'";

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => ToString();
}