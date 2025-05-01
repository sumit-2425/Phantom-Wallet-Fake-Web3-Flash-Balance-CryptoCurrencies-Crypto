using Microsoft.EntityFrameworkCore;

namespace Backend.Models;
public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    public DbSet<Pharmacy> Pharmacies { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Mask> Masks { get; set; }
    public DbSet<MaskType> MaskTypes { get; set; }
    public DbSet<OpeningHour> OpeningHours { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User to Transactions relationship
        modelBuilder.Entity<User>()
            .HasMany(u => u.Transactions)
            .WithOne(t => t.User)
            .HasForeignKey(t => t.UserId);

        // Pharmacy to Transactions relationship
        modelBuilder.Entity<Pharmacy>()
            .HasMany(p => p.Transactions)
            .WithOne(t => t.Pharmacy)
            .HasForeignKey(t => t.PharmacyId);

        // Pharmacy to Masks relationship
        modelBuilder.Entity<Pharmacy>()
            .HasMany(p => p.Masks)
            .WithOne(m => m.Pharmacy)
            .HasForeignKey(m => m.PharmacyId);

        // MaskType to Masks relationship
        modelBuilder.Entity<MaskType>()
            .HasMany(mt => mt.Masks)
            .WithOne(m => m.MaskType)
            .HasForeignKey(m => m.MaskTypeId);

        // Pharmacy to OpeningHours relationship
        modelBuilder.Entity<Pharmacy>()
            .HasMany(p => p.OpeningHours)
            .WithOne(oh => oh.Pharmacy)
            .HasForeignKey(oh => oh.PharmacyId);
    }

    public override int SaveChanges()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is BaseEntity entity)
            {
                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow; // Set CreatedAt when the entity is added
                    entity.UpdatedAt = entity.CreatedAt; // Set UpdatedAt as same as CreatedAt when the entity is added
                }

                if (entry.State == EntityState.Modified)
                {
                    entity.UpdatedAt = DateTime.UtcNow; // Set UpdatedAt when the entity is modified
                }
            }
        }

        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is BaseEntity entity)
            {
                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;  // Set CreatedAt when the entity is added
                    entity.UpdatedAt = entity.CreatedAt; // Set UpdatedAt as same as CreatedAt when the entity is added
                }

                if (entry.State == EntityState.Modified)
                {
                    entity.UpdatedAt = DateTime.UtcNow;  // Set UpdatedAt when the entity is modified
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
