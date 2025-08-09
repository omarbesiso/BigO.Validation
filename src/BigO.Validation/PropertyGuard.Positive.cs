using System.Numerics;
using System.Runtime.CompilerServices;

// ReSharper disable UnusedMember.Global

namespace BigO.Validation;

    /// <summary>
    ///     Guard helpers specialised for property setters and initialisers.
    /// </summary>
    public static partial class PropertyGuard
    {
        /// <summary>
        ///     Ensures a numeric property value is greater than zero.
        /// </summary>
        /// <typeparam name="T">Numeric type of the property.</typeparam>
        /// <param name="value">The value to validate.</param>
        /// <param name="propertyName">
        ///     Name of the property being validated, captured automatically via
        ///     <see cref="CallerMemberNameAttribute" /> when omitted.
        /// </param>
        /// <param name="exceptionMessage">Optional custom message.</param>
        /// <returns>The validated positive value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Positive<T>(
            T value,
            [CallerMemberName] string propertyName = "",
            string? exceptionMessage = null)
            where T : INumber<T>
            => Guard.Positive(value, propertyName, exceptionMessage);

        /// <summary>
        ///     Ensures a numeric property value is zero or greater.
        /// </summary>
        /// <typeparam name="T">Numeric type of the property.</typeparam>
        /// <param name="value">The value to validate.</param>
        /// <param name="propertyName">
        ///     Name of the property being validated, captured automatically via
        ///     <see cref="CallerMemberNameAttribute" /> when omitted.
        /// </param>
        /// <param name="exceptionMessage">Optional custom message.</param>
        /// <returns>The validated non-negative value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T NonNegative<T>(
            T value,
            [CallerMemberName] string propertyName = "",
            string? exceptionMessage = null)
            where T : INumber<T>
            => Guard.NonNegative(value, propertyName, exceptionMessage);
    }