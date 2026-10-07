using CarRental.Backend.Data;
using CarRental.Backend.Models;
using CarRental.Backend.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Project.Views.Dashboard.Admin
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AdminGaragesPage : Page
    {
        private readonly GarageService _garageService;
        private readonly CarService _carService;
        private static int? _selectedGarageId;
        private readonly ReservationService _reservationService;

        public AdminGaragesPage()
        {
            InitializeComponent();

            var context = new AppDbContext();
            _garageService = new GarageService(context);
            _carService = new CarService(context);
            _reservationService = new ReservationService(context);

            LoadGarages();
        }

        private void LoadGarages()
        {
            var garages = _garageService.GetAllGarages();

            GarageComboBox.ItemsSource = garages;
            GaragesCountText.Text = garages.Count.ToString();

            MaintenanceCarsText.Text = _carService
                .CountCarsByStatus(CarStatus.Maintenance)
                .ToString();

            if (!garages.Any())
                return;

            if (_selectedGarageId.HasValue)
            {
                var selectedGarage = garages
                    .FirstOrDefault(g => g.GarageId == _selectedGarageId.Value);

                if (selectedGarage != null)
                    GarageComboBox.SelectedItem = selectedGarage;
            }
            else
            {
                GarageComboBox.SelectedItem = garages.First();
            }
        }


        private void GarageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GarageComboBox.SelectedItem is Garage garage)
            {
                _selectedGarageId = garage.GarageId;

                SelectedGarageTextBlock.Text = $"Cars in {garage.Name}";

                using var db = new AppDbContext();

                var cars = db.Cars
                    .Where(c => c.GarageId == garage.GarageId)
                    .ToList();

                foreach (var car in cars)
                {
                    var reservation = db.Reservations
                        .OrderByDescending(r => r.ReservationId)
                        .FirstOrDefault(r =>
                            r.CarId == car.CarId &&
                            (r.Status == ReservationStatus.Confirmed ||
                             r.Status == ReservationStatus.PickedUp ||
                             r.Status == ReservationStatus.Returned));

                    if (reservation != null)
                    {
                        if (reservation.Status == ReservationStatus.PickedUp)
                            car.MaintenanceStatus = "Picked Up";
                        else if (reservation.Status == ReservationStatus.Returned)
                            car.MaintenanceStatus = "Returned";
                        else
                            car.MaintenanceStatus = "Reserved";
                    }
                    else
                    {
                        if (car.Status == CarStatus.Maintenance)
                            car.MaintenanceStatus = "Maintenance";
                        else if (car.Status == CarStatus.Available)
                            car.MaintenanceStatus = "Available";
                        else if (car.Status == CarStatus.Rented)
                            car.MaintenanceStatus = "Rented";
                    }
                }

                CarsListView.ItemsSource = cars;

                var carsInside = _garageService.CountCarsInGarage(garage.GarageId);
                var availableSpots = _garageService.CountAvailableSpots(garage.GarageId);

                GarageCapacityText.Text =
                    $"Capacity: {garage.Capacity} • Cars inside: {carsInside} • Available spots: {availableSpots}";
            }
        }

        private async void MoveToMaintenance_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not Car car)
                return;

            using var db = new AppDbContext();

            var activeReservation = db.Reservations
                .FirstOrDefault(r =>
                    r.CarId == car.CarId &&
                    (r.Status == ReservationStatus.Confirmed ||
                     r.Status == ReservationStatus.PickedUp));

            bool blockMaintenance = false;

            if (activeReservation != null)
            {
                if (activeReservation.Status == ReservationStatus.PickedUp)
                {
                    blockMaintenance = true;
                }
                else if (activeReservation.Status == ReservationStatus.Confirmed)
                {
                    var hoursUntilPickup =
                        (activeReservation.StartDate - DateTime.Now).TotalHours;

                    if (hoursUntilPickup <= 24)
                        blockMaintenance = true;
                }
            }

            if (blockMaintenance)
            {
                var warningDialog = new ContentDialog
                {
                    Title = "Action blocked",
                    Content = "This vehicle has an active reservation and cannot enter maintenance.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await warningDialog.ShowAsync();
                return;
            }

            var reasonBox = new ComboBox
            {
                Header = "Issue type",
                PlaceholderText = "Select issue type",
                SelectedIndex = 0
            };

            reasonBox.Items.Add("Inspection");
            reasonBox.Items.Add("Brakes");
            reasonBox.Items.Add("Engine");
            reasonBox.Items.Add("Tires");
            reasonBox.Items.Add("Electrical");
            reasonBox.Items.Add("Cleaning");
            reasonBox.Items.Add("Other");

            var notesBox = new TextBox
            {
                Header = "Description",
                PlaceholderText = "Describe the maintenance issue...",
                AcceptsReturn = true,
                Height = 100,
                TextWrapping = TextWrapping.Wrap
            };

            var estimatedDatePicker = new CalendarDatePicker
            {
                Header = "Estimated completion date"
            };

            var panel = new StackPanel
            {
                Spacing = 12
            };

            panel.Children.Add(reasonBox);
            panel.Children.Add(notesBox);
            panel.Children.Add(estimatedDatePicker);

            var dialog = new ContentDialog
            {
                Title = $"Move {car.Brand} {car.Model} to maintenance",
                Content = panel,
                PrimaryButtonText = "Set maintenance",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result != ContentDialogResult.Primary)
                return;

            string reason = reasonBox.SelectedItem?.ToString() ?? "Inspection";
            string notes = notesBox.Text.Trim();

            DateTime? estimatedCompletion = estimatedDatePicker.Date?.DateTime;

            _carService.MoveToMaintenance(
                car.CarId,
                car.GarageId,
                reason,
                notes,
                estimatedCompletion);

            LoadGarages();
            ReloadSelectedGarageCars();
        }

        private void MarkAvailable_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not Car car)
                return;

            using var db = new AppDbContext();

            var fullCar = db.Cars.FirstOrDefault(c => c.CarId == car.CarId);

            if (fullCar == null)
                return;

            fullCar.Status = CarStatus.Available;

            if (fullCar.UserId.HasValue)
            {
                var notificationService = new NotificationService(db);

                notificationService.CreateNotification(
                    fullCar.UserId.Value,
                    "Maintenance completed",
                    $"Your {fullCar.Brand} {fullCar.Model} is now available again.",
                    "Maintenance");
            }


            fullCar.MaintenanceStatus = "Completed";
            fullCar.MaintenanceReason = null;

            var maintenanceRecord = db.MaintenanceRecords
                .Where(m => m.CarId == fullCar.CarId && m.Status == "Open")
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefault();

            if (maintenanceRecord != null)
            {
                maintenanceRecord.Status = "Completed";
                maintenanceRecord.CompletedAt = DateTime.Now;
            }

            db.SaveChanges();

            var carService = new CarService(db);
            carService.NotifyCustomersCarAvailable(fullCar.CarId);

            LoadGarages();

            if (_selectedGarageId.HasValue)
            {
                var selectedGarage = GarageComboBox.Items
                    .Cast<Garage>()
                    .FirstOrDefault(g => g.GarageId == _selectedGarageId.Value);

                if (selectedGarage != null)
                {
                    GarageComboBox.SelectedItem = selectedGarage;
                    GarageComboBox_SelectionChanged(GarageComboBox, null);
                }
            }
        }

        private void CarsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Car car)
            {
                Frame.Navigate(typeof(AdminVehicleDetailsPage), car.CarId);
            }
        }

        private void ReloadSelectedGarageCars()
        {
            if (GarageComboBox.SelectedItem is Garage garage)
            {
                GarageComboBox_SelectionChanged(GarageComboBox, null);
            }
        }
    }
}
