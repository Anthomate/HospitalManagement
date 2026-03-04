using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class StaffMemberRepository(HospitalDbContext context)
    : BaseRepository<StaffMember>(context), IStaffMemberRepository
{
    public async Task<bool> ExistsByEmailAsync(
        string email,
        Guid? excludeId = null,
        CancellationToken ct = default)
        => await DbSet.AnyAsync(
            s => s.Email == email && (excludeId == null || s.Id != excludeId), ct);
}