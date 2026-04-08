using Moq;
using PatientService.Core.Entities;
using PatientService.Core.Enums;
using PatientService.Core.Repositories;
using PatientService.Core.Search;
using PatientService.Core.Services;
using PatientService.Core.ValueObjects;
using Xunit;

namespace PatientService.UnitTests.Services;

public sealed class PatientServiceTests
{
    private readonly Mock<IPatientRepository> _repositoryMock;
    private readonly IPatientService _service;

    public PatientServiceTests()
    {
        _repositoryMock = new Mock<IPatientRepository>();
        _service = new PatientService.Core.Services.PatientService(_repositoryMock.Object);
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_ShouldCallAddAndSaveChanges()
    {
        var patient = CreateTestPatient();

        _repositoryMock.Setup(r => r.AddAsync(patient, default)).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.SaveChangesAsync(default)).Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(patient, default);

        Assert.Equal(patient, result);
        _repositoryMock.Verify(r => r.AddAsync(patient, default), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    // --- GetByIdAsync ---

    [Fact]
    public async Task GetByIdAsync_WhenPatientExists_ShouldReturnPatient()
    {
        var patient = CreateTestPatient();
        _repositoryMock.Setup(r => r.GetByIdAsync(patient.Id, default)).ReturnsAsync(patient);

        var result = await _service.GetByIdAsync(patient.Id, default);

        Assert.Equal(patient, result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenPatientNotFound_ShouldReturnNull()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync((Patient?)null);

        var result = await _service.GetByIdAsync(id, default);

        Assert.Null(result);
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_WhenPatientNotFound_ShouldReturnNull_AndNotSave()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync((Patient?)null);

        var name = new HumanName(Guid.NewGuid(), "official", "Петров", new[] { "Пётр" });
        var result = await _service.UpdateAsync(id, true, name, Gender.Male, DateTimeOffset.UtcNow.AddDays(-1), default);

        Assert.Null(result);
        _repositoryMock.Verify(r => r.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenPatientFound_ShouldApplyChangesAndSave()
    {
        var patient = CreateTestPatient();
        _repositoryMock.Setup(r => r.GetByIdAsync(patient.Id, default)).ReturnsAsync(patient);
        _repositoryMock.Setup(r => r.SaveChangesAsync(default)).Returns(Task.CompletedTask);

        var newName = new HumanName(Guid.NewGuid(), "official", "Петров", new[] { "Пётр" });
        var newBirthDate = DateTimeOffset.UtcNow.AddDays(-10);

        var result = await _service.UpdateAsync(patient.Id, false, newName, Gender.Female, newBirthDate, default);

        Assert.NotNull(result);
        Assert.False(result!.Active);
        Assert.Equal("Петров", result.Name.Family);
        Assert.Equal(Gender.Female, result.Gender);
        Assert.Equal(newBirthDate, result.BirthDate);
        _repositoryMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_WhenPatientNotFound_ShouldReturnFalse_AndNotRemove()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync((Patient?)null);

        var result = await _service.DeleteAsync(id, default);

        Assert.False(result);
        _repositoryMock.Verify(r => r.Remove(It.IsAny<Patient>()), Times.Never);
        _repositoryMock.Verify(r => r.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenPatientFound_ShouldRemoveAndSave()
    {
        var patient = CreateTestPatient();
        _repositoryMock.Setup(r => r.GetByIdAsync(patient.Id, default)).ReturnsAsync(patient);
        _repositoryMock.Setup(r => r.SaveChangesAsync(default)).Returns(Task.CompletedTask);

        var result = await _service.DeleteAsync(patient.Id, default);

        Assert.True(result);
        _repositoryMock.Verify(r => r.Remove(patient), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    // --- SearchAsync ---

    [Fact]
    public async Task SearchAsync_WithNullCriteria_ShouldDelegateToRepository()
    {
        var patients = new List<Patient> { CreateTestPatient() };
        _repositoryMock
            .Setup(r => r.SearchAsync(null, default))
            .ReturnsAsync((IReadOnlyList<Patient>)patients);

        var result = await _service.SearchAsync(null, default);

        Assert.Single(result);
        _repositoryMock.Verify(r => r.SearchAsync(null, default), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_WithCriteria_ShouldPassCriteriaToRepository()
    {
        BirthDateSearchParser.TryParse("ge2024-01-01", out var criteria, out _);
        var patients = new List<Patient> { CreateTestPatient() };

        _repositoryMock
            .Setup(r => r.SearchAsync(criteria, default))
            .ReturnsAsync((IReadOnlyList<Patient>)patients);

        var result = await _service.SearchAsync(criteria, default);

        Assert.Single(result);
        _repositoryMock.Verify(r => r.SearchAsync(criteria, default), Times.Once);
    }

    // --- helpers ---

    private static Patient CreateTestPatient()
    {
        var name = new HumanName(Guid.NewGuid(), "official", "Иванов", new[] { "Иван", "Иванович" });
        return new Patient(true, name, Gender.Male, DateTimeOffset.UtcNow.AddDays(-1));
    }
}