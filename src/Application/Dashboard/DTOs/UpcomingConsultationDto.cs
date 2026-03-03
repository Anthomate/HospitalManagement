namespace Application.Dashboard.DTOs;

public record UpcomingConsultationDto(
    Guid Id,
    DateTime ScheduledAt,
    string PatientFullName,
    string PatientRecordNumber,
    string? Notes
);