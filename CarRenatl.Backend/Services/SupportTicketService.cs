using CarRental.Backend.Data;
using CarRental.Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CarRental.Backend.Services
{
    public class SupportTicketService
    {
        private readonly AppDbContext _context;

        public SupportTicketService(AppDbContext context)
        {
            _context = context;
        }

        public void CreateTicket(int userId, string subject, string category, string message)
        {
            if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(message))
                return;

            var ticket = new SupportTicket
            {
                UserId = userId,
                Subject = subject,
                Category = category,
                Message = message,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _context.SupportTickets.Add(ticket);
            _context.SaveChanges();

            var admins = _context.Users
                        .Where(u => u.Role == "Admin")
                        .ToList();

            var notificationService = new NotificationService(_context);

            foreach (var admin in admins)
            {
                notificationService.CreateNotification(
                    admin.UserId,
                    "New support ticket",
                    $"A new support ticket was opened: \"{subject}\".",
                    "Support");
            }
        }

        public List<SupportTicket> GetUserTickets(int userId)
        {
            return _context.SupportTickets
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();
        }

        public List<SupportTicket> GetRecentUserTickets(int userId, int count = 5)
        {
            return _context.SupportTickets
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Take(count)
                .ToList();
        }

        public int GetOpenTicketCount(int userId)
        {
            return _context.SupportTickets
                .Count(t => t.UserId == userId && t.Status == "Open");
        }

        public List<SupportTicket> GetAllTickets()
        {
            return _context.SupportTickets
                .OrderByDescending(t => t.CreatedAt)
                .ToList();
        }

        public void UpdateTicketStatus(int ticketId, string status)
        {
            var ticket = _context.SupportTickets
                .FirstOrDefault(t => t.Id == ticketId);

            if (ticket == null)
                return;

            ticket.Status = status;
            _context.SaveChanges();
        }

        public void ResolveTicketWithResponse(int ticketId, string adminResponse)
        {
            var ticket = _context.SupportTickets
                .FirstOrDefault(t => t.Id == ticketId);

            if (ticket == null)
                return;

            if (string.IsNullOrWhiteSpace(adminResponse))
                return;

            ticket.AdminResponse = adminResponse;
            ticket.Status = "Resolved";
            ticket.ResolvedAt = DateTime.Now;

            var notificationService = new NotificationService(_context);

            notificationService.CreateNotification(
                ticket.UserId,
                "Support ticket resolved",
                $"Your support ticket \"{ticket.Subject}\" has received an admin response.",
                "Support");

            _context.SaveChanges();
        }
    }
}