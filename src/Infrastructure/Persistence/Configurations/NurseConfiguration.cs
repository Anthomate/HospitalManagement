using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class NurseConfiguration : IEntityTypeConfiguration<Nurse>
{
    public void Configure(EntityTypeBuilder<Nurse> builder)
    {
        builder.Property(n => n.Service)
            .HasColumnName("Service")
            .HasMaxLength(100);

        builder.Property(n => n.Grade)
            .HasColumnName("Grade")
            .HasMaxLength(100);
    }
}