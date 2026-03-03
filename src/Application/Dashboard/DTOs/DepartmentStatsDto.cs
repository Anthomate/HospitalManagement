namespace Application.Dashboard.DTOs;

public record DepartmentStatsDto(
    Guid Id,
    string Name,
    string Location,
    string? MedicalDirectorName,
    int DoctorCount,
    int TotalConsultations,
    int ScheduledConsultations,
    int CompletedConsultations,
    int CancelledConsultations
);