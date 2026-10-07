using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRental.Backend.Models
{
    public enum CarStatus
    {
        Available,
        Rented,
        Maintenance
    }

    public class Car
    {
        public int CarId { get; set; }

        public int? UserId { get; set; }
        public User? Owner { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string PlateNumber { get; set; }
        public string ImagePath { get; set; } = "";

        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerDay { get; set; }

        public CarStatus Status { get; set; } = CarStatus.Available;

        public List<Reservation> Reservations { get; set; } = new List<Reservation>();

        public int? GarageId { get; set; }

        public Garage? Garage { get; set; }

        public string GarageLocation
        {
            get
            {
                if (Garage == null)
                    return "Garage not assigned";

                return $"{Garage.Name} - {Garage.City}";
            }
        }
        public string? MaintenanceStatus { get; set; }
        public string? MaintenanceReason { get; set; }

        public bool IsAvailable() => Status == CarStatus.Available;

        public string StatusColor
        {
            get
            {
                return Status switch
                {
                    CarStatus.Available => "#16A34A",
                    CarStatus.Maintenance => "#DC2626",
                    CarStatus.Rented => "#EA580C",
                    _ => "#475569"
                };
            }
        }

    }
}
