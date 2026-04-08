using System.Net;
using System.Net.Http.Json;
using PatientService.Api.Contracts.Patients;
using PatientService.IntegrationTests.Infrastructure;
using Xunit;

namespace PatientService.IntegrationTests.Patients;

[Collection("PatientApi")]
public sealed class PatientCrudTests : IAsyncLifetime
{
    private readonly PatientApiFactory _factory;
    private readonly HttpClient _client;

    public PatientCrudTests(PatientApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    // --- CREATE ---

    [Fact]
    public async Task Create_WithValidRequest_ShouldReturn201WithPatient()
    {
        var request = PatientTestHelpers.BuildCreateRequest();

        var response = await _client.PostAsJsonAsync("/api/patients", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PatientResponse>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body!.Id);
        Assert.Equal(request.Name.Family, body.Name.Family);
        Assert.Equal(request.Gender, body.Gender);
        Assert.True(body.Active);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task Create_WithMissingFamily_ShouldReturn400()
    {
        var request = PatientTestHelpers.BuildCreateRequest(family: "");

        var response = await _client.PostAsJsonAsync("/api/patients", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithFutureBirthDate_ShouldReturn400()
    {
        var request = PatientTestHelpers.BuildCreateRequest(
            birthDate: DateTimeOffset.UtcNow.AddDays(1));

        var response = await _client.PostAsJsonAsync("/api/patients", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithInvalidGender_ShouldReturn400()
    {
        var request = PatientTestHelpers.BuildCreateRequest(gender: "invalid");

        var response = await _client.PostAsJsonAsync("/api/patients", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // --- GET BY ID ---

    [Fact]
    public async Task GetById_WhenPatientExists_ShouldReturn200()
    {
        var created = await CreatePatientAsync();

        var response = await _client.GetAsync($"/api/patients/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PatientResponse>();
        Assert.NotNull(body);
        Assert.Equal(created.Id, body!.Id);
        Assert.Equal(created.Name.Family, body.Name.Family);
    }

    [Fact]
    public async Task GetById_WhenPatientNotFound_ShouldReturn404()
    {
        var response = await _client.GetAsync($"/api/patients/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // --- UPDATE ---

    [Fact]
    public async Task Update_WhenPatientExists_ShouldReturn200WithUpdatedData()
    {
        var created = await CreatePatientAsync();

        var updateRequest = PatientTestHelpers.BuildUpdateRequest(
            family: "Петров",
            gender: "female",
            birthDate: DateTimeOffset.UtcNow.AddDays(-5));

        var response = await _client.PutAsJsonAsync(
            $"/api/patients/{created.Id}", updateRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PatientResponse>();
        Assert.NotNull(body);
        Assert.Equal("Петров", body!.Name.Family);
        Assert.Equal("female", body.Gender);
    }

    [Fact]
    public async Task Update_WhenPatientNotFound_ShouldReturn404()
    {
        var updateRequest = PatientTestHelpers.BuildUpdateRequest();

        var response = await _client.PutAsJsonAsync(
            $"/api/patients/{Guid.NewGuid()}", updateRequest);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithMissingFamily_ShouldReturn400()
    {
        var created = await CreatePatientAsync();
        var updateRequest = PatientTestHelpers.BuildUpdateRequest(family: "");

        var response = await _client.PutAsJsonAsync(
            $"/api/patients/{created.Id}", updateRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // --- DELETE ---

    [Fact]
    public async Task Delete_WhenPatientExists_ShouldReturn204AndRemovePatient()
    {
        var created = await CreatePatientAsync();

        var deleteResponse = await _client.DeleteAsync($"/api/patients/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/patients/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_WhenPatientNotFound_ShouldReturn404()
    {
        var response = await _client.DeleteAsync($"/api/patients/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // --- helpers ---

    private async Task<PatientResponse> CreatePatientAsync(
        string family = "Иванов",
        DateTimeOffset? birthDate = null)
    {
        var request = PatientTestHelpers.BuildCreateRequest(
            family: family, birthDate: birthDate);
        var response = await _client.PostAsJsonAsync("/api/patients", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PatientResponse>())!;
    }
}