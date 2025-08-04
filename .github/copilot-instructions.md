# Copilot Instructions for Bin Buddies

<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

## Project Overview
This is a Blazor Server .NET 8 application for managing trash bin services. The application was converted from a PHP web application and includes:

- **Customer Management**: Track customer information, addresses, and account representatives
- **Todo List Management**: Weekly view of trash bin pickup/delivery tasks
- **Event Logging**: Track completion of bin service events
- **Account Representative Management**: Manage service representatives

## Architecture
- **Framework**: Blazor Server (.NET 8)
- **Database**: Entity Framework Core with SQL Server
- **UI**: Bootstrap 4 with Open Iconic icons
- **Services**: Dependency injection with scoped services

## Key Components
- `Models/`: Entity classes (Contact, Customer, AccountRepresentative, EventLog)
- `Data/ApplicationDbContext.cs`: EF Core database context
- `Services/`: Business logic interfaces and implementations
- `Pages/`: Blazor components for UI
- `Shared/`: Shared layout components

## Development Guidelines
1. Use Entity Framework Core for all database operations
2. Follow the existing service pattern for business logic
3. Use Bootstrap classes for consistent styling
4. Implement proper error handling with try-catch blocks
5. Use dependency injection for services
6. Follow async/await patterns for database operations

## Database Schema
- **contacts**: Customer contact information
- **customers**: Customer service details and trash day info
- **account_representatives**: Service representative information
- **events_log**: Event tracking (Take Out/Bring In tasks)

## Common Tasks
- Adding new pages: Create in `Pages/` directory with `@page` directive
- Adding services: Create interface in `Services/` and register in `Program.cs`
- Database changes: Update models and run EF migrations
- UI updates: Use Bootstrap classes and Open Iconic icons
