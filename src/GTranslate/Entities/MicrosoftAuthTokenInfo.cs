namespace GTranslate;

/// <summary>
/// Represents a Microsoft Authorization token.
/// </summary>
public sealed class MicrosoftAuthTokenInfo
{
    internal MicrosoftAuthTokenInfo(string token, string region)
    {
        Token = token;
        Region = region;
    }

    /// <summary>
    /// Gets the token.
    /// </summary>
    public string Token { get; }

    /// <summary>
    /// Gets the region of this token.
    /// </summary>
    public string Region { get; }

    /// <inheritdoc/>
    public override string ToString() => Token;
}