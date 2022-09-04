# GTranslate
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE) [![Nuget](https://img.shields.io/nuget/vpre/GTranslate)](https://www.nuget.org/packages/GTranslate)

A collection of free translation APIs (Google Translate, Bing Translator, Microsoft Translator and Yandex.Translate). Currently supports translation, transliteration, language detection and text-to-speech.

## Features:

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

See the [examples](examples) folder for an example program.
