namespace Application.Consultations.DTOs;

public record CreateConsultationDto(
    Guid PatientId,
    Guid DoctorId,
    DateTime ScheduledAt,
    string? Notes
);