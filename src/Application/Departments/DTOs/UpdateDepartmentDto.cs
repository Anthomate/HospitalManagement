namespace Application.Departments.DTOs;

public record UpdateDepartmentDto(
    string Name,
    string Location,
    Guid? MedicalDirectorId
);