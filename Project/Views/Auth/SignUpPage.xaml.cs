using CarRental.Backend.Models;
using CarRental.Backend.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Project.Views.Dashboard.CarRenter;
using Project.Views.Dashboard.Customer;
using System;
using System.Text.RegularExpressions;

namespace Project.Views.Auth
{
    public sealed partial class SignUpPage : Page
    {
        public SignUpPage()
        {
            InitializeComponent();
        }

        private void SignUp_Click(object sender, RoutedEventArgs e)
        {
            TxtError.Visibility = Visibility.Collapsed;

            var firstName = TxtFirstName.Text.Trim();
            var lastName = TxtLastName.Text.Trim();
            var email = TxtEmail.Text.Trim().ToLower();
            var phone = TxtPhone.Text.Trim()
                                    .Replace(" ", "")
                                    .Replace("-", "");
            var password = TxtPassword.Password;
            var confirmPassword = TxtConfirmPassword.Password;

            if (password != confirmPassword)
            {
                ShowError("Passwords do not match.");
                return;
            }

            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(phone) ||
                string.IsNullOrWhiteSpace(password))
            {
                ShowError("Please fill in all required fields.");
                return;
            }

            if (!Regex.IsMatch(firstName, @"^[A-Za-zăâîșțĂÂÎȘȚ -]{2,50}$"))
            {
                ShowError("First name must contain only letters and be at least 2 characters.");
                return;
            }

            if (!Regex.IsMatch(lastName, @"^[A-Za-zăâîșțĂÂÎȘȚ -]{2,50}$"))
            {
                ShowError("Last name must contain only letters and be at least 2 characters.");
                return;
            }

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ShowError("Please enter a valid email address.");
                return;
            }

            if (!Regex.IsMatch(phone, @"^\+?[0-9]{10,15}$"))
            {
                ShowError("Please enter a valid phone number.");
                return;
            }

            if (password.Length < 8)
            {
                ShowError("Password must be at least 8 characters.");
                return;
            }

            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                ShowError("Password must contain at least one uppercase letter.");
                return;
            }

            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                ShowError("Password must contain at least one lowercase letter.");
                return;
            }

            if (!Regex.IsMatch(password, @"[0-9]"))
            {
                ShowError("Password must contain at least one number.");
                return;
            }

            if (RoleComboBox.SelectedItem is not ComboBoxItem selectedRoleItem)
            {
                ShowError("Please choose a role.");
                return;
            }

            string selectedRole = selectedRoleItem.Content.ToString();

            try
            {
                UserService userService = new UserService();

                User newUser = new User
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    Username = email,
                    Password = password,
                    Role = selectedRole
                };

                userService.Register(newUser, phone);

                SessionManager.CurrentUser = newUser;

                if (selectedRole == "Customer")
                {
                    MainWindow.Current.Navigate(typeof(CustomerDashboardPage));
                }
                else if (selectedRole == "CarRenter")
                {
                    MainWindow.Current.Navigate(typeof(CarRenterDashboardPage));
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }
        private void ShowError(string message)
        {
            TxtError.Text = message;
            TxtError.Visibility = Visibility.Visible;
        }

        private void GoToSignIn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Current.Navigate(typeof(SignInPage));
        }
    }
}