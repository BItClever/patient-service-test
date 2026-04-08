using PatientService.Api.Contracts.Patients;

namespace PatientService.IntegrationTests.Infrastructure;

internal static class PatientTestHelpers
{
    public static CreatePatientRequest BuildCreateRequest(
        string family = "Иванов",
        string firstName = "Иван",
        string gender = "male",
        DateTimeOffset? birthDate = null)
    {
        return new CreatePatientRequest
        {
            Name = new HumanNameDto
            {
                Id = Guid.NewGuid(),
                Use = "official",
                Family = family,
                Given = new List<string> { firstName, "Иванович" }
            },
            Gender = gender,
            BirthDate = birthDate ?? DateTimeOffset.UtcNow.AddDays(-1),
            Active = true
        };
    }

    public static UpdatePatientRequest BuildUpdateRequest(
        string family = "Петров",
        string gender = "male",
        DateTimeOffset? birthDate = null)
    {
        return new UpdatePatientRequest
        {
            Name = new HumanNameDto
            {
                Family = family,
                Given = new List<string> { "Пётр", "Иванович" }
            },
            Gender = gender,
            BirthDate = birthDate ?? DateTimeOffset.UtcNow.AddDays(-2),
            Active = true
        };
    }
}