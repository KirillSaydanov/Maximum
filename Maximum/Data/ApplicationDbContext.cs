using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Maximum.Models;

namespace Maximum.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Дополнительная конфигурация модели, если потребуется

            builder.Entity<Client>(entity =>
            {
                entity.Property(e => e.BirthDate)
                    .HasColumnType("date");

                entity.Property(e => e.CreatedAtUtc)
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.UpdatedAtUtc)
                    .HasColumnType("timestamp with time zone");
            });

            builder.Entity<Appointment>(entity =>
            {
                entity.HasOne(a => a.Client)
                    .WithMany()
                    .HasForeignKey(a => a.ClientId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Employee)
                    .WithMany()
                    .HasForeignKey(a => a.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(a => a.StartAtUtc)
                    .HasColumnType("timestamp with time zone");

                entity.Property(a => a.EndAtUtc)
                    .HasColumnType("timestamp with time zone");

                entity.HasIndex(a => new { a.EmployeeId, a.StartAtUtc, a.EndAtUtc });
            });
        }
    }
}
