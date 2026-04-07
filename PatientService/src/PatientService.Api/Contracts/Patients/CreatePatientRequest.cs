namespace PatientService.Api.Contracts.Patients;

public sealed class CreatePatientRequest
{
    public bool Active { get; init; }
    public HumanNameDto Name { get; init; } = new();
    public string Gender { get; init; } = string.Empty;
    public DateTimeOffset BirthDate { get; init; }
}
