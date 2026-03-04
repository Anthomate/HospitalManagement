using Application.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Doctors.DTOs;
using Application.Doctors.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class DoctorService(
    IUnitOfWork uow,
    ILogger<DoctorService> logger) : IDoctorService
{
    public async Task<PagedResult<DoctorDto>> GetAllAsync(
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var result = await uow.Doctors.GetByDepartmentAsync(Guid.Empty, pagination, ct);
        var all = await uow.Doctors.GetAllAsync(ct);

        return new PagedResult<DoctorDto>
        {
            Items      = all.OrderBy(d => d.LastName).Select(ToDto).ToList(),
            TotalCount = all.Count,
            Page       = pagination.Page,
            PageSize   = pagination.PageSize
        };
    }

    public async Task<PagedResult<DoctorDto>> GetByDepartmentAsync(
        Guid departmentId,
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var result = await uow.Doctors.GetByDepartmentAsync(departmentId, pagination, ct);

        return new PagedResult<DoctorDto>
        {
            Items      = result.Items.Select(ToDto).ToList(),
            TotalCount = result.TotalCount,
            Page       = result.Page,
            PageSize   = result.PageSize
        };
    }

    public async Task<DoctorDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var doctor = await uow.Doctors.GetByIdAsync(id, ct);
        return doctor is null ? null : ToDto(doctor);
    }

    public async Task<DoctorDto> CreateAsync(
        CreateDoctorDto dto,
        CancellationToken ct = default)
    {
        logger.LogInformation(
            "Creating doctor {FirstName} {LastName} with license {LicenseNumber}",
            dto.FirstName, dto.LastName, dto.LicenseNumber);

        if (await uow.StaffMembers.ExistsByEmailAsync(dto.Email, null, ct))
            throw new AlreadyExistsException("Staff", "Email", dto.Email);

        var dept = await uow.Departments.GetByIdAsync(dto.DepartmentId, ct);
        if (dept is null)
            throw new NotFoundException("Department", dto.DepartmentId);

        if (await uow.Doctors.ExistsByLicenseNumberAsync(dto.LicenseNumber, null, ct))
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

        await uow.Doctors.AddAsync(doctor, ct);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Doctor {Id} created successfully", doctor.Id);
        return ToDto(doctor);
    }

    public async Task<DoctorDto?> UpdateAsync(
        Guid id,
        UpdateDoctorDto dto,
        CancellationToken ct = default)
    {
        var doctor = await uow.Doctors.GetByIdAsync(id, ct);
        if (doctor is null) return null;

        if (await uow.StaffMembers.ExistsByEmailAsync(dto.Email, id, ct))
            throw new AlreadyExistsException("Staff", "Email", dto.Email);

        var dept = await uow.Departments.GetByIdAsync(dto.DepartmentId, ct);
        if (dept is null)
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

        uow.Doctors.Update(doctor);

        try
        {
            await uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entry    = ex.Entries.Single();
            var dbValues = await entry.GetDatabaseValuesAsync(ct);

            if (dbValues is null)
                throw new NotFoundException("Doctor", id);

            throw new ConcurrencyConflictException(
                "The doctor was modified by another user. Please review and retry.",
                clientValues:   entry.CurrentValues.ToObject(),
                databaseValues: dbValues.ToObject());
        }

        logger.LogInformation("Doctor {Id} updated successfully", id);
        return ToDto(doctor);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        logger.LogWarning("Attempting to delete doctor {DoctorId}", id);

        var doctor = await uow.Doctors.GetByIdAsync(id, ct);
        if (doctor is null) return false;

        if (await uow.Doctors.IsDirectorAsync(id, ct))
            throw new BusinessRuleException(
                "Cannot delete a doctor who is a department's medical director. " +
                "Reassign the director first.");

        uow.Doctors.Remove(doctor);
        await uow.SaveChangesAsync(ct);

        logger.LogWarning("Doctor {DoctorId} deleted", id);
        return true;
    }

    private static DoctorDto ToDto(Doctor d) => new(
        d.Id, d.FirstName, d.LastName, d.Email, d.Phone, d.Address,
        d.HireDate, d.Salary, d.Specialty, d.LicenseNumber,
        d.DepartmentId, d.Department?.Name ?? string.Empty);
}