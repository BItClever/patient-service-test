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
        patient = null;
        errors = new Dictionary<string, string[]>();

        if (!GenderExtensions.TryParseApiValue(request.Gender, out Gender gender))
        {
            errors["gender"] = new[] { "Gender must be one of: male, female, other, unknown." };
            return false;
        }

        try
        {
            var name = new HumanName(
                request.Name.Id ?? Guid.NewGuid(),
                request.Name.Use,
                request.Name.Family,
                request.Name.Given);

            patient = new Patient(request.Active, name, gender, request.BirthDate!.Value);
            return true;
        }
        catch (ArgumentNullException ex) when (ex.ParamName == "name")
        {
            errors["name"] = new[] { ex.Message };
            return false;
        }
        catch (ArgumentOutOfRangeException ex) when (ex.ParamName == "birthDate")
        {
            errors["birthDate"] = new[] { ex.Message };
            return false;
        }
        catch (ArgumentException ex) when (ex.ParamName == "family")
        {
            errors["name.family"] = new[] { ex.Message };
            return false;
        }
        catch (Exception ex)
        {
            errors["request"] = new[] { ex.Message };
            return false;
        }
    }

    public static bool TryToHumanName(
        this HumanNameDto dto,
        out HumanName? name,
        out Dictionary<string, string[]> errors)
    {
        errors = new Dictionary<string, string[]>();
        name = null;

        try
        {
            name = new HumanName(
                dto.Id ?? Guid.NewGuid(),
                dto.Use,
                dto.Family,
                dto.Given);
            return true;
        }
        catch (ArgumentException ex) when (ex.ParamName == "family")
        {
            errors["name.family"] = new[] { ex.Message };
            return false;
        }
        catch (ArgumentException ex)
        {
            errors["name"] = new[] { ex.Message };
            return false;
        }
    }
}
