namespace Application.Doctors.DTOs;

public record UpdateDoctorDto(
    string FirstName,
    string LastName,
    string Specialty,
    Guid DepartmentId
);