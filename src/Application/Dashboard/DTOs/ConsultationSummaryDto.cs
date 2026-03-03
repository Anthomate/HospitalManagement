namespace Application.Dashboard.DTOs;

public record ConsultationSummaryDto(
    Guid Id,
    DateTime ScheduledAt,
    string Status,
    string DoctorFullName,
    string DoctorSpecialty,
    string? Notes
);