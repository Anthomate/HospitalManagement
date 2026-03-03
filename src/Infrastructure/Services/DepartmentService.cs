using Application.Common;
using Application.Common.Exceptions;
using Application.Departments.DTOs;
using Application.Departments.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class DepartmentService(HospitalDbContext context, ILogger<DepartmentService> logger) : IDepartmentService
{
    private static DepartmentDto ToDto(Domain.Entities.Department d, int doctorCount) => new(
        d.Id,
        d.Name,
        d.Location,
        d.MedicalDirector != null
            ? d.MedicalDirector.FirstName + " " + d.MedicalDirector.LastName
            : null,
        doctorCount,
        d.CreatedAt
    );

    public async Task<PagedResult<DepartmentDto>> GetAllAsync(
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var query = context.Departments
            .AsNoTracking()
            .OrderBy(d => d.Name);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(d => new DepartmentDto(
                d.Id,
                d.Name,
                d.Location,
                d.MedicalDirector != null
                    ? d.MedicalDirector.FirstName + " " + d.MedicalDirector.LastName
                    : null,
                d.StaffMembers.OfType<Doctor>().Count(),
                d.CreatedAt
            ))
            .ToListAsync(ct);

        return new PagedResult<DepartmentDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<DepartmentDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Departments
            .AsNoTracking()
            .Where(d => d.Id == id)
            .Select(d => new DepartmentDto(
                d.Id,
                d.Name,
                d.Location,
                d.MedicalDirector != null
                    ? d.MedicalDirector.FirstName + " " + d.MedicalDirector.LastName
                    : null,
                d.StaffMembers.OfType<Doctor>().Count(),
                d.CreatedAt
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<DepartmentDto> CreateAsync(
        CreateDepartmentDto dto,
        CancellationToken ct = default)
    {
        logger.LogInformation("Creating department {Name}", dto.Name);

        var nameExists = await context.Departments
            .AnyAsync(d => d.Name == dto.Name, ct);
        if (nameExists)
            throw new AlreadyExistsException("Department", "Name", dto.Name);

        var department = new Department { Name = dto.Name, Location = dto.Location };
        context.Departments.Add(department);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Department {Id} created successfully", department.Id);
        return (await GetByIdAsync(department.Id, ct))!;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        logger.LogWarning("Attempting to delete department {DepartmentId}", id);

        var hasDoctors = await context.Doctors
            .AnyAsync(d => d.DepartmentId == id, ct);
        if (hasDoctors)
            throw new BusinessRuleException(
                "Cannot delete a department that still has doctors. Reassign them first.");

        var deleted = await context.Departments
            .Where(d => d.Id == id)
            .ExecuteDeleteAsync(ct);

        if (deleted > 0)
            logger.LogWarning("Department {DepartmentId} deleted", id);

        return deleted > 0;
    }
    
    public async Task<DepartmentDto?> UpdateAsync(
        Guid id,
        UpdateDepartmentDto dto,
        CancellationToken ct = default)
    {
        var department = await context.Departments.FindAsync([id], ct);
        if (department is null) return null;

        var nameExists = await context.Departments
            .AnyAsync(d => d.Name == dto.Name && d.Id != id, ct);

        if (nameExists)
            throw new AlreadyExistsException("Department", "Name", dto.Name);

        if (dto.MedicalDirectorId.HasValue)
        {
            var doctorInDept = await context.Doctors.AnyAsync(
                d => d.Id == dto.MedicalDirectorId && d.DepartmentId == id, ct);

            if (!doctorInDept)
                throw new BusinessRuleException(
                    "The medical director must be a doctor belonging to this department.");
        }

        department.Name              = dto.Name;
        department.Location          = dto.Location;
        department.MedicalDirectorId = dto.MedicalDirectorId;

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

    public async Task<DepartmentDto?> AssignDirectorAsync(
        Guid departmentId,
        Guid doctorId,
        CancellationToken ct = default)
    {
        var department = await context.Departments.FindAsync([departmentId], ct);
        if (department is null) return null;

        var doctorInDept = await context.Doctors.AnyAsync(
            d => d.Id == doctorId && d.DepartmentId == departmentId, ct);
        if (!doctorInDept)
            throw new BusinessRuleException(
                "The medical director must be a doctor belonging to this department.");

        department.MedicalDirectorId = doctorId;
        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Doctor {DoctorId} assigned as director of department {DepartmentId}",
            doctorId, departmentId);

        return (await GetByIdAsync(departmentId, ct))!;
    }

    public async Task<DepartmentDto?> RemoveDirectorAsync(
        Guid departmentId,
        CancellationToken ct = default)
    {
        var department = await context.Departments.FindAsync([departmentId], ct);
        if (department is null) return null;

        department.MedicalDirectorId = null;
        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Medical director removed from department {DepartmentId}", departmentId);

        return (await GetByIdAsync(departmentId, ct))!;
    }
    
    public async Task<IReadOnlyList<DepartmentTreeDto>> GetDepartmentTreeAsync(
        CancellationToken ct = default)
    {
        var all = await context.Departments
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .Select(d => new
            {
                d.Id,
                d.Name,
                d.Location,
                d.ParentDepartmentId,
                MedicalDirectorName = d.MedicalDirector != null
                    ? d.MedicalDirector.FirstName + " " + d.MedicalDirector.LastName
                    : null,
                DoctorCount = d.StaffMembers.OfType<Doctor>().Count(),
            })
            .ToListAsync(ct);

        DepartmentTreeDto BuildNode(Guid? parentId) =>
            throw new NotImplementedException();

        var lookup = all.ToLookup(d => d.ParentDepartmentId);

        DepartmentTreeDto ToTree(Guid id)
        {
            var d = all.First(x => x.Id == id);
            return new DepartmentTreeDto(
                d.Id,
                d.Name,
                d.Location,
                d.MedicalDirectorName,
                d.DoctorCount,
                lookup[d.Id].Select(child => ToTree(child.Id)).ToList()
            );
        }

        return lookup[null]
            .Select(d => ToTree(d.Id))
            .ToList();
    }

    public async Task<DepartmentDto?> SetParentAsync(
        Guid departmentId,
        Guid? parentId,
        CancellationToken ct = default)
    {
        var department = await context.Departments.FindAsync([departmentId], ct);
        if (department is null) return null;

        if (parentId.HasValue)
        {
            if (parentId == departmentId)
                throw new BusinessRuleException("A department cannot be its own parent.");

            var wouldCycle = await CreatesCycleAsync(departmentId, parentId.Value, ct);
            if (wouldCycle)
                throw new BusinessRuleException(
                    "This assignment would create a circular hierarchy.");
        }

        department.ParentDepartmentId = parentId;
        await context.SaveChangesAsync(ct);
        return (await GetByIdAsync(departmentId, ct))!;
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

            currentId = await context.Departments
                .Where(d => d.Id == currentId)
                .Select(d => d.ParentDepartmentId)
                .FirstOrDefaultAsync(ct);
        }

        return false;
    }
}