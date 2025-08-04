using Microsoft.EntityFrameworkCore;
using BinBuddies.Data;
using BinBuddies.Models;

namespace BinBuddies.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;

        public CustomerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            return await _context.Customers
                .Include(c => c.Contact)
                .Include(c => c.AccountRep)
                .ToListAsync();
        }

        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            return await _context.Customers
                .Include(c => c.Contact)
                .Include(c => c.AccountRep)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> CreateCustomerAsync(Customer customer)
        {
            try
            {
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateCustomerAsync(Customer customer)
        {
            try
            {
                customer.UpdatedAt = DateTime.Now;
                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null) return false;

                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Customer>> GetCustomersByRepAsync(string repName)
        {
            return await _context.Customers
                .Include(c => c.Contact)
                .Include(c => c.AccountRep)
                .Where(c => c.AccountRepresentative == repName)
                .ToListAsync();
        }
    }
}
