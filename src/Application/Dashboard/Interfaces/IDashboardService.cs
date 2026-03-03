using Application.Dashboard.DTOs;

namespace Application.Dashboard.Interfaces;

public interface IDashboardService
{
    Task<PatientRecordDto?> GetPatientRecordAsync(Guid patientId, CancellationToken ct = default);
    Task<DoctorPlanningDto?> GetDoctorPlanningAsync(Guid doctorId, CancellationToken ct = default);
    Task<IReadOnlyList<DepartmentStatsDto>> GetDepartmentStatsAsync(CancellationToken ct = default);
}