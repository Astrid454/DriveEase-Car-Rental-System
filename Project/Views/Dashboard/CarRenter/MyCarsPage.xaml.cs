using CarRental.Backend.Data;
using CarRental.Backend.Models;
using CarRental.Backend.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;
using System.IO;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Project.Views.Dashboard.CarRenter
{
    public sealed partial class MyCarsPage : Page
    {
        public MyCarsPage()
        {
            InitializeComponent();
            LoadCars();
        }

        protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            LoadCars();
        }

        private void LoadCars()
        {
            if (!SessionManager.IsLoggedIn)
                return;

            using var db = new AppDbContext();
            var carService = new CarService(db);

            int currentUserId = SessionManager.CurrentUser.UserId;

            var cars = carService.GetCarsByOwner(currentUserId);

            TotalCarsText.Text = cars.Count.ToString();
            AvailableCarsText.Text = cars.Count(c => c.Status == CarStatus.Available).ToString();
            RentedCarsText.Text = cars.Count(c => c.Status == CarStatus.Rented).ToString();

            CarsListPanel.Children.Clear();

            foreach (var car in cars)
            {
                CarsListPanel.Children.Add(CreateCarCard(car));
            }
        }

        private Border CreateCarCard(Car car)
        {
            var statusText = car.Status.ToString();

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
                    FontSize = 28,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
            }

            var iconBox = new Border
            {
                Width = 72,
                Height = 72,
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
                    Text = statusText,
                    FontSize = 11,
                    Foreground = statusForeground
                }
            });

            infoPanel.Children.Add(titleRow);

            infoPanel.Children.Add(new TextBlock
            {
                Text = $"{car.Year} - {car.PlateNumber}",
                FontSize = 13,
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
            });

            infoPanel.Children.Add(new TextBlock
            {
                Text = $"€{car.PricePerDay:N2} / day",
                FontSize = 14,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            });

            Grid.SetColumn(infoPanel, 1);

            var actionsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                VerticalAlignment = VerticalAlignment.Center
            };

            var editButton = new Button
            {
                Content = "Edit",
                Tag = car.CarId
            };
            editButton.Click += EditCar_Click;

            var removeButton = new Button
            {
                Content = "Remove",
                Tag = car.CarId
            };

            removeButton.Click += RemoveCar_Click;

            actionsPanel.Children.Add(editButton);
            actionsPanel.Children.Add(removeButton);

            Grid.SetColumn(actionsPanel, 2);

            grid.Children.Add(iconBox);
            grid.Children.Add(infoPanel);
            grid.Children.Add(actionsPanel);

            card.Child = grid;

            return card;
        }

        private void EditCar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            int carId = (int)button.Tag;

            Frame.Navigate(typeof(AddCarPage), carId);
        }

        private async void RemoveCar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            int carId = (int)button.Tag;

            using var db = new AppDbContext();

            var activeReservation = db.Reservations
                .Where(r => r.CarId == carId)
                .Where(r =>
                    r.Status == ReservationStatus.Confirmed ||
                    r.Status == ReservationStatus.PickedUp ||
                    r.Status == ReservationStatus.Returned)
                .OrderByDescending(r => r.ReservationId)
                .FirstOrDefault();

            if (activeReservation != null)
            {
                string message = activeReservation.Status switch
                {
                    ReservationStatus.PickedUp =>
                        "This car is currently picked up by a customer and cannot be removed.",

                    ReservationStatus.Confirmed =>
                        "This car has an active reservation and cannot be removed.",

                    ReservationStatus.Returned =>
                        "This car was returned and is waiting for admin confirmation, so it cannot be removed yet.",

                    _ =>
                        "This car cannot be removed because it has an active rental process."
                };

                var blockedDialog = new ContentDialog
                {
                    Title = "Remove blocked",
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await blockedDialog.ShowAsync();
                return;
            }

            var dialog = new ContentDialog
            {
                Title = "Remove car",
                Content = "Are you sure you want to remove this car?",
                PrimaryButtonText = "Remove",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result != ContentDialogResult.Primary)
                return;

            var carService = new CarService(db);

            carService.DeleteCar(carId);

            LoadCars();
        }

        private void ToggleMaintenance_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            int carId = (int)button.Tag;

            using var db = new AppDbContext();
            var carService = new CarService(db);

            carService.ToggleMaintenance(carId);

            LoadCars();
        }

        private void AddCar_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AddCarPage));
        }
    }
}
