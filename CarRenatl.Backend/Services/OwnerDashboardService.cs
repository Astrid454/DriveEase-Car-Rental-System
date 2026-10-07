using CarRental.Backend.Data;
using CarRental.Backend.Models;
using System.Collections.Generic;
using System.Linq;

namespace CarRental.Backend.Services
{
    public class OwnerDashboardService
    {
        private readonly AppDbContext _context;

        public OwnerDashboardService(AppDbContext context)
        {
            _context = context;
        }

        public List<Car> GetOwnerCars(int ownerUserId)
        {
            return _context.Cars
                .Where(c => c.UserId == ownerUserId)
                .ToList();
        }

        public int GetListedCarsCount(int ownerUserId)
        {
            return GetOwnerCars(ownerUserId).Count;
        }

        public int GetAvailableCarsCount(int ownerUserId)
        {
            return GetOwnerCars(ownerUserId)
                .Count(c => c.Status == CarStatus.Available);
        }

        public int GetRentedCarsCount(int ownerUserId)
        {
            return GetOwnerCars(ownerUserId)
                .Count(c => c.Status == CarStatus.Rented);
        }

        public int GetMaintenanceCarsCount(int ownerUserId)
        {
            return GetOwnerCars(ownerUserId)
                .Count(c => c.Status == CarStatus.Maintenance);
        }

        public decimal GetPotentialMonthlyRevenue(int ownerUserId)
        {
            return GetOwnerCars(ownerUserId)
                .Sum(c => c.PricePerDay * 30);
        }
    }
}