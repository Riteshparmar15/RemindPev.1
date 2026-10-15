using Microsoft.EntityFrameworkCore;
using RemindPe.Models;

namespace RemindPe.Data;

public class KhataDbContext : DbContext
{
    public KhataDbContext(DbContextOptions<KhataDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(120);
            entity.Property(c => c.Phone).IsRequired().HasMaxLength(20);
            entity.HasIndex(c => c.Phone);
            entity.HasIndex(c => c.Name);

            entity.HasMany(c => c.Transactions)
                  .WithOne(t => t.Customer)
                  .HasForeignKey(t => t.CustomerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Amount).HasPrecision(18, 2).IsRequired();
            entity.Property(t => t.Type).IsRequired().HasMaxLength(10);
            entity.Property(t => t.Note).HasMaxLength(300);
            entity.HasIndex(t => t.CustomerId);
            entity.HasIndex(t => t.Date);
            entity.HasIndex(t => new { t.CustomerId, t.Date });
        });
    }
}
