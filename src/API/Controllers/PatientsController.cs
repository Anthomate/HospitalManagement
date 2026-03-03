using Application.Common;
using Application.Patients.DTOs;
using Application.Patients.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController(IPatientService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<PatientDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetAllAlphabeticalAsync(
            new PaginationParams { Page = page, PageSize = pageSize }, ct);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<PatientDto>>> Search(
        [FromQuery] string name,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.SearchByNameAsync(name,
            new PaginationParams { Page = page, PageSize = pageSize }, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PatientDto>> GetById(Guid id, CancellationToken ct)
    {
        var patient = await service.GetByIdAsync(id, ct);
        return patient is null ? NotFound() : Ok(patient);
    }

    [HttpGet("record/{recordNumber}")]
    public async Task<ActionResult<PatientDto>> GetByRecordNumber(string recordNumber, CancellationToken ct)
    {
        var patient = await service.GetByRecordNumberAsync(recordNumber, ct);
        return patient is null ? NotFound() : Ok(patient);
    }

    [HttpPost]
    public async Task<ActionResult<PatientDto>> Create(
        [FromBody] CreatePatientDto dto,
        CancellationToken ct)
    {
        var created = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PatientDto>> Update(
        Guid id,
        [FromBody] UpdatePatientDto dto,
        CancellationToken ct)
    {
        var updated = await service.UpdateAsync(id, dto, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await service.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
}