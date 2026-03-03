namespace Application.Doctors.DTOs;

public record DoctorDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Specialty,
    string LicenseNumber,
    Guid DepartmentId,
    string DepartmentName
);