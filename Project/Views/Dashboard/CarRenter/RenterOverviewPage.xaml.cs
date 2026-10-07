using CarRental.Backend.Data;
using CarRental.Backend.Models;
using CarRental.Backend.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Linq;

namespace Project.Views.Dashboard.CarRenter
{
    public sealed partial class RenterOverviewPage : Page
    {
        public RenterOverviewPage()
        {
            InitializeComponent();
            LoadOverviewData();
        }

        private void LoadOverviewData()
        {
            if (!SessionManager.IsLoggedIn)
                return;

            using var db = new AppDbContext();
            var ownerService = new OwnerDashboardService(db);

            int currentUserId = SessionManager.CurrentUser.UserId;

            var cars = ownerService.GetOwnerCars(currentUserId);

            int totalCars = ownerService.GetListedCarsCount(currentUserId);
            int availableCars = ownerService.GetAvailableCarsCount(currentUserId);
            int rentedCars = ownerService.GetRentedCarsCount(currentUserId);
            int maintenanceCars = ownerService.GetMaintenanceCarsCount(currentUserId);

            const decimal COMPANY_FEE_PERCENT = 0.15m;

            decimal grossPotentialMonth = ownerService.GetPotentialMonthlyRevenue(currentUserId);
            decimal potentialMonth = grossPotentialMonth * (1 - COMPANY_FEE_PERCENT);

            ListedCarsText.Text = totalCars.ToString();
            AvailableCarsText.Text = availableCars.ToString();
            MaintenanceCarsText.Text = maintenanceCars.ToString();
            PotentialMonthText.Text = $"€{potentialMonth:0}";

            FleetSummaryPanel.Children.Clear();

            if (cars.Count == 0)
            {
                FleetSummaryPanel.Children.Add(CreateSummaryCard(
                    "No cars listed yet",
                    "Go to My Fleet and list your first vehicle.",
                    "Ready",
                    "#221A6EF5",
                    "#FF6EA8FE"
                ));

                FleetHealthText.Text = "You do not have any listed vehicles yet.";
                FleetHealthBadgeText.Text = "Add your first vehicle to activate your owner dashboard.";
                NextStepText.Text = "Start by listing your first car in My Fleet.";
                NextStepButton.Content = "Go to My Fleet";
                return;
            }

            FleetSummaryPanel.Children.Add(CreateSummaryCard(
                "Fleet loaded successfully",
                $"{totalCars} vehicles are listed under your owner account.",
                "Today",
                "#2234C759",
                "#FF34C759"
            ));

            FleetSummaryPanel.Children.Add(CreateSummaryCard(
                "Availability status",
                $"{availableCars} available, {rentedCars} rented, {maintenanceCars} in maintenance.",
                "Live",
                "#221A6EF5",
                "#FF6EA8FE"
            ));

            FleetSummaryPanel.Children.Add(CreateSummaryCard(
                "Monthly potential revenue",
                $"Your fleet can generate approximately €{potentialMonth:0} net per month after DriveEase's 15% service fee.",
                "Estimate",
                "#22FF9500",
                "#FFFF9500"
            ));

            FleetHealthText.Text =
                $"{availableCars} vehicles are ready for rent. " +
                $"{rentedCars} vehicles are currently rented. " +
                $"{maintenanceCars} vehicles are in maintenance.";

            if (maintenanceCars > 0)
            {
                FleetHealthBadgeText.Text = $"{maintenanceCars} vehicle(s) need maintenance attention.";
                NextStepText.Text = "Check Vehicle Monitoring and update maintenance status where needed.";
                NextStepButton.Content = "Check monitoring";
                NextStepButton.Click -= GoToMyFleet_Click;
                NextStepButton.Click += GoToVehicleMonitoring_Click;
            }
            else
            {
                FleetHealthBadgeText.Text = "All listed vehicles are in good condition.";
                NextStepText.Text = "Review your fleet or add another vehicle to increase potential revenue.";
                NextStepButton.Content = "Go to My Fleet";
                NextStepButton.Click -= GoToVehicleMonitoring_Click;
                NextStepButton.Click += GoToMyFleet_Click;
            }
        }

        private Border CreateSummaryCard(string title, string description, string timeLabel, string backgroundHex, string foregroundHex)
        {
            var card = new Border
            {
                Padding = new Thickness(14),
                CornerRadius = new CornerRadius(10),
                BorderThickness = new Thickness(1),
                BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"]
            };

            var grid = new Grid
            {
                ColumnSpacing = 12
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var iconBox = new Border
            {
                Width = 36,
                Height = 36,
                CornerRadius = new CornerRadius(18),
                Background = HexToBrush(backgroundHex),
                Child = new FontIcon
                {
                    Glyph = "\uE73E",
                    FontSize = 16,
                    Foreground = HexToBrush(foregroundHex),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };

            Grid.SetColumn(iconBox, 0);

            var textPanel = new StackPanel();

            textPanel.Children.Add(new TextBlock
            {
                Text = title,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            });

            textPanel.Children.Add(new TextBlock
            {
                Text = description,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            });

            Grid.SetColumn(textPanel, 1);

            var timeText = new TextBlock
            {
                Text = timeLabel,
                FontSize = 12,
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            };

            Grid.SetColumn(timeText, 2);

            grid.Children.Add(iconBox);
            grid.Children.Add(textPanel);
            grid.Children.Add(timeText);

            card.Child = grid;

            return card;
        }

        private SolidColorBrush HexToBrush(string hex)
        {
            hex = hex.Replace("#", "");

            byte a = 255;
            byte r;
            byte g;
            byte b;

            if (hex.Length == 8)
            {
                a = System.Convert.ToByte(hex.Substring(0, 2), 16);
                r = System.Convert.ToByte(hex.Substring(2, 2), 16);
                g = System.Convert.ToByte(hex.Substring(4, 2), 16);
                b = System.Convert.ToByte(hex.Substring(6, 2), 16);
            }
            else
            {
                r = System.Convert.ToByte(hex.Substring(0, 2), 16);
                g = System.Convert.ToByte(hex.Substring(2, 2), 16);
                b = System.Convert.ToByte(hex.Substring(4, 2), 16);
            }

            return new SolidColorBrush(Windows.UI.Color.FromArgb(a, r, g, b));
        }

        private CarRenterDashboardPage GetDashboardPage()
        {
            FrameworkElement current = this;

            while (current != null)
            {
                if (current is CarRenterDashboardPage dashboard)
                    return dashboard;

                current = current.Parent as FrameworkElement;
            }

            return null;
        }

        private void GoToMyFleet_Click(object sender, RoutedEventArgs e)
        {
            GetDashboardPage()?.NavigateToMyFleet();
        }

        private void GoToBookingRequests_Click(object sender, RoutedEventArgs e)
        {
            GetDashboardPage()?.NavigateToBookingRequests();
        }

        private void GoToVehicleMonitoring_Click(object sender, RoutedEventArgs e)
        {
            GetDashboardPage()?.NavigateToVehicleMonitoring();
        }

        private void GoToRevenue_Click(object sender, RoutedEventArgs e)
        {
            GetDashboardPage()?.NavigateToRevenue();
        }
    }
}