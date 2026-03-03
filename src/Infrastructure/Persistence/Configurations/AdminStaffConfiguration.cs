using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AdminStaffConfiguration : IEntityTypeConfiguration<AdminStaff>
{
    public void Configure(EntityTypeBuilder<AdminStaff> builder)
    {
        builder.Property(a => a.Function)
            .HasColumnName("Function")
            .HasMaxLength(100);
    }
}