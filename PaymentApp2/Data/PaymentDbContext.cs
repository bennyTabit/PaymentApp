using Microsoft.EntityFrameworkCore;

using PaymentApp.Models;

namespace PaymentApp.Data;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
    {
    }

    public DbSet<Payment> Payments { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(100);

            entity.HasData(
                new User { Id = 1, Name = "John Doe", Email = "john@example.com" },
                new User { Id = 2, Name = "Jane Smith", Email = "jane@example.com" }
            );
        });

        // Configure Payment entity
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Amount).HasPrecision(18, 2);

            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserId);

            // Add some sample data
            entity.HasData(
                new Payment
                {
                    Id = 1,
                    UserId = 1,
                    Name = "Rent",
                    Amount = 1200.00m,
                    DueDate = DateTime.Today.AddDays(1),
                    IsRecurring = true,
                    RecurrenceType = RecurrenceType.Monthly,
                    Notes = "Monthly rent payment"
                },
                new Payment
                {
                    Id = 2,
                    UserId = 1,
                    Name = "Electric Bill",
                    Amount = 85.50m,
                    DueDate = DateTime.Today.AddDays(15),
                    IsRecurring = true,
                    RecurrenceType = RecurrenceType.Monthly,
                    Notes = "Utility bill - account #12345"
                },
                new Payment
                {
                    Id = 3,
                    UserId = 1,
                    Name = "Netflix Subscription",
                    Amount = 15.99m,
                    DueDate = DateTime.Today.AddDays(-2),
                    PaidDate = DateTime.Today.AddDays(-5),
                    IsRecurring = true,
                    RecurrenceType = RecurrenceType.Monthly,
                    Notes = "Premium plan"
                },
                new Payment
                {
                    Id = 4,
                    UserId = 2,
                    Name = "Car Insurance",
                    Amount = 450.00m,
                    DueDate = DateTime.Today.AddDays(-5),
                    IsRecurring = false,
                    RecurrenceType = RecurrenceType.None,
                    Notes = "Quarterly payment - due soon!"
                },
                new Payment
                {
                    Id = 5,
                    UserId = 2,
                    Name = "Phone Bill",
                    Amount = 65.00m,
                    DueDate = DateTime.Today.AddDays(10),
                    IsRecurring = true,
                    RecurrenceType = RecurrenceType.Monthly,
                    Notes = "Verizon - unlimited plan"
                }
            );
        });
    }
}