using Domain.ValueObjects;

namespace Application.Patients.DTOs;

public record PatientDto(
    Guid Id,
    string FirstName,
    string LastName,
    DateOnly BirthDate,
    string RecordNumber,
    string Email,
    string? Phone,
    Address Address,
    DateTime CreatedAt    
);