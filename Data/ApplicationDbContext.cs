using Microsoft.EntityFrameworkCore;
using PetPhotographyApp.Models;

namespace PetPhotographyApp.Data
{
    /// <summary>
    /// Database context for the Pet Photography Application
    /// Currently contains only your entities - Alyssa can add hers later
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        // Constructor - receives database configuration options
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Your entities only
        public DbSet<Owner> Owners { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        /// <summary>
        /// Configures entity relationships and database constraints
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Owner entity configuration
            modelBuilder.Entity<Owner>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(200);
                
                // Create index on email for faster lookups
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Pet entity configuration
            modelBuilder.Entity<Pet>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Species).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Breed).HasMaxLength(100);
                entity.Property(e => e.Color).HasMaxLength(20);
                entity.Property(e => e.SpecialNotes).HasMaxLength(500);
                
                // Configure relationship: Pet belongs to Owner
                entity.HasOne(p => p.Owner)
                      .WithMany(o => o.Pets)
                      .HasForeignKey(p => p.OwnerId)
                      .OnDelete(DeleteBehavior.Cascade); // Delete pets when owner is deleted
            });

            // Notification entity configuration
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                
                // Configure relationship: Notification belongs to Owner
                entity.HasOne(n => n.Owner)
                      .WithMany(o => o.Notifications)
                      .HasForeignKey(n => n.OwnerId)
                      .OnDelete(DeleteBehavior.Cascade); // Delete notifications when owner is deleted
            });
        }
    }
}