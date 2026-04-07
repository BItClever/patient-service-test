namespace PatientService.Api.Contracts.Patients;

public sealed class HumanNameDto
{
    public Guid? Id { get; init; }
    public string? Use { get; init; }
    public string Family { get; init; } = string.Empty;
    public List<string> Given { get; init; } = new();
}
