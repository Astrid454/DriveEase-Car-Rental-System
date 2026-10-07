using CarRental.Backend.Data;
using CarRental.Backend.Models;
using CarRental.Backend.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Linq;

namespace Project.Views.Dashboard
{
    public sealed partial class ProfilePage : Page
    {
        private User _currentUser;

        public ProfilePage()
        {
            InitializeComponent();

            ReservationRemindersSwitch.Toggled += ReservationRemindersSwitch_Toggled;

            LoadProfile();
        }

        private void LoadProfile()
        {
            _currentUser = SessionManager.CurrentUser;

            if (_currentUser == null)
                return;

            FullNameText.Text = $"{_currentUser.FirstName} {_currentUser.LastName}";
            RoleText.Text = $"{_currentUser.Role} Account";
            EmailText.Text = _currentUser.Email;

            FirstNameBox.Text = _currentUser.FirstName;
            LastNameBox.Text = _currentUser.LastName;
            EmailBox.Text = _currentUser.Email;

            string initials = "";

            if (!string.IsNullOrWhiteSpace(_currentUser.FirstName))
                initials += _currentUser.FirstName[0];

            if (!string.IsNullOrWhiteSpace(_currentUser.LastName))
                initials += _currentUser.LastName[0];

            AvatarInitialsText.Text = initials.ToUpper();

            ReservationRemindersSwitch.IsOn = _currentUser.ReservationRemindersEnabled;
        }

        private void SaveProfile_Click(object sender, RoutedEventArgs e)
        {
            ProfileStatusText.Text = "";

            if (_currentUser == null)
                return;

            if (string.IsNullOrWhiteSpace(FirstNameBox.Text) ||
                string.IsNullOrWhiteSpace(LastNameBox.Text) ||
                string.IsNullOrWhiteSpace(EmailBox.Text))
            {
                ProfileStatusText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 248, 113, 113));

                ProfileStatusText.Text = "Please complete all required fields.";
                return;
            }

            using var db = new AppDbContext();
            var userService = new UserService(db);

            bool success = userService.UpdateProfile(
                _currentUser.UserId,
                FirstNameBox.Text.Trim(),
                LastNameBox.Text.Trim(),
                EmailBox.Text.Trim(),
                PhoneBox.Text.Trim());

            if (!success)
            {
                ProfileStatusText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 248, 113, 113));

                ProfileStatusText.Text = "Profile could not be updated.";
                return;
            }

            _currentUser.FirstName = FirstNameBox.Text.Trim();
            _currentUser.LastName = LastNameBox.Text.Trim();
            _currentUser.Email = EmailBox.Text.Trim();

            SessionManager.CurrentUser = _currentUser;

            LoadProfile();

            ProfileStatusText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Windows.UI.Color.FromArgb(255, 52, 211, 153));

            ProfileStatusText.Text = "Profile updated successfully.";
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            PasswordStatusText.Text = "";

            if (_currentUser == null)
                return;

            if (string.IsNullOrWhiteSpace(CurrentPasswordBox.Password) ||
                string.IsNullOrWhiteSpace(NewPasswordBox.Password) ||
                string.IsNullOrWhiteSpace(ConfirmPasswordBox.Password))
            {
                PasswordStatusText.Text = "Please complete all password fields.";
                return;
            }

            if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
            {
                PasswordStatusText.Text = "New passwords do not match.";
                return;
            }

            using var db = new AppDbContext();
            var userService = new UserService(db);

            bool success = userService.ChangePassword(
                _currentUser.UserId,
                CurrentPasswordBox.Password,
                NewPasswordBox.Password);

            if (!success)
            {
                PasswordStatusText.Text = "Current password is incorrect.";
                return;
            }

            CurrentPasswordBox.Password = "";
            NewPasswordBox.Password = "";
            ConfirmPasswordBox.Password = "";

            PasswordStatusText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Windows.UI.Color.FromArgb(255, 52, 211, 153));

            PasswordStatusText.Text = "Password changed successfully.";
        }

        private void UpdateLicense_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LicenseNumberBox.Text) ||
                string.IsNullOrWhiteSpace(LicenseExpiryBox.Text))
            {
                LicenseStatusText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 248, 113, 113));

                LicenseStatusText.Text = "Please complete license number and expiry date.";
                return;
            }

            LicenseStatusText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Windows.UI.Color.FromArgb(255, 52, 211, 153));

            LicenseStatusText.Text = "Driving license information updated.";
        }

        private void ReservationRemindersSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
                return;

            using var db = new AppDbContext();

            var user = db.Users.FirstOrDefault(u => u.UserId == _currentUser.UserId);

            if (user == null)
                return;

            user.ReservationRemindersEnabled =
                ReservationRemindersSwitch.IsOn;

            db.SaveChanges();

            _currentUser.ReservationRemindersEnabled =
                ReservationRemindersSwitch.IsOn;

            SessionManager.CurrentUser = _currentUser;
        }
    }
}