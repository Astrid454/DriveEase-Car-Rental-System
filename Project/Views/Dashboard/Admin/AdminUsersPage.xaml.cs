using CarRental.Backend.Data;
using CarRental.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;

namespace Project.Views.Dashboard.Admin
{
    public sealed partial class AdminUsersPage : Page
    {
        public AdminUsersPage()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            using var db = new AppDbContext();

            CustomersPanel.Children.Clear();
            RentersPanel.Children.Clear();

            var customers = db.Users
                .Where(u => u.Role == "Customer")
                .ToList();

            var renters = db.Users
                .Where(u => u.Role == "CarRenter")
                .ToList();

            foreach (var customer in customers)
            {
                CustomersPanel.Children.Add(
                    CreateUserCard(
                        customer,
                        customer.Email
                    ));
            }

            foreach (var renter in renters)
            {
                int carsCount = db.Cars.Count(c => c.UserId == renter.UserId);

                RentersPanel.Children.Add(
                    CreateUserCard(
                        renter,
                        $"{renter.Email} • {carsCount} cars"
                    ));
            }
        }

        private Border CreateUserCard(User user, string details)
        {
            var contentPanel = new StackPanel
            {
                Spacing = 6
            };

            contentPanel.Children.Add(new TextBlock
            {
                Text = $"{user.FirstName} {user.LastName}",
                FontSize = 16,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            });

            contentPanel.Children.Add(new TextBlock
            {
                Text = details,
                FontSize = 13
            });

            contentPanel.Children.Add(new TextBlock
            {
                Text = user.Role,
                FontSize = 12,
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            });

            contentPanel.Children.Add(new TextBlock
            {
                Text = "Click to view details",
                FontSize = 12,
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            });

            var deleteButton = new Button
            {
                Content = "Delete",
                Margin = new Thickness(0, 8, 0, 0),
                Tag = user
            };

            deleteButton.Tapped += DeleteUser_Tapped;

            contentPanel.Children.Add(deleteButton);

            var card = new Border
            {
                Padding = new Thickness(16),
                CornerRadius = new CornerRadius(12),
                BorderThickness = new Thickness(1),
                BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(1, 255, 255, 255)),
                Tag = user,
                Child = contentPanel
            };

            card.PointerEntered += UserCard_PointerEntered;
            card.PointerExited += UserCard_PointerExited;
            card.Tapped += UserCard_Tapped;

            return card;
        }

        private void UserCard_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender is not Border card)
                return;

            card.Background = (Brush)Application.Current.Resources["CardBackgroundFillColorSecondaryBrush"];
        }

        private void UserCard_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender is not Border card)
                return;

            card.Background = null;
        }

        private async void UserCard_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (sender is not Border card || card.Tag is not User user)
                return;

            using var db = new AppDbContext();

            var content = new StackPanel
            {
                Spacing = 10
            };

            content.Children.Add(new TextBlock { Text = $"Name: {user.FirstName} {user.LastName}" });
            content.Children.Add(new TextBlock { Text = $"Email: {user.Email}" });

            string phone = "Not available";

            if (user.Role == "Customer")
            {
                var customerProfile = db.Customers.FirstOrDefault(c => c.Email == user.Email);

                if (customerProfile != null && !string.IsNullOrWhiteSpace(customerProfile.Phone))
                    phone = customerProfile.Phone;
            }

            if (user.Role == "CarRenter")
            {
                var renterProfile = db.CarRenters.FirstOrDefault(r => r.Email == user.Email);

                if (renterProfile != null && !string.IsNullOrWhiteSpace(renterProfile.Phone))
                    phone = renterProfile.Phone;
            }

            content.Children.Add(new TextBlock { Text = $"Phone: {phone}" });
            content.Children.Add(new TextBlock { Text = $"Role: {user.Role}" });

            if (user.Role == "Customer")
            {
                int reservationsCount = db.Reservations
                    .Include(r => r.Customer)
                    .Count(r => r.Customer.Email == user.Email);

                int completedReservations = db.Reservations
                    .Include(r => r.Customer)
                    .Count(r => r.Customer.Email == user.Email &&
                                r.Status == ReservationStatus.Completed);

                int cancelledReservations = db.Reservations
                    .Include(r => r.Customer)
                    .Count(r => r.Customer.Email == user.Email &&
                                r.Status == ReservationStatus.Cancelled);

                content.Children.Add(new TextBlock { Text = $"Total reservations: {reservationsCount}" });
                content.Children.Add(new TextBlock { Text = $"Completed reservations: {completedReservations}" });
                content.Children.Add(new TextBlock { Text = $"Cancelled reservations: {cancelledReservations}" });
                content.Children.Add(new TextBlock { Text = $"Reservation reminders: {(user.ReservationRemindersEnabled ? "Enabled" : "Disabled")}" });
            }

            if (user.Role == "CarRenter")
            {
                int carsCount = db.Cars.Count(c => c.UserId == user.UserId);
                int availableCars = db.Cars.Count(c => c.UserId == user.UserId && c.Status == CarStatus.Available);
                int rentedCars = db.Cars.Count(c => c.UserId == user.UserId && c.Status == CarStatus.Rented);
                int maintenanceCars = db.Cars.Count(c => c.UserId == user.UserId && c.Status == CarStatus.Maintenance);

                content.Children.Add(new TextBlock { Text = $"Cars listed: {carsCount}" });
                content.Children.Add(new TextBlock { Text = $"Available cars: {availableCars}" });
                content.Children.Add(new TextBlock { Text = $"Rented cars: {rentedCars}" });
                content.Children.Add(new TextBlock { Text = $"Cars in maintenance: {maintenanceCars}" });
            }

            var dialog = new ContentDialog
            {
                Title = "User Details",
                Content = content,
                CloseButtonText = "Close",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }

        private async void DeleteUser_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            e.Handled = true;

            if (sender is not Button button || button.Tag is not User user)
                return;

            var reasonBox = new ComboBox
            {
                PlaceholderText = "Choose reason",
                Width = 300
            };

            reasonBox.Items.Add("Fake account");
            reasonBox.Items.Add("Violation of rules");
            reasonBox.Items.Add("Inactive account");
            reasonBox.Items.Add("User requested deletion");
            reasonBox.Items.Add("Other");

            var content = new StackPanel { Spacing = 12 };

            content.Children.Add(new TextBlock
            {
                Text = $"Are you sure you want to delete {user.FirstName} {user.LastName}?"
            });

            content.Children.Add(reasonBox);

            var dialog = new ContentDialog
            {
                Title = "Delete user",
                Content = content,
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync().AsTask();

            if (result != ContentDialogResult.Primary)
                return;

            if (reasonBox.SelectedItem == null)
            {
                var errorDialog = new ContentDialog
                {
                    Title = "Reason required",
                    Content = "Please choose a reason before deleting the user.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await errorDialog.ShowAsync().AsTask();
                return;
            }

            using var db = new AppDbContext();

            var userToDelete = db.Users.FirstOrDefault(u => u.UserId == user.UserId);

            if (userToDelete == null)
                return;

            db.Users.Remove(userToDelete);
            db.SaveChanges();

            LoadUsers();
        }
    }
}