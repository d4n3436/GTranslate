using Xunit;

namespace GTranslate.Tests;

internal sealed class LiveFactAttribute : FactAttribute
{
    public LiveFactAttribute()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("RUN_TRANSLATION_LIVE_TESTS"), "true", StringComparison.OrdinalIgnoreCase))
        {
            Skip = "Set RUN_TRANSLATION_LIVE_TESTS=true to run live provider tests.";
        }
    }
}
