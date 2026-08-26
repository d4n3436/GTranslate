using GTranslate.Translators;

namespace GTranslate.Tests;

public sealed class BingDictionaryTests
{
    [Fact]
    public async Task LookupMapsFixtureAndReusesCredentialLifecycle()
    {
        int credentialRequests = 0;
        using var client = new HttpClient(new FixtureHttpMessageHandler(async (request, _) =>
        {
            if (request.RequestUri!.AbsolutePath == "/translator")
            {
                credentialRequests++;
                return new HttpResponseMessage
                {
                    Content = new StringContent("var params_AbusePreventionHelper = [4102444800000,\"fixture-token\"")
                };
            }

            string body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync();

            Assert.Equal("/tlookupv3", request.RequestUri.AbsolutePath);
            Assert.Contains("from=en", body);
            Assert.Contains("text=bank", body);
            return FixtureHttpMessageHandler.Json(Fixture.Read("Bing", "polysemous-word"));
        }));
        using var translator = new BingTranslator(client);

        var result = await translator.LookupDictionaryAsync("bank", "zh-CN", "en");

        Assert.Equal(1, credentialRequests);
        Assert.Equal(nameof(BingTranslator), result.Service);
        Assert.Contains(result.Groups, x => x.PartOfSpeech == "NOUN" && x.Entries.Count > 1);
        var bank = result.Groups.SelectMany(x => x.Entries).First();
        Assert.NotNull(bank.Confidence);
        Assert.NotEmpty(bank.BackTranslations);
        Assert.NotNull(bank.Transliteration);
    }

    [Fact]
    public async Task LookupReturnsEmptyResultForValidEmptyFixture()
    {
        using var client = CreateSequentialClient(Fixture.Read("Bing", "empty-result"));
        using var translator = new BingTranslator(client);

        var result = await translator.LookupDictionaryAsync("sentence", "zh-CN", "en");

        Assert.Empty(result.Groups);
    }

    [Theory]
    [InlineData("single-word")]
    [InlineData("optional-section-missing")]
    public async Task LookupAcceptsSingleAndOptionalSectionFixtures(string fixture)
    {
        using var client = CreateSequentialClient(Fixture.Read("Bing", fixture));
        using var translator = new BingTranslator(client);

        var result = await translator.LookupDictionaryAsync("bank", "zh-CN", "en");

        Assert.Single(result.Groups);
        Assert.Single(result.Groups[0].Entries);
    }

    private static HttpClient CreateSequentialClient(string lookupFixture)
    {
        return new HttpClient(new FixtureHttpMessageHandler((request, _) =>
        {
            string response = request.RequestUri!.AbsolutePath switch
            {
                "/translator" => "var params_AbusePreventionHelper = [4102444800000,\"fixture-token\"",
                _ => lookupFixture
            };
            return Task.FromResult(FixtureHttpMessageHandler.Json(response));
        }));
    }
}
