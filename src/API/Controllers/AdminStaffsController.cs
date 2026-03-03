using Application.AdminStaff.DTOs;
using Application.AdminStaff.Interfaces;
using Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminStaffsController(IAdminStaffService service, ILogger<AdminStaffsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<AdminStaffDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetAllAsync(
            new PaginationParams { Page = page, PageSize = pageSize }, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminStaffDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("department/{departmentId:guid}")]
    public async Task<ActionResult<PagedResult<AdminStaffDto>>> GetByDepartment(
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
    public async Task<ActionResult<AdminStaffDto>> Create(
        [FromBody] CreateAdminStaffDto dto,
        CancellationToken ct)
    {
        logger.LogInformation(
            "POST /adminstaff — Function: {Function}, Department: {DepartmentId}",
            dto.Function, dto.DepartmentId);

        var created = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AdminStaffDto>> Update(
        Guid id,
        [FromBody] UpdateAdminStaffDto dto,
        CancellationToken ct)
    {
        var updated = await service.UpdateAsync(id, dto, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        logger.LogWarning("DELETE /adminstaff/{AdminStaffId} requested", id);

        var deleted = await service.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
}