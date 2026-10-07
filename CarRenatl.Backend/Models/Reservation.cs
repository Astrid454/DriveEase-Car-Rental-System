using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Backend.Models
{
    public enum ReservationStatus
    {
        Pending,
        Confirmed,
        PickedUp,
        Returned,
        Cancelled,
        Completed
    }

    public class Reservation
    {
        public int ReservationId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCost { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
        public string PickupLocation { get; set; } = "";
        public DateTime? PickedUpAt { get; set; }
        public DateTime? ReturnedAt { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LateFee { get; set; } = 0;
        public int CarId { get; set; }
        public int CustomerId { get; set; }

        public Car Car { get; set; }
        public Customer Customer { get; set; }

        public decimal CalculateTotal()
        {
            if (Car == null)
                return 0;

            int days = (EndDate.Date - StartDate.Date).Days;

            if (days <= 0)
                return 0;

            TotalCost = days * Car.PricePerDay;
            return TotalCost;
        }

        public bool ValidatePeriod()
        {
            return EndDate > StartDate;
        }
    }
}
