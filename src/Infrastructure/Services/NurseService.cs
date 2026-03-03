using Application.Common;
using Application.Common.Exceptions;
using Application.Nurses.DTOs;
using Application.Nurses.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class NurseService(
    HospitalDbContext context,
    ILogger<NurseService> logger) : INurseService
{
    public async Task<PagedResult<NurseDto>> GetAllAsync(
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var query = context.Nurses.AsNoTracking().OrderBy(n => n.LastName);
        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(n => new NurseDto(
                n.Id, n.FirstName, n.LastName,
                n.Email, n.Phone, n.Address, n.HireDate, n.Salary,
                n.LicenseNumber, n.Service, n.Grade,
                n.DepartmentId, n.Department.Name))
            .ToListAsync(ct);

        return new PagedResult<NurseDto>
        {
            Items = items, TotalCount = totalCount,
            Page = pagination.Page, PageSize = pagination.PageSize
        };
    }

    public async Task<PagedResult<NurseDto>> GetByDepartmentAsync(
        Guid departmentId,
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var query = context.Nurses
            .AsNoTracking()
            .Where(n => n.DepartmentId == departmentId)
            .OrderBy(n => n.LastName);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(n => new NurseDto(
                n.Id, n.FirstName, n.LastName,
                n.Email, n.Phone, n.Address, n.HireDate, n.Salary,
                n.LicenseNumber, n.Service, n.Grade,
                n.DepartmentId, n.Department.Name))
            .ToListAsync(ct);

        return new PagedResult<NurseDto>
        {
            Items = items, TotalCount = totalCount,
            Page = pagination.Page, PageSize = pagination.PageSize
        };
    }

    public async Task<NurseDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Nurses
            .AsNoTracking()
            .Where(n => n.Id == id)
            .Select(n => new NurseDto(
                n.Id, n.FirstName, n.LastName,
                n.Email, n.Phone, n.Address, n.HireDate, n.Salary,
                n.LicenseNumber, n.Service, n.Grade,
                n.DepartmentId, n.Department.Name))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<NurseDto> CreateAsync(
        CreateNurseDto dto,
        CancellationToken ct = default)
    {
        logger.LogInformation(
            "Creating nurse {FirstName} {LastName} with license {LicenseNumber}",
            dto.FirstName, dto.LastName, dto.LicenseNumber);

        var emailExists = await context.StaffMembers
            .AnyAsync(s => s.Email == dto.Email, ct);
        if (emailExists)
            throw new AlreadyExistsException("Staff", "Email", dto.Email);

        var licenseExists = await context.MedicalStaffs
            .AnyAsync(m => m.LicenseNumber == dto.LicenseNumber, ct);
        if (licenseExists)
            throw new AlreadyExistsException("MedicalStaff", "LicenseNumber", dto.LicenseNumber);

        var deptExists = await context.Departments
            .AnyAsync(d => d.Id == dto.DepartmentId, ct);
        if (!deptExists)
            throw new NotFoundException("Department", dto.DepartmentId);

        var nurse = new Nurse
        {
            FirstName     = dto.FirstName,
            LastName      = dto.LastName,
            Email         = dto.Email,
            Phone         = dto.Phone,
            Address       = dto.Address,
            HireDate      = dto.HireDate,
            Salary        = dto.Salary,
            LicenseNumber = dto.LicenseNumber,
            Service       = dto.Service,
            Grade         = dto.Grade,
            DepartmentId  = dto.DepartmentId
        };

        context.Nurses.Add(nurse);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Nurse {Id} created successfully", nurse.Id);
        return (await GetByIdAsync(nurse.Id, ct))!;
    }

    public async Task<NurseDto?> UpdateAsync(
        Guid id,
        UpdateNurseDto dto,
        CancellationToken ct = default)
    {
        var nurse = await context.Nurses.FindAsync([id], ct);
        if (nurse is null) return null;

        var emailTaken = await context.StaffMembers
            .AnyAsync(s => s.Email == dto.Email && s.Id != id, ct);
        if (emailTaken)
            throw new AlreadyExistsException("Staff", "Email", dto.Email);

        var deptExists = await context.Departments
            .AnyAsync(d => d.Id == dto.DepartmentId, ct);
        if (!deptExists)
            throw new NotFoundException("Department", dto.DepartmentId);

        nurse.FirstName    = dto.FirstName;
        nurse.LastName     = dto.LastName;
        nurse.Email        = dto.Email;
        nurse.Phone        = dto.Phone;
        nurse.Address      = dto.Address;
        nurse.HireDate     = dto.HireDate;
        nurse.Salary       = dto.Salary;
        nurse.Service      = dto.Service;
        nurse.Grade        = dto.Grade;
        nurse.DepartmentId = dto.DepartmentId;

        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entry = ex.Entries.Single();
            var dbValues = await entry.GetDatabaseValuesAsync(ct);

            if (dbValues is null)
                throw new NotFoundException("Nurse", id);

            throw new ConcurrencyConflictException(
                "The nurse was modified by another user. Please review and retry.",
                clientValues: entry.CurrentValues.ToObject(),
                databaseValues: dbValues.ToObject()
            );
        }

        logger.LogInformation("Nurse {Id} updated successfully", id);
        return (await GetByIdAsync(id, ct))!;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        logger.LogWarning("Attempting to delete nurse {NurseId}", id);

        var deleted = await context.Nurses
            .Where(n => n.Id == id)
            .ExecuteDeleteAsync(ct);

        if (deleted > 0)
            logger.LogWarning("Nurse {NurseId} deleted", id);

        return deleted > 0;
    }
}