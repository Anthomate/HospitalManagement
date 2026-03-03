using Application.Common;
using Application.Nurses.DTOs;
using Application.Nurses.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class NurseService(HospitalDbContext context) : INurseService
{
    public async Task<PagedResult<NurseDto>> GetAllAsync(
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var query = context.Nurses
            .AsNoTracking()
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

    public async Task<NurseDto> CreateAsync(CreateNurseDto dto, CancellationToken ct = default)
    {
        var emailExists = await context.StaffMembers
            .AnyAsync(s => s.Email == dto.Email, ct);
        if (emailExists)
            throw new InvalidOperationException($"Email '{dto.Email}' is already used.");

        var licenseExists = await context.MedicalStaffs
            .AnyAsync(m => m.LicenseNumber == dto.LicenseNumber, ct);
        if (licenseExists)
            throw new InvalidOperationException($"License '{dto.LicenseNumber}' already exists.");

        var deptExists = await context.Departments
            .AnyAsync(d => d.Id == dto.DepartmentId, ct);
        if (!deptExists)
            throw new InvalidOperationException($"Department '{dto.DepartmentId}' not found.");

        var nurse = new Nurse
        {
            FirstName     = dto.FirstName,
            LastName      = dto.LastName,
            Email         = dto.Email,
            Phone         = dto.Phone,
            Address       = dto.Address,
            LicenseNumber = dto.LicenseNumber,
            Service       = dto.Service,
            Grade         = dto.Grade,
            DepartmentId  = dto.DepartmentId
        };

        context.Nurses.Add(nurse);
        await context.SaveChangesAsync(ct);

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
            throw new InvalidOperationException($"Email '{dto.Email}' is already used.");

        var deptExists = await context.Departments
            .AnyAsync(d => d.Id == dto.DepartmentId, ct);
        if (!deptExists)
            throw new InvalidOperationException($"Department '{dto.DepartmentId}' not found.");

        nurse.FirstName    = dto.FirstName;
        nurse.LastName     = dto.LastName;
        nurse.Email        = dto.Email;
        nurse.Phone        = dto.Phone;
        nurse.Address      = dto.Address;
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
                throw new InvalidOperationException("The record was deleted by another user.");

            throw new ConcurrencyConflictException(
                "The record was modified by another user. Please review and retry.",
                clientValues: entry.CurrentValues.ToObject(),
                databaseValues: dbValues.ToObject()
            );
        }
        return (await GetByIdAsync(id, ct))!;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var deleted = await context.Nurses
            .Where(n => n.Id == id)
            .ExecuteDeleteAsync(ct);

        return deleted > 0;
    }
}