using GTranslate.Translators;

namespace GTranslate.Tests;

public sealed class DictionaryCapabilityTests
{
    [Fact]
    public void SupportedProvidersImplementDictionaryTranslator()
    {
        using var google = new GoogleTranslator();
        using var bing = new BingTranslator();
        using var microsoft = new MicrosoftTranslator();

        Assert.IsAssignableFrom<IDictionaryTranslator>(google);
        Assert.IsAssignableFrom<IDictionaryTranslator>(bing);
        Assert.IsAssignableFrom<IDictionaryTranslator>(microsoft);
    }

    [Fact]
    public void GoogleRpcDoesNotImplementDictionaryTranslator()
    {
        using var translator = new GoogleTranslator2();

        Assert.IsNotAssignableFrom<IDictionaryTranslator>(translator);
    }
}
