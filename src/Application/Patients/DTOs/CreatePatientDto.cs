using Domain.ValueObjects;

namespace Application.Patients.DTOs;

public record CreatePatientDto(
    string FirstName,
    string LastName,
    DateOnly BirthDate,
    string RecordNumber,
    string Email,
    string? Phone,
    Address Address
);