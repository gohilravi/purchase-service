using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using purchase_service.Models;

namespace purchase_service.Data;

[ExcludeFromCodeCoverage]
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Buyer> Buyers { get; set; }
    public DbSet<Purchase> Purchases { get; set; }
    public DbSet<StatusType> StatusTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Buyer
        modelBuilder.Entity<Buyer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        // Configure Purchase
        modelBuilder.Entity<Purchase>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OfferId).IsRequired();
            entity.Property(e => e.BuyerId).IsRequired();
            entity.Property(e => e.StatusTypeId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.LastModifiedAt).IsRequired();

            entity.HasOne(e => e.Buyer)
                .WithMany()
                .HasForeignKey(e => e.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.StatusType)
                .WithMany()
                .HasForeignKey(e => e.StatusTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure StatusType
        modelBuilder.Entity<StatusType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.LastModifiedAt).IsRequired();
        });
    }
}

