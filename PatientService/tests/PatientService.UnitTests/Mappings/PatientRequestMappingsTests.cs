using PatientService.Api.Contracts.Patients;
using PatientService.Api.Mappings;
using Xunit;

namespace PatientService.UnitTests.Mappings;

public sealed class PatientRequestMappingsTests
{
    [Fact]
    public void ToHumanName_WithValidDto_ShouldCreateHumanName()
    {
        var dto = new HumanNameDto
        {
            Id = Guid.NewGuid(),
            Use = "official",
            Family = "Иванов",
            Given = new List<string> { "Иван", "Иванович" }
        };

        var name = dto.ToHumanName();

        Assert.Equal(dto.Id, name.Id);
        Assert.Equal(dto.Use, name.Use);
        Assert.Equal(dto.Family, name.Family);
        Assert.Equal(dto.Given, name.Given);
    }

    [Fact]
    public void ToHumanName_WithNullId_ShouldGenerateNewId()
    {
        var dto = new HumanNameDto
        {
            Family = "Иванов"
        };

        var name = dto.ToHumanName();

        Assert.NotEqual(Guid.Empty, name.Id);
        Assert.Equal("Иванов", name.Family);
    }

    [Fact]
    public void ToHumanName_WithFallbackId_ShouldUseFallback()
    {
        var fallbackId = Guid.NewGuid();
        var dto = new HumanNameDto
        {
            Family = "Иванов"
        };

        var name = dto.ToHumanName(fallbackId);

        Assert.Equal(fallbackId, name.Id);
    }

    [Fact]
    public void ToHumanName_WithEmptyGiven_ShouldFilterEmptyStrings()
    {
        var dto = new HumanNameDto
        {
            Family = "Иванов",
            Given = new List<string> { "Иван", "", "  ", "Иванович" }
        };

        var name = dto.ToHumanName();

        Assert.Equal(2, name.Given.Count);
        Assert.Contains("Иван", name.Given);
        Assert.Contains("Иванович", name.Given);
    }
}