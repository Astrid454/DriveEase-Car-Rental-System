using CarRental.Backend.Data;
using CarRental.Backend.Models;
using System.Collections.Generic;
using System.Linq;

namespace CarRental.Backend.Services
{
    public class NotificationService
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        public void CreateNotification(int userId, string title, string message, string type = "General")
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            _context.SaveChanges();
        }

        public List<Notification> GetUserNotifications(int userId)
        {
            return _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();
        }

        public int GetUnreadCount(int userId)
        {
            return _context.Notifications
                .Count(n => n.UserId == userId && !n.IsRead);
        }

        public int GetUnreadNotificationsCount(int userId)
        {
            return _context.Notifications
                .Count(n => n.UserId == userId && !n.IsRead);
        }

        public void MarkAsRead(int notificationId)
        {
            var notification = _context.Notifications
                .FirstOrDefault(n => n.NotificationId == notificationId);

            if (notification == null)
                return;

            notification.IsRead = true;
            _context.SaveChanges();
        }

        public void MarkAllAsRead(int userId)
        {
            var notifications = _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToList();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            _context.SaveChanges();
        }
    }
}