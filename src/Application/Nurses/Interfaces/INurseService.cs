using Application.Common;
using Application.Nurses.DTOs;

namespace Application.Nurses.Interfaces;

public interface INurseService
{
    Task<PagedResult<NurseDto>> GetAllAsync(PaginationParams pagination, CancellationToken ct = default);
    Task<PagedResult<NurseDto>> GetByDepartmentAsync(Guid departmentId, PaginationParams pagination, CancellationToken ct = default);
    Task<NurseDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<NurseDto> CreateAsync(CreateNurseDto dto, CancellationToken ct = default);
    Task<NurseDto?> UpdateAsync(Guid id, UpdateNurseDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}