using PatientService.Core.ValueObjects;
using Xunit;

namespace PatientService.UnitTests.Domain;

public sealed class HumanNameTests
{
    [Fact]
    public void Constructor_WhenFamilyIsEmpty_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            new HumanName(Guid.NewGuid(), "official", "", null));
    }

    [Fact]
    public void Constructor_WhenFamilyIsWhitespace_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
            new HumanName(Guid.NewGuid(), "official", "   ", null));
    }

    [Fact]
    public void Constructor_ShouldTrimFamily()
    {
        var name = new HumanName(Guid.NewGuid(), "official", "  Иванов  ", null);
        Assert.Equal("Иванов", name.Family);
    }

    [Fact]
    public void Constructor_WhenUseIsEmpty_ShouldSetUseToNull()
    {
        var name = new HumanName(Guid.NewGuid(), "", "Иванов", null);
        Assert.Null(name.Use);
    }

    [Fact]
    public void Constructor_WhenUseIsProvided_ShouldTrimUse()
    {
        var name = new HumanName(Guid.NewGuid(), "  official  ", "Иванов", null);
        Assert.Equal("official", name.Use);
    }

    [Fact]
    public void Constructor_WhenGivenContainsEmptyStrings_ShouldFilterThem()
    {
        var name = new HumanName(Guid.NewGuid(), null, "Иванов",
            new[] { "Иван", "", "  ", "Иванович" });

        Assert.Equal(2, name.Given.Count);
        Assert.Contains("Иван", name.Given);
        Assert.Contains("Иванович", name.Given);
    }

    [Fact]
    public void Constructor_WhenGivenIsNull_ShouldSetEmptyList()
    {
        var name = new HumanName(Guid.NewGuid(), null, "Иванов", null);
        Assert.Empty(name.Given);
    }

    [Fact]
    public void Constructor_WhenIdIsEmpty_ShouldGenerateNewId()
    {
        var name = new HumanName(Guid.Empty, null, "Иванов", null);
        Assert.NotEqual(Guid.Empty, name.Id);
    }

    [Fact]
    public void Constructor_WhenIdIsProvided_ShouldPreserveId()
    {
        var id = Guid.NewGuid();
        var name = new HumanName(id, null, "Иванов", null);
        Assert.Equal(id, name.Id);
    }

    [Fact]
    public void Update_WhenCalledWithNewValues_ShouldUpdateAllFields()
    {
        var name = new HumanName(Guid.NewGuid(), "official", "Иванов", new[] { "Иван" });

        name.Update("nickname", "Петров", new[] { "Пётр", "Иванович" });

        Assert.Equal("nickname", name.Use);
        Assert.Equal("Петров", name.Family);
        Assert.Equal(2, name.Given.Count);
    }

    [Fact]
    public void Update_WhenFamilyIsEmpty_ShouldThrow()
    {
        var name = new HumanName(Guid.NewGuid(), "official", "Иванов", null);
        Assert.Throws<ArgumentException>(() => name.Update(null, "", null));
    }
}