using Application.AdminStaff.DTOs;
using Application.AdminStaff.Interfaces;
using Application.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class AdminStaffService(
    IUnitOfWork uow,
    ILogger<AdminStaffService> logger) : IAdminStaffService
{
    public async Task<PagedResult<AdminStaffDto>> GetAllAsync(
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var result = await uow.AdminStaffs.GetAllPagedAsync(pagination, ct);

        return new PagedResult<AdminStaffDto>
        {
            Items      = result.Items.Select(ToDto).ToList(),
            TotalCount = result.TotalCount,
            Page       = result.Page,
            PageSize   = result.PageSize
        };
    }

    public async Task<PagedResult<AdminStaffDto>> GetByDepartmentAsync(
        Guid departmentId,
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var result = await uow.AdminStaffs.GetByDepartmentAsync(departmentId, pagination, ct);

        return new PagedResult<AdminStaffDto>
        {
            Items      = result.Items.Select(ToDto).ToList(),
            TotalCount = result.TotalCount,
            Page       = result.Page,
            PageSize   = result.PageSize
        };
    }

    public async Task<AdminStaffDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var adminStaff = await uow.AdminStaffs.GetByIdAsync(id, ct);
        return adminStaff is null ? null : ToDto(adminStaff);
    }

    public async Task<AdminStaffDto> CreateAsync(
        CreateAdminStaffDto dto,
        CancellationToken ct = default)
    {
        logger.LogInformation(
            "Creating admin staff {FirstName} {LastName} with function {Function}",
            dto.FirstName, dto.LastName, dto.Function);

        if (await uow.StaffMembers.ExistsByEmailAsync(dto.Email, null, ct))
            throw new AlreadyExistsException("Staff", "Email", dto.Email);

        var dept = await uow.Departments.GetByIdAsync(dto.DepartmentId, ct);
        if (dept is null)
            throw new NotFoundException("Department", dto.DepartmentId);

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

        await uow.AdminStaffs.AddAsync(adminStaff, ct);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("AdminStaff {Id} created successfully", adminStaff.Id);
        return (await GetByIdAsync(adminStaff.Id, ct))!;
    }

    public async Task<AdminStaffDto?> UpdateAsync(
        Guid id,
        UpdateAdminStaffDto dto,
        CancellationToken ct = default)
    {
        var adminStaff = await uow.AdminStaffs.GetByIdAsync(id, ct);
        if (adminStaff is null) return null;

        if (await uow.StaffMembers.ExistsByEmailAsync(dto.Email, id, ct))
            throw new AlreadyExistsException("Staff", "Email", dto.Email);

        var dept = await uow.Departments.GetByIdAsync(dto.DepartmentId, ct);
        if (dept is null)
            throw new NotFoundException("Department", dto.DepartmentId);

        adminStaff.FirstName    = dto.FirstName;
        adminStaff.LastName     = dto.LastName;
        adminStaff.Email        = dto.Email;
        adminStaff.Phone        = dto.Phone;
        adminStaff.Address      = dto.Address;
        adminStaff.HireDate     = dto.HireDate;
        adminStaff.Salary       = dto.Salary;
        adminStaff.Function     = dto.Function;
        adminStaff.DepartmentId = dto.DepartmentId;

        uow.AdminStaffs.Update(adminStaff);

        try
        {
            await uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entry    = ex.Entries.Single();
            var dbValues = await entry.GetDatabaseValuesAsync(ct);

            if (dbValues is null)
                throw new NotFoundException("AdminStaff", id);

            throw new ConcurrencyConflictException(
                "The admin staff was modified by another user. Please review and retry.",
                clientValues:   entry.CurrentValues.ToObject(),
                databaseValues: dbValues.ToObject()
            );
        }

        logger.LogInformation("AdminStaff {Id} updated successfully", id);
        return (await GetByIdAsync(id, ct))!;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        logger.LogWarning("Attempting to delete admin staff {AdminStaffId}", id);

        var adminStaff = await uow.AdminStaffs.GetByIdAsync(id, ct);
        if (adminStaff is null) return false;

        uow.AdminStaffs.Remove(adminStaff);
        await uow.SaveChangesAsync(ct);

        logger.LogWarning("AdminStaff {AdminStaffId} deleted", id);
        return true;
    }

    private static AdminStaffDto ToDto(AdminStaff a) => new(
        a.Id, a.FirstName, a.LastName,
        a.Email, a.Phone, a.Address,
        a.HireDate, a.Salary,
        a.Function,
        a.DepartmentId, a.Department?.Name ?? string.Empty
    );
}