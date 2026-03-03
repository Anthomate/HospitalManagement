using Domain.ValueObjects;

namespace Application.Nurses.DTOs;

public record UpdateNurseDto(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    Address Address,
    DateOnly HireDate,
    decimal Salary,  
    string Service,
    string Grade,
    Guid DepartmentId
);