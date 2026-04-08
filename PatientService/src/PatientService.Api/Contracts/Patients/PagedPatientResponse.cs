namespace PatientService.Api.Contracts.Patients;

public sealed class PagedPatientResponse
{
    public IReadOnlyCollection<PatientResponse> Items { get; init; } = new List<PatientResponse>();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}