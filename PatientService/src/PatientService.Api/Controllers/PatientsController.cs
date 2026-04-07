using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientService.Api.Contracts.Patients;
using PatientService.Api.Mappings;
using PatientService.Api.Validation;
using PatientService.Core.Search;
using PatientService.Persistence.Contexts;

namespace PatientService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PatientsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public PatientsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Creates a new patient.
    /// </summary>
    /// <param name="request">Patient payload.</param>
    /// <returns>The created patient.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(PatientResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PatientResponse>> Create([FromBody] CreatePatientRequest request, CancellationToken cancellationToken)
    {
        var validationErrors = PatientRequestValidator.Validate(request);
        if (validationErrors.Count > 0)
        {
            return ValidationProblem(new ValidationProblemDetails(validationErrors));
        }

        if (!request.TryToDomain(out var patient, out var mappingErrors))
        {
            return ValidationProblem(new ValidationProblemDetails(mappingErrors));
        }

        _dbContext.Patients.Add(patient!);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = patient!.ToResponse();

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
    public async Task<ActionResult<PatientResponse>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var patient = await _dbContext.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

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

        var patient = await _dbContext.Patients
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (patient is null)
        {
            return NotFound();
        }

        if (!request.TryApplyToDomain(patient, out var mappingErrors))
        {
            return ValidationProblem(new ValidationProblemDetails(mappingErrors));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(patient.ToResponse());
    }

    /// <summary>
    /// Deletes a patient by identifier.
    /// </summary>
    /// <param name="id">Patient identifier.</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var patient = await _dbContext.Patients
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (patient is null)
        {
            return NotFound();
        }

        _dbContext.Patients.Remove(patient);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Returns patients optionally filtered by FHIR-like birthDate query.
    /// </summary>
    /// <param name="birthDate">
    /// FHIR-like birthDate search value, for example:
    /// 2024,
    /// 2024-01,
    /// 2024-01-13,
    /// ge2024-01-01,
    /// lt2024-02-01.
    /// </param>
    /// <returns>Matching patients.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<PatientResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyCollection<PatientResponse>>> Search(
        [FromQuery] string? birthDate,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Patients
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(birthDate))
        {
            if (!BirthDateSearchParser.TryParse(birthDate, out var criteria, out var error))
            {
                return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    ["birthDate"] = new[] { error }
                }));
            }

            query = query.ApplyBirthDateSearch(criteria!);
        }

        var patients = await query
            .OrderBy(x => x.BirthDate)
            .ToListAsync(cancellationToken);

        var response = patients
            .Select(x => x.ToResponse())
            .ToList();

        return Ok(response);
    }
}