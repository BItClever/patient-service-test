using System.Net;
using System.Net.Http.Json;
using PatientService.Api.Contracts.Patients;
using PatientService.IntegrationTests.Infrastructure;
using Xunit;

namespace PatientService.IntegrationTests.Patients;

[Collection("PatientApi")]
public sealed class PatientSearchTests : IAsyncLifetime
{
    private readonly PatientApiFactory _factory;
    private readonly HttpClient _client;

    public PatientSearchTests(PatientApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Search_WithNoBirthDateFilter_ShouldReturnAllPatients()
    {
        await CreatePatientWithBirthDateAsync(new DateTimeOffset(2024, 1, 13, 0, 0, 0, TimeSpan.Zero));
        await CreatePatientWithBirthDateAsync(new DateTimeOffset(2024, 3, 10, 0, 0, 0, TimeSpan.Zero));

        var response = await _client.GetAsync("/api/patients");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<PatientResponse>>();
        Assert.Equal(2, body!.Count);
    }

    [Fact]
    public async Task Search_WithExactYear_ShouldReturnOnlyMatchingYear()
    {
        await CreatePatientWithBirthDateAsync(new DateTimeOffset(2024, 1, 13, 0, 0, 0, TimeSpan.Zero));
        await CreatePatientWithBirthDateAsync(new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero));

        var response = await _client.GetAsync("/api/patients?birthDate=2024");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<PatientResponse>>();
        Assert.Single(body!);
        Assert.Equal(2024, body![0].BirthDate.Year);
    }

    [Fact]
    public async Task Search_WithExactMonth_ShouldReturnOnlyMatchingMonth()
    {
        await CreatePatientWithBirthDateAsync(new DateTimeOffset(2024, 1, 13, 0, 0, 0, TimeSpan.Zero));
        await CreatePatientWithBirthDateAsync(new DateTimeOffset(2024, 2, 5, 0, 0, 0, TimeSpan.Zero));

        var response = await _client.GetAsync("/api/patients?birthDate=2024-01");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<PatientResponse>>();
        Assert.Single(body!);
        Assert.Equal(1, body![0].BirthDate.Month);
    }

    [Fact]
    public async Task Search_WithExactDay_ShouldReturnOnlyMatchingDay()
    {
        await CreatePatientWithBirthDateAsync(new DateTimeOffset(2024, 1, 13, 10, 0, 0, TimeSpan.Zero));
        await CreatePatientWithBirthDateAsync(new DateTimeOffset(2024, 1, 14, 10, 0, 0, TimeSpan.Zero));

        var response = await _client.GetAsync("/api/patients?birthDate=2024-01-13");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<PatientResponse>>();
        Assert.Single(body!);
        Assert.Equal(13, body![0].BirthDate.Day);
    }

    [Fact]
    public async Task Search_WithGePrefix_ShouldReturnPatientsFromDateInclusive()
    {
        await CreatePatientWithBirthDateAsync(new DateTimeOffset(2024, 1, 13, 0, 0, 0, TimeSpan.Zero));
        await CreatePatientWithBirthDateAsync(new DateTimeOffset(2023, 12, 31, 0, 0, 0, TimeSpan.Zero));

        var response = await _client.GetAsync("/api/patients?birthDate=ge2024-01-01");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<PatientResponse>>();
        Assert.Single(body!);
        Assert.Equal(2024, body![0].BirthDate.Year);
    }

    [Fact]
    public async Task Search_WithLtPrefix_ShouldReturnPatientsBeforeDate()
    {
        await CreatePatientWithBirthDateAsync(new DateTimeOffset(2024, 1, 13, 0, 0, 0, TimeSpan.Zero));
        await CreatePatientWithBirthDateAsync(new DateTimeOffset(2024, 2, 5, 0, 0, 0, TimeSpan.Zero));

        var response = await _client.GetAsync("/api/patients?birthDate=lt2024-02-01");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<PatientResponse>>();
        Assert.Single(body!);
        Assert.Equal(1, body![0].BirthDate.Month);
    }

    [Fact]
    public async Task Search_WithNePrefix_ShouldExcludeMatchingDay()
    {
        await CreatePatientWithBirthDateAsync(new DateTimeOffset(2024, 1, 13, 10, 0, 0, TimeSpan.Zero));
        await CreatePatientWithBirthDateAsync(new DateTimeOffset(2024, 1, 14, 10, 0, 0, TimeSpan.Zero));

        var response = await _client.GetAsync("/api/patients?birthDate=ne2024-01-13");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<PatientResponse>>();
        Assert.Single(body!);
        Assert.Equal(14, body![0].BirthDate.Day);
    }

    [Fact]
    public async Task Search_WithInvalidPrefix_ShouldReturn400()
    {
        var response = await _client.GetAsync("/api/patients?birthDate=xx2024-01-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Search_WithNoMatchingPatients_ShouldReturnEmptyList()
    {
        await CreatePatientWithBirthDateAsync(new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero));

        var response = await _client.GetAsync("/api/patients?birthDate=2024");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<PatientResponse>>();
        Assert.Empty(body!);
    }

    // --- helpers ---

    private async Task<PatientResponse> CreatePatientWithBirthDateAsync(DateTimeOffset birthDate)
    {
        var request = PatientTestHelpers.BuildCreateRequest(birthDate: birthDate);
        var response = await _client.PostAsJsonAsync("/api/patients", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PatientResponse>())!;
    }
}