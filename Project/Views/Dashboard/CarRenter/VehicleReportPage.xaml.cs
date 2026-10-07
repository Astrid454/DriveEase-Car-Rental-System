using CarRental.Backend.Data;
using CarRental.Backend.Models;
using CarRental.Backend.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Project.Views.Dashboard.CarRenter
{
    public sealed partial class VehicleReportPage : Page
    {
        private Car _car;
        private MaintenanceRecord _maintenanceRecord;

        public VehicleReportPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            int carId = (int)e.Parameter;

            using var db = new AppDbContext();

            _car = db.Cars
                .Include(c => c.Garage)
                .FirstOrDefault(c => c.CarId == carId);

            _maintenanceRecord = db.MaintenanceRecords
                .Where(m => m.CarId == carId && m.Status == "Open")
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefault();

            if (_car == null)
                return;

            LoadReport();
        }

        private void LoadReport()
        {
            if (!string.IsNullOrWhiteSpace(_car.ImagePath))
            {
                CarImage.Source = new BitmapImage(new Uri(_car.ImagePath));
            }

            CarNameText.Text = $"{_car.Brand} {_car.Model}";
            PlateText.Text = $"Plate number: {_car.PlateNumber}";
            YearText.Text = $"Year: {_car.Year}";
            StatusText.Text = $"Current status: {_car.Status}";
            LocationText.Text = $"Garage: {_car.GarageLocation}";

            OperationalStatusText.Text = _car.Status.ToString();
            MaintenanceStatusText.Text =
                _maintenanceRecord?.Status == "Open"
                    ? "Active"
                    : "No active maintenance";
            AvailabilityText.Text = _car.Status == CarStatus.Available ? "Ready" : "Unavailable";

            if (_maintenanceRecord == null)
            {
                MaintenanceReasonText.Text = "Reason: No active maintenance record.";
                MaintenanceNotesText.Text = "Notes: Vehicle is currently ready for use.";
                MaintenanceCreatedText.Text = "";
                MaintenanceEstimatedText.Text = "";
                GuidanceText.Text = "This vehicle is available for future reservations.";
                return;
            }

            MaintenanceReasonText.Text = $"Reason: {_maintenanceRecord.Reason}";
            MaintenanceNotesText.Text = string.IsNullOrWhiteSpace(_maintenanceRecord.Notes)
                ? "Notes: No additional notes."
                : $"Notes: {_maintenanceRecord.Notes}";

            MaintenanceCreatedText.Text = $"Started at: {_maintenanceRecord.CreatedAt:dd MMM yyyy, HH:mm}";

            MaintenanceEstimatedText.Text = _maintenanceRecord.EstimatedCompletionDate.HasValue
                ? $"Estimated completion: {_maintenanceRecord.EstimatedCompletionDate.Value:dd MMM yyyy}"
                : "Estimated completion: Not specified";

            GuidanceText.Text =
                "This vehicle is controlled by DriveEase maintenance while it is unavailable. " +
                "You will receive a notification when the vehicle becomes available again.";
        }

        private void Back_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}