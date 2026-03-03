using Application.Common;
using Application.Departments.DTOs;
using Application.Departments.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController(IDepartmentService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<DepartmentDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetAllAsync(
            new PaginationParams { Page = page, PageSize = pageSize }, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DepartmentDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<DepartmentDto>> Create(
        [FromBody] CreateDepartmentDto dto,
        CancellationToken ct)
    {
        var created = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DepartmentDto>> Update(
        Guid id,
        [FromBody] UpdateDepartmentDto dto,
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

    [HttpPatch("{id:guid}/director/{doctorId:guid}")]
    public async Task<ActionResult<DepartmentDto>> AssignDirector(
        Guid id,
        Guid doctorId,
        CancellationToken ct)
    {
        var result = await service.AssignDirectorAsync(id, doctorId, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}/director")]
    public async Task<ActionResult<DepartmentDto>> RemoveDirector(Guid id, CancellationToken ct)
    {
        var result = await service.RemoveDirectorAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }
}