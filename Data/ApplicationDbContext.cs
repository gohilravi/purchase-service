using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using purchase_service.Models;
using purchase.Models;

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
            entity.HasKey(e => e.Id).HasName("Buyer_pkey");

            entity.ToTable("Buyer");

            entity.HasIndex(e => e.Email, "Buyer_Email_key").IsUnique();

            entity.Property(e => e.Company).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(now() AT TIME ZONE 'UTC'::text)");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("(now() AT TIME ZONE 'UTC'::text)");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
        });

        // Configure Purchase
        modelBuilder.Entity<Purchase>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Purchase_pkey");

            entity.ToTable("Purchase");

            entity.HasIndex(e => e.BuyerId, "idx_Purchase_BuyerId");

            entity.HasIndex(e => e.OfferId, "idx_Purchase_OfferId");

            entity.HasIndex(e => e.Status, "idx_Purchase_Status");

            entity.Property(e => e.Amount).HasPrecision(12, 2);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(now() AT TIME ZONE 'UTC'::text)");
            entity.Property(e => e.LastModifiedAt).HasDefaultValueSql("(now() AT TIME ZONE 'UTC'::text)");
            entity.Property(e => e.PurchaseDate).HasDefaultValueSql("(now() AT TIME ZONE 'UTC'::text)");
            entity.Property(e => e.Status)
                .HasMaxLength(30);
        });
    }
}

