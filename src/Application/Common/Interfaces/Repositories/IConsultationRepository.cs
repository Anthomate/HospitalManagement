using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Interfaces.Repositories;

public interface IConsultationRepository : IRepository<Consultation>
{
    Task<bool> SlotTakenAsync(Guid patientId, Guid doctorId, DateTime scheduledAt, CancellationToken ct = default);
    Task<PagedResult<Consultation>> GetByPatientAsync(Guid patientId, PaginationParams pagination, CancellationToken ct = default);
    Task<PagedResult<Consultation>> GetByDoctorAsync(Guid doctorId, PaginationParams pagination, CancellationToken ct = default);
    Task<PagedResult<Consultation>> GetByStatusAsync(ConsultationStatus status, PaginationParams pagination, CancellationToken ct = default);
    Task<PagedResult<Consultation>> GetUpcomingByPatientAsync(Guid patientId, PaginationParams pagination, CancellationToken ct = default);
    Task<IReadOnlyList<Consultation>> GetTodayByDoctorAsync(Guid doctorId, CancellationToken ct = default);
}