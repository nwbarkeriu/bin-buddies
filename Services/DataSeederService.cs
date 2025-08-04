using BinBuddies.Data;
using BinBuddies.Models;
using Microsoft.EntityFrameworkCore;

namespace BinBuddies.Services
{
    public class DataSeederService
    {
        private readonly ApplicationDbContext _context;

        public DataSeederService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SeedDataAsync()
        {
            // Check if data already exists
            if (await _context.Contacts.AnyAsync())
                return;

            // Create Account Representatives
            var accountReps = new List<AccountRepresentative>
            {
                new AccountRepresentative
                {
                    Name = "John Smith",
                    Email = "john.smith@binbuddies.com",
                    Phone = "(555) 123-4567",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                new AccountRepresentative
                {
                    Name = "Sarah Johnson",
                    Email = "sarah.johnson@binbuddies.com",
                    Phone = "(555) 234-5678",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                new AccountRepresentative
                {
                    Name = "Mike Davis",
                    Email = "mike.davis@binbuddies.com",
                    Phone = "(555) 345-6789",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }
            };

            _context.AccountRepresentatives.AddRange(accountReps);
            await _context.SaveChangesAsync();

            // Create Contacts and Customers
            var contactsAndCustomers = new List<(Contact Contact, Customer Customer)>
            {
                (new Contact
                {
                    FullName = "Emily Wilson",
                    Address = "123 Oak Street, Springfield",
                    Email = "emily.wilson@email.com",
                    Phone = "(555) 111-2222",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }, new Customer
                {
                    TrashDay = "Monday",
                    AccountRepresentative = "John Smith",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }),
                (new Contact
                {
                    FullName = "Robert Brown",
                    Address = "456 Pine Avenue, Springfield",
                    Email = "robert.brown@email.com",
                    Phone = "(555) 222-3333",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }, new Customer
                {
                    TrashDay = "Tuesday",
                    AccountRepresentative = "Sarah Johnson",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }),
                (new Contact
                {
                    FullName = "Lisa Garcia",
                    Address = "789 Elm Road, Springfield",
                    Email = "lisa.garcia@email.com",
                    Phone = "(555) 333-4444",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }, new Customer
                {
                    TrashDay = "Wednesday",
                    AccountRepresentative = "Mike Davis",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }),
                (new Contact
                {
                    FullName = "David Martinez",
                    Address = "321 Maple Drive, Springfield",
                    Email = "david.martinez@email.com",
                    Phone = "(555) 444-5555",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }, new Customer
                {
                    TrashDay = "Thursday",
                    AccountRepresentative = "John Smith",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }),
                (new Contact
                {
                    FullName = "Jennifer Lee",
                    Address = "654 Cedar Lane, Springfield",
                    Email = "jennifer.lee@email.com",
                    Phone = "(555) 555-6666",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }, new Customer
                {
                    TrashDay = "Friday",
                    AccountRepresentative = "Sarah Johnson",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }),
                (new Contact
                {
                    FullName = "Thomas Anderson",
                    Address = "987 Birch Street, Springfield",
                    Email = "thomas.anderson@email.com",
                    Phone = "(555) 666-7777",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                }, new Customer
                {
                    TrashDay = "Monday",
                    AccountRepresentative = "Mike Davis",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                })
            };

            foreach (var (contact, customer) in contactsAndCustomers)
            {
                _context.Contacts.Add(contact);
                await _context.SaveChangesAsync();
                
                customer.ContactId = contact.Id;
                _context.Customers.Add(customer);
            }

            await _context.SaveChangesAsync();

            // Create Event Logs for the upcoming week
            var contacts = await _context.Contacts.ToListAsync();
            var reps = await _context.AccountRepresentatives.ToListAsync();
            var random = new Random();

            var eventLogs = new List<EventLog>();

            foreach (var contact in contacts)
            {
                // Create 2-4 events per customer for the next 7 days
                var eventCount = random.Next(2, 5);
                for (int i = 0; i < eventCount; i++)
                {
                    var eventDate = DateTime.Today.AddDays(random.Next(0, 7));
                    var eventType = random.Next(2) == 0 ? "Take Out" : "Bring In";
                    var rep = reps[random.Next(reps.Count)];

                    eventLogs.Add(new EventLog
                    {
                        CustomerId = contact.Id,
                        RepId = rep.Id,
                        EventType = eventType,
                        EventDate = eventDate.AddHours(random.Next(6, 18)), // Random time between 6 AM and 6 PM
                        Completed = random.Next(4) == 0, // 25% chance of being completed
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    });
                }
            }

            _context.EventLogs.AddRange(eventLogs);
            await _context.SaveChangesAsync();
        }
    }
}
