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
        }
    }
}
