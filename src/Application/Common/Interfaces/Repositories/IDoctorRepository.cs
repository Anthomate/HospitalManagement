using Domain.Entities;

namespace Application.Common.Interfaces.Repositories;

public interface IDoctorRepository : IRepository<Doctor>
{
    Task<bool> ExistsByLicenseNumberAsync(string licenseNumber, Guid? excludeId = null, CancellationToken ct = default);
    Task<bool> IsDirectorAsync(Guid doctorId, CancellationToken ct = default);
    Task<PagedResult<Doctor>> GetByDepartmentAsync(Guid departmentId, PaginationParams pagination, CancellationToken ct = default);
}