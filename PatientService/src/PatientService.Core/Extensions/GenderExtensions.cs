using PatientService.Core.Enums;

namespace PatientService.Core.Extensions;

public static class GenderExtensions
{
    public static string ToApiValue(this Gender gender)
    {
        return gender switch
        {
            Gender.Male => "male",
            Gender.Female => "female",
            Gender.Other => "other",
            _ => "unknown"
        };
    }

    public static bool TryParseApiValue(string? value, out Gender gender)
    {
        switch (value?.Trim().ToLowerInvariant())
        {
            case "male":
                gender = Gender.Male;
                return true;

            case "female":
                gender = Gender.Female;
                return true;

            case "other":
                gender = Gender.Other;
                return true;

            case "unknown":
                gender = Gender.Unknown;
                return true;

            default:
                gender = Gender.Unknown;
                return false;
        }
    }
}
