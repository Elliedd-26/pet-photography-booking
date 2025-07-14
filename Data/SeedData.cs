using Microsoft.EntityFrameworkCore;
using PetPhotographyApp.Models;

namespace PetPhotographyApp.Data
{
    /// <summary>
    /// Seeds the database with initial test data
    /// </summary>
    public static class SeedData
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Ensure database is created
            context.Database.EnsureCreated();

            // Check if we already have data
            if (context.Owners.Any())
            {
                return; // Database has been seeded
            }

            Console.WriteLine("🌱 Seeding database with test data...");

            // Add Owners
            var owners = new[]
            {
                new Owner
                {
                    Name = "Sarah Johnson",
                    Email = "sarah.johnson@email.com",
                    Phone = "416-555-0123",
                    Address = "123 Queen Street, Toronto, ON",
                    CreatedAt = DateTime.Now.AddDays(-30)
                },
                new Owner
                {
                    Name = "Mike Chen",
                    Email = "mike.chen@email.com", 
                    Phone = "416-555-0124",
                    Address = "456 King Street, Toronto, ON",
                    CreatedAt = DateTime.Now.AddDays(-25)
                },
                new Owner
                {
                    Name = "Emily Rodriguez",
                    Email = "emily.rodriguez@email.com",
                    Phone = "416-555-0125",
                    Address = "789 Yonge Street, Toronto, ON",
                    CreatedAt = DateTime.Now.AddDays(-20)
                },
                new Owner
                {
                    Name = "David Kim",
                    Email = "david.kim@email.com",
                    Phone = "416-555-0126",
                    Address = "321 Bloor Street, Toronto, ON",
                    CreatedAt = DateTime.Now.AddDays(-15)
                }
            };

            context.Owners.AddRange(owners);
            context.SaveChanges();

            // Add Pets (after owners are saved to get their IDs)
            var pets = new[]
            {
                // Sarah's pets
                new Pet
                {
                    Name = "Max",
                    Species = "Dog",
                    Breed = "Golden Retriever",
                    Age = 3,
                    Color = "Golden",
                    SpecialNotes = "Very energetic, loves treats and belly rubs. Great with cameras!",
                    OwnerId = owners[0].Id,
                    CreatedAt = DateTime.Now.AddDays(-28)
                },
                new Pet
                {
                    Name = "Whiskers",
                    Species = "Cat", 
                    Breed = "Persian",
                    Age = 2,
                    Color = "White with gray markings",
                    SpecialNotes = "Shy around strangers, needs gentle approach. Loves feather toys.",
                    OwnerId = owners[0].Id,
                    CreatedAt = DateTime.Now.AddDays(-27)
                },
                
                // Mike's pets
                new Pet
                {
                    Name = "Buddy",
                    Species = "Dog",
                    Breed = "Labrador",
                    Age = 5,
                    Color = "Black",
                    SpecialNotes = "Calm and patient, perfect model. Responds well to hand signals.",
                    OwnerId = owners[1].Id,
                    CreatedAt = DateTime.Now.AddDays(-23)
                },
                
                // Emily's pets
                new Pet
                {
                    Name = "Luna",
                    Species = "Cat",
                    Breed = "British Shorthair",
                    Age = 1,
                    Color = "Gray",
                    SpecialNotes = "Playful kitten, very photogenic. Loves laser pointers.",
                    OwnerId = owners[2].Id,
                    CreatedAt = DateTime.Now.AddDays(-18)
                },
                new Pet
                {
                    Name = "Charlie",
                    Species = "Rabbit",
                    Breed = "Holland Lop",
                    Age = 2,
                    Color = "Brown and white",
                    SpecialNotes = "Calm and gentle, sits still for photos. Loves carrots as treats.",
                    OwnerId = owners[2].Id,
                    CreatedAt = DateTime.Now.AddDays(-17)
                },
                
                // David's pet
                new Pet
                {
                    Name = "Rocky",
                    Species = "Dog",
                    Breed = "French Bulldog", 
                    Age = 4,
                    Color = "Brindle",
                    SpecialNotes = "Short attention span, works best with quick sessions. Very food motivated.",
                    OwnerId = owners[3].Id,
                    CreatedAt = DateTime.Now.AddDays(-13)
                }
            };

            context.Pets.AddRange(pets);
            context.SaveChanges();

            // Add Notifications
            var notifications = new[]
            {
                // Welcome notifications
                new Notification
                {
                    Message = "Welcome to Pet Photography! We're excited to capture beautiful moments with your pets.",
                    Type = "Welcome",
                    IsRead = true,
                    OwnerId = owners[0].Id,
                    CreatedAt = DateTime.Now.AddDays(-30)
                },
                new Notification
                {
                    Message = "Welcome to Pet Photography! Complete your pet profiles to get started with bookings.",
                    Type = "Welcome", 
                    IsRead = true,
                    OwnerId = owners[1].Id,
                    CreatedAt = DateTime.Now.AddDays(-25)
                },
                new Notification
                {
                    Message = "Welcome to Pet Photography! We specialize in capturing the unique personality of every pet.",
                    Type = "Welcome",
                    IsRead = false,
                    OwnerId = owners[2].Id,
                    CreatedAt = DateTime.Now.AddDays(-20)
                },
                
                // Booking-related notifications
                new Notification
                {
                    Message = "Great news! Your photo session request has been received. Our team will contact you within 24 hours.",
                    Type = "BookingConfirmation",
                    IsRead = true,
                    OwnerId = owners[0].Id,
                    CreatedAt = DateTime.Now.AddDays(-10)
                },
                new Notification
                {
                    Message = "Reminder: Don't forget to bring Max's favorite treats for tomorrow's photo session!",
                    Type = "Reminder",
                    IsRead = false,
                    OwnerId = owners[0].Id,
                    CreatedAt = DateTime.Now.AddDays(-1)
                },
                
                // General notifications
                new Notification
                {
                    Message = "New service alert: We now offer outdoor adventure photo sessions! Perfect for active dogs.",
                    Type = "ServiceUpdate",
                    IsRead = false,
                    OwnerId = owners[1].Id,
                    CreatedAt = DateTime.Now.AddDays(-5)
                },
                new Notification
                {
                    Message = "Tip: Bring your pet's favorite toy to help them feel comfortable during the photo session.",
                    Type = "Tip",
                    IsRead = false,
                    OwnerId = owners[2].Id,
                    CreatedAt = DateTime.Now.AddDays(-3)
                },
                new Notification
                {
                    Message = "Spring special: Book a session this month and get 20% off your second pet!",
                    Type = "Promotion",
                    IsRead = false,
                    OwnerId = owners[3].Id,
                    CreatedAt = DateTime.Now.AddDays(-2)
                }
            };

            context.Notifications.AddRange(notifications);
            context.SaveChanges();

            Console.WriteLine("✅ Database seeded successfully!");
            Console.WriteLine($"Added {owners.Length} owners, {pets.Length} pets, and {notifications.Length} notifications.");
        }
    }
}