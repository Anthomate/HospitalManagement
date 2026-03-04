using Domain.Entities;

namespace Application.Common.Interfaces.Repositories;

public interface IPatientRepository : IRepository<Patient>
{
    Task<bool> ExistsByRecordNumberAsync(string recordNumber, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null, CancellationToken ct = default);
    Task<bool> HasActiveConsultationsAsync(Guid patientId, CancellationToken ct = default);
    Task<PagedResult<Patient>> GetAllAlphabeticalAsync(PaginationParams pagination, CancellationToken ct = default);
    Task<PagedResult<Patient>> SearchByNameAsync(string name, PaginationParams pagination, CancellationToken ct = default);
    Task<Patient?> GetByRecordNumberAsync(string recordNumber, CancellationToken ct = default);
}