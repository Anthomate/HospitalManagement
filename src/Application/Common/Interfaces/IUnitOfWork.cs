using Application.Common.Interfaces.Repositories;

namespace Application.Common.Interfaces;

public interface IUnitOfWork : IAsyncDisposable
{
    IPatientRepository Patients { get; }
    IDepartmentRepository Departments { get; }
    IConsultationRepository Consultations { get; }
    IDoctorRepository Doctors { get; }
    INurseRepository Nurses { get; }
    IAdminStaffRepository AdminStaffs { get; }
    IStaffMemberRepository StaffMembers { get; } 

    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}