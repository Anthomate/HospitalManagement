using Domain.Entities;

namespace Application.Common.Interfaces.Repositories;


public interface INurseRepository : IRepository<Nurse>
{
    Task<bool> ExistsByLicenseNumberAsync(string licenseNumber, Guid? excludeId = null, CancellationToken ct = default);
    Task<PagedResult<Nurse>> GetByDepartmentAsync(Guid departmentId, PaginationParams pagination, CancellationToken ct = default);
}