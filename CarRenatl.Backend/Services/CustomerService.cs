using CarRental.Backend.Data;
using CarRental.Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Backend.Services
{
    public class CustomerService
    {
        private readonly AppDbContext _context;

        public CustomerService(AppDbContext context)
        {
            _context = context;
        }

        public void AddCustomer(Customer customer)
        {
            _context.Customers.Add(customer);
            _context.SaveChanges();
        }

        public void UpdateCustomer(Customer customer)
        {
            _context.Customers.Update(customer);
            _context.SaveChanges();
        }

        public void DeleteCustomer(int customerId)
        {
            Customer customer = _context.Customers.FirstOrDefault(c => c.CustomerId == customerId);

            if (customer != null)
            {
                _context.Customers.Remove(customer);
                _context.SaveChanges();
            }
        }

        public List<Reservation> GetCustomerReservations(int customerId)
        {
            return _context.Reservations
                .Where(r => r.CustomerId == customerId)
                .ToList();
        }
    }
}
