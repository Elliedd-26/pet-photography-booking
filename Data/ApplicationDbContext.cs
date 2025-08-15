using Microsoft.EntityFrameworkCore;
using PetPhotographyApp.Models;

namespace PetPhotographyApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Owner> Owners { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<Photographer> Photographers { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Booking_Service> Booking_Services { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Owner
            modelBuilder.Entity<Owner>(entity =>
            {
                entity.HasKey(e => e.OwnerId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Configure Pet
            modelBuilder.Entity<Pet>(entity =>
            {
                entity.HasKey(e => e.PetId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Species).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Breed).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.HasOne(p => p.Owner)
                      .WithMany(o => o.Pets)
                      .HasForeignKey(p => p.OwnerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Photographer
            modelBuilder.Entity<Photographer>(entity =>
            {
                entity.HasKey(e => e.PhotographerId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Specialty).IsRequired().HasMaxLength(100);
            });

            // Configure Booking
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(e => e.BookingId);
                entity.Property(e => e.Location).HasMaxLength(200);
                entity.HasOne(b => b.Owner)
                      .WithMany(o => o.Bookings)
                      .HasForeignKey(b => b.OwnerId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(b => b.Pet)
                      .WithMany(p => p.Bookings)
                      .HasForeignKey(b => b.PetId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(b => b.Photographer)
                      .WithMany(ph => ph.Bookings)
                      .HasForeignKey(b => b.PhotographerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Service
            modelBuilder.Entity<Service>(entity =>
            {
                entity.HasKey(e => e.ServiceId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Price).HasPrecision(10, 2);
            });

            // Configure Booking_Service (many-to-many)
            modelBuilder.Entity<Booking_Service>(entity =>
            {
                entity.HasKey(bs => new { bs.BookingId, bs.ServiceId });
                entity.HasOne(bs => bs.Booking)
                      .WithMany(b => b.BookingServices)
                      .HasForeignKey(bs => bs.BookingId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(bs => bs.Service)
                      .WithMany(s => s.BookingServices)
                      .HasForeignKey(bs => bs.ServiceId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Notification
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.NotificationId);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Type).HasMaxLength(50);
                entity.HasOne(n => n.RecipientOwner)
                      .WithMany(o => o.Notifications)
                      .HasForeignKey(n => n.RecipientOwnerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        

            // SEED DATA
/*
            modelBuilder.Entity<Owner>().HasData(
                new Owner { OwnerId = 1, Name = "Alice", Email = "alice@example.com", PhoneNumber = "1234567890" },
                new Owner { OwnerId = 2, Name = "Bob", Email = "bob@example.com", PhoneNumber = "2345678901" }
            );

            modelBuilder.Entity<Pet>().HasData(
                new Pet { PetId = 1, Name = "Fluffy", Species = "Dog", Breed = "Poodle", Age = 3, OwnerId = 1 },
                new Pet { PetId = 2, Name = "Mittens", Species = "Cat", Breed = "Siamese", Age = 2, OwnerId = 2 }
            );

            modelBuilder.Entity<Photographer>().HasData(
                new Photographer { PhotographerId = 1, Name = "Sophie", Email = "sophie@photo.com", PhoneNumber = "8888888888", Specialty = "Pets", IsAvailable = true }
            );

            modelBuilder.Entity<Service>().HasData(
                new Service { ServiceId = 1, Name = "Basic Pet Portrait", Description = "Simple headshot of your pet", Price = 49.99m },
                new Service { ServiceId = 2, Name = "Outdoor Session", Description = "Photos in a park", Price = 79.99m }
            );

            modelBuilder.Entity<Booking>().HasData(
                new Booking
                {
                    BookingId = 1,
                    BookingDate = DateTime.Now.AddDays(3),
                    Location = "Toronto Studio",
                    Notes = "Fluffy is a bit shy",
                    OwnerId = 1,
                    PetId = 1,
                    PhotographerId = 1,
                }
            );

            modelBuilder.Entity<Booking_Service>().HasData(
                new Booking_Service { BookingId = 1, ServiceId = 1 }
            );
            */
        }
    }
}
