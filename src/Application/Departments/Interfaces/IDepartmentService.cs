using Application.Common;
using Application.Departments.DTOs;

namespace Application.Departments.Interfaces;

public interface IDepartmentService
{
    Task<PagedResult<DepartmentDto>> GetAllAsync(PaginationParams pagination, CancellationToken ct = default);
    Task<DepartmentDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto, CancellationToken ct = default);
    Task<DepartmentDto?> UpdateAsync(Guid id, UpdateDepartmentDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<DepartmentDto?> AssignDirectorAsync(Guid departmentId, Guid doctorId, CancellationToken ct = default);
    Task<DepartmentDto?> RemoveDirectorAsync(Guid departmentId, CancellationToken ct = default);
}