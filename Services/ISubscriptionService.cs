using BinBuddies.Models;

namespace BinBuddies.Services
{
    public interface ISubscriptionService
    {
        // Subscription Management
        Task<Subscription?> GetActiveSubscriptionAsync(string userId);
        Task<List<ServiceRequest>> GetUpcomingServicesAsync(string userId);
        Task<int> GetCompletedServicesCountAsync(string userId, int month);
        Task<Subscription> CreateSubscriptionAsync(string userId, int planId, string billingCycle = "monthly");
        Task<bool> CancelSubscriptionAsync(int subscriptionId);
        Task<bool> UpdateSubscriptionAsync(int subscriptionId, int newPlanId);
        Task<List<Payment>> GetPaymentHistoryAsync(string userId);
        Task<ServiceRequest> CreateServiceRequestAsync(string userId, string serviceType, DateTime requestedDate);
        
        // Plan Management
        Task<List<SubscriptionPlan>> GetAvailablePlansAsync();
        Task<SubscriptionPlan?> GetPlanByIdAsync(int planId);
        Task<decimal> CalculatePlanPriceAsync(int planId, string billingCycle, string zipCode);
        
        // Customer Profile Management
        Task<CustomerProfile?> GetCustomerProfileAsync(string userId);
        Task<CustomerProfile> CreateOrUpdateCustomerProfileAsync(string userId, CustomerProfile profile);
        
        // Service Area Management
        Task<bool> IsServiceAvailableAsync(string zipCode);
        Task<ServiceArea?> GetServiceAreaAsync(string zipCode);
        
        // Analytics and Reporting
        Task<Dictionary<string, object>> GetCustomerDashboardDataAsync(string userId);
        Task<List<ServiceRequest>> GetServiceHistoryAsync(string userId, int pageSize = 10, int page = 1);
        
        // Employee specific methods
        Task<List<ServiceRequest>> GetAssignedServicesAsync(string employeeId, DateTime? date = null);
        Task<bool> CompleteServiceAsync(int serviceRequestId, string notes = "");
        Task<List<ServiceRequest>> GetPendingServicesAsync(DateTime? date = null);
        
        // Manager specific methods
        Task<Dictionary<string, object>> GetManagerDashboardDataAsync();
        Task<List<Subscription>> GetAllSubscriptionsAsync(int pageSize = 50, int page = 1);
        Task<List<ServiceRequest>> GetAllServiceRequestsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<bool> AssignServiceToEmployeeAsync(int serviceRequestId, string employeeId);
    }
}
