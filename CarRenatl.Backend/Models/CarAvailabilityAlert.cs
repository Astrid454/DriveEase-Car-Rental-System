using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Backend.Models
{
    public class CarAvailabilityAlert
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public int CarId { get; set; }

        public bool IsNotified { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? NotifiedAt { get; set; }
    }
}
