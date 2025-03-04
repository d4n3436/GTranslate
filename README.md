# GTranslate
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE) [![Nuget](https://img.shields.io/nuget/vpre/GTranslate)](https://www.nuget.org/packages/GTranslate)

GTranslate is a collection of free translation APIs (Google Translate, Bing Translator, Microsoft Translator and Yandex.Translate). Currently supports translation, transliteration, language detection and text-to-speech.

## Features

- 5 translation services:
  - Google Translate (old and new APIs)
  - Bing Translator
  - Microsoft Azure Translator
  - Yandex.Translate

- Support for translation, transliteration, language detection and text-to-speech in the included translators.

- Support for all the languages of each translator.

- A language class with methods for getting the supported languages and determining the availability of a language in a specific translator.

- Interfaces, allowing to write custom translators and languages.

- An `AggregateTranslator` class that groups the default translators for ease of use with the ability to add custom translators.

## Installation
Install via [NuGet](https://www.nuget.org/packages/GTranslate)

Or via command:
```
dotnet add package GTranslate
```

## Usage

### Translation
```c#
using GTranslate.Translators;

// Create an instance of the Google Translator
var translator = new GoogleTranslator();

// Translate "Hello world" to Spanish (es)
var result = await translator.TranslateAsync("Hello world", "es");

Console.WriteLine(result);

// Output:
// Translation: 'Hola Mundo', TargetLanguage: 'Spanish (es)', SourceLanguage: 'English (en)', Service: GoogleTranslator
```

### Transliteration
Transliteration is similar to translation but way it works is specific to each translator. Some translators only support transliteration implicitly and others have dedicated transliteration endpoints (like Yandex).
```c#
using GTranslate.Translators;

var translator = new YandexTranslator();

// Transliterate "Hello world" (in Russian) into English (latin script)
var result = await translator.Transliterate("Привет, мир", "en");

Console.WriteLine(result);

// Output:
// Transliteration: 'privet, mir', TargetLanguage: 'English (en)', SourceLanguage: 'Russian (ru)', Service: YandexTranslator
```

It's recommended to use `MicrosoftTranslator` for transliteration because of its superior API that allows you to explicitly specify the source and target script.

### Languages
GTranslate provides an easy way to access languages through the `Language` class. A `Language` object contains the English name, native name, ISO 639-1 code, ISO 639-3 code and the supported services (translation engines).

To get a `Language` object from its ISO 639-1 code, use the `Language.GetLanguage` or `Language.TryGetLanguage` methods. If the language was not found `Language.GetLanguage` will throw an exception and `Language.TryGetLanguage` will simply return `false`.
A language can also be obtained through its English/native name, ISO-6393 code and some aliases (like `zh-Hans` or `zh-Hant`).

```c#
using GTranslate;

var french = Language.GetLanguage("fr"); // Get the French language

string input = Console.ReadLine();
if (Language.TryGetLanguage(input, out var language)
{
    // Use language from input
}
```

GTranslate exposes the complete list of languages through a language dictionary class `LanguageDictionary` which can be accessed through `Language.LanguageDictionary`. 
It is essentially a read-only dictionary of ISO 639-1 codes and their respective languages.

### Results

Calling `TranslateAsync` returns an object deriving from `ITranslationResult`. It contains the translation, souce text, service, source language and target language.

The same applies to `TransliterateAsync` and `ITransliterationResult` but the transliteration is present instead of the translation.

Some translation engines will provide results with extra data in them. This extra data is exposed through properties in their concrete classes. For example, `GoogleTranslationResult` from (`GoogleTranslator.TranslateAsync`) will sometimes provide the confidence of the translation and the transliteration.
