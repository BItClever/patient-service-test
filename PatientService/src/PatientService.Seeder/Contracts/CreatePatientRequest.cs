namespace PatientService.Seeder.Contracts;

public sealed class CreatePatientRequest
{
    public HumanNameDto Name { get; init; } = new();
    public string Gender { get; init; } = string.Empty;
    public DateTimeOffset BirthDate { get; init; }
    public bool Active { get; init; }
}