namespace Application.Patients.DTOs;

public record UpdatePatientDto(
    string FirstName,
    string LastName,
    DateOnly BirthDate,
    string Email,
    string? Phone,
    string? Address
);