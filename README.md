# DriveEase - Car Rental Management System

DriveEase is a full-stack desktop car rental management system developed using C# .NET 8, WinUI 3, Entity Framework Core and SQL Server.

The application supports multiple user roles and provides functionality for managing cars, reservations, payments, favorites, support requests and administrative operations.

## Features

### Customer
- Browse and filter available cars
- View detailed car information
- Create and manage reservations
- Save cars to favorites
- Complete payments
- Submit support tickets
- Manage profile and account settings

### Car Renter
- Add, edit and manage listed cars
- Confirm or reject rental requests
- Monitor vehicle status
- Manage maintenance status
- View revenue and earnings statistics

### Admin
- Manage users and garages
- View platform-wide statistics
- Manage support tickets
- Monitor returned reservations
- View company revenue

## Tech Stack

- C#
- .NET 8
- WinUI 3
- Entity Framework Core
- SQL Server
- XAML

## Architecture

The application follows a layered architecture:

- **UI Layer** - WinUI 3 pages and XAML components
- **Business Layer** - service classes and application logic
- **Data Layer** - Entity Framework Core and SQL Server

## Database

The project uses Entity Framework Core with Code-First migrations.

Main entities include:

- Users
- Customers
- Car Renters
- Cars
- Reservations
- Payments
- Favorite Cars
- Support Tickets
- Garages
- Notifications
- Maintenance Records

## Main Functionalities

- Role-based authentication and navigation
- CRUD operations for cars and user-related data
- Reservation availability validation
- Rental cost calculation
- Payment management
- Favorites system
- Vehicle monitoring
- Revenue dashboards
- Support ticket management
- Notification handling

## Project Structure

```text
CarRental.Backend/
├── Data/
├── Migrations/
├── Models/
└── Services/

Project/
├── Assets/
├── Models/
├── Properties/
├── Views/
├── App.xaml
├── MainWindow.xaml
└── SessionManager.cs
