namespace Application.Common.Interfaces.Repositories;

public interface IAdminStaffRepository : IRepository<Domain.Entities.AdminStaff>
{
    Task<PagedResult<Domain.Entities.AdminStaff>> GetByDepartmentAsync(Guid departmentId, PaginationParams pagination, CancellationToken ct = default);
}