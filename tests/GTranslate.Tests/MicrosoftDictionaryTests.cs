using GTranslate.Translators;

namespace GTranslate.Tests;

public sealed class MicrosoftDictionaryTests
{
    [Fact]
    public async Task LookupMapsTranslationsAndExamples()
    {
        using var client = new HttpClient(new FixtureHttpMessageHandler((request, _) =>
        {
            Assert.True(request.Headers.Contains("X-MT-Signature"));
            string fixture = request.RequestUri!.AbsolutePath.EndsWith("/lookup", StringComparison.Ordinal)
                ? Fixture.Read("Microsoft", "polysemous-word")
                : Fixture.Read("Microsoft", "examples");
            return Task.FromResult(FixtureHttpMessageHandler.Json(fixture));
        }));
        using var translator = new MicrosoftTranslator(client);

        var result = await translator.LookupDictionaryAsync("bank", "zh-CN", "en");

        Assert.Equal(nameof(MicrosoftTranslator), result.Service);
        var entry = Assert.Single(result.Groups).Entries[0];
        Assert.NotNull(entry.Confidence);
        Assert.NotEmpty(entry.BackTranslations);
        Assert.NotEmpty(entry.Examples);
    }

    [Fact]
    public async Task LookupDoesNotRequestExamplesForEmptyResult()
    {
        int requests = 0;
        using var client = new HttpClient(new FixtureHttpMessageHandler((_, _) =>
        {
            requests++;
            return Task.FromResult(FixtureHttpMessageHandler.Json(Fixture.Read("Microsoft", "empty-result")));
        }));
        using var translator = new MicrosoftTranslator(client);

        var result = await translator.LookupDictionaryAsync("sentence", "zh-CN", "en");

        Assert.Equal(1, requests);
        Assert.Empty(result.Groups);
    }

    [Theory]
    [InlineData("single-word")]
    [InlineData("optional-section-missing")]
    public async Task LookupAcceptsSingleAndOptionalSectionFixtures(string fixture)
    {
        using var client = new HttpClient(new FixtureHttpMessageHandler((request, _) =>
        {
            string response = request.RequestUri!.AbsolutePath.EndsWith("/lookup", StringComparison.Ordinal)
                ? Fixture.Read("Microsoft", fixture)
                : "[]";
            return Task.FromResult(FixtureHttpMessageHandler.Json(response));
        }));
        using var translator = new MicrosoftTranslator(client);

        var result = await translator.LookupDictionaryAsync("bank", "zh-CN", "en");

        Assert.Single(result.Groups);
        Assert.Single(result.Groups[0].Entries);
    }
}
