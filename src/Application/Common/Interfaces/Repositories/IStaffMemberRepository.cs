using Domain.Entities;

namespace Application.Common.Interfaces.Repositories;

public interface IStaffMemberRepository : IRepository<StaffMember>
{
    Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null, CancellationToken ct = default);
}