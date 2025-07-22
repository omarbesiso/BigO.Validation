// using BigO.Cqrs; // Remove if not needed
// Assuming ObjectValidator is accessible internally

// ReSharper disable UnusedMember.Global

namespace BigO.Validation.DataAnnotations;

/// <summary>
///     Provides public extension methods for validating object instances using DataAnnotations.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    ///     Validates the current object instance using its DataAnnotations attributes
    ///     and the <see cref="System.ComponentModel.DataAnnotations.IValidatableObject" /> interface, if implemented.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated. Must be a reference type.</typeparam>
    /// <param name="request">The object instance to validate. Must not be null.</param>
    /// <param name="errors">
    ///     When this method returns <c>false</c>, contains a dictionary of validation errors.
    ///     Keys are the names of members with errors, or a specific key (e.g., "Global") for object-level errors.
    ///     Values are arrays of error messages for that key.
    ///     When this method returns <c>true</c>, this dictionary is empty.
    /// </param>
    /// <returns><c>true</c> if the <paramref name="request" /> object passes all validation rules; otherwise, <c>false</c>.</returns>
    /// <remarks>
    ///     This method simplifies calls to the underlying validation logic based on
    ///     <see cref="System.ComponentModel.DataAnnotations.Validator" />.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if <paramref name="request" /> is null (ensure the underlying
    ///     validation mechanism handles this or add a check here).
    /// </exception>
    public static void Validate<T>(this T request, out IReadOnlyDictionary<string, IReadOnlyList<string>> errors) where T : class
    {
        Guard.NotNull(request);
        AnnotationValidator.TryValidate(request, out errors);
    }
}