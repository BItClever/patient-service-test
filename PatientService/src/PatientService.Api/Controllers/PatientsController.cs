using Microsoft.AspNetCore.Mvc;
using PatientService.Api.Contracts.Patients;
using PatientService.Api.Mappings;
using PatientService.Api.Validation;
using PatientService.Core.Search;
using PatientService.Core.Services;

namespace PatientService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly ILogger<PatientsController> _logger;

    public PatientsController(IPatientService patientService, ILogger<PatientsController> logger)
    {
        _patientService = patientService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new patient.
    /// </summary>
    /// <param name="request">Patient payload.</param>
    /// <returns>The created patient.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(PatientResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PatientResponse>> Create(
        [FromBody] CreatePatientRequest request,
        CancellationToken cancellationToken)
    {
        var validationErrors = PatientRequestValidator.Validate(request);
        if (validationErrors.Count > 0)
        {
            return ValidationProblem(new ValidationProblemDetails(validationErrors));
        }

        var patient = request.ToDomain();
        var created = await _patientService.CreateAsync(patient, cancellationToken);

        _logger.LogInformation("Patient created with id {PatientId}", created.Id);

        var response = created.ToResponse();
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Returns a patient by identifier.
    /// </summary>
    /// <param name="id">Patient identifier.</param>
    /// <returns>The patient if found.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PatientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientResponse>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var patient = await _patientService.GetByIdAsync(id, cancellationToken);
        if (patient is null)
        {
            return NotFound();
        }

        return Ok(patient.ToResponse());
    }

    /// <summary>
    /// Updates an existing patient.
    /// </summary>
    /// <param name="id">Patient identifier.</param>
    /// <param name="request">Updated patient payload.</param>
    /// <returns>The updated patient.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PatientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdatePatientRequest request,
        CancellationToken cancellationToken)
    {
        var validationErrors = PatientRequestValidator.Validate(request);
        if (validationErrors.Count > 0)
        {
            return ValidationProblem(new ValidationProblemDetails(validationErrors));
        }

        // Validation already passed — parse without re-checking the result
        PatientService.Core.Extensions.GenderExtensions.TryParseApiValue(request.Gender, out var gender);
        var name = request.Name.ToHumanName();

        var patient = await _patientService.UpdateAsync(
            id, request.Active, name, gender, request.BirthDate!.Value, cancellationToken);

        if (patient is null)
        {
            return NotFound();
        }

        _logger.LogInformation("Patient updated with id {PatientId}", id);
        return Ok(patient.ToResponse());
    }

    /// <summary>
    /// Deletes a patient by identifier.
    /// </summary>
    /// <param name="id">Patient identifier.</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await _patientService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        _logger.LogInformation("Patient deleted with id {PatientId}", id);
        return NoContent();
    }

    /// <summary>
    /// Returns patients optionally filtered by FHIR-like birthDate query.
    /// </summary>
    /// <param name="birthDate">
    /// FHIR-like birthDate search value, for example:
    /// 2024, 2024-01, 2024-01-13, ge2024-01-01, lt2024-02-01.
    /// Omit to return all patients.
    /// </param>
    /// <param name="page">Page number (1-based). Default is 1.</param>
    /// <param name="size">Page size. Default is 50, max is 100.</param>
    /// <returns>Paged list of matching patients.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedPatientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedPatientResponse>> Search(
        [FromQuery] string? birthDate,
        [FromQuery] int page = 1,
        [FromQuery] int size = 50,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            return ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]> { ["page"] = new[] { "Page must be greater than 0." } }));
        }

        if (size < 1 || size > 100)
        {
            return ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]> { ["size"] = new[] { "Size must be between 1 and 100." } }));
        }

        BirthDateSearchCriteria? criteria = null;

        if (!string.IsNullOrWhiteSpace(birthDate))
        {
            if (!BirthDateSearchParser.TryParse(birthDate, out criteria, out var error))
            {
                return ValidationProblem(new ValidationProblemDetails(
                    new Dictionary<string, string[]> { ["birthDate"] = new[] { error } }));
            }
        }

        var result = await _patientService.SearchAsync(criteria, page, size, cancellationToken);
        var response = new PagedPatientResponse
        {
            Items = result.Items.Select(x => x.ToResponse()).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages
        };
        return Ok(response);
    }
}