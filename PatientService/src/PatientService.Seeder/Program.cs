using System.Net.Http.Json;
using PatientService.Seeder.Generation;

var baseUrl = ResolveBaseUrl(args);
var count = ResolveCount(args);

Console.WriteLine($"Patient seeder started.");
Console.WriteLine($"API base URL: {baseUrl}");
Console.WriteLine($"Patients to create: {count}");

using var httpClient = new HttpClient
{
    BaseAddress = new Uri(NormalizeBaseUrl(baseUrl)),
    Timeout = TimeSpan.FromSeconds(30)
};

var cancellationToken = CancellationToken.None;

var apiIsAvailable = await WaitForApiAsync(httpClient, cancellationToken);
if (!apiIsAvailable)
{
    Console.WriteLine("API is not available. Seeder stopped.");
    return 1;
}

var successCount = 0;
var failedCount = 0;

var patientsToCreate = PatientGenerator.CreateMany(count);

for (var i = 0; i < patientsToCreate.Count; i++)
{
    var request = patientsToCreate[i];

    try
    {
        using var response = await httpClient.PostAsJsonAsync("api/patients", request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            successCount++;
            Console.WriteLine($"[{i + 1}/{count}] Created successfully. Status: {(int)response.StatusCode}");
        }
        else
        {
            failedCount++;
            Console.WriteLine($"[{i + 1}/{count}] Failed. Status: {(int)response.StatusCode}. Body: {responseBody}");
        }
    }
    catch (Exception ex)
    {
        failedCount++;
        Console.WriteLine($"[{i + 1}/{count}] Exception: {ex.Message}");
    }
}

Console.WriteLine("Seeding finished.");
Console.WriteLine($"Created: {successCount}");
Console.WriteLine($"Failed: {failedCount}");

return failedCount == 0 ? 0 : 1;

static string ResolveBaseUrl(string[] args)
{
    if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
    {
        return args[0];
    }

    var envValue = Environment.GetEnvironmentVariable("PATIENT_API_BASE_URL");
    if (!string.IsNullOrWhiteSpace(envValue))
    {
        return envValue;
    }

    return "http://localhost:8080";
}

static int ResolveCount(string[] args)
{
    if (args.Length > 1 && int.TryParse(args[1], out var countFromArgs) && countFromArgs > 0)
    {
        return countFromArgs;
    }

    var envValue = Environment.GetEnvironmentVariable("PATIENT_SEED_COUNT");
    if (int.TryParse(envValue, out var countFromEnv) && countFromEnv > 0)
    {
        return countFromEnv;
    }

    return 100;
}

static string NormalizeBaseUrl(string baseUrl)
{
    return baseUrl.EndsWith("/")
        ? baseUrl
        : baseUrl + "/";
}

static async Task<bool> WaitForApiAsync(HttpClient httpClient, CancellationToken cancellationToken)
{
    const int maxAttempts = 15;
    var delay = TimeSpan.FromSeconds(2);

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            using var response = await httpClient.GetAsync("api/patients", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"API is available (attempt {attempt}/{maxAttempts}).");
                return true;
            }

            Console.WriteLine($"API responded with status {(int)response.StatusCode} (attempt {attempt}/{maxAttempts}).");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API is not ready yet (attempt {attempt}/{maxAttempts}): {ex.Message}");
        }

        await Task.Delay(delay, cancellationToken);
    }

    return false;
}