using Application.Common;
using Application.Doctors.DTOs;
using Application.Doctors.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class DoctorService(HospitalDbContext context) : IDoctorService
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
                d.HireDate, d.Salary,d.Specialty, d.LicenseNumber,
                d.DepartmentId, d.Department.Name))
            .ToListAsync(ct);

        return new PagedResult<DoctorDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
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
                d.HireDate, d.Salary,d.Specialty, d.LicenseNumber,
                d.DepartmentId, d.Department.Name))
            .ToListAsync(ct);

        return new PagedResult<DoctorDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<DoctorDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Doctors
            .AsNoTracking()
            .Where(d => d.Id == id)
            .Select(d => new DoctorDto(
                d.Id, d.FirstName, d.LastName, d.Email, d.Phone, d.Address,
                d.HireDate, d.Salary,d.Specialty, d.LicenseNumber,
                d.DepartmentId, d.Department.Name))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<DoctorDto> CreateAsync(CreateDoctorDto dto, CancellationToken ct = default)
    {
        var emailExists = await context.StaffMembers
            .AnyAsync(s => s.Email == dto.Email, ct);

        if (emailExists)
            throw new InvalidOperationException($"Email '{dto.Email}' is already used.");
        var deptExists = await context.Departments.AnyAsync(d => d.Id == dto.DepartmentId, ct);
        if (!deptExists)
            throw new InvalidOperationException($"Department '{dto.DepartmentId}' not found.");

        var licenseExists = await context.Doctors
            .AnyAsync(d => d.LicenseNumber == dto.LicenseNumber, ct);
        if (licenseExists)
            throw new InvalidOperationException($"License number '{dto.LicenseNumber}' already exists.");

        var doctor = new Doctor
        {
            FirstName     = dto.FirstName,
            LastName      = dto.LastName,
            Specialty     = dto.Specialty,
            LicenseNumber = dto.LicenseNumber,
            DepartmentId  = dto.DepartmentId
        };

        context.Doctors.Add(doctor);
        await context.SaveChangesAsync(ct);

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
        
        var deptExists = await context.Departments.AnyAsync(d => d.Id == dto.DepartmentId, ct);
        if (!deptExists)
            throw new InvalidOperationException($"Department '{dto.DepartmentId}' not found.");

        doctor.FirstName    = dto.FirstName;
        doctor.LastName     = dto.LastName;
        doctor.Email        = dto.Email;
        doctor.Phone        = dto.Phone;
        doctor.Address      = dto.Address;
        doctor.Specialty    = dto.Specialty;
        doctor.DepartmentId = dto.DepartmentId;

        await context.SaveChangesAsync(ct);
        return (await GetByIdAsync(id, ct))!;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var isDirector = await context.Departments
            .AnyAsync(d => d.MedicalDirectorId == id, ct);

        if (isDirector)
            throw new InvalidOperationException(
                "Cannot delete a doctor who is a department's medical director. Reassign the director first.");

        var deleted = await context.Doctors
            .Where(d => d.Id == id)
            .ExecuteDeleteAsync(ct);

        return deleted > 0;
    }
}