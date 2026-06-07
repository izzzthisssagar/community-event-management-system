# Community Event Management System

A web application built for the **CET254 Advanced Programming** module (Assignment 1) at the
University of Sunderland. The system lets a community organisation manage events, the venues
those events run at, the activities they include, the participants who attend, and the
registrations that link participants to events.

## Tech Stack

| Area | Technology |
|------|------------|
| Framework | .NET 10 |
| UI | Blazor Web App (Interactive Server render mode) |
| ORM | Entity Framework Core 9 |
| Database | MySQL (via Pomelo.EntityFrameworkCore.MySql) |
| Authentication | ASP.NET Core Cookie Authentication + BCrypt password hashing |
| Validation | FluentValidation |
| Testing | xUnit, SQLite In-Memory, Moq, bUnit |

## Architecture

The solution is organised into clear layers to keep the code clean and easy to follow:

- **Domain** – the core entities, interfaces and custom exceptions (no dependencies on anything else).
- **Infrastructure** – the EF Core `DbContext`, the Fluent API configurations, the database seeder and the repositories.
- **Application** – the services that hold the business logic, and the FluentValidation validators.
- **Presentation** – the Blazor components/pages, the MVC `AuthController` and the view models.

## Key OOP Features Demonstrated

- **Inheritance** – every entity inherits from an abstract `BaseEntity`; the `Activity` class is abstract with `Workshop`, `Game` and `Talk` subclasses.
- **Polymorphism** – the `ICancelable` interface is implemented by both `Event` and `Registration`; each `Activity` subclass overrides `GetActivityDetails()`.
- **Encapsulation** – entities use private backing fields exposed as `IReadOnlyCollection`, with business rules kept inside the entities.
- **Method overloading** – `EventService.GetEventsAsync()` has several overloads for different filters.

## Getting Started

1. Make sure MySQL is running (this project was developed against XAMPP MySQL).
2. Check the connection string in `CommunityEventManagement/appsettings.json`.
3. Apply the database migrations and run the app:
   ```bash
   dotnet run --project CommunityEventManagement
   ```
4. The database is seeded automatically on first run with sample data and two demo accounts.

## Demo accounts

| Role | Email | Password |
|------|-------|----------|
| Admin | `admin@events.com` | `Admin123!` |
| User | `user@events.com` | `User123!` |

New visitors can also create their own account from the **Create an account** link on the login page.

## Running the Tests

```bash
dotnet test
```

## Author

Sagar Thapa — University of Sunderland.
