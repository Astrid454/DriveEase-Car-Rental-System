using CarRental.Backend.Data;
using CarRental.Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace CarRental.Backend.Services
{
    public class FavoriteService
    {
        private readonly AppDbContext _context;

        public FavoriteService(AppDbContext context)
        {
            _context = context;
        }

        public bool IsFavorite(int userId, int carId)
        {
            return _context.FavoriteCars.Any(f =>
                f.UserId == userId &&
                f.CarId == carId);
        }

        public bool ToggleFavorite(int userId, int carId)
        {
            var existingFavorite = _context.FavoriteCars.FirstOrDefault(f =>
                f.UserId == userId &&
                f.CarId == carId);

            if (existingFavorite == null)
            {
                _context.FavoriteCars.Add(new FavoriteCars
                {
                    UserId = userId,
                    CarId = carId
                });

                _context.SaveChanges();
                return true;
            }

            _context.FavoriteCars.Remove(existingFavorite);
            _context.SaveChanges();
            return false;
        }

        public List<Car> GetFavoriteCars(int userId)
        {
            return _context.FavoriteCars
                .Include(f => f.Car)
                    .ThenInclude(c => c.Garage)
                .Where(f => f.UserId == userId)
                .Select(f => f.Car)
                .Where(c => c != null)
                .ToList();
        }

        public FavoriteCars GetLatestFavorite(int userId)
        {
            return _context.FavoriteCars
                .Include(f => f.Car)
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.Id)
                .FirstOrDefault();
        }

        public int GetFavoriteCount(int userId)
        {
            return _context.FavoriteCars.Count(f => f.UserId == userId);
        }
    }
}