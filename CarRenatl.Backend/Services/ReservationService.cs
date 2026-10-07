using CarRental.Backend.Data;
using CarRental.Backend.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CarRental.Backend.Services
{
    public class ReservationService
    {
        private readonly AppDbContext _context;
        private const decimal COMPANY_FEE_PERCENT = 0.15m;

        public ReservationService(AppDbContext context)
        {
            _context = context;
        }

        public bool CreateReservation(
            int carId,
            string customerEmail,
            DateTime startDate,
            DateTime endDate,
            string pickupLocation)
        {
            var car = _context.Cars.FirstOrDefault(c => c.CarId == carId);

            if (car == null)
                return false;

            var customer = GetCustomerByEmail(customerEmail);

            if (customer == null)
                return false;

            if (!CheckAvailability(carId, startDate, endDate))
                return false;

            decimal totalCost = CalculateCost(carId, startDate, endDate);

            var reservation = new Reservation
            {
                CarId = carId,
                CustomerId = customer.CustomerId,
                StartDate = startDate,
                EndDate = endDate,
                PickupLocation = pickupLocation,
                TotalCost = totalCost,
                Status = ReservationStatus.Pending
            };

            _context.Reservations.Add(reservation);
            _context.SaveChanges();

            if (car.UserId.HasValue)
            {
                new NotificationService(_context).CreateNotification(
                    car.UserId.Value,
                    "Your car has a new reservation request",
                    $"Your {car.Brand} {car.Model} was requested by a customer. Please review the booking request.",
                    "Reservation");
            }

            return true;
        }

        public int CreateReservationAndReturnId(
            int carId,
            string customerEmail,
            DateTime startDate,
            DateTime endDate,
            string pickupLocation)
        {
            var car = _context.Cars.FirstOrDefault(c => c.CarId == carId);

            if (car == null)
                return 0;

            var customer = GetCustomerByEmail(customerEmail);

            if (customer == null)
                return 0;

            if (!CheckAvailability(carId, startDate, endDate))
                return 0;

            decimal totalCost = CalculateCost(carId, startDate, endDate);

            var reservation = new Reservation
            {
                CarId = carId,
                CustomerId = customer.CustomerId,
                StartDate = startDate,
                EndDate = endDate,
                PickupLocation = pickupLocation,
                TotalCost = totalCost,
                Status = ReservationStatus.Pending
            };

            _context.Reservations.Add(reservation);
            _context.SaveChanges();

            if (car.UserId.HasValue)
            {
                new NotificationService(_context).CreateNotification(
                    car.UserId.Value,
                    "Your car has a new reservation request",
                    $"Your {car.Brand} {car.Model} was requested by a customer. Please review the booking request.",
                    "Reservation");
            }

            return reservation.ReservationId;
        }

        public List<Reservation> GetCustomerReservations(string customerEmail)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.Email == customerEmail);

            if (customer == null)
                return new List<Reservation>();

            return _context.Reservations
                .Include(r => r.Car)
                .Where(r => r.CustomerId == customer.CustomerId)
                .OrderByDescending(r => r.StartDate)
                .ToList();
        }

        public Reservation GetUpcomingReservation(string customerEmail)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.Email == customerEmail);

            if (customer == null)
                return null;

            return _context.Reservations
                .Include(r => r.Car)
                .Where(r =>
                    r.CustomerId == customer.CustomerId &&
                    r.Status != ReservationStatus.Cancelled &&
                    r.EndDate >= DateTime.Now)
                .OrderBy(r => r.StartDate)
                .FirstOrDefault();
        }

        public List<string> GetUnavailablePeriods(int carId)
        {
            return _context.Reservations
                .Where(r =>
                    r.CarId == carId &&
                    r.Status != ReservationStatus.Cancelled &&
                    r.EndDate >= DateTime.Now)
                .OrderBy(r => r.StartDate)
                .Select(r =>
                    $"{r.StartDate:dd MMM yyyy, HH:mm} → {r.EndDate:dd MMM yyyy, HH:mm}")
                .ToList();
        }

        public int GetActiveReservationsCount(string customerEmail)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.Email == customerEmail);

            if (customer == null)
                return 0;

            return _context.Reservations.Count(r =>
                r.CustomerId == customer.CustomerId &&
                r.Status != ReservationStatus.Cancelled &&
                r.EndDate >= DateTime.Now);
        }

        public Reservation GetLatestReservation(string customerEmail)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.Email == customerEmail);

            if (customer == null)
                return null;

            return _context.Reservations
                .Include(r => r.Car)
                .Where(r => r.CustomerId == customer.CustomerId)
                .OrderByDescending(r => r.ReservationId)
                .FirstOrDefault();
        }

        public Customer GetCustomerByEmail(string email)
        {
            return _context.Customers
                .FirstOrDefault(c => c.Email == email);
        }

        public Reservation GetReservationById(int reservationId)
        {
            return _context.Reservations
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .FirstOrDefault(r => r.ReservationId == reservationId);
        }

        public void CancelReservation(int reservationId)
        {
            var reservation = _context.Reservations
                .Include(r => r.Car)
                .FirstOrDefault(r => r.ReservationId == reservationId);

            if (reservation == null)
                return;

            reservation.Status = ReservationStatus.Cancelled;
            reservation.Car.Status = CarStatus.Available;

            var payment = _context.Payments
                .FirstOrDefault(p => p.ReservationId == reservation.ReservationId);

            if (payment != null && payment.MethodOfPayment == "Card")
            {
                payment.Status = "Refunded";
                payment.RefundedAt = DateTime.Now;
            }

            var notificationService = new NotificationService(_context);

            if (reservation.Car.UserId.HasValue)
            {
                notificationService.CreateNotification(
                    reservation.Car.UserId.Value,
                    "Reservation cancelled",
                    $"A reservation for your {reservation.Car.Brand} {reservation.Car.Model} was cancelled.",
                    "Reservation");
            }

            var customerUser = _context.Users
                .FirstOrDefault(u =>
                u.Email == reservation.Customer.Email &&
                u.Role == "Customer");

            if (customerUser != null)
            {
                string customerMessage =
                    payment != null && payment.MethodOfPayment == "Card"
                    ? $"Your reservation for {reservation.Car.Brand} {reservation.Car.Model} was cancelled and your payment has been refunded."
                    : $"Your reservation for {reservation.Car.Brand} {reservation.Car.Model} was cancelled.";

                notificationService.CreateNotification(
                    customerUser.UserId,
                    "Reservation cancelled",
                    customerMessage,
                    "Reservation");
            }

            _context.SaveChanges();
        }

        public bool CheckAvailability(int carId, DateTime startDate, DateTime endDate)
        {
            bool exists = _context.Reservations.Any(r =>
                r.CarId == carId &&
                r.Status != ReservationStatus.Cancelled &&
                startDate < r.EndDate &&
                endDate > r.StartDate);

            return !exists;
        }

        public bool ValidatePeriod(DateTime startDate, DateTime endDate)
        {
            return startDate >= DateTime.Now && endDate > startDate;
        }

        public bool ValidateWorkingHours(DateTime startDate, DateTime endDate)
        {
            return startDate.Hour >= 8 &&
                   startDate.Hour <= 20 &&
                   endDate.Hour >= 8 &&
                   endDate.Hour <= 20;
        }


        public void MarkAsPickedUp(int reservationId)
        {
            var reservation = _context.Reservations
                .Include(r => r.Car)
                .FirstOrDefault(r => r.ReservationId == reservationId);

            if (reservation == null)
                return;

            if (reservation.Status != ReservationStatus.Confirmed)
                return;

            DateTime pickupStartTime = reservation.StartDate.AddMinutes(-15);
            DateTime pickupDeadline = reservation.StartDate.AddHours(1);
            DateTime now = DateTime.Now;

            if (now < pickupStartTime || now > pickupDeadline)
                return;

            reservation.Status = ReservationStatus.PickedUp;
            reservation.PickedUpAt = DateTime.Now;

            if (reservation.Car.UserId.HasValue)
            {
                var notificationService = new NotificationService(_context);

                notificationService.CreateNotification(
                    reservation.Car.UserId.Value,
                    "Vehicle picked up",
                    $"{reservation.Car.Brand} {reservation.Car.Model} was picked up by the customer.",
                    "Vehicle");
            }

            _context.SaveChanges();
        }

        public void MarkAsReturned(int reservationId)
        {
            var reservation = _context.Reservations
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .FirstOrDefault(r => r.ReservationId == reservationId);

            if (reservation == null)
                return;

            if (reservation.Status != ReservationStatus.PickedUp)
                return;

            reservation.Status = ReservationStatus.Returned;
            reservation.ReturnedAt = DateTime.Now;

            if (reservation.Car.UserId.HasValue)
            {
                var notificationService = new NotificationService(_context);

                notificationService.CreateNotification(
                    reservation.Car.UserId.Value,
                    "Vehicle returned",
                    $"{reservation.Car.Brand} {reservation.Car.Model} was returned to the DriveEase garage.",
                    "Vehicle");


                var admins = _context.Users
                            .Where(u => u.Role == "Admin")
                            .ToList();

                foreach (var admin in admins)
                {
                    notificationService.CreateNotification(
                        admin.UserId,
                        "Vehicle return pending",
                        $"{reservation.Car.Brand} {reservation.Car.Model} is waiting for admin confirmation.",
                        "Return");
                }
            }


            if (reservation.ReturnedAt > reservation.EndDate)
            {
                double lateHours =
                    Math.Ceiling((reservation.ReturnedAt.Value - reservation.EndDate).TotalHours);

                reservation.LateFee = (decimal)lateHours * 10m;

                var notificationService = new NotificationService(_context);

                var customerUser = _context.Users
                    .FirstOrDefault(u =>
                        u.Email == reservation.Customer.Email &&
                        u.Role == "Customer");

                if (customerUser != null)
                {
                    notificationService.CreateNotification(
                        customerUser.UserId,
                        "Late fee applied",
                        $"A late fee of €{reservation.LateFee} was applied because {reservation.Car.Brand} {reservation.Car.Model} was returned late.",
                        "Payment");
                }

                if (reservation.Car.UserId.HasValue)
                {
                    notificationService.CreateNotification(
                        reservation.Car.UserId.Value,
                        "Late return reported",
                        $"{reservation.Car.Brand} {reservation.Car.Model} was returned late by the customer.",
                        "Vehicle");
                }
            }

            _context.SaveChanges();
        }


        public void CancelExpiredPickups()
        {
            var expiredReservations = _context.Reservations
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .Where(r =>
                    r.Status == ReservationStatus.Confirmed &&
                    DateTime.Now > r.StartDate.AddHours(1))
                .ToList();

            var admins = _context.Users
                .Where(u => u.Role == "Admin")
                .ToList();

            var notificationService = new NotificationService(_context);

            foreach (var reservation in expiredReservations)
            {
                reservation.Status = ReservationStatus.Cancelled;
                reservation.Car.Status = CarStatus.Available;

                var payment = _context.Payments
                    .FirstOrDefault(p => p.ReservationId == reservation.ReservationId);

                bool wasRefunded = false;

                if (payment != null && payment.MethodOfPayment == "Card")
                {
                    payment.Status = "Refunded";
                    payment.RefundedAt = DateTime.Now;
                    wasRefunded = true;
                }

                var customerUser = _context.Users
                    .FirstOrDefault(u =>
                        u.Email == reservation.Customer.Email &&
                        u.Role == "Customer");

                string customerMessage = wasRefunded
                    ? $"Your reservation for {reservation.Car.Brand} {reservation.Car.Model} was cancelled because the car was not picked up on time. Your card payment has been refunded."
                    : $"Your reservation for {reservation.Car.Brand} {reservation.Car.Model} was cancelled because the car was not picked up on time.";

                if (customerUser != null && !NotificationExists(customerUser.UserId, "Reservation cancelled", customerMessage))
                {
                    notificationService.CreateNotification(
                        customerUser.UserId,
                        "Reservation cancelled",
                        customerMessage,
                        "Reservation");
                }

                if (reservation.Car.UserId.HasValue)
                {
                    string renterMessage =
                        $"The reservation for your {reservation.Car.Brand} {reservation.Car.Model} was cancelled because the customer did not pick up the car on time.";

                    if (!NotificationExists(reservation.Car.UserId.Value, "Reservation cancelled", renterMessage))
                    {
                        notificationService.CreateNotification(
                            reservation.Car.UserId.Value,
                            "Reservation cancelled",
                            renterMessage,
                            "Reservation");
                    }
                }

                foreach (var admin in admins)
                {
                    string adminMessage =
                        $"{reservation.Car.Brand} {reservation.Car.Model} reservation was cancelled because the customer did not pick up the car.";

                    if (!NotificationExists(admin.UserId, "Reservation cancelled", adminMessage))
                    {
                        notificationService.CreateNotification(
                            admin.UserId,
                            "Reservation cancelled",
                            adminMessage,
                            "Reservation");
                    }
                }
            }

            _context.SaveChanges();
        }


        private bool NotificationExists(int userId, string title, string message)
        {
            return _context.Notifications.Any(n =>
                n.UserId == userId &&
                n.Title == title &&
                n.Message == message);
        }

        public void CompleteReservation(int reservationId)
        {
            var reservation = _context.Reservations
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .FirstOrDefault(r => r.ReservationId == reservationId);

            if (reservation == null)
                return;

            if (reservation.Status != ReservationStatus.Returned)
                return;

            reservation.Status = ReservationStatus.Completed;

            var notificationService = new NotificationService(_context);

            var customerUser = _context.Users
                .FirstOrDefault(u =>
                    u.Email == reservation.Customer.Email &&
                    u.Role == "Customer");

            if (customerUser != null)
            {
                notificationService.CreateNotification(
                    customerUser.UserId,
                    "Rental completed",
                    $"Your rental for {reservation.Car.Brand} {reservation.Car.Model} has been completed.",
                    "Reservation");
            }

            if (reservation.Car.UserId.HasValue)
            {
                notificationService.CreateNotification(
                    reservation.Car.UserId.Value,
                    "Payout activated",
                    $"Revenue for {reservation.Car.Brand} {reservation.Car.Model} is now available in your balance.",
                    "Revenue");
            }

            _context.SaveChanges();
        }


        public void CheckCustomerReservationReminders(string customerEmail)
        {
            var customer = _context.Customers
                .FirstOrDefault(c => c.Email == customerEmail);

            if (customer == null)
                return;

            var customerUser = _context.Users
                .FirstOrDefault(u =>
                    u.Email == customer.Email &&
                    u.Role == "Customer");

            if (customerUser == null)
                return;

            if (!customerUser.ReservationRemindersEnabled)
                return;

            var now = DateTime.Now;

            var reservations = _context.Reservations
                .Include(r => r.Car)
                .Where(r =>
                    r.CustomerId == customer.CustomerId &&
                    r.Status == ReservationStatus.Confirmed)
                .ToList();

            var notificationService = new NotificationService(_context);

            foreach (var reservation in reservations)
            {
                var pickupDiff = reservation.StartDate - now;
                var returnDiff = reservation.EndDate - now;

                if (pickupDiff.TotalMinutes <= 60 && pickupDiff.TotalMinutes > 55)
                {
                    CreateReminderIfNotExists(
                        customerUser.UserId,
                        "Pickup reminder",
                        $"Your pickup for {reservation.Car.Brand} {reservation.Car.Model} is in 1 hour.",
                        "Reminder",
                        notificationService);
                }

                if (pickupDiff.TotalMinutes <= 15 && pickupDiff.TotalMinutes > 10)
                {
                    CreateReminderIfNotExists(
                        customerUser.UserId,
                        "Pickup reminder",
                        $"Your pickup for {reservation.Car.Brand} {reservation.Car.Model} is in 15 minutes.",
                        "Reminder",
                        notificationService);
                }

                if (returnDiff.TotalMinutes <= 60 && returnDiff.TotalMinutes > 55)
                {
                    CreateReminderIfNotExists(
                        customerUser.UserId,
                        "Return reminder",
                        $"Your rental for {reservation.Car.Brand} {reservation.Car.Model} should be returned in 1 hour.",
                        "Reminder",
                        notificationService);
                }
            }
        }

        private void CreateReminderIfNotExists(
            int userId,
            string title,
            string message,
            string type,
            NotificationService notificationService)
        {
            bool alreadyExists = _context.Notifications.Any(n =>
                n.UserId == userId &&
                n.Title == title &&
                n.Message == message);

            if (alreadyExists)
                return;

            notificationService.CreateNotification(userId, title, message, type);
        }

        public decimal CalculateCost(int carId, DateTime startDate, DateTime endDate)
        {
            Car car = _context.Cars.FirstOrDefault(c => c.CarId == carId);

            if (car == null)
                return 0;

            if (endDate <= startDate)
                return 0;

            TimeSpan duration = endDate - startDate;

            int billableDays = (int)Math.Ceiling(duration.TotalHours / 24.0);

            if (billableDays < 1)
                billableDays = 1;

            return billableDays * car.PricePerDay;
        }






        // OWNER SERVICES
        public List<Reservation> GetOwnerReservations(int ownerUserId)
        {
            return _context.Reservations
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .Where(r => r.Car.UserId == ownerUserId)
                .OrderByDescending(r => r.StartDate)
                .ToList();
        }

        public int CountOwnerReservationsByStatus(int ownerUserId, ReservationStatus status)
        {
            return _context.Reservations
                .Include(r => r.Car)
                .Count(r => r.Car.UserId == ownerUserId && r.Status == status);
        }

        public void ConfirmReservation(int reservationId)
        {
            var reservation = _context.Reservations
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .FirstOrDefault(r => r.ReservationId == reservationId);

            if (reservation == null)
                return;

            reservation.Status = ReservationStatus.Confirmed;

            var customerUser = _context.Users
                .FirstOrDefault(u =>
                    u.Email == reservation.Customer.Email &&
                    u.Role == "Customer");

            if (customerUser != null)
            {
                var notificationService = new NotificationService(_context);

                notificationService.CreateNotification(
                    customerUser.UserId,
                    "Reservation approved",
                    $"Your reservation for {reservation.Car.Brand} {reservation.Car.Model} has been approved.",
                    "Reservation");
            }

            _context.SaveChanges();
        }

        public void RejectReservation(int reservationId)
        {
            var reservation = _context.Reservations
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .FirstOrDefault(r => r.ReservationId == reservationId);

            if (reservation == null)
                return;

            reservation.Status = ReservationStatus.Cancelled;

            var customerUser = _context.Users
                .FirstOrDefault(u =>
                    u.Email == reservation.Customer.Email &&
                    u.Role == "Customer");

            if (customerUser != null)
            {
                var notificationService = new NotificationService(_context);

                notificationService.CreateNotification(
                    customerUser.UserId,
                    "Reservation rejected",
                    $"Your reservation for {reservation.Car.Brand} {reservation.Car.Model} was rejected.",
                    "Reservation");
            }

            _context.SaveChanges();
        }

        public decimal GetOwnerTotalRevenue(int ownerUserId)
        {
            return _context.Reservations
                .Include(r => r.Car)
                .Where(r => r.Car.UserId == ownerUserId &&
                           (r.Status == ReservationStatus.Completed))
                .Sum(r => r.TotalCost * (1 - COMPANY_FEE_PERCENT));
        }

        public decimal GetOwnerMonthlyRevenue(int ownerUserId)
        {
            var now = DateTime.Now;

            return _context.Reservations
                .Include(r => r.Car)
                .Where(r => r.Car.UserId == ownerUserId &&
                           (r.Status == ReservationStatus.Completed) &&
                           r.StartDate.Month == now.Month &&
                           r.StartDate.Year == now.Year)
                .Sum(r => r.TotalCost * (1 - COMPANY_FEE_PERCENT));
        }

        public decimal GetOwnerPendingRevenue(int ownerUserId)
        {
            return _context.Reservations
                .Include(r => r.Car)
                .Where(r => r.Car.UserId == ownerUserId &&
                            r.Status == ReservationStatus.Pending)
                .Sum(r => r.TotalCost * (1 - COMPANY_FEE_PERCENT));
        }

        public int GetOwnerCompletedRentalsCount(int ownerUserId)
        {
            return _context.Reservations
                .Include(r => r.Car)
                .Count(r => r.Car.UserId == ownerUserId &&
                           (r.Status == ReservationStatus.Completed));
        }

        public List<Reservation> GetOwnerRecentEarnings(int ownerUserId, int count = 5)
        {
            return _context.Reservations
                .Include(r => r.Car)
                .Where(r => r.Car.UserId == ownerUserId &&
                           (r.Status == ReservationStatus.Completed))
                .OrderByDescending(r => r.StartDate)
                .Take(count)
                .ToList();
        }

        public Car GetOwnerTopEarningCar(int ownerUserId)
        {
            return _context.Reservations
                .Include(r => r.Car)
                .Where(r => r.Car.UserId == ownerUserId &&
                           (r.Status == ReservationStatus.Completed))
                .GroupBy(r => r.Car)
                .OrderByDescending(g => g.Sum(r => r.TotalCost))
                .Select(g => g.Key)
                .FirstOrDefault();
        }

        public decimal GetCarRevenue(int carId)
        {
            return _context.Reservations
                .Where(r => r.CarId == carId &&
                           (r.Status == ReservationStatus.Completed))
                .Sum(r => r.TotalCost * (1 - COMPANY_FEE_PERCENT));
        }

        public decimal GetOwnerRevenueForMonth(int ownerUserId, int month, int year)
        {
            return _context.Reservations
                .Include(r => r.Car)
                .Where(r =>
                    r.Car.UserId == ownerUserId &&
                    r.Status == ReservationStatus.Completed &&
                    r.StartDate.Month == month &&
                    r.StartDate.Year == year)
                .Sum(r => r.TotalCost * (1 - COMPANY_FEE_PERCENT));
        }


        // OWNER 
        public List<Reservation> GetReturnedReservations()
        {
            return _context.Reservations
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .Where(r => r.Status == ReservationStatus.Returned)
                .OrderByDescending(r => r.ReturnedAt)
                .ToList();
        }

        public void CheckOverdueVehicles()
        {
            var overdueReservations = _context.Reservations
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .Where(r =>
                    r.Status == ReservationStatus.PickedUp &&
                    r.EndDate < DateTime.Now)
                .ToList();

            var admins = _context.Users
                .Where(u => u.Role == "Admin")
                .ToList();

            var notificationService = new NotificationService(_context);

            foreach (var reservation in overdueReservations)
            {
                foreach (var admin in admins)
                {
                    bool alreadyNotified = _context.Notifications.Any(n =>
                        n.UserId == admin.UserId &&
                        n.Title == "Vehicle overdue" &&
                        n.Message.Contains($"{reservation.Car.Brand} {reservation.Car.Model}"));

                    if (!alreadyNotified)
                    {
                        notificationService.CreateNotification(
                            admin.UserId,
                            "Vehicle overdue",
                            $"{reservation.Car.Brand} {reservation.Car.Model} has not been returned on time.",
                            "Overdue");
                    }
                }
            }

            _context.SaveChanges();
        }
    }
}