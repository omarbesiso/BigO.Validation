using System.Numerics;
using System.Runtime.CompilerServices;

// ReSharper disable UnusedMember.Global

namespace BigO.Validation;

/// <summary>
///     Guard helpers specialised for property setters and initialisers.
/// </summary>
public static partial class PropertyGuard
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Positive<T>(
        T value,
        [CallerMemberName] string propertyName = "",
        string? exceptionMessage = null)
        where T : INumber<T>
        => Guard.Positive(value, propertyName, exceptionMessage);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T NonNegative<T>(
        T value,
        [CallerMemberName] string propertyName = "",
        string? exceptionMessage = null)
        where T : INumber<T>
        => Guard.NonNegative(value, propertyName, exceptionMessage);
}