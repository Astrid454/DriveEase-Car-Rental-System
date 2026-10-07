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
    public sealed partial class AdminCompanyRevenuePage : Page
    {
        private const decimal COMPANY_FEE_PERCENT = 0.15m;

        public AdminCompanyRevenuePage()
        {
            InitializeComponent();
            LoadCompanyRevenue();
        }

        private void LoadCompanyRevenue()
        {
            using var db = new AppDbContext();

            var completedReservations = db.Reservations
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .Where(r => r.Status == ReservationStatus.Completed)
                .OrderByDescending(r => r.ReturnedAt)
                .ToList();

            decimal totalCompanyRevenue = completedReservations
                .Sum(r => (r.TotalCost + r.LateFee) * COMPANY_FEE_PERCENT);

            var now = DateTime.Now;

            decimal monthlyCompanyRevenue = completedReservations
                .Where(r =>
                    r.StartDate.Month == now.Month &&
                    r.StartDate.Year == now.Year)
                .Sum(r => (r.TotalCost + r.LateFee) * COMPANY_FEE_PERCENT);

            TotalCompanyRevenueText.Text = $"€ {totalCompanyRevenue:0.00}";
            MonthlyCompanyRevenueText.Text = $"€ {monthlyCompanyRevenue:0.00}";
            CompletedRentalsText.Text = completedReservations.Count.ToString();

            TransactionsPanel.Children.Clear();

            if (completedReservations.Count == 0)
            {
                TransactionsPanel.Children.Add(new TextBlock
                {
                    Text = "No completed rental transactions yet.",
                    FontSize = 14,
                    Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
                });

                return;
            }

            foreach (var reservation in completedReservations)
            {
                TransactionsPanel.Children.Add(CreateTransactionCard(reservation));
            }
        }

        private Border CreateTransactionCard(Reservation reservation)
        {
            decimal finalTotal = reservation.TotalCost + reservation.LateFee;
            decimal companyFee = finalTotal * COMPANY_FEE_PERCENT;
            decimal ownerNet = finalTotal - companyFee;

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
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });

            var customerPanel = new StackPanel { Spacing = 6 };

            customerPanel.Children.Add(new TextBlock
            {
                Text = $"{reservation.Customer.FirstName} {reservation.Customer.LastName}",
                FontSize = 18,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            });

            customerPanel.Children.Add(new TextBlock
            {
                Text = $"Customer email: {reservation.Customer.Email}",
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            });

            customerPanel.Children.Add(new TextBlock
            {
                Text = $"Completed: {reservation.ReturnedAt:dd MMM yyyy, HH:mm}",
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
                Text = $"{reservation.StartDate:dd MMM yyyy, HH:mm} - {reservation.EndDate:dd MMM yyyy, HH:mm}",
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            });

            Grid.SetColumn(carPanel, 1);

            var moneyPanel = new StackPanel { Spacing = 6 };

            moneyPanel.Children.Add(new TextBlock
            {
                Text = $"Final paid: € {finalTotal:0.00}",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            });

            moneyPanel.Children.Add(new TextBlock
            {
                Text = $"DriveEase fee: € {companyFee:0.00}",
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 96, 165, 250)),
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            });

            moneyPanel.Children.Add(new TextBlock
            {
                Text = $"Owner net: € {ownerNet:0.00}",
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            });

            moneyPanel.Children.Add(new TextBlock
            {
                Text = $"Late fee included: € {reservation.LateFee:0.00}",
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 245, 158, 11))
            });

            Grid.SetColumn(moneyPanel, 2);

            grid.Children.Add(customerPanel);
            grid.Children.Add(carPanel);
            grid.Children.Add(moneyPanel);

            card.Child = grid;

            return card;
        }
    }
}