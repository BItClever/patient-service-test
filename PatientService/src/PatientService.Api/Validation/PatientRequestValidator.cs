using PatientService.Api.Contracts.Patients;

namespace PatientService.Api.Validation;

public static class PatientRequestValidator
{
    public static Dictionary<string, string[]> Validate(CreatePatientRequest request)
    {
        return ValidateInternal(request.Name, request.Gender, request.BirthDate);
    }

    public static Dictionary<string, string[]> Validate(UpdatePatientRequest request)
    {
        return ValidateInternal(request.Name, request.Gender, request.BirthDate);
    }

    private static Dictionary<string, string[]> ValidateInternal(
        HumanNameDto? name,
        string? gender,
        DateTimeOffset birthDate)
    {
        var errors = new Dictionary<string, List<string>>();

        if (name is null)
        {
            AddError(errors, "name", "Name is required.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(name.Family))
            {
                AddError(errors, "name.family", "Family is required.");
            }
        }

        if (birthDate == default)
        {
            AddError(errors, "birthDate", "BirthDate is required.");
        }

        if (birthDate > DateTimeOffset.UtcNow)
        {
            AddError(errors, "birthDate", "BirthDate cannot be in the future.");
        }

        if (string.IsNullOrWhiteSpace(gender))
        {
            AddError(errors, "gender", "Gender is required.");
        }
        if (!string.IsNullOrWhiteSpace(gender) &&
    !new[] { "male", "female", "other", "unknown" }
        .Contains(gender.Trim().ToLowerInvariant()))
        {
            AddError(errors, "gender",
                "Gender must be one of: male, female, other, unknown.");
        }

        return errors.ToDictionary(x => x.Key, x => x.Value.ToArray());
    }

    private static void AddError(Dictionary<string, List<string>> errors, string key, string message)
    {
        if (!errors.TryGetValue(key, out var messages))
        {
            messages = new List<string>();
            errors[key] = messages;
        }

        messages.Add(message);
    }
}