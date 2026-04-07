using PatientService.Api.Contracts.Patients;
using PatientService.Core.Entities;
using PatientService.Core.Extensions;

namespace PatientService.Api.Mappings;

public static class PatientMappings
{
    public static PatientResponse ToResponse(this Patient patient)
    {
        return new PatientResponse
        {
            Id = patient.Id,
            Active = patient.Active,
            Name = new HumanNameDto
            {
                Id = patient.Name.Id,
                Use = patient.Name.Use,
                Family = patient.Name.Family,
                Given = patient.Name.Given.ToList()
            },
            Gender = patient.Gender.ToApiValue(),
            BirthDate = patient.BirthDate
        };
    }
}
