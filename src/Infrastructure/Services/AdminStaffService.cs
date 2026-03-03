using Application.AdminStaff.DTOs;
using Application.AdminStaff.Interfaces;
using Application.Common;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class AdminStaffService(HospitalDbContext context) : IAdminStaffService
{
    public async Task<PagedResult<AdminStaffDto>> GetAllAsync(
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var query = context.AdminStaffs
            .AsNoTracking()
            .OrderBy(a => a.LastName);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(a => new AdminStaffDto(
                a.Id, a.FirstName, a.LastName,
                a.Email, a.Phone, a.Address,
                a.HireDate, a.Salary,
                a.Function,
                a.DepartmentId, a.Department.Name))
            .ToListAsync(ct);

        return new PagedResult<AdminStaffDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<PagedResult<AdminStaffDto>> GetByDepartmentAsync(
        Guid departmentId,
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var query = context.AdminStaffs
            .AsNoTracking()
            .Where(a => a.DepartmentId == departmentId)
            .OrderBy(a => a.LastName);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(a => new AdminStaffDto(
                a.Id, a.FirstName, a.LastName,
                a.Email, a.Phone, a.Address,
                a.HireDate, a.Salary,
                a.Function,
                a.DepartmentId, a.Department.Name))
            .ToListAsync(ct);

        return new PagedResult<AdminStaffDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<AdminStaffDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.AdminStaffs
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new AdminStaffDto(
                a.Id, a.FirstName, a.LastName,
                a.Email, a.Phone, a.Address,
                a.HireDate, a.Salary,
                a.Function,
                a.DepartmentId, a.Department.Name))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<AdminStaffDto> CreateAsync(
        CreateAdminStaffDto dto,
        CancellationToken ct = default)
    {
        var emailExists = await context.StaffMembers
            .AnyAsync(s => s.Email == dto.Email, ct);
        if (emailExists)
            throw new InvalidOperationException($"Email '{dto.Email}' is already used.");

        var deptExists = await context.Departments
            .AnyAsync(d => d.Id == dto.DepartmentId, ct);
        if (!deptExists)
            throw new InvalidOperationException($"Department '{dto.DepartmentId}' not found.");

        var adminStaff = new AdminStaff
        {
            FirstName    = dto.FirstName,
            LastName     = dto.LastName,
            Email        = dto.Email,
            Phone        = dto.Phone,
            Address      = dto.Address,
            HireDate     = dto.HireDate,
            Salary       = dto.Salary,
            Function     = dto.Function,
            DepartmentId = dto.DepartmentId
        };

        context.AdminStaffs.Add(adminStaff);
        await context.SaveChangesAsync(ct);

        return (await GetByIdAsync(adminStaff.Id, ct))!;
    }

    public async Task<AdminStaffDto?> UpdateAsync(
        Guid id,
        UpdateAdminStaffDto dto,
        CancellationToken ct = default)
    {
        var adminStaff = await context.AdminStaffs.FindAsync([id], ct);
        if (adminStaff is null) return null;

        var emailTaken = await context.StaffMembers
            .AnyAsync(s => s.Email == dto.Email && s.Id != id, ct);
        if (emailTaken)
            throw new InvalidOperationException($"Email '{dto.Email}' is already used.");

        var deptExists = await context.Departments
            .AnyAsync(d => d.Id == dto.DepartmentId, ct);
        if (!deptExists)
            throw new InvalidOperationException($"Department '{dto.DepartmentId}' not found.");

        adminStaff.FirstName    = dto.FirstName;
        adminStaff.LastName     = dto.LastName;
        adminStaff.Email        = dto.Email;
        adminStaff.Phone        = dto.Phone;
        adminStaff.Address      = dto.Address;
        adminStaff.HireDate     = dto.HireDate;
        adminStaff.Salary       = dto.Salary;
        adminStaff.Function     = dto.Function;
        adminStaff.DepartmentId = dto.DepartmentId;

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
        var deleted = await context.AdminStaffs
            .Where(a => a.Id == id)
            .ExecuteDeleteAsync(ct);

        return deleted > 0;
    }
}