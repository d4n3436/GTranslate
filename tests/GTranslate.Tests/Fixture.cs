namespace GTranslate.Tests;

internal static class Fixture
{
    public static string Read(string provider, string name)
        => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "Dictionary", provider, $"{name}.json"));
}
