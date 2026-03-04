using FluentAssertions;
using Infrastructure.Persistence;
using Tests.Helpers;

namespace Tests;

public class UnitOfWorkTests : IDisposable
{
    private readonly HospitalDbContext _context;
    private readonly UnitOfWork _uow;

    public UnitOfWorkTests()
    {
        _context = TestDbContextFactory.Create();
        _uow     = new UnitOfWork(_context);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistAllChanges()
    {
        var patient = SeedData.CreatePatient();

        await _uow.Patients.AddAsync(patient);
        await _uow.SaveChangesAsync();

        var result = await _uow.Patients.GetByIdAsync(patient.Id);
        result.Should().NotBeNull();
        result!.RecordNumber.Should().Be("REC-001");
    }

    [Fact]
    public async Task Repositories_ShouldShareSameContext()
    {
        var dept = SeedData.CreateDepartment();
        await _uow.Departments.AddAsync(dept);
        await _uow.SaveChangesAsync();

        var doctor = SeedData.CreateDoctor(dept.Id);
        await _uow.Doctors.AddAsync(doctor);
        await _uow.SaveChangesAsync();

        var savedDoctor = await _uow.Doctors.GetByIdAsync(doctor.Id);
        savedDoctor.Should().NotBeNull();
        savedDoctor!.DepartmentId.Should().Be(dept.Id);

        var savedDept = await _uow.Departments.GetByIdAsync(dept.Id);
        savedDept.Should().NotBeNull();
    }

    // [Fact]
    // public async Task BeginTransaction_ShouldRollbackOnError()
    // {
    //     await _uow.BeginTransactionAsync();
    //
    //     var patient = SeedData.CreatePatient();
    //     await _uow.Patients.AddAsync(patient);
    //     await _uow.SaveChangesAsync();
    //
    //     await _uow.RollbackTransactionAsync();
    //
    //     var result = await _uow.Patients.GetByIdAsync(patient.Id);
    //     result.Should().BeNull();
    // }
    //
    // [Fact]
    // public async Task BeginTransaction_ShouldCommitSuccessfully()
    // {
    //     await _uow.BeginTransactionAsync();
    //
    //     var patient = SeedData.CreatePatient();
    //     await _uow.Patients.AddAsync(patient);
    //     await _uow.SaveChangesAsync();
    //
    //     await _uow.CommitTransactionAsync();
    //
    //     var result = await _uow.Patients.GetByIdAsync(patient.Id);
    //     result.Should().NotBeNull();
    // }

    [Fact]
    public async Task CommitTransactionAsync_ShouldThrow_WhenNoTransactionInProgress()
    {
        await _uow.Invoking(u => u.CommitTransactionAsync())
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No transaction*");
    }

    [Fact]
    public async Task RollbackTransactionAsync_ShouldThrow_WhenNoTransactionInProgress()
    {
        await _uow.Invoking(u => u.RollbackTransactionAsync())
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No transaction*");
    }

    public void Dispose() => _context.Dispose();
}