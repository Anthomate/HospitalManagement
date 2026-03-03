namespace Application.Departments.DTOs;

public record DepartmentDto(
    Guid Id,
    string Name,
    string Location,
    string? MedicalDirectorName,
    int DoctorCount,
    DateTime CreatedAt
);