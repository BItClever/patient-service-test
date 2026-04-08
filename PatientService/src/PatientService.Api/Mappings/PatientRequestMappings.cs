using PatientService.Api.Contracts.Patients;
using PatientService.Core.Entities;
using PatientService.Core.Extensions;
using PatientService.Core.ValueObjects;

namespace PatientService.Api.Mappings;

public static class PatientRequestMappings
{
    public static Patient ToDomain(this CreatePatientRequest request)
    {
        GenderExtensions.TryParseApiValue(request.Gender, out var gender);

        var name = new HumanName(
            request.Name.Id ?? Guid.NewGuid(),
            request.Name.Use,
            request.Name.Family,
            request.Name.Given);

        return new Patient(request.Active, name, gender, request.BirthDate!.Value);
    }

    public static HumanName ToHumanName(this HumanNameDto dto, Guid? fallbackId = null)
    {
        return new HumanName(
            dto.Id ?? fallbackId ?? Guid.NewGuid(),
            dto.Use,
            dto.Family,
            dto.Given);
    }
}