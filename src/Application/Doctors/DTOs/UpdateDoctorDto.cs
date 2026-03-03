using Domain.ValueObjects;

namespace Application.Doctors.DTOs;

public record UpdateDoctorDto(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    Address Address,
    DateOnly HireDate,
    decimal Salary,  
    string Specialty,
    Guid DepartmentId
);