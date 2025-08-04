using Microsoft.EntityFrameworkCore;
using BinBuddies.Data;
using BinBuddies.Models;

namespace BinBuddies.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Subscription?> GetActiveSubscriptionAsync(string userId)
        {
            return await _context.Subscriptions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == SubscriptionStatus.Active);
        }

        public async Task<List<ServiceRequest>> GetUpcomingServicesAsync(string userId)
        {
            var today = DateTime.UtcNow.Date;
            return await _context.ServiceRequests
                .Include(sr => sr.User)
                .Include(sr => sr.AssignedEmployee)
                .Where(sr => sr.UserId == userId && 
                           sr.ScheduledDate >= today && 
                           sr.Status != ServiceRequestStatus.Completed &&
                           sr.Status != ServiceRequestStatus.Cancelled)
                .OrderBy(sr => sr.ScheduledDate)
                .Take(10)
                .ToListAsync();
        }

        public async Task<int> GetCompletedServicesCountAsync(string userId, int month)
        {
            var startDate = new DateTime(DateTime.UtcNow.Year, month, 1);
            var endDate = startDate.AddMonths(1);
            
            return await _context.ServiceRequests
                .CountAsync(sr => sr.UserId == userId && 
                                sr.Status == ServiceRequestStatus.Completed &&
                                sr.CompletedDate.HasValue &&
                                sr.CompletedDate.Value >= startDate && 
                                sr.CompletedDate.Value < endDate);
        }

        public async Task<Subscription> CreateSubscriptionAsync(string userId, int planId, string billingCycle = "monthly")
        {
            var plan = await GetPlanByIdAsync(planId);
            if (plan == null)
                throw new ArgumentException("Invalid plan ID");

            var subscription = new Subscription
            {
                UserId = userId,
                PlanName = plan.Name,
                MonthlyPrice = billingCycle == "yearly" ? plan.YearlyPrice / 12 : plan.MonthlyPrice,
                Status = SubscriptionStatus.Trial, // Start with 30-day trial
                StartDate = DateTime.UtcNow,
                NextBillingDate = DateTime.UtcNow.AddDays(30), // 30-day free trial
                BillingCycle = billingCycle,
                BinsIncluded = plan.BinsIncluded,
                PickupsPerWeek = plan.PickupsPerWeek
            };

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            return subscription;
        }

        public async Task<bool> CancelSubscriptionAsync(int subscriptionId)
        {
            var subscription = await _context.Subscriptions.FindAsync(subscriptionId);
            if (subscription == null) return false;

            subscription.Status = SubscriptionStatus.Cancelled;
            subscription.EndDate = DateTime.UtcNow;
            subscription.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateSubscriptionAsync(int subscriptionId, int newPlanId)
        {
            var subscription = await _context.Subscriptions.FindAsync(subscriptionId);
            var newPlan = await GetPlanByIdAsync(newPlanId);
            
            if (subscription == null || newPlan == null) return false;

            subscription.PlanName = newPlan.Name;
            subscription.MonthlyPrice = subscription.BillingCycle == "yearly" ? newPlan.YearlyPrice / 12 : newPlan.MonthlyPrice;
            subscription.BinsIncluded = newPlan.BinsIncluded;
            subscription.PickupsPerWeek = newPlan.PickupsPerWeek;
            subscription.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Payment>> GetPaymentHistoryAsync(string userId)
        {
            return await _context.Payments
                .Include(p => p.Subscription)
                .Where(p => p.Subscription.UserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<ServiceRequest> CreateServiceRequestAsync(string userId, string serviceType, DateTime requestedDate)
        {
            var serviceRequest = new ServiceRequest
            {
                UserId = userId,
                ServiceType = serviceType,
                ScheduledDate = requestedDate,
                Status = ServiceRequestStatus.Scheduled,
                CreatedAt = DateTime.UtcNow
            };

            _context.ServiceRequests.Add(serviceRequest);
            await _context.SaveChangesAsync();

            return serviceRequest;
        }

        // Plan Management
        public async Task<List<SubscriptionPlan>> GetAvailablePlansAsync()
        {
            return await _context.SubscriptionPlans
                .Where(p => p.IsActive)
                .OrderBy(p => p.MonthlyPrice)
                .ToListAsync();
        }

        public async Task<SubscriptionPlan?> GetPlanByIdAsync(int planId)
        {
            return await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.Id == planId && p.IsActive);
        }

        public async Task<decimal> CalculatePlanPriceAsync(int planId, string billingCycle, string zipCode)
        {
            var plan = await GetPlanByIdAsync(planId);
            if (plan == null) return 0;

            var basePrice = billingCycle == "yearly" ? plan.YearlyPrice : plan.MonthlyPrice;
            
            // Apply service area multiplier if applicable
            var serviceArea = await GetServiceAreaAsync(zipCode);
            if (serviceArea != null)
            {
                basePrice *= serviceArea.ServiceMultiplier;
            }

            return basePrice;
        }

        // Customer Profile Management
        public async Task<CustomerProfile?> GetCustomerProfileAsync(string userId)
        {
            return await _context.CustomerProfiles
                .Include(cp => cp.User)
                .FirstOrDefaultAsync(cp => cp.UserId == userId);
        }

        public async Task<CustomerProfile> CreateOrUpdateCustomerProfileAsync(string userId, CustomerProfile profile)
        {
            var existingProfile = await GetCustomerProfileAsync(userId);
            
            if (existingProfile != null)
            {
                // Update existing profile
                existingProfile.ServiceAddress = profile.ServiceAddress;
                existingProfile.ServiceCity = profile.ServiceCity;
                existingProfile.ServiceState = profile.ServiceState;
                existingProfile.ServiceZipCode = profile.ServiceZipCode;
                existingProfile.PreferredServiceDay = profile.PreferredServiceDay;
                existingProfile.PreferredServiceTime = profile.PreferredServiceTime;
                existingProfile.SpecialInstructions = profile.SpecialInstructions;
                existingProfile.AllowWeekendService = profile.AllowWeekendService;
                existingProfile.AllowHolidayService = profile.AllowHolidayService;
                existingProfile.NotificationPreference = profile.NotificationPreference;
                existingProfile.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                return existingProfile;
            }
            else
            {
                // Create new profile
                profile.UserId = userId;
                profile.CreatedAt = DateTime.UtcNow;
                profile.UpdatedAt = DateTime.UtcNow;
                
                _context.CustomerProfiles.Add(profile);
                await _context.SaveChangesAsync();
                return profile;
            }
        }

        // Service Area Management
        public async Task<bool> IsServiceAvailableAsync(string zipCode)
        {
            return await _context.ServiceAreas
                .AnyAsync(sa => sa.ZipCode == zipCode && sa.IsActive);
        }

        public async Task<ServiceArea?> GetServiceAreaAsync(string zipCode)
        {
            return await _context.ServiceAreas
                .FirstOrDefaultAsync(sa => sa.ZipCode == zipCode && sa.IsActive);
        }

        // Analytics and Reporting
        public async Task<Dictionary<string, object>> GetCustomerDashboardDataAsync(string userId)
        {
            var subscription = await GetActiveSubscriptionAsync(userId);
            var upcomingServices = await GetUpcomingServicesAsync(userId);
            var completedThisMonth = await GetCompletedServicesCountAsync(userId, DateTime.UtcNow.Month);
            var profile = await GetCustomerProfileAsync(userId);
            
            return new Dictionary<string, object>
            {
                ["subscription"] = subscription,
                ["upcomingServices"] = upcomingServices,
                ["completedServicesThisMonth"] = completedThisMonth,
                ["nextServiceDate"] = upcomingServices.FirstOrDefault()?.ScheduledDate,
                ["customerProfile"] = profile
            };
        }

        public async Task<List<ServiceRequest>> GetServiceHistoryAsync(string userId, int pageSize = 10, int page = 1)
        {
            return await _context.ServiceRequests
                .Include(sr => sr.AssignedEmployee)
                .Where(sr => sr.UserId == userId)
                .OrderByDescending(sr => sr.ScheduledDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // Employee specific methods
        public async Task<List<ServiceRequest>> GetAssignedServicesAsync(string employeeId, DateTime? date = null)
        {
            var targetDate = date ?? DateTime.UtcNow.Date;
            
            return await _context.ServiceRequests
                .Include(sr => sr.User)
                .Where(sr => sr.AssignedEmployeeId == employeeId && 
                           sr.ScheduledDate.HasValue && 
                           sr.ScheduledDate.Value.Date == targetDate &&
                           sr.Status != ServiceRequestStatus.Completed &&
                           sr.Status != ServiceRequestStatus.Cancelled)
                .OrderBy(sr => sr.ScheduledDate)
                .ToListAsync();
        }

        public async Task<bool> CompleteServiceAsync(int serviceRequestId, string notes = "")
        {
            var serviceRequest = await _context.ServiceRequests.FindAsync(serviceRequestId);
            if (serviceRequest == null) return false;

            serviceRequest.Status = ServiceRequestStatus.Completed;
            serviceRequest.CompletedDate = DateTime.UtcNow;
            serviceRequest.Notes = notes;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ServiceRequest>> GetPendingServicesAsync(DateTime? date = null)
        {
            var targetDate = date ?? DateTime.UtcNow.Date;
            
            return await _context.ServiceRequests
                .Include(sr => sr.User)
                .Include(sr => sr.AssignedEmployee)
                .Where(sr => sr.ScheduledDate.HasValue && 
                           sr.ScheduledDate.Value.Date == targetDate &&
                           sr.Status == ServiceRequestStatus.Scheduled)
                .OrderBy(sr => sr.ScheduledDate)
                .ToListAsync();
        }

        // Manager specific methods
        public async Task<Dictionary<string, object>> GetManagerDashboardDataAsync()
        {
            var today = DateTime.UtcNow.Date;
            var thisMonth = DateTime.UtcNow.Month;
            var thisYear = DateTime.UtcNow.Year;
            
            var totalCustomers = await _context.Subscriptions
                .CountAsync(s => s.Status == SubscriptionStatus.Active);
                
            var todaysServices = await _context.ServiceRequests
                .CountAsync(sr => sr.ScheduledDate.HasValue && sr.ScheduledDate.Value.Date == today);
                
            var completedToday = await _context.ServiceRequests
                .CountAsync(sr => sr.CompletedDate.HasValue && 
                                sr.CompletedDate.Value.Date == today);
                                
            var monthlyRevenue = await _context.Subscriptions
                .Where(s => s.Status == SubscriptionStatus.Active)
                .SumAsync(s => s.MonthlyPrice);
                
            return new Dictionary<string, object>
            {
                ["totalActiveCustomers"] = totalCustomers,
                ["todaysServices"] = todaysServices,
                ["completedToday"] = completedToday,
                ["monthlyRevenue"] = monthlyRevenue,
                ["completionRate"] = todaysServices > 0 ? (double)completedToday / todaysServices * 100 : 0
            };
        }

        public async Task<List<Subscription>> GetAllSubscriptionsAsync(int pageSize = 50, int page = 1)
        {
            return await _context.Subscriptions
                .Include(s => s.User)
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<ServiceRequest>> GetAllServiceRequestsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.ServiceRequests
                .Include(sr => sr.User)
                .Include(sr => sr.AssignedEmployee)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(sr => sr.ScheduledDate >= startDate.Value);
                
            if (endDate.HasValue)
                query = query.Where(sr => sr.ScheduledDate <= endDate.Value);

            return await query
                .OrderByDescending(sr => sr.ScheduledDate)
                .ToListAsync();
        }

        public async Task<bool> AssignServiceToEmployeeAsync(int serviceRequestId, string employeeId)
        {
            var serviceRequest = await _context.ServiceRequests.FindAsync(serviceRequestId);
            if (serviceRequest == null) return false;

            serviceRequest.AssignedEmployeeId = employeeId;
            serviceRequest.Status = ServiceRequestStatus.Assigned;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
