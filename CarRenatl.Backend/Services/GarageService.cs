using CarRental.Backend.Data;
using CarRental.Backend.Models;
using System.Collections.Generic;
using System.Linq;

namespace CarRental.Backend.Services
{
    public class GarageService
    {
        private readonly AppDbContext _context;

        public GarageService(AppDbContext context)
        {
            _context = context;
        }

        public List<Garage> GetAllGarages()
        {
            return _context.Garages.ToList();
        }

        public Garage GetGarageById(int garageId)
        {
            return _context.Garages
                .FirstOrDefault(g => g.GarageId == garageId);
        }

        public List<Car> GetCarsByGarage(int garageId)
        {
            return _context.Cars
                .Where(c => c.GarageId == garageId)
                .ToList();
        }

        public int CountCarsInGarage(int garageId)
        {
            return _context.Cars
                .Count(c => c.GarageId == garageId);
        }

        public int CountAvailableSpots(int garageId)
        {
            var garage = GetGarageById(garageId);

            if (garage == null)
                return 0;

            var carsInside = CountCarsInGarage(garageId);

            return garage.Capacity - carsInside;
        }

        public bool HasAvailableSpot(int garageId)
        {
            return CountAvailableSpots(garageId) > 0;
        }

        public void AddGarage(Garage garage)
        {
            _context.Garages.Add(garage);
            _context.SaveChanges();
        }

        public void UpdateGarage(Garage garage)
        {
            _context.Garages.Update(garage);
            _context.SaveChanges();
        }

        public void DeleteGarage(int garageId)
        {
            var garage = GetGarageById(garageId);

            if (garage != null)
            {
                _context.Garages.Remove(garage);
                _context.SaveChanges();
            }
        }
    }
}