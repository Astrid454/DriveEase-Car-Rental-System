using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Project.Views.Auth;
using Project.Views.Dashboard.Notifications;
using System;
using CarRental.Backend.Data;
using CarRental.Backend.Services;

namespace Project.Views.Dashboard.Admin
{
    public sealed partial class AdminDashboardPage : Page
    {
        private Button _activeNavButton;

        public AdminDashboardPage()
        {
            InitializeComponent();
            Loaded += AdminDashboardPage_Loaded;
        }

        private void AdminDashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUserInfo();

            using var context = new AppDbContext();
            var reservationService = new ReservationService(context);
            reservationService.CheckOverdueVehicles();

            LoadNotificationsBadge();

            SetActivePage(BtnOverview, typeof(AdminOverviewPage), "Admin Overview");
        }

        private void LoadUserInfo()
        {
            var user = SessionManager.CurrentUser;

            if (user == null)
                return;

            SidebarUserName.Text = $"{user.FirstName} {user.LastName}";

            string initials = "";

            if (!string.IsNullOrEmpty(user.FirstName))
                initials += user.FirstName[0];

            if (!string.IsNullOrEmpty(user.LastName))
                initials += user.LastName[0];

            AvatarInitials.Text = initials.ToUpper();
        }

        public void LoadNotificationsBadge()
        {
            var user = SessionManager.CurrentUser;

            if (user == null)
                return;

            using var context = new AppDbContext();
            var notificationService = new NotificationService(context);

            int unreadCount = notificationService.GetUnreadNotificationsCount(user.UserId);

            if (unreadCount > 0)
            {
                NotificationsBadge.Visibility = Visibility.Visible;
                NotificationsBadgeText.Text = unreadCount.ToString();
            }
            else
            {
                NotificationsBadge.Visibility = Visibility.Collapsed;
            }
        }

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn == _activeNavButton)
                return;

            switch (btn.Tag?.ToString())
            {
                case "Overview":
                    SetActivePage(btn, typeof(AdminOverviewPage), "Admin Overview");
                    break;

                case "ReturnedReservations":
                    SetActivePage(btn, typeof(AdminReturnedReservationsPage), "Returned Reservations");
                    break;

                case "CompanyRevenue":
                    SetActivePage(btn, typeof(AdminCompanyRevenuePage), "Company Revenue");
                    break;

                case "SupportTickets":
                    SetActivePage(btn, typeof(AdminSupportTicketsPage), "Support Tickets");
                    break;

                case "Notifications":
                    SetActivePage(btn, typeof(NotificationsPage), "Notifications");
                    break;

                case "Users":
                    SetActivePage(btn, typeof(AdminUsersPage), "Users");
                    break;

                case "Garages":
                    SetActivePage(btn, typeof(AdminGaragesPage), "Garages");
                    break;
            }
        }

        private void SetActivePage(Button btn, Type pageType, string title)
        {
            if (_activeNavButton != null)
                SetNavButtonInactive(_activeNavButton);

            _activeNavButton = btn;
            SetNavButtonActive(_activeNavButton);

            PageTitle.Text = title;
            PageSubtitle.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy");

            ContentFrame.Navigate(pageType);
        }

        private void SetNavButtonActive(Button btn)
        {
            btn.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(30, 255, 255, 255));

            if (btn.Content is not StackPanel panel)
                return;

            foreach (var child in panel.Children)
            {
                if (child is TextBlock tb)
                    tb.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255));

                if (child is FontIcon fi)
                    fi.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255));
            }
        }

        private void SetNavButtonInactive(Button btn)
        {
            btn.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));

            if (btn.Content is not StackPanel panel)
                return;

            foreach (var child in panel.Children)
            {
                if (child is TextBlock tb)
                    tb.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(204, 255, 255, 255));

                if (child is FontIcon fi)
                    fi.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(204, 255, 255, 255));
            }
        }

        private void SignOut_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.CurrentUser = null;
            MainWindow.Current.Navigate(typeof(SignInPage));
        }
    }
}