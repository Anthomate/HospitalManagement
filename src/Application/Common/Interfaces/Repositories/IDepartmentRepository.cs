using Domain.Entities;

namespace Application.Common.Interfaces.Repositories;

public interface IDepartmentRepository : IRepository<Department>
{
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default);
    Task<bool> HasStaffMembersAsync(Guid departmentId, CancellationToken ct = default);
    Task<bool> HasSubDepartmentsAsync(Guid departmentId, CancellationToken ct = default);
    Task<List<Department>> GetAllWithDetailsAsync(CancellationToken ct = default);
}