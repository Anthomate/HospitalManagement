using Application.AdminStaff.Interfaces;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Consultations.Interfaces;
using Application.Dashboard.Interfaces;
using Application.Departments.Interfaces;
using Application.Doctors.Interfaces;
using Application.Nurses.Interfaces;
using Application.Patients.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<HospitalDbContext>(options => options.UseNpgsql(
            configuration.GetConnectionString("DefaultConnection"),
            npgsql => npgsql.MigrationsAssembly("Infrastructure"))
        );
        
        // Repositories
        services.AddScoped<IPatientRepository,      PatientRepository>();
        services.AddScoped<IDepartmentRepository,   DepartmentRepository>();
        services.AddScoped<IConsultationRepository, ConsultationRepository>();
        services.AddScoped<IDoctorRepository,       DoctorRepository>();
        services.AddScoped<INurseRepository,        NurseRepository>();
        services.AddScoped<IAdminStaffRepository,   AdminStaffRepository>();
        services.AddScoped<IStaffMemberRepository,  StaffMemberRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<INurseService, NurseService>();
        services.AddScoped<IAdminStaffService, AdminStaffService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IConsultationService, ConsultationService>();
        services.AddScoped<IDashboardService, DashboardService>();
        
        return services;
    }
}