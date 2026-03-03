using Domain.ValueObjects;

namespace Application.AdminStaff.DTOs;

public record CreateAdminStaffDto(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    Address Address,
    DateOnly HireDate,
    decimal Salary,
    string Function,
    Guid DepartmentId
);