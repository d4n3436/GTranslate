using System;

namespace GTranslate;

internal readonly struct BingCredentials
{
    public BingCredentials(string token, long key, Guid impressionGuid)
    {
        Token = token;
        Key = key;
        ImpressionGuid = impressionGuid;
    }

    public string Token { get; }

    public long Key { get; }

    public Guid ImpressionGuid { get; }
}