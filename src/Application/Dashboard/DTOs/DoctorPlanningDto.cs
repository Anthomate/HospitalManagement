namespace Application.Dashboard.DTOs;

public record DoctorPlanningDto(
    Guid Id,
    string FullName,
    string Specialty,
    string LicenseNumber,
    string DepartmentName,
    string DepartmentLocation,
    IReadOnlyList<UpcomingConsultationDto> UpcomingConsultations
);