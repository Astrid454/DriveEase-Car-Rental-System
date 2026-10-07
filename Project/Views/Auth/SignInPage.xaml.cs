using CarRental.Backend.Models;
using CarRental.Backend.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Project.Views.Dashboard.Admin;
using Project.Views.Dashboard.CarRenter;
using Project.Views.Dashboard.Customer;
using System;
using System.Text.RegularExpressions;

namespace Project.Views.Auth
{
    public sealed partial class SignInPage : Page
    {
        private string _selectedRole = "Customer";

        public SignInPage()
        {
            this.InitializeComponent();
        }

        private void RoleToggle_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            _selectedRole = btn.Tag.ToString();

            ResetRoleButton(BtnCustomerRole);
            ResetRoleButton(BtnRenterRole);
            ResetRoleButton(BtnAdminRole);

            ActivateRoleButton(btn);
        }

        private void ResetRoleButton(Button btn)
        {
            btn.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
            btn.BorderThickness = new Thickness(0);
        }

        private void ActivateRoleButton(Button btn)
        {
            btn.Background = (Brush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"];
            btn.BorderThickness = new Thickness(0.5);
        }

        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            TxtError.Visibility = Visibility.Collapsed;

            var email = TxtEmail.Text.Trim().ToLower();
            var password = TxtPassword.Password;

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ShowError("Please fill in all fields.");
                return;
            }

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ShowError("Please enter a valid email address.");
                return;
            }

            if (password.Length < 8)
            {
                ShowError("Invalid password.");
                return;
            }

            try
            {
                UserService userService = new UserService();

                User user = userService.Login(email, password, _selectedRole);

                if (user == null)
                {
                    ShowError("Invalid email, password, or role.");
                    return;
                }

                SessionManager.CurrentUser = user;

                if (user.Role == "Admin")
                {
                    MainWindow.Current.Navigate(typeof(AdminDashboardPage));
                }
                else if (user.Role == "CarRenter")
                {
                    MainWindow.Current.Navigate(typeof(CarRenterDashboardPage));
                }
                else
                {
                    MainWindow.Current.Navigate(typeof(CustomerDashboardPage));
                }
            }
            catch
            {
                ShowError("Something went wrong while signing in.");
            }
        }

        private async void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            TxtError.Visibility = Visibility.Collapsed;

            TextBox emailBox = new TextBox
            {
                PlaceholderText = "Email",
                Text = TxtEmail.Text.Trim().ToLower(),
                Margin = new Thickness(0, 0, 0, 12)
            };

            PasswordBox newPasswordBox = new PasswordBox
            {
                PlaceholderText = "New password",
                Margin = new Thickness(0, 0, 0, 12)
            };

            PasswordBox confirmPasswordBox = new PasswordBox
            {
                PlaceholderText = "Confirm new password"
            };

            StackPanel panel = new StackPanel();
            panel.Children.Add(emailBox);
            panel.Children.Add(newPasswordBox);
            panel.Children.Add(confirmPasswordBox);

            ContentDialog dialog = new ContentDialog
            {
                Title = "Reset password",
                Content = panel,
                PrimaryButtonText = "Reset password",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            ContentDialogResult result = await dialog.ShowAsync();

            if (result != ContentDialogResult.Primary)
            {
                return;
            }

            string email = emailBox.Text.Trim().ToLower();
            string newPassword = newPasswordBox.Password;
            string confirmPassword = confirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(newPassword) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                ShowError("Please fill in all reset password fields.");
                return;
            }

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ShowError("Please enter a valid email address.");
                return;
            }

            if (newPassword.Length < 8)
            {
                ShowError("New password must be at least 8 characters.");
                return;
            }

            if (newPassword != confirmPassword)
            {
                ShowError("Passwords do not match.");
                return;
            }

            try
            {
                UserService userService = new UserService();

                bool passwordReset = userService.ResetPassword(email, newPassword, _selectedRole);

                if (!passwordReset)
                {
                    ShowError("No account found with this email and role.");
                    return;
                }

                TxtPassword.Password = "";
                ShowError("Password reset successfully. You can now sign in.");
            }
            catch
            {
                ShowError("Something went wrong while resetting password.");
            }
        }

        private void ShowError(string message)
        {
            TxtError.Text = message;
            TxtError.Visibility = Visibility.Visible;
        }

        private void GoToSignUp_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Current.Navigate(typeof(SignUpPage));
        }
    }
}