using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Persistence;

public class UnitOfWork(HospitalDbContext context) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    private IPatientRepository?      _patients;
    private IDepartmentRepository?   _departments;
    private IConsultationRepository? _consultations;
    private IDoctorRepository?       _doctors;
    private INurseRepository?        _nurses;
    private IAdminStaffRepository?   _adminStaffs;
    private IStaffMemberRepository?  _staffMembers;

    public IPatientRepository      Patients      => _patients      ??= new PatientRepository(context);
    public IDepartmentRepository   Departments   => _departments   ??= new DepartmentRepository(context);
    public IConsultationRepository Consultations => _consultations ??= new ConsultationRepository(context);
    public IDoctorRepository       Doctors       => _doctors       ??= new DoctorRepository(context);
    public INurseRepository        Nurses        => _nurses        ??= new NurseRepository(context);
    public IAdminStaffRepository   AdminStaffs   => _adminStaffs   ??= new AdminStaffRepository(context);
    public IStaffMemberRepository  StaffMembers  => _staffMembers  ??= new StaffMemberRepository(context);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await context.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default)
        => _transaction = await context.Database.BeginTransactionAsync(ct);

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is null)
            throw new InvalidOperationException("No transaction in progress.");

        await _transaction.CommitAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is null)
            throw new InvalidOperationException("No transaction in progress.");

        await _transaction.RollbackAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null)
            await _transaction.DisposeAsync();

        await context.DisposeAsync();
    }
}