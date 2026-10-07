using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.Backend.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string MethodOfPayment { get; set; } = string.Empty;
        public int ReservationId { get; set; }
        public string ReservationName { get; set; } = string.Empty;
        public string Status { get; set; } = "Paid";
        public DateTime? RefundedAt { get; set; }
    }
}
