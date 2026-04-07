using System.Globalization;
using System.Text.RegularExpressions;

namespace PatientService.Core.Search;

public static class BirthDateSearchParser
{
    private static readonly Dictionary<string, BirthDateSearchPrefix> PrefixMap = new(StringComparer.Ordinal)
    {
        ["eq"] = BirthDateSearchPrefix.Eq,
        ["ne"] = BirthDateSearchPrefix.Ne,
        ["gt"] = BirthDateSearchPrefix.Gt,
        ["ge"] = BirthDateSearchPrefix.Ge,
        ["lt"] = BirthDateSearchPrefix.Lt,
        ["le"] = BirthDateSearchPrefix.Le,
        ["sa"] = BirthDateSearchPrefix.Sa,
        ["eb"] = BirthDateSearchPrefix.Eb,
        ["ap"] = BirthDateSearchPrefix.Ap
    };

    private static readonly Regex YearRegex =
        new(@"^\d{4}$", RegexOptions.Compiled);

    private static readonly Regex YearMonthRegex =
        new(@"^\d{4}-\d{2}$", RegexOptions.Compiled);

    private static readonly Regex DateRegex =
        new(@"^\d{4}-\d{2}-\d{2}$", RegexOptions.Compiled);

    private static readonly Regex HourRegex =
        new(@"^\d{4}-\d{2}-\d{2}T\d{2}([zZ]|[+\-]\d{2}:\d{2})?$", RegexOptions.Compiled);

    private static readonly Regex MinuteRegex =
        new(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}([zZ]|[+\-]\d{2}:\d{2})?$", RegexOptions.Compiled);

    private static readonly Regex SecondRegex =
        new(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}([zZ]|[+\-]\d{2}:\d{2})?$", RegexOptions.Compiled);

    private static readonly Regex FractionRegex =
        new(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.(\d{1,7})([zZ]|[+\-]\d{2}:\d{2})?$", RegexOptions.Compiled);

    public static bool TryParse(string? input, out BirthDateSearchCriteria? criteria, out string error)
    {
        criteria = null;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(input))
        {
            error = "birthDate query value is required.";
            return false;
        }

        var trimmed = input.Trim();
        var prefix = BirthDateSearchPrefix.Eq;
        var value = trimmed;

        if (trimmed.Length >= 2 && char.IsLetter(trimmed[0]) && char.IsLetter(trimmed[1]))
        {
            var prefixToken = trimmed[..2].ToLowerInvariant();

            if (!PrefixMap.TryGetValue(prefixToken, out prefix))
            {
                error = $"Unsupported birthDate prefix '{prefixToken}'. Supported prefixes: eq, ne, gt, ge, lt, le, sa, eb, ap.";
                return false;
            }

            value = trimmed[2..];
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            error = "birthDate value is required.";
            return false;
        }

        if (!TryParseRange(value, out var startUtc, out var endUtcExclusive, out error))
        {
            return false;
        }

        criteria = new BirthDateSearchCriteria(prefix, startUtc, endUtcExclusive, trimmed);
        return true;
    }

    private static bool TryParseRange(
        string value,
        out DateTimeOffset startUtc,
        out DateTimeOffset endUtcExclusive,
        out string error)
    {
        startUtc = default;
        endUtcExclusive = default;
        error = string.Empty;

        if (YearRegex.IsMatch(value))
        {
            if (!DateTime.TryParseExact(value, "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var yearDate))
            {
                error = "Invalid year format in birthDate.";
                return false;
            }

            startUtc = AsUtc(yearDate);
            endUtcExclusive = startUtc.AddYears(1);
            return true;
        }

        if (YearMonthRegex.IsMatch(value))
        {
            if (!DateTime.TryParseExact(value, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var monthDate))
            {
                error = "Invalid year-month format in birthDate.";
                return false;
            }

            startUtc = AsUtc(monthDate);
            endUtcExclusive = startUtc.AddMonths(1);
            return true;
        }

        if (DateRegex.IsMatch(value))
        {
            if (!DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dayDate))
            {
                error = "Invalid date format in birthDate.";
                return false;
            }

            startUtc = AsUtc(dayDate);
            endUtcExclusive = startUtc.AddDays(1);
            return true;
        }

        if (!DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var dateTimeOffset))
        {
            error = "Invalid date/time format in birthDate.";
            return false;
        }

        startUtc = dateTimeOffset.ToUniversalTime();

        if (HourRegex.IsMatch(value))
        {
            endUtcExclusive = startUtc.AddHours(1);
            return true;
        }

        if (MinuteRegex.IsMatch(value))
        {
            endUtcExclusive = startUtc.AddMinutes(1);
            return true;
        }

        if (SecondRegex.IsMatch(value))
        {
            endUtcExclusive = startUtc.AddSeconds(1);
            return true;
        }

        var fractionMatch = FractionRegex.Match(value);
        if (fractionMatch.Success)
        {
            var fractionDigits = fractionMatch.Groups[1].Value.Length;
            var precisionTicks = (long)Math.Pow(10, 7 - fractionDigits);
            endUtcExclusive = startUtc.AddTicks(precisionTicks);
            return true;
        }

        error = "Unsupported birthDate precision. Supported formats: yyyy, yyyy-MM, yyyy-MM-dd, yyyy-MM-ddTHH, yyyy-MM-ddTHH:mm, yyyy-MM-ddTHH:mm:ss, and fractional seconds with optional timezone.";
        return false;
    }

    private static DateTimeOffset AsUtc(DateTime value)
    {
        return new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc));
    }
}