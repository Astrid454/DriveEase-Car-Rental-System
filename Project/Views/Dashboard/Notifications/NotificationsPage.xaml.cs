using CarRental.Backend.Data;
using CarRental.Backend.Models;
using CarRental.Backend.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using System.Linq;
using Project.Views.Dashboard.Customer;
using Project.Views.Dashboard.CarRenter;
using Project.Views.Dashboard.Admin;

namespace Project.Views.Dashboard.Notifications
{
    public sealed partial class NotificationsPage : Page
    {
        private List<Notification> _notifications = new();

        public NotificationsPage()
        {
            InitializeComponent();
            LoadNotifications();
        }

        private void LoadNotifications()
        {
            if (SessionManager.CurrentUser == null)
                return;

            using var db = new AppDbContext();
            var notificationService = new NotificationService(db);

            int userId = SessionManager.CurrentUser.UserId;

            _notifications = notificationService.GetUserNotifications(userId);

            UnreadCountText.Text = _notifications.Count(n => !n.IsRead).ToString();
            TotalNotificationsText.Text = _notifications.Count.ToString();

            NotificationsPanel.Children.Clear();

            if (_notifications.Count == 0)
            {
                NotificationsPanel.Children.Add(new TextBlock
                {
                    Text = "No notifications yet.",
                    FontSize = 14,
                    Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
                });

                return;
            }

            foreach (var notification in _notifications)
            {
                NotificationsPanel.Children.Add(CreateNotificationCard(notification));
            }
        }

        private Border CreateNotificationCard(Notification notification)
        {
            var card = new Border
            {
                Padding = new Thickness(18),
                CornerRadius = new CornerRadius(14),
                BorderThickness = new Thickness(1),
                BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
                Background = notification.IsRead
                    ? new SolidColorBrush(Windows.UI.Color.FromArgb(25, 30, 41, 59))
                    : new SolidColorBrush(Windows.UI.Color.FromArgb(55, 59, 130, 246))
            };

            var grid = new Grid
            {
                ColumnSpacing = 16
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var infoPanel = new StackPanel
            {
                Spacing = 6
            };

            infoPanel.Children.Add(new TextBlock
            {
                Text = notification.Title,
                FontSize = 18,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            });

            infoPanel.Children.Add(new TextBlock
            {
                Text = notification.Message,
                TextWrapping = TextWrapping.Wrap,
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            });

            infoPanel.Children.Add(new TextBlock
            {
                Text = $"{notification.Type} • {notification.CreatedAt:dd MMM yyyy, HH:mm}",
                FontSize = 12,
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            });

            Grid.SetColumn(infoPanel, 0);

            var actionPanel = new StackPanel
            {
                Spacing = 8,
                VerticalAlignment = VerticalAlignment.Center
            };

            if (!notification.IsRead)
            {
                var markReadButton = new Button
                {
                    Content = "Mark as read",
                    Tag = notification.NotificationId
                };

                markReadButton.Click += MarkAsRead_Click;
                actionPanel.Children.Add(markReadButton);
            }

            Grid.SetColumn(actionPanel, 1);

            grid.Children.Add(infoPanel);
            grid.Children.Add(actionPanel);

            card.Child = grid;

            return card;
        }

        private void MarkAsRead_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            int notificationId = (int)button.Tag;

            using var db = new AppDbContext();
            var notificationService = new NotificationService(db);

            notificationService.MarkAsRead(notificationId);

            LoadNotifications();
            RefreshDashboardBadge();
        }

        private void MarkAllAsRead_Click(object sender, RoutedEventArgs e)
        {
            if (SessionManager.CurrentUser == null)
                return;

            using var db = new AppDbContext();
            var notificationService = new NotificationService(db);

            notificationService.MarkAllAsRead(SessionManager.CurrentUser.UserId);

            LoadNotifications();
            RefreshDashboardBadge();
        }

        private void RefreshDashboardBadge()
        {
            DependencyObject parent = this;

            while (parent != null)
            {
                if (parent is CustomerDashboardPage customerDashboard)
                {
                    customerDashboard.LoadNotificationsBadge();
                    return;
                }

                if (parent is CarRenterDashboardPage renterDashboard)
                {
                    renterDashboard.LoadNotificationsBadge();
                    return;
                }

                if (parent is AdminDashboardPage adminDashboard)
                {
                    adminDashboard.LoadNotificationsBadge();
                    return;
                }

                parent = VisualTreeHelper.GetParent(parent);
            }
        }
    }
}