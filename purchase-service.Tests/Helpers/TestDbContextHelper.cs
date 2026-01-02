using Microsoft.EntityFrameworkCore;
using purchase_service.Data;
using purchase_service.Models;

namespace purchase_service.Tests.Helpers;

public static class TestDbContextHelper
{
    public static ApplicationDbContext CreateInMemoryDbContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        SeedTestData(context);
        return context;
    }

    private static void SeedTestData(ApplicationDbContext context)
    {
        // Check if already seeded to avoid duplicate data
        if (context.Buyers.Any() || context.StatusTypes.Any())
        {
            return; // Already seeded
        }

        // Seed Buyers
        var buyers = new List<Buyer>
        {
            new Buyer { Id = 1, Name = "John Doe", Email = "john@example.com", CreatedAt = DateTime.UtcNow },
            new Buyer { Id = 2, Name = "Jane Smith", Email = "jane@example.com", CreatedAt = DateTime.UtcNow }
        };
        context.Buyers.AddRange(buyers);

        // Seed StatusTypes
        var now = DateTime.UtcNow;
        var statusTypes = new List<StatusType>
        {
            new StatusType { Id = 1, Status = "Assigned", CreatedAt = now, LastModifiedAt = now },
            new StatusType { Id = 2, Status = "Canceled", CreatedAt = now, LastModifiedAt = now },
            new StatusType { Id = 3, Status = "Completed", CreatedAt = now, LastModifiedAt = now }
        };
        context.StatusTypes.AddRange(statusTypes);

        context.SaveChanges();
    }
}

