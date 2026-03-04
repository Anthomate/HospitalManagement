using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class DepartmentRepository(HospitalDbContext context)
    : BaseRepository<Department>(context), IDepartmentRepository
{
    public async Task<bool> ExistsByNameAsync(
        string name,
        Guid? excludeId = null,
        CancellationToken ct = default)
        => await DbSet.AnyAsync(
            d => d.Name == name && (excludeId == null || d.Id != excludeId), ct);

    public async Task<bool> HasStaffMembersAsync(
        Guid departmentId,
        CancellationToken ct = default)
        => await Context.StaffMembers.AnyAsync(
            s => s.DepartmentId == departmentId, ct);

    public async Task<bool> HasSubDepartmentsAsync(
        Guid departmentId,
        CancellationToken ct = default)
        => await DbSet.AnyAsync(
            d => d.ParentDepartmentId == departmentId, ct);

    public async Task<List<Department>> GetAllWithDetailsAsync(
        CancellationToken ct = default)
        => await DbSet
            .Include(d => d.MedicalDirector)
            .Include(d => d.StaffMembers)
            .OrderBy(d => d.Name)
            .ToListAsync(ct);
}