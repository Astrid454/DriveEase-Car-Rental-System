using CarRental.Backend.Data;
using CarRental.Backend.Models;
using CarRental.Backend.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;

namespace Project.Views.Dashboard.CarRenter
{
    public sealed partial class VehicleMonitoringPage : Page
    {
        public VehicleMonitoringPage()
        {
            InitializeComponent();
            LoadVehicles();
        }

        private void LoadVehicles()
        {
            if (!SessionManager.IsLoggedIn)
                return;

            using var db = new AppDbContext();
            var carService = new CarService(db);

            int currentUserId = SessionManager.CurrentUser.UserId;

            var cars = carService.GetCarsByOwner(currentUserId);

            NeedsInspectionText.Text = carService
                .CountOwnerCarsByStatus(currentUserId, CarStatus.Rented)
                .ToString();

            InMaintenanceText.Text = carService
                .CountOwnerCarsByStatus(currentUserId, CarStatus.Maintenance)
                .ToString();

            ReportsThisMonthText.Text = carService
                .CountOwnerCarsNeedingReports(currentUserId)
                .ToString();

            var selectedFilter = GetSelectedFilter();

            if (selectedFilter != "All vehicles")
            {
                cars = cars
                    .Where(c => c.Status.ToString() == selectedFilter)
                    .ToList();
            }

            VehicleStatusPanel.Children.Clear();

            foreach (var car in cars)
            {
                VehicleStatusPanel.Children.Add(CreateVehicleCard(car));
            }
        }

        private string GetSelectedFilter()
        {
            if (StatusFilterBox.SelectedItem is ComboBoxItem item && item.Content != null)
                return item.Content.ToString();

            return "All vehicles";
        }

        private Border CreateVehicleCard(Car car)
        {
            var statusLabel = GetStatusLabel(car.Status);

            var statusBackground = car.Status switch
            {
                CarStatus.Available => new SolidColorBrush(Windows.UI.Color.FromArgb(34, 52, 199, 89)),
                CarStatus.Rented => new SolidColorBrush(Windows.UI.Color.FromArgb(34, 255, 149, 0)),
                CarStatus.Maintenance => new SolidColorBrush(Windows.UI.Color.FromArgb(34, 255, 59, 48)),
                _ => new SolidColorBrush(Windows.UI.Color.FromArgb(34, 120, 120, 120))
            };

            var statusForeground = car.Status switch
            {
                CarStatus.Available => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 52, 199, 89)),
                CarStatus.Rented => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 149, 0)),
                CarStatus.Maintenance => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 59, 48)),
                _ => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 120, 120, 120))
            };

            var card = new Border
            {
                Padding = new Thickness(16),
                CornerRadius = new CornerRadius(10),
                BorderThickness = new Thickness(1),
                BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"]
            };

            var grid = new Grid
            {
                ColumnSpacing = 16
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            FrameworkElement imageContent;

            if (!string.IsNullOrWhiteSpace(car.ImagePath))
            {
                imageContent = new Image
                {
                    Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(car.ImagePath)),
                    Stretch = Stretch.UniformToFill
                };
            }
            else
            {
                imageContent = new FontIcon
                {
                    Glyph = "\uE804",
                    FontSize = 26,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
            }

            var iconBox = new Border
            {
                Width = 64,
                Height = 64,
                CornerRadius = new CornerRadius(10),
                Background = (Brush)Application.Current.Resources["CardBackgroundFillColorSecondaryBrush"],
                Child = imageContent
            };

            Grid.SetColumn(iconBox, 0);

            var infoPanel = new StackPanel
            {
                Spacing = 6
            };

            var titleRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8
            };

            titleRow.Children.Add(new TextBlock
            {
                Text = $"{car.Brand} {car.Model}",
                FontSize = 17,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            });

            titleRow.Children.Add(new Border
            {
                Padding = new Thickness(8, 3, 8, 3),
                CornerRadius = new CornerRadius(10),
                Background = statusBackground,
                Child = new TextBlock
                {
                    Text = statusLabel,
                    FontSize = 11,
                    Foreground = statusForeground
                }
            });

            infoPanel.Children.Add(titleRow);

            infoPanel.Children.Add(new TextBlock
            {
                Text = GetVehicleStatusLine(car),
                FontSize = 13,
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            });

            infoPanel.Children.Add(new TextBlock
            {
                Text = GetVehicleNoteLine(car),
                FontSize = 13,
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            });

            Grid.SetColumn(infoPanel, 1);

            var actionsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                VerticalAlignment = VerticalAlignment.Center
            };

            var reportButton = new Button
            {
                Content = "View Report",
                Tag = car.CarId
            };

            reportButton.Click += ViewReport_Click;

            actionsPanel.Children.Add(reportButton);

            Grid.SetColumn(actionsPanel, 2);

            grid.Children.Add(iconBox);
            grid.Children.Add(infoPanel);
            grid.Children.Add(actionsPanel);

            card.Child = grid;

            return card;
        }

        private string GetStatusLabel(CarStatus status)
        {
            return status switch
            {
                CarStatus.Available => "Good Condition",
                CarStatus.Rented => "Needs Inspection",
                CarStatus.Maintenance => "In Maintenance",
                _ => "Unknown"
            };
        }

        private string GetVehicleStatusLine(Car car)
        {
            return car.Status switch
            {
                CarStatus.Available => "Vehicle ready - No issues reported",
                CarStatus.Rented => "Vehicle currently rented - Inspection required after return",
                CarStatus.Maintenance => "Service scheduled - Inspection or repair required",
                _ => "Status unknown"
            };
        }

        private string GetVehicleNoteLine(Car car)
        {
            return car.Status switch
            {
                CarStatus.Available => "Maintenance note: Vehicle is ready for the next rental.",
                CarStatus.Rented => "Maintenance note: Awaiting return and post-rental check.",
                CarStatus.Maintenance => GetMaintenanceNote(car.CarId),
                _ => "Maintenance note: No information available."
            };
        }

        private string GetMaintenanceNote(int carId)
        {
            var record = GetLatestMaintenanceRecord(carId);

            if (record == null)
                return "Maintenance note: Vehicle unavailable until inspection is completed.";

            string note = $"Maintenance issue: {record.Reason}";

            if (!string.IsNullOrWhiteSpace(record.Notes))
                note += $" - {record.Notes}";

            if (record.EstimatedCompletionDate.HasValue)
                note += $" | Estimated completion: {record.EstimatedCompletionDate.Value:dd MMM yyyy}";

            return note;
        }

        private void ViewReport_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            int carId = (int)button.Tag;

            Frame.Navigate(typeof(VehicleReportPage), carId);
        }

        private MaintenanceRecord GetLatestMaintenanceRecord(int carId)
        {
            using var db = new AppDbContext();

            return db.MaintenanceRecords
                .Where(m => m.CarId == carId && m.Status == "Open")
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefault();
        }

        private void StatusFilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VehicleStatusPanel == null)
                return;

            LoadVehicles();
        }
    }
}