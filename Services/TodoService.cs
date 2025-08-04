using Microsoft.EntityFrameworkCore;
using BinBuddies.Data;
using BinBuddies.Models;

namespace BinBuddies.Services
{
    public class TodoService : ITodoService
    {
        private readonly ApplicationDbContext _context;

        public TodoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<EventLog>> GetWeeklyEventsAsync(int? repId = null)
        {
            var query = _context.EventLogs
                .Include(e => e.Customer)
                .Include(e => e.AccountRep)
                .Where(e => e.EventDate >= DateTime.Today &&
                           e.EventDate < DateTime.Today.AddDays(7));

            if (repId.HasValue)
            {
                query = query.Where(e => e.RepId == repId.Value);
            }

            return await query
                .OrderBy(e => e.EventDate)
                .ToListAsync();
        }

        public async Task<List<EventLog>> GetCompletedTasksAsync(int repId)
        {
            return await _context.EventLogs
                .Include(e => e.Customer)
                .Where(e => e.RepId == repId && e.Completed)
                .OrderByDescending(e => e.EventDate)
                .ToListAsync();
        }

        public async Task<bool> MarkEventCompleteAsync(int eventId)
        {
            var eventLog = await _context.EventLogs.FindAsync(eventId);
            if (eventLog == null) return false;

            eventLog.Completed = true;
            eventLog.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CreateEventAsync(EventLog eventLog)
        {
            try
            {
                _context.EventLogs.Add(eventLog);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateEventAsync(EventLog eventLog)
        {
            try
            {
                eventLog.UpdatedAt = DateTime.Now;
                _context.EventLogs.Update(eventLog);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string?> GetCustomerTrashDayAsync(int customerId)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.ContactId == customerId);
            return customer?.TrashDay;
        }
    }
}
