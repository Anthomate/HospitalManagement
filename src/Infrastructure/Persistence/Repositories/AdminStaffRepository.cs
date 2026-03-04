using Application.Common;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class AdminStaffRepository(HospitalDbContext context)
    : BaseRepository<AdminStaff>(context), IAdminStaffRepository
{
    public async Task<PagedResult<AdminStaff>> GetByDepartmentAsync(
        Guid departmentId,
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking()
            .Where(a => a.DepartmentId == departmentId)
            .OrderBy(a => a.LastName);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(ct);

        return new PagedResult<AdminStaff>
        {
            Items = items, TotalCount = totalCount,
            Page = pagination.Page, PageSize = pagination.PageSize
        };
    }
}