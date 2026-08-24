using System.Net;
using GTranslate.Translators;

namespace GTranslate.Tests;

public sealed class GoogleDictionaryTests
{
    [Fact]
    public async Task LookupMapsPolysemousFixture()
    {
        using var client = CreateClient(Fixture.Read("Google", "polysemous-word"));
        using var translator = new GoogleTranslator(client);

        var result = await translator.LookupDictionaryAsync("charge", "zh-TW", "en");

        Assert.Equal(nameof(GoogleTranslator), result.Service);
        Assert.Equal("charge", result.Headword);
        Assert.Contains(result.Groups, x => x.PartOfSpeech == "noun" && x.Entries.Count > 1);
        Assert.Contains(result.Groups, x => x.PartOfSpeech == "verb");
        Assert.Contains(result.Groups.SelectMany(x => x.Entries), x => x.BackTranslations.Contains("charge"));
        Assert.NotEmpty(result.Examples);
    }

    [Fact]
    public async Task LookupMapsSingleWordFixture()
    {
        using var client = CreateClient(Fixture.Read("Google", "single-word"));
        using var translator = new GoogleTranslator(client);

        var result = await translator.LookupDictionaryAsync("hello", "zh-TW", "en");

        Assert.Single(result.Groups);
        Assert.Single(result.Groups[0].Entries);
    }

    [Fact]
    public async Task TranslateAsyncDoesNotRequestAdditionalRichSections()
    {
        int requests = 0;
        using var client = new HttpClient(new FixtureHttpMessageHandler((request, _) =>
        {
            requests++;
            Assert.Contains("dt=t", request.RequestUri!.Query);
            Assert.Contains("dt=bd", request.RequestUri.Query);
            Assert.DoesNotContain("dt=at", request.RequestUri.Query);
            Assert.DoesNotContain("dt=ex", request.RequestUri.Query);
            Assert.DoesNotContain("dt=md", request.RequestUri.Query);
            Assert.DoesNotContain("dt=ss", request.RequestUri.Query);
            return Task.FromResult(FixtureHttpMessageHandler.Json("{\"sentences\":[{\"trans\":\"你好\"}],\"src\":\"en\"}"));
        }));
        using var translator = new GoogleTranslator(client);

        var result = await translator.TranslateAsync("hello", "zh-TW", "en");

        Assert.Equal(1, requests);
        Assert.Equal("你好", result.Translation);
    }

    [Theory]
    [InlineData("empty-result")]
    [InlineData("optional-section-missing")]
    public async Task LookupAcceptsValidResponsesWithoutDictionarySections(string fixture)
    {
        using var client = CreateClient(Fixture.Read("Google", fixture));
        using var translator = new GoogleTranslator(client);

        var result = await translator.LookupDictionaryAsync("a complete sentence", "zh-TW", "en");

        Assert.Empty(result.Groups);
    }

    [Fact]
    public async Task LookupPropagatesHttpFailures()
    {
        using var client = new HttpClient(new FixtureHttpMessageHandler(static (_, _) =>
            Task.FromResult(FixtureHttpMessageHandler.Json("{}", HttpStatusCode.TooManyRequests))));
        using var translator = new GoogleTranslator(client);

        await Assert.ThrowsAsync<HttpRequestException>(() => translator.LookupDictionaryAsync("charge", "zh-TW", "en"));
    }

    [Fact]
    public async Task LookupPassesCancellationToHttpLayer()
    {
        using var client = new HttpClient(new FixtureHttpMessageHandler(static async (_, cancellationToken) =>
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return FixtureHttpMessageHandler.Json("{}");
        }));
        using var translator = new GoogleTranslator(client);
        using var source = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => translator.LookupDictionaryAsync("charge", "zh-TW", "en", source.Token));
    }

    private static HttpClient CreateClient(string fixture)
        => new(new FixtureHttpMessageHandler((_, _) => Task.FromResult(FixtureHttpMessageHandler.Json(fixture))));
}
