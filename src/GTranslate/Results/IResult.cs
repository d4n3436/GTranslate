namespace GTranslate.Results
{
    /// <summary>
    /// Represents a result from a translation service.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    public interface IResult<out T>
    {
        /// <summary>
        /// Gets the service this result is from.
        /// </summary>
        string Service { get; }

        /// <summary>
        /// Gets the result.
        /// </summary>
        T Result { get; }
    }
}