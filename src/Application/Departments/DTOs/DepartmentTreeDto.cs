namespace Application.Departments.DTOs;

public record DepartmentTreeDto(
    Guid Id,
    string Name,
    string Location,
    string? MedicalDirectorName,
    int DoctorCount,
    List<DepartmentTreeDto> SubDepartments
);