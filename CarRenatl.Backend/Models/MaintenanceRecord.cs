using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Backend.Models
{
    public class MaintenanceRecord
    {
        public int Id { get; set; }

        public int CarId { get; set; }
        public Car? Car { get; set; }

        public int? SupportTicketId { get; set; }
        public int? GarageId { get; set; }

        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }

        public string Status { get; set; } = "Open";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? CompletedAt { get; set; }

        public DateTime? EstimatedCompletionDate { get; set; }
    }
}
