using CarRental.Backend.Data;
using CarRental.Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Backend.Services
{
    public class CarService
    {
        private readonly AppDbContext _context;

        public CarService(AppDbContext context)
        {
            _context = context;
        }

        public List<Car> GetAvailableCars()
        {
            return _context.Cars
                 .Where(c => c.Status == CarStatus.Available)
                 .ToList();
        }

        public void AddCar(Car car)
        {
            _context.Cars.Add(car);
            _context.SaveChanges();
        }

        public bool PlateNumberExists(string plateNumber)
        {
            if (string.IsNullOrWhiteSpace(plateNumber))
                return false;

            string normalizedPlate = plateNumber.Trim().ToUpper();

            return _context.Cars.Any(c => c.PlateNumber.ToUpper() == normalizedPlate);
        }

        public bool PlateNumberExistsForOtherCar(string plateNumber, int carId)
        {
            if (string.IsNullOrWhiteSpace(plateNumber))
                return false;

            string normalizedPlate = plateNumber.Trim().ToUpper();

            return _context.Cars.Any(c =>
                c.CarId != carId &&
                c.PlateNumber.ToUpper() == normalizedPlate);
        }

        public Car GetCarById(int carId)
        {
            return _context.Cars
                .Include(c => c.Garage)
                .FirstOrDefault(c => c.CarId == carId);
        }

        public List<Car> GetAllCars()
        {
            return _context.Cars
                .Include(c => c.Garage)
                .ToList();
        }

        public List<Car> GetCarsByOwner(int ownerUserId)
        {
            return _context.Cars
                .Where(c => c.UserId == ownerUserId)
                .ToList();
        }

        public void ToggleMaintenance(int carId)
        {
            var car = GetCarById(carId);

            if (car == null)
                return;

            bool willEnterMaintenance = car.Status != CarStatus.Maintenance;

            car.Status = willEnterMaintenance
                ? CarStatus.Maintenance
                : CarStatus.Available;

            if (willEnterMaintenance && car.UserId.HasValue)
            {
                var notificationService = new NotificationService(_context);

                notificationService.CreateNotification(
                    car.UserId.Value,
                    "Vehicle moved to maintenance",
                    $"Your {car.Brand} {car.Model} was moved to maintenance by DriveEase.",
                    "Maintenance");
            }

            _context.SaveChanges();
        }

        public void MoveToMaintenance(
            int carId,
            int? garageId,
            string reason,
            string notes,
            DateTime? estimatedCompletionDate)
        {
            var car = _context.Cars
                .FirstOrDefault(c => c.CarId == carId);

            if (car == null)
                return;

            car.Status = CarStatus.Maintenance;
            car.MaintenanceStatus = "Open";
            car.MaintenanceReason = reason;

            var record = new MaintenanceRecord
            {
                CarId = car.CarId,
                GarageId = garageId,
                Reason = reason,
                Notes = notes,
                Status = "Open",
                CreatedAt = DateTime.Now,
                EstimatedCompletionDate = estimatedCompletionDate
            };

            _context.MaintenanceRecords.Add(record);

            if (car.UserId.HasValue)
            {
                var message = estimatedCompletionDate.HasValue
                    ? $"Your {car.Brand} {car.Model} was moved to maintenance. Reason: {reason}. Estimated completion: {estimatedCompletionDate.Value:dd MMM yyyy}."
                    : $"Your {car.Brand} {car.Model} was moved to maintenance. Reason: {reason}.";

                new NotificationService(_context).CreateNotification(
                    car.UserId.Value,
                    "Vehicle moved to maintenance",
                    message,
                    "Maintenance");
            }

            _context.SaveChanges();
        }

        public int CountOwnerCarsByStatus(int ownerUserId, CarStatus status)
        {
            return _context.Cars
                .Count(c => c.UserId == ownerUserId && c.Status == status);
        }

        public int CountOwnerCarsNeedingReports(int ownerUserId)
        {
            return _context.Cars
                .Count(c => c.UserId == ownerUserId &&
                           (c.Status == CarStatus.Rented ||
                            c.Status == CarStatus.Maintenance));
        }

        public void UpdateCar(Car car)
        {
            _context.Cars.Update(car);
            _context.SaveChanges();
        }

        public void DeleteCar(int id)
        {
            var car = _context.Cars.FirstOrDefault(c => c.CarId == id);

            if (car != null)
            {
                _context.Cars.Remove(car);
                _context.SaveChanges();
            }
        }

        public int CountCarsByStatus(CarStatus status)
        {
            return _context.Cars.Count(c => c.Status == status);
        }


        public void CreateAvailabilityAlert(int userId, int carId)
        {
            bool alreadyExists = _context.CarAvailabilityAlerts.Any(a =>
                a.UserId == userId &&
                a.CarId == carId &&
                !a.IsNotified);

            if (alreadyExists)
                return;

            _context.CarAvailabilityAlerts.Add(new CarAvailabilityAlert
            {
                UserId = userId,
                CarId = carId
            });

            _context.SaveChanges();
        }

        public bool HasActiveAvailabilityAlert(int userId, int carId)
        {
            return _context.CarAvailabilityAlerts.Any(a =>
                a.UserId == userId &&
                a.CarId == carId &&
                !a.IsNotified);
        }

        public void NotifyCustomersCarAvailable(int carId)
        {
            var car = _context.Cars.FirstOrDefault(c => c.CarId == carId);

            if (car == null)
                return;

            var alerts = _context.CarAvailabilityAlerts
                .Where(a => a.CarId == carId && !a.IsNotified)
                .ToList();

            var notificationService = new NotificationService(_context);

            foreach (var alert in alerts)
            {
                notificationService.CreateNotification(
                    alert.UserId,
                    "Car available",
                    $"{car.Brand} {car.Model} is now available for reservation.",
                    "Vehicle");

                alert.IsNotified = true;
                alert.NotifiedAt = DateTime.Now;
            }

            _context.SaveChanges();
        }
    }
}
