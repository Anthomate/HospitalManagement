using Domain.ValueObjects;

namespace Application.Nurses.DTOs;

public record CreateNurseDto(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    Address Address,
    DateOnly HireDate,
    decimal Salary,  
    string LicenseNumber,
    string Service,
    string Grade,
    Guid DepartmentId
);