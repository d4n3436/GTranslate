using System;

namespace GTranslate;

/// <summary>
/// The exception that is thrown by a translation service.
/// </summary>
public class TranslatorException : Exception
{
    /// <summary>
    /// Gets the translation service that caused this exception.
    /// </summary>
    public string Service { get; } = "Unknown";

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslatorException"/> class.
    /// </summary>
    public TranslatorException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslatorException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public TranslatorException(string? message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslatorException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="service">The translation service that caused this exception.</param>
    public TranslatorException(string? message, string service)
        : this(message)
    {
        Service = service;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslatorException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    public TranslatorException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslatorException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="service">The translation service that caused this exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    public TranslatorException(string? message, string service, Exception? innerException)
        : this(message, innerException)
    {
        Service = service;
    }
}