using Application.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Nurses.DTOs;
using Application.Nurses.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class NurseService(
    IUnitOfWork uow,
    ILogger<NurseService> logger) : INurseService
{
    public async Task<PagedResult<NurseDto>> GetAllAsync(
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var result = await uow.Nurses.GetAllPagedAsync(pagination, ct);

        return new PagedResult<NurseDto>
        {
            Items      = result.Items.Select(ToDto).ToList(),
            TotalCount = result.TotalCount,
            Page       = result.Page,
            PageSize   = result.PageSize
        };
    }

    public async Task<PagedResult<NurseDto>> GetByDepartmentAsync(
        Guid departmentId,
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var result = await uow.Nurses.GetByDepartmentAsync(departmentId, pagination, ct);

        return new PagedResult<NurseDto>
        {
            Items      = result.Items.Select(ToDto).ToList(),
            TotalCount = result.TotalCount,
            Page       = result.Page,
            PageSize   = result.PageSize
        };
    }

    public async Task<NurseDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var nurse = await uow.Nurses.GetByIdAsync(id, ct);
        return nurse is null ? null : ToDto(nurse);
    }

    public async Task<NurseDto> CreateAsync(
        CreateNurseDto dto,
        CancellationToken ct = default)
    {
        logger.LogInformation(
            "Creating nurse {FirstName} {LastName} with license {LicenseNumber}",
            dto.FirstName, dto.LastName, dto.LicenseNumber);

        if (await uow.StaffMembers.ExistsByEmailAsync(dto.Email, null, ct))
            throw new AlreadyExistsException("Staff", "Email", dto.Email);

        if (await uow.Nurses.ExistsByLicenseNumberAsync(dto.LicenseNumber, null, ct))
            throw new AlreadyExistsException("MedicalStaff", "LicenseNumber", dto.LicenseNumber);

        var dept = await uow.Departments.GetByIdAsync(dto.DepartmentId, ct);
        if (dept is null)
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

        await uow.Nurses.AddAsync(nurse, ct);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Nurse {Id} created successfully", nurse.Id);
        return (await GetByIdAsync(nurse.Id, ct))!;
    }

    public async Task<NurseDto?> UpdateAsync(
        Guid id,
        UpdateNurseDto dto,
        CancellationToken ct = default)
    {
        var nurse = await uow.Nurses.GetByIdAsync(id, ct);
        if (nurse is null) return null;

        if (await uow.StaffMembers.ExistsByEmailAsync(dto.Email, id, ct))
            throw new AlreadyExistsException("Staff", "Email", dto.Email);

        var dept = await uow.Departments.GetByIdAsync(dto.DepartmentId, ct);
        if (dept is null)
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

        uow.Nurses.Update(nurse);

        try
        {
            await uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entry    = ex.Entries.Single();
            var dbValues = await entry.GetDatabaseValuesAsync(ct);

            if (dbValues is null)
                throw new NotFoundException("Nurse", id);

            throw new ConcurrencyConflictException(
                "The nurse was modified by another user. Please review and retry.",
                clientValues:   entry.CurrentValues.ToObject(),
                databaseValues: dbValues.ToObject()
            );
        }

        logger.LogInformation("Nurse {Id} updated successfully", id);
        return (await GetByIdAsync(id, ct))!;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        logger.LogWarning("Attempting to delete nurse {NurseId}", id);

        var nurse = await uow.Nurses.GetByIdAsync(id, ct);
        if (nurse is null) return false;

        uow.Nurses.Remove(nurse);
        await uow.SaveChangesAsync(ct);

        logger.LogWarning("Nurse {NurseId} deleted", id);
        return true;
    }

    private static NurseDto ToDto(Nurse n) => new(
        n.Id, n.FirstName, n.LastName,
        n.Email, n.Phone, n.Address,
        n.HireDate, n.Salary,
        n.LicenseNumber, n.Service, n.Grade,
        n.DepartmentId, n.Department?.Name ?? string.Empty
    );
}