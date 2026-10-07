using CarRental.Backend.Data;
using CarRental.Backend.Models;
using CarRental.Backend.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Linq;

namespace Project.Views.Dashboard.Admin
{
    public sealed partial class AdminReturnedReservationsPage : Page
    {
        public AdminReturnedReservationsPage()
        {
            InitializeComponent();
            LoadReturnedReservations();
        }

        private void LoadReturnedReservations()
        {
            using var db = new AppDbContext();
            var reservationService = new ReservationService(db);

            var reservations = reservationService.GetReturnedReservations();

            ReturnedReservationsPanel.Children.Clear();

            ReturnedCountText.Text = reservations.Count.ToString();
            LateFeesText.Text = $"€ {reservations.Sum(r => r.LateFee):0.00}";
            PendingFinalValueText.Text = $"€ {reservations.Sum(r => r.TotalCost + r.LateFee):0.00}";

            if (reservations.Count == 0)
            {
                ReturnedReservationsPanel.Children.Add(new TextBlock
                {
                    Text = "No returned reservations waiting for confirmation.",
                    Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
                    FontSize = 14
                });

                return;
            }

            foreach (var reservation in reservations)
            {
                ReturnedReservationsPanel.Children.Add(CreateReturnedReservationCard(reservation));
            }
        }

        private Border CreateReturnedReservationCard(Reservation reservation)
        {
            decimal finalTotal = reservation.TotalCost + reservation.LateFee;
            decimal driveEaseFee = finalTotal * 0.15m;
            decimal ownerNet = finalTotal * 0.85m;

            var card = new Border
            {
                Padding = new Thickness(18),
                CornerRadius = new CornerRadius(14),
                BorderThickness = new Thickness(1),
                BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(35, 30, 41, 59))
            };

            var grid = new Grid
            {
                ColumnSpacing = 18
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.3, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var customerPanel = new StackPanel { Spacing = 6 };

            customerPanel.Children.Add(new TextBlock
            {
                Text = $"{reservation.Customer.FirstName} {reservation.Customer.LastName}",
                FontSize = 18,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            });

            customerPanel.Children.Add(new TextBlock
            {
                Text = reservation.Customer.Email,
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            });

            customerPanel.Children.Add(new TextBlock
            {
                Text = reservation.Customer.Phone,
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            });

            Grid.SetColumn(customerPanel, 0);

            var carPanel = new StackPanel { Spacing = 6 };

            carPanel.Children.Add(new TextBlock
            {
                Text = $"{reservation.Car.Brand} {reservation.Car.Model}",
                FontSize = 18,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            });

            carPanel.Children.Add(new TextBlock
            {
                Text = $"Plate: {reservation.Car.PlateNumber}",
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            });

            carPanel.Children.Add(new TextBlock
            {
                Text = $"Returned at: {reservation.ReturnedAt:dd MMM yyyy, HH:mm}",
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            });

            Grid.SetColumn(carPanel, 1);

            var moneyPanel = new StackPanel { Spacing = 6 };

            moneyPanel.Children.Add(new TextBlock
            {
                Text = $"Base: € {reservation.TotalCost:0.00}"
            });

            moneyPanel.Children.Add(new TextBlock
            {
                Text = $"Late fee: € {reservation.LateFee:0.00}",
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 245, 158, 11))
            });

            moneyPanel.Children.Add(new TextBlock
            {
                Text = $"Final: € {finalTotal:0.00}",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            });

            moneyPanel.Children.Add(new TextBlock
            {
                Text = $"DriveEase: € {driveEaseFee:0.00}",
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            });

            moneyPanel.Children.Add(new TextBlock
            {
                Text = $"Owner net: € {ownerNet:0.00}",
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            });

            Grid.SetColumn(moneyPanel, 2);

            var completeButton = new Button
            {
                Content = "Complete rental",
                Tag = reservation.ReservationId,
                VerticalAlignment = VerticalAlignment.Center
            };

            completeButton.Click += CompleteRental_Click;

            Grid.SetColumn(completeButton, 3);

            grid.Children.Add(customerPanel);
            grid.Children.Add(carPanel);
            grid.Children.Add(moneyPanel);
            grid.Children.Add(completeButton);

            card.Child = grid;

            return card;
        }

        private void CompleteRental_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            int reservationId = (int)button.Tag;

            using var db = new AppDbContext();
            var reservationService = new ReservationService(db);

            reservationService.CompleteReservation(reservationId);

            LoadReturnedReservations();
        }
    }
}