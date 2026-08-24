using GTranslate.Translators;

namespace GTranslate.Tests;

public sealed class DictionaryLiveTests
{
    [LiveFact]
    public async Task GoogleWebReturnsRichDictionaryData()
    {
        using var translator = new GoogleTranslator();
        var result = await translator.LookupDictionaryAsync("bank", "ja", "en");
        Assert.NotEmpty(result.Groups);
    }

    [LiveFact]
    public async Task BingReturnsRichDictionaryData()
    {
        using var translator = new BingTranslator();
        var result = await translator.LookupDictionaryAsync("bank", "zh-CN", "en");
        Assert.NotEmpty(result.Groups);
    }

    [LiveFact]
    public async Task MicrosoftReturnsRichDictionaryDataAndExamples()
    {
        using var translator = new MicrosoftTranslator();
        var result = await translator.LookupDictionaryAsync("bank", "zh-CN", "en");
        Assert.NotEmpty(result.Groups);
        Assert.Contains(result.Groups.SelectMany(x => x.Entries), x => x.Examples.Count > 0);
    }
}
