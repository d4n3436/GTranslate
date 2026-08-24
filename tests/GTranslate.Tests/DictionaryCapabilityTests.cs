using GTranslate;
using GTranslate.Translators;

namespace GTranslate.Tests;

public sealed class DictionaryCapabilityTests
{
    [Fact]
    public void SupportedProvidersAdvertiseDictionaryCapability()
    {
        AssertDictionarySupported(new GoogleTranslator());
        AssertDictionarySupported(new BingTranslator());
        AssertDictionarySupported(new MicrosoftTranslator());
    }

    [Fact]
    public void GoogleRpcDoesNotAdvertiseUnverifiedDictionaryCapability()
    {
        using var translator = new GoogleTranslator2();
        var capabilities = Assert.IsAssignableFrom<ITranslatorCapabilities>(translator).Capabilities;

        Assert.False(capabilities.HasFlag(TranslationServiceCapabilities.Dictionary));
        Assert.IsNotAssignableFrom<IDictionaryTranslator>(translator);
    }

    private static void AssertDictionarySupported(IDictionaryTranslator translator)
    {
        using var disposable = Assert.IsAssignableFrom<IDisposable>(translator);
        Assert.True(translator.Capabilities.HasFlag(TranslationServiceCapabilities.Dictionary));
    }
}
