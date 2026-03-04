using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Tests.Helpers;

public static class TestDbContextFactory
{
    // Chaque test reçoit une base isolée via un nom unique
    // → pas d'interférence entre tests parallèles
    public static HospitalDbContext Create()
    {
        var options = new DbContextOptionsBuilder<HospitalDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new HospitalDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}