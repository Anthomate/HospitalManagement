namespace Application.Doctors.DTOs;

public record CreateDoctorDto(
    string FirstName,
    string LastName,
    string Specialty,
    string LicenseNumber,
    Guid DepartmentId
);