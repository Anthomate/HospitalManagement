using Application.Common;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class NurseRepository(HospitalDbContext context)
    : BaseRepository<Nurse>(context), INurseRepository
{
    public async Task<bool> ExistsByLicenseNumberAsync(
        string licenseNumber,
        Guid? excludeId = null,
        CancellationToken ct = default)
        => await DbSet.AnyAsync(
            n => n.LicenseNumber == licenseNumber &&
                 (excludeId == null || n.Id != excludeId), ct);

    public async Task<PagedResult<Nurse>> GetByDepartmentAsync(
        Guid departmentId,
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking()
            .Where(n => n.DepartmentId == departmentId)
            .OrderBy(n => n.LastName);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Nurse>
        {
            Items = items, TotalCount = totalCount,
            Page = pagination.Page, PageSize = pagination.PageSize
        };
    }
}