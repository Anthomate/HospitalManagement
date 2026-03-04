using Application.Common;
using Application.Patients.DTOs;

namespace Application.Patients.Interfaces;

public interface IPatientService
{
    Task<PagedResult<PatientDto>> GetAllAsync(PaginationParams pagination, CancellationToken ct = default);
    Task<PatientDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PatientDto?> GetByRecordNumberAsync(string recordNumber, CancellationToken ct = default);
    Task<PatientDto> CreateAsync(CreatePatientDto dto, CancellationToken ct = default);
    Task<PatientDto?> UpdateAsync(Guid id, UpdatePatientDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<PatientDto>> SearchByNameAsync(string name, PaginationParams pagination, CancellationToken ct = default);
}