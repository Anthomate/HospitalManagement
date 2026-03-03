using Domain.Enums;

namespace Application.Consultations.DTOs;

public record UpdateConsultationDto(
    DateTime ScheduledAt,
    ConsultationStatus Status,
    string? Notes
);