# Bin Buddies - Trash Bin Management System

A comprehensive Blazor Server .NET 8 web application for managing trash bin pickup and delivery services. This application was converted from a PHP web application and provides an efficient way to track customers, manage weekly todo lists, and log service events.

## Features

### 🏠 Dashboard
- Real-time statistics and overview
- Today's priority tasks
- Quick access to main functionality
- Mobile-optimized interface

### 📋 Todo List Management
- Weekly view of scheduled trash bin events
- Filter by account representative using `?ar=<rep_id>`
- Mark events as completed (Take Out/Bring In)
- Real-time updates and progress tracking
- Mobile-friendly card layout

### 👥 Customer Management
- Complete customer database with contact information
- Address and contact details management
- Account representative assignments
- Search and filter capabilities
- Responsive table/card views

### 📅 Event Management
- Schedule trash bin pickup and delivery events
- Track event completion status
- Historical event logging
- Account representative task assignment
- Mobile-optimized event cards

### 📱 Mobile Experience
- QR code landing page at `/mobile`
- Touch-friendly interface with 44px minimum touch targets
- Responsive design for all screen sizes
- Mobile-first navigation
- Quick access to priority tasks
- Optimized for service technicians on-the-go

## Technology Stack

- **Framework**: ASP.NET Core Blazor Server (.NET 8)
- **Database**: Entity Framework Core with SQL Server
- **UI Framework**: Bootstrap 4
- **Icons**: Open Iconic
- **Architecture**: Clean Architecture with Dependency Injection

## Project Structure

```
BinBuddies/
├── Data/
│   └── ApplicationDbContext.cs     # EF Core database context
├── Models/
│   ├── Contact.cs                  # Customer contact entity
│   ├── Customer.cs                 # Customer service details
│   ├── AccountRepresentative.cs    # Service representative entity
│   └── EventLog.cs                 # Event tracking entity
├── Services/
│   ├── ICustomerService.cs         # Customer service interface
│   ├── CustomerService.cs          # Customer business logic
│   ├── ITodoService.cs             # Todo service interface
│   └── TodoService.cs              # Todo business logic
├── Pages/
│   ├── Index.razor                 # Home dashboard
│   ├── Todo.razor                  # Todo list management
│   ├── Customers.razor             # Customer management
│   └── Events.razor                # Event management (planned)
└── Shared/
    ├── MainLayout.razor            # Main application layout
    └── NavMenu.razor               # Navigation menu
```

## Database Schema

### Tables
- **contacts**: Customer contact information (name, address, email, phone)
- **customers**: Customer service details (trash day, account representative)
- **account_representatives**: Service representative information
- **events_log**: Event tracking (pickup/delivery tasks and completion status)

### Relationships
- One-to-One: Contact ↔ Customer
- One-to-Many: AccountRepresentative → Customer
- One-to-Many: Contact → EventLog
- One-to-Many: AccountRepresentative → EventLog

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server or SQL Server LocalDB
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd bin-buddies
   ```

2. **Install dependencies**
   ```bash
   dotnet restore
   ```

3. **Update database connection**
   - Modify `appsettings.json` with your SQL Server connection string
   - Default uses SQL Server LocalDB

4. **Create and migrate database**
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access the application**
   - Open browser to `https://localhost:5001` or `http://localhost:5000`

## Usage

### Todo List Management
1. Navigate to **Todo List** from the main menu
2. View weekly scheduled events
3. Use checkboxes to mark tasks as completed
4. Filter by account representative using `?ar=<rep_id>` query parameter
5. Submit completed tasks to update the database

### Customer Management
1. Navigate to **Customers** from the main menu
2. View all customers in a searchable table
3. Add new customers (feature to be implemented)
4. Edit existing customer information
5. Delete customers with confirmation

### Account Representatives
- Representatives are linked to customers and events
- Filter todo lists by representative
- Track completed tasks per representative

## Development

### Adding New Features
1. **Models**: Add new entity classes in `Models/` directory
2. **Services**: Create service interfaces and implementations
3. **Pages**: Add Blazor components in `Pages/` directory
4. **Database**: Update `ApplicationDbContext` and run migrations

### Database Migrations
```bash
# Add new migration
dotnet ef migrations add <MigrationName>

# Update database
dotnet ef database update

# Remove last migration (if not applied)
dotnet ef migrations remove
```

### Running in Development
```bash
# Run with hot reload
dotnet watch run

# Run with specific environment
dotnet run --environment Development
```

## Configuration

### Connection Strings
Update `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BinBuddiesDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### Environment Variables
- `ASPNETCORE_ENVIRONMENT`: Set to `Development`, `Staging`, or `Production`
- `ConnectionStrings__DefaultConnection`: Override connection string

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

For support, email [support@example.com] or create an issue in the repository.

## Changelog

### Version 1.0.0
- ✅ Complete conversion from PHP to Blazor Server (.NET 8)
- ✅ Customer management functionality with responsive design
- ✅ Todo list with weekly view and filtering
- ✅ Event tracking system with completion status
- ✅ Account representative management
- ✅ Mobile-optimized interface with QR code landing page
- ✅ Touch-friendly UI components (44px minimum touch targets)
- ✅ Real-time data updates with Entity Framework Core
- ✅ Sample data seeding for immediate testing
- ✅ Bootstrap responsive design with custom mobile CSS
- ✅ Accessibility improvements and high contrast mode support
