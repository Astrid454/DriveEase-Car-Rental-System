using CarRental.Backend.Data;
using CarRental.Backend.Models;
using CarRental.Backend.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Linq;
using Microsoft.UI.Xaml;

namespace Project.Views.Dashboard.CarRenter
{
    public sealed partial class EarningsPage : Page
    {
        private readonly AppDbContext _db;
        private readonly ReservationService _reservationService;

        private int _ownerUserId;

        public EarningsPage()
        {
            InitializeComponent();

            _db = new AppDbContext();
            _reservationService = new ReservationService(_db);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (SessionManager.CurrentUser == null)
                return;

            _ownerUserId = SessionManager.CurrentUser.UserId;

            LoadEarnings();
        }

        private void LoadEarnings()
        {
            TotalEarningsText.Text =
                $"€{_reservationService.GetOwnerTotalRevenue(_ownerUserId):N2}";

            MonthlyEarningsText.Text =
                $"€{_reservationService.GetOwnerMonthlyRevenue(_ownerUserId):N2}";

            CompletedRentalsText.Text =
                _reservationService.GetOwnerCompletedRentalsCount(_ownerUserId).ToString();

            PendingPayoutText.Text =
                $"€{_reservationService.GetOwnerPendingRevenue(_ownerUserId):N2}";

            LoadTopCar();
            LoadRecentTransactions();
            LoadMonthlyBreakdown();
        }

        private void LoadTopCar()
        {
            Car topCar = _reservationService.GetOwnerTopEarningCar(_ownerUserId);

            if (topCar == null)
            {
                TopCarNameText.Text = "No earnings yet";
                TopCarDetailsText.Text = "You don't have completed rentals yet.";
                return;
            }

            decimal revenue = _reservationService.GetCarRevenue(topCar.CarId);

            TopCarNameText.Text = $"{topCar.Brand} {topCar.Model}";
            TopCarDetailsText.Text = $"€{revenue:N2} earned from completed rentals.";
        }

        private void LoadRecentTransactions()
        {
            var transactions = _reservationService.GetOwnerRecentEarnings(_ownerUserId);

            RecentTransactionsPanel.Children.Clear();

            if (!transactions.Any())
            {
                RecentTransactionsPanel.Children.Add(new TextBlock
                {
                    Text = "No recent transactions yet.",
                    FontSize = 14
                });

                return;
            }

            foreach (var reservation in transactions)
            {
                var days = Math.Ceiling((reservation.EndDate - reservation.StartDate).TotalDays);

                RecentTransactionsPanel.Children.Add(new Border
                {
                    Padding = new Microsoft.UI.Xaml.Thickness(14),
                    CornerRadius = new Microsoft.UI.Xaml.CornerRadius(10),
                    BorderThickness = new Microsoft.UI.Xaml.Thickness(1),
                    Child = new Grid
                    {
                        ColumnSpacing = 12,
                        ColumnDefinitions =
                        {
                            new ColumnDefinition { Width = GridLength.Auto },
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                            new ColumnDefinition { Width = GridLength.Auto }
                        },
                        Children =
                        {
                            CreateIcon(),
                            CreateTransactionInfo(reservation, days),
                            CreateAmountText(reservation.TotalCost)
                        }
                    }
                });
            }
        }

        private void LoadMonthlyBreakdown()
        {
            var now = DateTime.Now;

            var months = Enumerable.Range(0, 4)
                .Select(i => now.AddMonths(-3 + i))
                .ToList();

            Month1NameText.Text = months[0].ToString("MMMM");
            Month2NameText.Text = months[1].ToString("MMMM");
            Month3NameText.Text = months[2].ToString("MMMM");
            Month4NameText.Text = months[3].ToString("MMMM");

            Month1RevenueText.Text =
                $"€{_reservationService.GetOwnerRevenueForMonth(_ownerUserId, months[0].Month, months[0].Year):N2}";

            Month2RevenueText.Text =
                $"€{_reservationService.GetOwnerRevenueForMonth(_ownerUserId, months[1].Month, months[1].Year):N2}";

            Month3RevenueText.Text =
                $"€{_reservationService.GetOwnerRevenueForMonth(_ownerUserId, months[2].Month, months[2].Year):N2}";

            Month4RevenueText.Text =
                $"€{_reservationService.GetOwnerRevenueForMonth(_ownerUserId, months[3].Month, months[3].Year):N2}";
        }

        private Border CreateIcon()
        {
            return new Border
            {
                Width = 38,
                Height = 38,
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius(19),
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(34, 52, 199, 89)),
                Child = new FontIcon
                {
                    Glyph = "\uE8C7",
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Windows.UI.Color.FromArgb(255, 52, 199, 89)),
                    HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
                    VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
                }
            };
        }

        private StackPanel CreateTransactionInfo(Reservation reservation, double days)
        {
            var panel = new StackPanel();
            Grid.SetColumn(panel, 1);

            panel.Children.Add(new TextBlock
            {
                Text = $"{reservation.Car.Brand} {reservation.Car.Model} rental completed",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            });

            panel.Children.Add(new TextBlock
            {
                Text = $"{reservation.StartDate:MMM dd} - {reservation.EndDate:MMM dd} : {days} days",
                FontSize = 13
            });

            return panel;
        }

        private TextBlock CreateAmountText(decimal amount)
        {
            var text = new TextBlock
            {
                Text = $"+€{amount:N2}",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
            };

            Grid.SetColumn(text, 2);
            return text;
        }
    }
}