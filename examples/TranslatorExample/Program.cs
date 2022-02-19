using System;
using System.Threading.Tasks;
using GTranslate.Translators;

namespace TranslatorExample;

internal static class Program
{
    private static async Task Main()
    {
        Console.WriteLine("Translator Example\n");
        var translator = new AggregateTranslator();

        while (true)
        {
            Console.Write("Enter a text to translate or enter 'e' to exit: ");
            string text = Console.ReadLine() ?? string.Empty;
            if (text == "e")
            {
                break;
            }

            Console.Write("Language to translate to: ");
            string language = Console.ReadLine() ?? string.Empty;

            try
            {
                var result = await translator.TranslateAsync(text, language);
                Console.WriteLine($"Translation: {result.Translation}");
                Console.WriteLine($"Source Language: {result.SourceLanguage}");
                Console.WriteLine($"Target Language: {result.TargetLanguage}");
                Console.WriteLine($"Service: {result.Service}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}