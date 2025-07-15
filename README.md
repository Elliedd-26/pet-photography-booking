# Pet Photography Booking Web App

## Team Assignment
- **Elliedd-26**: Owner, Pet, Notification features
- **Alyssaak09**: Photographer, Booking, Service features

## Project Description
A pet photography booking management system that allows pet owners to book professional photographers for their pets.

## Features
- Pet owner management
- Pet information management
- Photographer management
- Booking system
- Service type management
- Notification system

## Tech Stack
- ASP.NET Core MVC
- Entity Framework Core
- SQL Server
- Bootstrap

## Development Rules
1. Work on individual branches
2. Push code to your branch daily
3. Merge to main via Pull Requests
4. Communicate before modifying shared files

## Getting Started

### 1. Copy configuration file
```bash
cp appsettings.example.json appsettings.json
```
### 2. Restore packages
```bash
dotnet restore
```
### 3. Run the project
```bash
dotnet run
```

### 4. Database setup
```bash
# Create migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```
## Project Structure
pet-photography-booking/\
├── Controllers/\
│   ├── OwnerController.cs (Elliedd-26)\
│   ├── PetController.cs (Elliedd-26)\
│   ├── NotificationController.cs (Elliedd-26)\
│   ├── PhotographerController.cs (Alyssa)\
│   ├── BookingController.cs (Alyssa)\
│   └── ServiceController.cs (Alyssa)\
├── Models/\
│   ├── Owner.cs (Elliedd-26)\
│   ├── Pet.cs (Elliedd-26)\
│   ├── Notification.cs (Elliedd-26)\
│   ├── Photographer.cs (Alyssa)\
│   ├── Booking.cs (Alyssa)\
│   └── Service.cs (Alyssa)\
├── Views/
│   ├── Owner/ (Elliedd-26)\
│   ├── Pet/ (Elliedd-26)\
│   ├── Notification/ (Elliedd-26)\
│   ├── Photographer/ (Alyssa)\
│   ├── Booking/ (Alyssa)\
│   └── Service/ (Alyssa)\
├── Data/\
│   └── ApplicationDbContext.cs\
└── wwwroot/\

## Entity Relationships
Owner (1) ←→ (M) Pet\
Owner (1) ←→ (M) Booking\
Owner (1) ←→ (M) Notification\
Pet (1) ←→ (M) Booking\
Photographer (1) ←→ (M) Booking\
Photographer (1) ←→ (M) Service\
Photographer (1) ←→ (M) Notification\
Service (1) ←→ (M) Booking\
Booking (1) ←→ (M) Notification

## Development Timeline
MVP Completion: July 18, 2025

Creative Collaboration: August 1, 2025

Final Documentation: August 15, 2025

## Contact
GitHub: Elliedd-26\
Repository: pet-photography-booking
