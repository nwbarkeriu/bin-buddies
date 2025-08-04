using BinBuddies.Models;

namespace BinBuddies.Services
{
    public interface ITodoService
    {
        Task<List<EventLog>> GetWeeklyEventsAsync(int? repId = null);
        Task<List<EventLog>> GetCompletedTasksAsync(int repId);
        Task<bool> MarkEventCompleteAsync(int eventId);
        Task<bool> CreateEventAsync(EventLog eventLog);
        Task<bool> UpdateEventAsync(EventLog eventLog);
        Task<string?> GetCustomerTrashDayAsync(int customerId);
    }
}
