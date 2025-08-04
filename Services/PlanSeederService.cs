using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BinBuddies.Data;
using BinBuddies.Models;

namespace BinBuddies.Services
{
    public class PlanSeederService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PlanSeederService> _logger;

        public PlanSeederService(ApplicationDbContext context, ILogger<PlanSeederService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedSubscriptionPlansAsync()
        {
            try
            {
                // Check if plans already exist
                if (await _context.SubscriptionPlans.AnyAsync())
                {
                    _logger.LogInformation("Subscription plans already exist, skipping seeding.");
                    return;
                }

                var plans = new List<SubscriptionPlan>
                {
                    new SubscriptionPlan
                    {
                        Name = "Basic Plan",
                        Description = "Perfect for small homes and apartments",
                        MonthlyPrice = 29.00m,
                        YearlyPrice = 299.00m, // ~17% discount
                        BinsIncluded = 1,
                        PickupsPerWeek = 1,
                        IncludesHolidayAdjustment = true,
                        IncludesGPSTracking = false,
                        IncludesSMSNotifications = true,
                        IncludesEmailNotifications = true,
                        IncludesMobileApp = true,
                        IncludesPrioritySupport = false,
                        IncludesAccountManager = false,
                        IncludesServiceReports = false,
                        SupportLevel = "Email",
                        IsActive = true,
                        IsPopular = false,
                        BadgeColor = "#007bff"
                    },
                    new SubscriptionPlan
                    {
                        Name = "Premium Plan",
                        Description = "Great for busy families and larger homes",
                        MonthlyPrice = 49.00m,
                        YearlyPrice = 499.00m, // ~15% discount
                        BinsIncluded = 2,
                        PickupsPerWeek = 2,
                        IncludesHolidayAdjustment = true,
                        IncludesGPSTracking = true,
                        IncludesSMSNotifications = true,
                        IncludesEmailNotifications = true,
                        IncludesMobileApp = true,
                        IncludesPrioritySupport = true,
                        IncludesAccountManager = false,
                        IncludesServiceReports = true,
                        SupportLevel = "Priority",
                        IsActive = true,
                        IsPopular = true,
                        BadgeColor = "#28a745"
                    },
                    new SubscriptionPlan
                    {
                        Name = "Business Plan",
                        Description = "Designed for commercial properties and businesses",
                        MonthlyPrice = 99.00m,
                        YearlyPrice = 1099.00m, // ~8% discount
                        BinsIncluded = 5,
                        PickupsPerWeek = 0, // Custom schedule
                        IncludesHolidayAdjustment = true,
                        IncludesGPSTracking = true,
                        IncludesSMSNotifications = true,
                        IncludesEmailNotifications = true,
                        IncludesMobileApp = true,
                        IncludesPrioritySupport = true,
                        IncludesAccountManager = true,
                        IncludesServiceReports = true,
                        SupportLevel = "24/7",
                        IsActive = true,
                        IsPopular = false,
                        IsEnterprise = true,
                        BadgeColor = "#17a2b8"
                    }
                };

                _context.SubscriptionPlans.AddRange(plans);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Successfully seeded {plans.Count} subscription plans.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding subscription plans");
                throw;
            }
        }

        public async Task SeedServiceAreasAsync()
        {
            try
            {
                // Check if service areas already exist
                if (await _context.ServiceAreas.AnyAsync())
                {
                    _logger.LogInformation("Service areas already exist, skipping seeding.");
                    return;
                }

                var serviceAreas = new List<ServiceArea>
                {
                    // Dallas Metro Area
                    new ServiceArea { Name = "Dallas Central", City = "Dallas", State = "TX", ZipCode = "75201", ServiceMultiplier = 1.0m },
                    new ServiceArea { Name = "Dallas North", City = "Dallas", State = "TX", ZipCode = "75202", ServiceMultiplier = 1.0m },
                    new ServiceArea { Name = "Dallas East", City = "Dallas", State = "TX", ZipCode = "75203", ServiceMultiplier = 1.0m },
                    new ServiceArea { Name = "Dallas West", City = "Dallas", State = "TX", ZipCode = "75204", ServiceMultiplier = 1.0m },
                    new ServiceArea { Name = "Dallas South", City = "Dallas", State = "TX", ZipCode = "75205", ServiceMultiplier = 1.0m },

                    // Fort Worth Area
                    new ServiceArea { Name = "Fort Worth Central", City = "Fort Worth", State = "TX", ZipCode = "76101", ServiceMultiplier = 1.1m },
                    new ServiceArea { Name = "Fort Worth North", City = "Fort Worth", State = "TX", ZipCode = "76102", ServiceMultiplier = 1.1m },
                    new ServiceArea { Name = "Fort Worth South", City = "Fort Worth", State = "TX", ZipCode = "76103", ServiceMultiplier = 1.1m },

                    // Austin Area
                    new ServiceArea { Name = "Austin Central", City = "Austin", State = "TX", ZipCode = "78701", ServiceMultiplier = 1.2m },
                    new ServiceArea { Name = "Austin North", City = "Austin", State = "TX", ZipCode = "78702", ServiceMultiplier = 1.2m },
                    new ServiceArea { Name = "Austin South", City = "Austin", State = "TX", ZipCode = "78703", ServiceMultiplier = 1.2m },

                    // Houston Area
                    new ServiceArea { Name = "Houston Central", City = "Houston", State = "TX", ZipCode = "77001", ServiceMultiplier = 1.15m },
                    new ServiceArea { Name = "Houston North", City = "Houston", State = "TX", ZipCode = "77002", ServiceMultiplier = 1.15m },
                    new ServiceArea { Name = "Houston South", City = "Houston", State = "TX", ZipCode = "77003", ServiceMultiplier = 1.15m },

                    // San Antonio Area
                    new ServiceArea { Name = "San Antonio Central", City = "San Antonio", State = "TX", ZipCode = "78201", ServiceMultiplier = 1.05m },
                    new ServiceArea { Name = "San Antonio North", City = "San Antonio", State = "TX", ZipCode = "78202", ServiceMultiplier = 1.05m },

                    // Plano Area
                    new ServiceArea { Name = "Plano Central", City = "Plano", State = "TX", ZipCode = "75023", ServiceMultiplier = 1.1m },
                    new ServiceArea { Name = "Plano East", City = "Plano", State = "TX", ZipCode = "75024", ServiceMultiplier = 1.1m },
                    new ServiceArea { Name = "Plano West", City = "Plano", State = "TX", ZipCode = "75025", ServiceMultiplier = 1.1m },

                    // Additional suburbs
                    new ServiceArea { Name = "Irving", City = "Irving", State = "TX", ZipCode = "75061", ServiceMultiplier = 1.05m },
                    new ServiceArea { Name = "Garland", City = "Garland", State = "TX", ZipCode = "75040", ServiceMultiplier = 1.05m },
                    new ServiceArea { Name = "Richardson", City = "Richardson", State = "TX", ZipCode = "75080", ServiceMultiplier = 1.1m },
                    new ServiceArea { Name = "Mesquite", City = "Mesquite", State = "TX", ZipCode = "75149", ServiceMultiplier = 1.0m },
                    new ServiceArea { Name = "Arlington", City = "Arlington", State = "TX", ZipCode = "76010", ServiceMultiplier = 1.05m },
                    new ServiceArea { Name = "Grand Prairie", City = "Grand Prairie", State = "TX", ZipCode = "75050", ServiceMultiplier = 1.0m }
                };

                _context.ServiceAreas.AddRange(serviceAreas);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Successfully seeded {serviceAreas.Count} service areas.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding service areas");
                throw;
            }
        }

        public async Task SeedTestDataAsync()
        {
            try
            {
                // Create sample customer profiles for existing users
                var customers = await _context.Users
                    .Where(u => u.Email != "admin@binbuddies.com")
                    .Take(10)
                    .ToListAsync();

                foreach (var customer in customers)
                {
                    var existingProfile = await _context.CustomerProfiles
                        .FirstOrDefaultAsync(cp => cp.UserId == customer.Id);

                    if (existingProfile == null)
                    {
                        var profile = new CustomerProfile
                        {
                            UserId = customer.Id,
                            ServiceAddress = customer.Address ?? "123 Sample St",
                            ServiceCity = customer.City ?? "Dallas",
                            ServiceState = customer.State ?? "TX",
                            ServiceZipCode = customer.ZipCode ?? "75201",
                            PreferredServiceDay = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" }[Random.Shared.Next(5)],
                            PreferredServiceTime = new[] { "Morning", "Afternoon", "Evening" }[Random.Shared.Next(3)],
                            AllowWeekendService = Random.Shared.Next(2) == 1,
                            AllowHolidayService = true,
                            NotificationPreference = "Both",
                            SpecialInstructions = Random.Shared.Next(2) == 1 ? "Please place bins at the curb before 7 AM" : null
                        };

                        _context.CustomerProfiles.Add(profile);
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully seeded test customer profiles.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding test data");
                throw;
            }
        }
    }
}
