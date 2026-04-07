namespace PatientService.Core.ValueObjects;

public sealed class HumanName
{
    public Guid Id { get; private set; }
    public string? Use { get; private set; }
    public string Family { get; private set; } = string.Empty;
    public List<string> Given { get; private set; } = new();

    private HumanName()
    {
    }

    public HumanName(Guid id, string? use, string family, IEnumerable<string>? given)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        SetValues(use, family, given);
    }

    public void Update(string? use, string family, IEnumerable<string>? given)
    {
        SetValues(use, family, given);
    }

    private void SetValues(string? use, string family, IEnumerable<string>? given)
    {
        if (string.IsNullOrWhiteSpace(family))
        {
            throw new ArgumentException("Family name is required.", nameof(family));
        }

        Family = family.Trim();
        Use = string.IsNullOrWhiteSpace(use) ? null : use.Trim();
        Given = given?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .ToList()
            ?? new List<string>();
    }
}
