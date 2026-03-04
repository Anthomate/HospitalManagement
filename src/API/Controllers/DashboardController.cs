using Application.Dashboard.DTOs;
using Application.Dashboard.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController(IDashboardService service) : ControllerBase
{
    [HttpGet("patients/{patientId:guid}/record")]
    public async Task<ActionResult<PatientRecordDto>> GetPatientRecord(
        Guid patientId,
        CancellationToken ct)
    {
        var result = await service.GetPatientRecordAsync(patientId, ct);
        return Ok(result);
    }

    [HttpGet("doctors/{doctorId:guid}/planning")]
    public async Task<ActionResult<DoctorPlanningDto>> GetDoctorPlanning(
        Guid doctorId,
        CancellationToken ct)
    {
        var result = await service.GetDoctorPlanningAsync(doctorId, ct);
        return Ok(result);
    }

    [HttpGet("departments/stats")]
    public async Task<ActionResult<IReadOnlyList<DepartmentStatsDto>>> GetDepartmentStats(
        CancellationToken ct)
    {
        var result = await service.GetDepartmentStatsAsync(ct);
        return Ok(result);
    }
}