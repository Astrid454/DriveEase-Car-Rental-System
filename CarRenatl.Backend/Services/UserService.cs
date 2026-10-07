using CarRental.Backend.Data;
using CarRental.Backend.Models;
using System.Linq;

namespace CarRental.Backend.Services
{
    public class UserService
    {
        private readonly AppDbContext _context;

        public UserService()
        {
            _context = new AppDbContext();
        }

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public User Login(string email, string password, string role)
        {
            return _context.Users
                .FirstOrDefault(u =>
                    u.Email == email &&
                    u.Password == password &&
                    u.Role == role);
        }


        public User Register(User user, string phone)
        {
            bool emailExists = _context.Users.Any(u => u.Email == user.Email);

            if (emailExists)
            {
                throw new Exception("Email already exists.");
            }

            _context.Users.Add(user);
            _context.SaveChanges();

            if (user.Role == "Customer")
            {
                Customer customer = new Customer
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = phone
                };

                _context.Customers.Add(customer);
            }
            else if (user.Role == "CarRenter")
            {
                CarRenter carRenter = new CarRenter
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = phone
                };

                _context.CarRenters.Add(carRenter);
            }

            _context.SaveChanges();

            return user;
        }

        public bool UpdateProfile(
    int userId,
    string firstName,
    string lastName,
    string email,
    string phone)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);

            if (user == null)
                return false;

            string oldEmail = user.Email;

            user.FirstName = firstName;
            user.LastName = lastName;
            user.Email = email;

            if (user.Role == "Customer")
            {
                var customer = _context.Customers.FirstOrDefault(c => c.Email == oldEmail);

                if (customer != null)
                {
                    customer.FirstName = firstName;
                    customer.LastName = lastName;
                    customer.Email = email;
                    customer.Phone = phone;
                }
            }

            if (user.Role == "CarRenter")
            {
                var carRenter = _context.CarRenters.FirstOrDefault(c => c.Email == oldEmail);

                if (carRenter != null)
                {
                    carRenter.FirstName = firstName;
                    carRenter.LastName = lastName;
                    carRenter.Email = email;
                    carRenter.Phone = phone;
                }
            }

            _context.SaveChanges();

            return true;
        }

        public bool ChangePassword(
            int userId,
            string currentPassword,
            string newPassword)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);

            if (user == null)
                return false;

            if (user.Password != currentPassword)
                return false;

            user.Password = newPassword;

            _context.SaveChanges();

            return true;
        }

        public bool ResetPassword(string email, string newPassword, string role)
        {
            var user = _context.Users.FirstOrDefault(u =>
                u.Email == email &&
                u.Role == role);

            if (user == null)
                return false;

            user.Password = newPassword;

            _context.SaveChanges();

            return true;
        }
    }
}