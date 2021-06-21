﻿using System;
using System.Diagnostics;

namespace GTranslate
{
    /// <summary>
    /// Represents a generic cached object.
    /// </summary>
    /// <typeparam name="T">The type of the value to cache.</typeparam>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    internal class CachedObject<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CachedObject{T}"/> class with a specified value and no expiration date.
        /// </summary>
        /// <param name="value">The value.</param>
        public CachedObject(T value)
        {
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedObject{T}"/> class with a specified value and expiration date.
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="expirationDate">The date this object will expire.</param>
        public CachedObject(T value, DateTimeOffset expirationDate) : this(value)
        {
            ExpirationDate = expirationDate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedObject{T}"/> class with a specified value and duration.
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="duration">The duration this object will be valid.</param>
        public CachedObject(T value, TimeSpan duration) : this(value)
        {
            ExpirationDate = CachedDate.Add(duration);
        }

        /// <summary>
        /// Gets the cached object.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Gets the date this object will expire.
        /// </summary>
        public DateTimeOffset ExpirationDate { get; } = DateTimeOffset.MaxValue;

        /// <summary>
        /// Gets the date this object was cached.
        /// </summary>
        public DateTimeOffset CachedDate { get; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Returns whether this object has expired.
        /// </summary>
        /// <returns><see langword="true"/> if the language was found, otherwise <see langword="false"/>.</returns>
        public bool IsExpired() => DateTimeOffset.UtcNow > ExpirationDate;

        /// <inheritdoc/>
        public override string ToString() => $"Value: {Value}, Expired: {IsExpired()}";

        private string DebuggerDisplay => ToString();
    }
}