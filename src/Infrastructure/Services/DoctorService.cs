using Application.Common;
using Application.Common.Exceptions;
using Application.Doctors.DTOs;
using Application.Doctors.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class DoctorService(
    HospitalDbContext context,
    ILogger<DoctorService> logger) : IDoctorService
{
    public async Task<PagedResult<DoctorDto>> GetAllAsync(
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var query = context.Doctors
            .AsNoTracking()
            .OrderBy(d => d.LastName);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(d => new DoctorDto(
                d.Id, d.FirstName, d.LastName, d.Email, d.Phone, d.Address,
                d.HireDate, d.Salary, d.Specialty, d.LicenseNumber,
                d.DepartmentId, d.Department.Name))
            .ToListAsync(ct);

        return new PagedResult<DoctorDto>
        {
            Items = items, TotalCount = totalCount,
            Page = pagination.Page, PageSize = pagination.PageSize
        };
    }

    public async Task<PagedResult<DoctorDto>> GetByDepartmentAsync(
        Guid departmentId,
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var query = context.Doctors
            .AsNoTracking()
            .Where(d => d.DepartmentId == departmentId)
            .OrderBy(d => d.LastName);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(d => new DoctorDto(
                d.Id, d.FirstName, d.LastName, d.Email, d.Phone, d.Address,
                d.HireDate, d.Salary, d.Specialty, d.LicenseNumber,
                d.DepartmentId, d.Department.Name))
            .ToListAsync(ct);

        return new PagedResult<DoctorDto>
        {
            Items = items, TotalCount = totalCount,
            Page = pagination.Page, PageSize = pagination.PageSize
        };
    }

    public async Task<DoctorDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var dto = await context.Doctors
            .AsNoTracking()
            .Where(d => d.Id == id)
            .Select(d => new DoctorDto(
                d.Id, d.FirstName, d.LastName, d.Email, d.Phone, d.Address,
                d.HireDate, d.Salary, d.Specialty, d.LicenseNumber,
                d.DepartmentId, d.Department.Name))
            .FirstOrDefaultAsync(ct);
        
        return dto;
    }

    public async Task<DoctorDto> CreateAsync(
        CreateDoctorDto dto,
        CancellationToken ct = default)
    {
        logger.LogInformation(
            "Creating doctor {FirstName} {LastName} with license {LicenseNumber}",
            dto.FirstName, dto.LastName, dto.LicenseNumber);

        var emailExists = await context.StaffMembers
            .AnyAsync(s => s.Email == dto.Email, ct);
        if (emailExists)
            throw new AlreadyExistsException("Staff", "Email", dto.Email);

        var deptExists = await context.Departments
            .AnyAsync(d => d.Id == dto.DepartmentId, ct);
        if (!deptExists)
            throw new NotFoundException("Department", dto.DepartmentId);

        var licenseExists = await context.MedicalStaffs
            .AnyAsync(d => d.LicenseNumber == dto.LicenseNumber, ct);
        if (licenseExists)
            throw new AlreadyExistsException("MedicalStaff", "LicenseNumber", dto.LicenseNumber);

        var doctor = new Doctor
        {
            FirstName     = dto.FirstName,
            LastName      = dto.LastName,
            Email         = dto.Email,
            Phone         = dto.Phone,
            Address       = dto.Address,
            HireDate      = dto.HireDate,
            Salary        = dto.Salary,
            Specialty     = dto.Specialty,
            LicenseNumber = dto.LicenseNumber,
            DepartmentId  = dto.DepartmentId
        };

        context.Doctors.Add(doctor);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Doctor {Id} created successfully", doctor.Id);

        return (await GetByIdAsync(doctor.Id, ct))!;
    }

    public async Task<DoctorDto?> UpdateAsync(
        Guid id,
        UpdateDoctorDto dto,
        CancellationToken ct = default)
    {
        var doctor = await context.Doctors.FindAsync([id], ct);
        if (doctor is null) return null;

        var emailTaken = await context.StaffMembers
            .AnyAsync(s => s.Email == dto.Email && s.Id != id, ct);
        if (emailTaken)
            throw new AlreadyExistsException("Staff", "Email", dto.Email);

        var deptExists = await context.Departments
            .AnyAsync(d => d.Id == dto.DepartmentId, ct);
        if (!deptExists)
            throw new NotFoundException("Department", dto.DepartmentId);

        doctor.FirstName    = dto.FirstName;
        doctor.LastName     = dto.LastName;
        doctor.Email        = dto.Email;
        doctor.Phone        = dto.Phone;
        doctor.Address      = dto.Address;
        doctor.HireDate     = dto.HireDate;
        doctor.Salary       = dto.Salary;
        doctor.Specialty    = dto.Specialty;
        doctor.DepartmentId = dto.DepartmentId;

        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entry = ex.Entries.Single();
            var dbValues = await entry.GetDatabaseValuesAsync(ct);

            if (dbValues is null)
                throw new NotFoundException("Doctor", id);

            throw new ConcurrencyConflictException(
                "The doctor was modified by another user. Please review and retry.",
                clientValues: entry.CurrentValues.ToObject(),
                databaseValues: dbValues.ToObject()
            );
        }

        logger.LogInformation("Doctor {Id} updated successfully", id);
        return (await GetByIdAsync(id, ct))!;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        logger.LogWarning("Attempting to delete doctor {DoctorId}", id);

        var isDirector = await context.Departments
            .AnyAsync(d => d.MedicalDirectorId == id, ct);
        if (isDirector)
            throw new BusinessRuleException(
                "Cannot delete a doctor who is a department's medical director. " +
                "Reassign the director first.");

        var deleted = await context.Doctors
            .Where(d => d.Id == id)
            .ExecuteDeleteAsync(ct);

        if (deleted > 0)
            logger.LogWarning("Doctor {DoctorId} deleted", id);

        return deleted > 0;
    }
}