using System.ComponentModel.DataAnnotations;

namespace BigO.Validation.DataAnnotations;

/// <summary>
///     Provides utility methods for validating objects using System.ComponentModel.DataAnnotations.
/// </summary>
public static class AnnotationValidator
{
    /// <summary>
    ///     Key used in the errors dictionary for object-level validation errors
    ///     that are not tied to a specific member.
    /// </summary>
    public const string GlobalErrorsKey = "Global";

    /// <summary>
    ///     Validates the specified model instance using DataAnnotations attributes and <see cref="IValidatableObject" />.
    /// </summary>
    /// <param name="model">The object instance to validate. Must not be null.</param>
    /// <param name="errors">
    ///     When this method returns, contains a dictionary of validation errors if validation failed.
    ///     The dictionary keys are member names (or the value of <see cref="GlobalErrorsKey" /> for object-level errors).
    ///     The dictionary values are arrays of error messages associated with that member or the object.
    ///     If validation is successful, this dictionary will be empty.
    /// </param>
    /// <param name="services"> Optional. An <see cref="IServiceProvider" /> to use for dependency injection during validation.</param>
    /// <returns><c>true</c> if the <paramref name="model" /> passes validation; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="model" /> is null.</exception>
    public static bool TryValidate(object model, out IReadOnlyDictionary<string, IReadOnlyList<string>> errors,
        IServiceProvider? services = null)
    {
        Guard.NotNull(model, nameof(model));

        var validationContext = new ValidationContext(model, services, null);
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(model, validationContext, validationResults, true);

        if (isValid)
        {
            errors = new Dictionary<string, IReadOnlyList<string>>();
            return true;
        }

        var groupedErrors = new Dictionary<string, List<string>>();
        foreach (var result in validationResults)
        {
            // Ensure there's an error message to add.
            // ErrorMessage can be null if the ValidationResult was created manually without one,
            // though Validator.TryValidateObject usually populates it.
            if (string.IsNullOrWhiteSpace(result.ErrorMessage))
            {
                continue;
            }

            if (result.MemberNames.Any())
            {
                foreach (var memberName in result.MemberNames)
                {
                    if (string.IsNullOrEmpty(memberName))
                    {
                        continue; // Should not happen with valid MemberNames from ValidationResult
                    }

                    if (!groupedErrors.TryGetValue(memberName, out var memberErrorList))
                    {
                        memberErrorList = [];
                        groupedErrors[memberName] = memberErrorList;
                    }

                    memberErrorList.Add(result.ErrorMessage);
                }
            }
            else // Object-level error
            {
                if (!groupedErrors.TryGetValue(GlobalErrorsKey, out var globalErrorList))
                {
                    globalErrorList = [];
                    groupedErrors[GlobalErrorsKey] = globalErrorList;
                }

                globalErrorList.Add(result.ErrorMessage);
            }
        }

        errors = groupedErrors.ToDictionary(
            kvp => kvp.Key, IReadOnlyList<string> (kvp) => kvp.Value.AsReadOnly());

        return false;
    }
}