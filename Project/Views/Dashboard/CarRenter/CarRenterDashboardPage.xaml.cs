using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using CarRental.Backend.Data;
using CarRental.Backend.Services;
using Project.Views.Auth;
using Project.Views.Dashboard.Notifications;

namespace Project.Views.Dashboard.CarRenter
{
    public sealed partial class CarRenterDashboardPage : Page
    {
        private Button _activeNavButton;

        public CarRenterDashboardPage()
        {
            InitializeComponent();
            Loaded += CarRenterDashboardPage_Loaded;
        }

        private void CarRenterDashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUserInfo();

            LoadNotificationsBadge();

            SetActivePage(BtnOverview, typeof(RenterOverviewPage), "Owner Overview");
        }

        private void LoadUserInfo()
        {
            var user = SessionManager.CurrentUser;
            if (user == null) return;

            SidebarUserName.Text = $"{user.FirstName} {user.LastName}";
            SidebarUserRole.Text = "Car Renter";

            string initials = "";
            if (!string.IsNullOrEmpty(user.FirstName)) initials += user.FirstName[0];
            if (!string.IsNullOrEmpty(user.LastName)) initials += user.LastName[0];

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
                    SetActivePage(btn, typeof(RenterOverviewPage), "Owner Overview");
                    break;

                case "MyFleet":
                    SetActivePage(btn, typeof(MyCarsPage), "My Cars");
                    break;

                case "BookingRequests":
                    SetActivePage(btn, typeof(RentalRequestsPage), "Booking Requests");
                    break;

                case "Revenue":
                    SetActivePage(btn, typeof(EarningsPage), "Revenue");
                    break;

                case "Notifications":
                    SetActivePage(btn, typeof(NotificationsPage), "Notifications");
                    break;

                case "VehicleMonitoring":
                    SetActivePage(btn, typeof(VehicleMonitoringPage), "Vehicle Monitoring");
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

        public void NavigateToMyFleet()
        {
            SetActivePage(BtnMyFleet, typeof(MyCarsPage), "My Cars");
        }

        public void NavigateToBookingRequests()
        {
            SetActivePage(BtnBookingRequests, typeof(RentalRequestsPage), "Booking Requests");
        }

        public void NavigateToRevenue()
        {
            SetActivePage(BtnRevenue, typeof(EarningsPage), "Revenue");
        }

        public void NavigateToVehicleMonitoring()
        {
            SetActivePage(BtnVehicleStatus, typeof(VehicleMonitoringPage), "Vehicle Monitoring");
        }
    }
}