using PatientService.Api.Contracts.Patients;
using PatientService.Core.Entities;
using PatientService.Core.Enums;
using PatientService.Core.Extensions;
using PatientService.Core.ValueObjects;

namespace PatientService.Api.Mappings;

public static class PatientRequestMappings
{
    public static bool TryToDomain(
        this CreatePatientRequest request,
        out Patient? patient,
        out Dictionary<string, string[]> errors)
    {
        return TryCreatePatient(
            request.Name,
            request.Active,
            request.Gender,
            request.BirthDate,
            out patient,
            out errors);
    }

    public static bool TryApplyToDomain(
        this UpdatePatientRequest request,
        Patient existingPatient,
        out Dictionary<string, string[]> errors)
    {
        errors = new Dictionary<string, string[]>();

        if (!GenderExtensions.TryParseApiValue(request.Gender, out var gender))
        {
            errors["gender"] = new[] { "Gender must be one of: male, female, other, unknown." };
            return false;
        }

        try
        {
            var name = new HumanName(
                request.Name.Id ?? existingPatient.Name.Id,
                request.Name.Use,
                request.Name.Family,
                request.Name.Given);

            existingPatient.Update(
                request.Active,
                name,
                gender,
                request.BirthDate);

            return true;
        }
        catch (ArgumentNullException ex)
        {
            errors["request"] = new[] { ex.Message };
            return false;
        }
        catch (ArgumentOutOfRangeException ex)
        {
            errors["request"] = new[] { ex.Message };
            return false;
        }
        catch (ArgumentException ex)
        {
            errors["request"] = new[] { ex.Message };
            return false;
        }
    }

    private static bool TryCreatePatient(
        HumanNameDto nameDto,
        bool active,
        string genderValue,
        DateTimeOffset birthDate,
        out Patient? patient,
        out Dictionary<string, string[]> errors)
    {
        patient = null;
        errors = new Dictionary<string, string[]>();

        if (!GenderExtensions.TryParseApiValue(genderValue, out Gender gender))
        {
            errors["gender"] = new[] { "Gender must be one of: male, female, other, unknown." };
            return false;
        }

        try
        {
            var name = new HumanName(
                nameDto.Id ?? Guid.NewGuid(),
                nameDto.Use,
                nameDto.Family,
                nameDto.Given);

            patient = new Patient(active, name, gender, birthDate);
            return true;
        }
        catch (ArgumentNullException ex)
        {
            errors["request"] = new[] { ex.Message };
            return false;
        }
        catch (ArgumentOutOfRangeException ex)
        {
            errors["request"] = new[] { ex.Message };
            return false;
        }
        catch (ArgumentException ex)
        {
            errors["request"] = new[] { ex.Message };
            return false;
        }
    }
}