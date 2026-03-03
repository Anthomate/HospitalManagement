using Domain.ValueObjects;

namespace Application.Dashboard.DTOs;

public record PatientRecordDto(
    Guid Id,
    string FullName,
    DateOnly BirthDate,
    string RecordNumber,
    string Email,
    string? Phone,
    Address Address,
    IReadOnlyList<ConsultationSummaryDto> Consultations
);