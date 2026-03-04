using Application.Common;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class DoctorRepository(HospitalDbContext context)
    : BaseRepository<Doctor>(context), IDoctorRepository
{
    public async Task<bool> ExistsByLicenseNumberAsync(
        string licenseNumber,
        Guid? excludeId = null,
        CancellationToken ct = default)
        => await DbSet.AnyAsync(
            d => d.LicenseNumber == licenseNumber &&
                 (excludeId == null || d.Id != excludeId), ct);

    public async Task<bool> IsDirectorAsync(
        Guid doctorId,
        CancellationToken ct = default)
        => await Context.Departments.AnyAsync(
            d => d.MedicalDirectorId == doctorId, ct);

    public async Task<PagedResult<Doctor>> GetByDepartmentAsync(
        Guid departmentId,
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking()
            .Where(d => d.DepartmentId == departmentId)
            .OrderBy(d => d.LastName);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Doctor>
        {
            Items = items, TotalCount = totalCount,
            Page = pagination.Page, PageSize = pagination.PageSize
        };
    }
}