using Application.Common;
using Application.Consultations.DTOs;
using Application.Consultations.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConsultationsController(IConsultationService service, ILogger<ConsultationsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ConsultationDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetAllAsync(
            new PaginationParams { Page = page, PageSize = pageSize }, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ConsultationDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("patient/{patientId:guid}")]
    public async Task<ActionResult<PagedResult<ConsultationDto>>> GetByPatient(
        Guid patientId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetByPatientAsync(patientId,
            new PaginationParams { Page = page, PageSize = pageSize }, ct);
        return Ok(result);
    }

    [HttpGet("patient/{patientId:guid}/upcoming")]
    public async Task<ActionResult<PagedResult<ConsultationDto>>> GetUpcomingByPatient(
        Guid patientId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetUpcomingByPatientAsync(patientId,
            new PaginationParams { Page = page, PageSize = pageSize }, ct);
        return Ok(result);
    }

    [HttpGet("doctor/{doctorId:guid}")]
    public async Task<ActionResult<PagedResult<ConsultationDto>>> GetByDoctor(
        Guid doctorId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetByDoctorAsync(doctorId,
            new PaginationParams { Page = page, PageSize = pageSize }, ct);
        return Ok(result);
    }

    [HttpGet("doctor/{doctorId:guid}/today")]
    public async Task<ActionResult<IReadOnlyList<ConsultationDto>>> GetTodayByDoctor(
        Guid doctorId,
        CancellationToken ct)
    {
        var result = await service.GetTodayByDoctorAsync(doctorId, ct);
        return Ok(result);
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<PagedResult<ConsultationDto>>> GetByStatus(
        ConsultationStatus status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetByStatusAsync(status,
            new PaginationParams { Page = page, PageSize = pageSize }, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ConsultationDto>> Create(
        [FromBody] CreateConsultationDto dto,
        CancellationToken ct)
    {
        logger.LogInformation(
            "POST /consultations — Patient: {PatientId}, Doctor: {DoctorId}, ScheduledAt: {ScheduledAt}",
            dto.PatientId, dto.DoctorId, dto.ScheduledAt);

        var created = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ConsultationDto>> UpdateStatus(
        Guid id,
        [FromBody] ConsultationStatus newStatus,
        CancellationToken ct)
    {
        logger.LogInformation(
            "PATCH /consultations/{ConsultationId}/status — NewStatus: {Status}",
            id, newStatus);

        var updated = await service.UpdateStatusAsync(id, newStatus, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        logger.LogWarning("PATCH /consultations/{ConsultationId}/cancel requested", id);

        await service.CancelAsync(id, ct);
        return NoContent();
    }
}