using Application.Common;
using Application.Nurses.DTOs;
using Application.Nurses.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NursesController(INurseService service, ILogger<NursesController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<NurseDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetAllAsync(
            new PaginationParams { Page = page, PageSize = pageSize }, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NurseDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("department/{departmentId:guid}")]
    public async Task<ActionResult<PagedResult<NurseDto>>> GetByDepartment(
        Guid departmentId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetByDepartmentAsync(departmentId,
            new PaginationParams { Page = page, PageSize = pageSize }, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<NurseDto>> Create(
        [FromBody] CreateNurseDto dto,
        CancellationToken ct)
    {
        logger.LogInformation(
            "POST /nurses — LicenseNumber: {LicenseNumber}",
            dto.LicenseNumber);

        var created = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<NurseDto>> Update(
        Guid id,
        [FromBody] UpdateNurseDto dto,
        CancellationToken ct)
    {
        var updated = await service.UpdateAsync(id, dto, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        logger.LogWarning("DELETE /nurses/{NurseId} requested", id);

        var deleted = await service.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
}