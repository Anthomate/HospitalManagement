using Application.Consultations.Interfaces;
using Application.Dashboard.Interfaces;
using Application.Departments.Interfaces;
using Application.Doctors.Interfaces;
using Application.Patients.Interfaces;
using Infrastructure.Persistence;
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
        
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<IConsultationService, ConsultationService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        
        return services;
    }
}