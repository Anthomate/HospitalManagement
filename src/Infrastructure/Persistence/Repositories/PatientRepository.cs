using Application.Common;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class PatientRepository(HospitalDbContext context)
    : BaseRepository<Patient>(context), IPatientRepository
{
    public async Task<bool> ExistsByRecordNumberAsync(
        string recordNumber,
        CancellationToken ct = default)
        => await DbSet.AnyAsync(p => p.RecordNumber == recordNumber, ct);

    public async Task<bool> ExistsByEmailAsync(
        string email,
        Guid? excludeId = null,
        CancellationToken ct = default)
        => await DbSet.AnyAsync(
            p => p.Email == email && (excludeId == null || p.Id != excludeId), ct);

    public async Task<bool> HasActiveConsultationsAsync(
        Guid patientId,
        CancellationToken ct = default)
        => await Context.Consultations.AnyAsync(
            c => c.PatientId == patientId &&
                 c.Status != ConsultationStatus.Cancelled, ct);

    public async Task<PagedResult<Patient>> GetAllAlphabeticalAsync(
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking()
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Patient>
        {
            Items = items, TotalCount = totalCount,
            Page = pagination.Page, PageSize = pagination.PageSize
        };
    }

    public async Task<PagedResult<Patient>> SearchByNameAsync(
        string name,
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var normalized = name.Trim().ToLower();

        var query = DbSet.AsNoTracking()
            .Where(p => p.LastName.ToLower().Contains(normalized) ||
                        p.FirstName.ToLower().Contains(normalized))
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Patient>
        {
            Items = items, TotalCount = totalCount,
            Page = pagination.Page, PageSize = pagination.PageSize
        };
    }

    public async Task<Patient?> GetByRecordNumberAsync(
        string recordNumber,
        CancellationToken ct = default)
        => await DbSet
            .FirstOrDefaultAsync(p => p.RecordNumber == recordNumber, ct);
}