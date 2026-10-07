using CarRental.Backend.Data;
using CarRental.Backend.Models;
using System;
using System.Linq;

namespace CarRental.Backend.Services
{
    public class PaymentService
    {
        private readonly AppDbContext _context;

        public PaymentService(AppDbContext context)
        {
            _context = context;
        }

        public void CreatePayment(
            decimal amount,
            string methodOfPayment,
            int reservationId,
            string reservationName)
        {
            var payment = new Payment
            {
                Amount = amount,
                PaymentDate = DateTime.Now,
                MethodOfPayment = methodOfPayment,
                ReservationId = reservationId,
                ReservationName = reservationName
            };

            _context.Payments.Add(payment);
            _context.SaveChanges();
        }

        public Payment GetPaymentByReservationId(int reservationId)
        {
            return _context.Payments
                .FirstOrDefault(p => p.ReservationId == reservationId);
        }

        public void RefundPayment(int reservationId)
        {
            var payment = _context.Payments
                .FirstOrDefault(p => p.ReservationId == reservationId);

            if (payment == null)
                return;

            if (payment.MethodOfPayment != "Card")
                return;

            payment.Status = "Refunded";
            payment.RefundedAt = DateTime.Now;

            _context.SaveChanges();
        }
    }
}