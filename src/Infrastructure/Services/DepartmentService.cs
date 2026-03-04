using Application.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Departments.DTOs;
using Application.Departments.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class DepartmentService(
    IUnitOfWork uow,
    ILogger<DepartmentService> logger) : IDepartmentService
{
    public async Task<PagedResult<DepartmentDto>> GetAllAsync(
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var result = await uow.Departments.GetAllPagedAsync(pagination, ct);

        return new PagedResult<DepartmentDto>
        {
            Items      = result.Items.Select(d => ToDto(d)).ToList(),
            TotalCount = result.TotalCount,
            Page       = result.Page,
            PageSize   = result.PageSize
        };
    }

    public async Task<DepartmentDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var dept = await uow.Departments.GetByIdAsync(id, ct);
        return dept is null ? null : ToDto(dept);
    }

    public async Task<DepartmentDto> CreateAsync(
        CreateDepartmentDto dto,
        CancellationToken ct = default)
    {
        logger.LogInformation("Creating department {Name}", dto.Name);

        if (await uow.Departments.ExistsByNameAsync(dto.Name, null, ct))
            throw new AlreadyExistsException("Department", "Name", dto.Name);

        var department = new Department { Name = dto.Name, Location = dto.Location };

        await uow.Departments.AddAsync(department, ct);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Department {Id} created successfully", department.Id);
        return ToDto(department);
    }

    public async Task<DepartmentDto?> UpdateAsync(
        Guid id,
        UpdateDepartmentDto dto,
        CancellationToken ct = default)
    {
        var department = await uow.Departments.GetByIdAsync(id, ct);
        if (department is null) return null;

        if (await uow.Departments.ExistsByNameAsync(dto.Name, id, ct))
            throw new AlreadyExistsException("Department", "Name", dto.Name);

        if (dto.MedicalDirectorId.HasValue)
        {
            var doctor = await uow.Doctors.GetByIdAsync(dto.MedicalDirectorId.Value, ct);
            if (doctor is null || doctor.DepartmentId != id)
                throw new BusinessRuleException(
                    "The medical director must be a doctor belonging to this department.");
        }

        department.Name              = dto.Name;
        department.Location          = dto.Location;
        department.MedicalDirectorId = dto.MedicalDirectorId;

        uow.Departments.Update(department);

        try
        {
            await uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entry    = ex.Entries.Single();
            var dbValues = await entry.GetDatabaseValuesAsync(ct);

            if (dbValues is null)
                throw new NotFoundException("Department", id);

            throw new ConcurrencyConflictException(
                "The department was modified by another user. Please review and retry.",
                clientValues:   entry.CurrentValues.ToObject(),
                databaseValues: dbValues.ToObject()
            );
        }

        return ToDto(department);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        logger.LogWarning("Attempting to delete department {DepartmentId}", id);

        var department = await uow.Departments.GetByIdAsync(id, ct);
        if (department is null) return false;

        if (await uow.Departments.HasStaffMembersAsync(id, ct))
            throw new BusinessRuleException(
                "Cannot delete a department that still has staff members. Reassign them first.");

        if (await uow.Departments.HasSubDepartmentsAsync(id, ct))
            throw new BusinessRuleException(
                "Cannot delete a department that has sub-departments. Reassign them first.");

        uow.Departments.Remove(department);
        await uow.SaveChangesAsync(ct);

        logger.LogWarning("Department {DepartmentId} deleted", id);
        return true;
    }

    public async Task<DepartmentDto?> AssignDirectorAsync(
        Guid departmentId,
        Guid doctorId,
        CancellationToken ct = default)
    {
        var department = await uow.Departments.GetByIdAsync(departmentId, ct);
        if (department is null) return null;

        var doctor = await uow.Doctors.GetByIdAsync(doctorId, ct);
        if (doctor is null || doctor.DepartmentId != departmentId)
            throw new BusinessRuleException(
                "The medical director must be a doctor belonging to this department.");

        department.MedicalDirectorId = doctorId;
        uow.Departments.Update(department);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Doctor {DoctorId} assigned as director of department {DepartmentId}",
            doctorId, departmentId);

        return ToDto(department);
    }

    public async Task<DepartmentDto?> RemoveDirectorAsync(
        Guid departmentId,
        CancellationToken ct = default)
    {
        var department = await uow.Departments.GetByIdAsync(departmentId, ct);
        if (department is null) return null;

        department.MedicalDirectorId = null;
        uow.Departments.Update(department);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Medical director removed from department {DepartmentId}", departmentId);

        return ToDto(department);
    }

    public async Task<IReadOnlyList<DepartmentTreeDto>> GetDepartmentTreeAsync(
        CancellationToken ct = default)
    {
        var all = await uow.Departments.GetAllWithDetailsAsync(ct);

        var lookup = all.ToLookup(d => d.ParentDepartmentId);

        DepartmentTreeDto ToTree(Department d) => new(
            d.Id,
            d.Name,
            d.Location,
            d.MedicalDirector is not null
                ? $"{d.MedicalDirector.FirstName} {d.MedicalDirector.LastName}"
                : null,
            d.StaffMembers.OfType<Doctor>().Count(),
            lookup[d.Id].Select(child => ToTree(child)).ToList()
        );

        return lookup[null].Select(d => ToTree(d)).ToList();
    }

    public async Task<DepartmentDto?> SetParentAsync(
        Guid departmentId,
        Guid? parentId,
        CancellationToken ct = default)
    {
        var department = await uow.Departments.GetByIdAsync(departmentId, ct);
        if (department is null) return null;

        if (parentId.HasValue)
        {
            if (parentId == departmentId)
                throw new BusinessRuleException("A department cannot be its own parent.");

            if (await CreatesCycleAsync(departmentId, parentId.Value, ct))
                throw new BusinessRuleException(
                    "This assignment would create a circular hierarchy.");
        }

        department.ParentDepartmentId = parentId;
        uow.Departments.Update(department);
        await uow.SaveChangesAsync(ct);

        return ToDto(department);
    }
    
    private async Task<bool> CreatesCycleAsync(
        Guid departmentId,
        Guid candidateParentId,
        CancellationToken ct)
    {
        var currentId = (Guid?)candidateParentId;

        while (currentId.HasValue)
        {
            if (currentId == departmentId) return true;

            var parent = await uow.Departments.GetByIdAsync(currentId.Value, ct);
            currentId = parent?.ParentDepartmentId;
        }

        return false;
    }

    private static DepartmentDto ToDto(Department d) => new(
        d.Id,
        d.Name,
        d.Location,
        d.MedicalDirector is not null
            ? $"{d.MedicalDirector.FirstName} {d.MedicalDirector.LastName}"
            : null,
        d.StaffMembers.OfType<Doctor>().Count(),
        d.CreatedAt
    );
}