using CarRental.Backend.Data;
using CarRental.Backend.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Project.Views.Auth;
using Project.Views.Dashboard.Notifications;
using System;


namespace Project.Views.Dashboard.Customer
{
    public sealed partial class CustomerDashboardPage : Page
    {
        private Button _activeNavButton;

        public CustomerDashboardPage()
        {
            this.InitializeComponent();
            this.Loaded += DashboardPage_Loaded;
        }

        private void DashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUserInfo();

            using var context = new AppDbContext();
            var reservationService = new ReservationService(context);

            if (SessionManager.CurrentUser != null)
            {
                reservationService.CheckCustomerReservationReminders(
                    SessionManager.CurrentUser.Email);
            }

            LoadNotificationsBadge();

            SetActivePage(BtnOverview, typeof(OverviewPage), "Overview");
        }

        private void LoadUserInfo()
        {
            var user = SessionManager.CurrentUser;
            if (user == null) return;

            SidebarUserName.Text = $"{user.FirstName} {user.LastName}";
            SidebarUserRole.Text = user.Role;

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

            var context = new AppDbContext();

            var notificationService = new NotificationService(context);

            int unreadCount = notificationService
                .GetUnreadNotificationsCount(user.UserId);

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
            var btn = sender as Button;
            if (btn == null || btn == _activeNavButton) return;

            switch (btn.Tag?.ToString())
            {
                case "Overview":
                    SetActivePage(btn, typeof(OverviewPage), "Overview");
                    break;
                case "Reservations":
                    SetActivePage(btn, typeof(ReservationsPage), "My reservations");
                    break;
                case "Cars":
                    SetActivePage(btn, typeof(CarsPage), "Available cars");
                    break;
                case "Profile":
                    SetActivePage(btn, typeof(ProfilePage), "My profile");
                    break;
                case "Favorites":
                    SetActivePage(btn, typeof(FavoritesPage), "Favorites");
                    break;
                case "Support":
                    SetActivePage(btn, typeof(SupportPage), "Help & Support");
                    break;
                case "Notifications":
                    SetActivePage(btn, typeof(NotificationsPage), "Notifications");
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
            var panel = btn.Content as StackPanel;
            if (panel == null) return;
            foreach (var child in panel.Children)
            {
                if (child is TextBlock tb) tb.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255));
                if (child is FontIcon fi) fi.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255));
            }
        }

        private void SetNavButtonInactive(Button btn)
        {
            btn.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));
            var panel = btn.Content as StackPanel;
            if (panel == null) return;
            foreach (var child in panel.Children)
            {
                if (child is TextBlock tb) tb.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(204, 255, 255, 255));
                if (child is FontIcon fi) fi.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(204, 255, 255, 255));
            }
        }

       private void PrimaryAction_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(NewReservationPage));
        }

        private void SignOut_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.CurrentUser = null;
            MainWindow.Current.Navigate(typeof(SignInPage));
        }
    }
}