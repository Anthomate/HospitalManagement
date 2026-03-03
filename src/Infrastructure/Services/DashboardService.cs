using Application.Common.Exceptions;
using Application.Dashboard.DTOs;
using Application.Dashboard.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class DashboardService(HospitalDbContext context, ILogger<DashboardService> logger) : IDashboardService
{
    public async Task<PatientRecordDto?> GetPatientRecordAsync(
        Guid patientId,
        CancellationToken ct = default)
    {
        logger.LogInformation("Fetching patient record for {PatientId}", patientId);

        var record = await context.Patients
            .AsNoTracking()
            .Where(p => p.Id == patientId)
            .Select(p => new PatientRecordDto(
                p.Id,
                p.FirstName + " " + p.LastName,
                p.BirthDate,
                p.RecordNumber,
                p.Email,
                p.Phone,
                p.Address,
                p.Consultations
                    .OrderByDescending(c => c.ScheduledAt)
                    .Select(c => new ConsultationSummaryDto(
                        c.Id,
                        c.ScheduledAt,
                        c.Status.ToString(),
                        c.Doctor.FirstName + " " + c.Doctor.LastName,
                        c.Doctor.Specialty,
                        c.Notes
                    ))
                    .ToList()
            ))
            .FirstOrDefaultAsync(ct);

        if (record is null)
            throw new NotFoundException("Patient", patientId);

        return record;
    }
    
    public async Task<DoctorPlanningDto?> GetDoctorPlanningAsync(
        Guid doctorId,
        CancellationToken ct = default)
    {
        logger.LogInformation("Fetching planning for doctor {DoctorId}", doctorId);

        var now = DateTime.UtcNow;

        var planning = await context.Doctors
            .AsNoTracking()
            .Where(d => d.Id == doctorId)
            .Select(d => new DoctorPlanningDto(
                d.Id,
                d.FirstName + " " + d.LastName,
                d.Specialty,
                d.LicenseNumber,
                d.Department.Name,
                d.Department.Location,
                d.Consultations
                    .Where(c => c.ScheduledAt > now
                                && c.Status == ConsultationStatus.Scheduled)
                    .OrderBy(c => c.ScheduledAt)
                    .Select(c => new UpcomingConsultationDto(
                        c.Id,
                        c.ScheduledAt,
                        c.Patient.FirstName + " " + c.Patient.LastName,
                        c.Patient.RecordNumber,
                        c.Notes
                    ))
                    .ToList()
            ))
            .FirstOrDefaultAsync(ct);

        if (planning is null)
            throw new NotFoundException("Doctor", doctorId);

        return planning;
    }
    
    public async Task<IReadOnlyList<DepartmentStatsDto>> GetDepartmentStatsAsync(
        CancellationToken ct = default)
    {
        logger.LogInformation("Fetching department statistics");

        var stats = await context.Departments
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentStatsDto(
                d.Id,
                d.Name,
                d.Location,
                d.MedicalDirector != null
                    ? d.MedicalDirector.FirstName + " " + d.MedicalDirector.LastName
                    : null,
                d.StaffMembers.OfType<Doctor>().Count(),
                d.StaffMembers.OfType<Doctor>()
                    .SelectMany(doc => doc.Consultations).Count(),
                d.StaffMembers.OfType<Doctor>()
                    .SelectMany(doc => doc.Consultations)
                    .Count(c => c.Status == ConsultationStatus.Scheduled),
                d.StaffMembers.OfType<Doctor>()
                    .SelectMany(doc => doc.Consultations)
                    .Count(c => c.Status == ConsultationStatus.Completed),
                d.StaffMembers.OfType<Doctor>()
                    .SelectMany(doc => doc.Consultations)
                    .Count(c => c.Status == ConsultationStatus.Cancelled)
            ))
            .ToListAsync(ct);

        logger.LogInformation("Fetched stats for {Count} departments", stats.Count);

        return stats;
    }
}