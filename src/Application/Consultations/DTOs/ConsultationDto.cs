using Domain.Enums;

namespace Application.Consultations.DTOs;

public record ConsultationDto(
    Guid Id,
    Guid PatientId,
    string PatientFullName,
    Guid DoctorId,
    string DoctorFullName,
    DateTime ScheduledAt,
    ConsultationStatus Status,
    string? Notes,
    DateTime CreatedAt
);