using CarRental.Backend.Data;
using CarRental.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Linq;

namespace Project.Views.Dashboard.Admin
{
    public sealed partial class AdminVehicleDetailsPage : Page
    {
        public AdminVehicleDetailsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            int carId = (int)e.Parameter;

            using var db = new AppDbContext();

            var car = db.Cars
                .Include(c => c.Garage)
                .Include(c => c.Owner)
                .FirstOrDefault(c => c.CarId == carId);

            if (car == null)
                return;

            var activeReservation = db.Reservations
                .Include(r => r.Customer)
                .FirstOrDefault(r =>
                    r.CarId == carId &&
                    (r.Status == ReservationStatus.Confirmed ||
                     r.Status == ReservationStatus.PickedUp ||
                     r.Status == ReservationStatus.Returned));

            CarTitleText.Text = $"{car.Brand} {car.Model}";
            PlateText.Text = $"Plate number: {car.PlateNumber}";
            YearText.Text = $"Year: {car.Year}";
            GarageText.Text = car.Garage == null
                ? "Garage: Not assigned"
                : $"Garage: {car.Garage.Name} - {car.Garage.City}";
            OwnerText.Text = car.Owner == null
                ? $"Owner ID: {car.UserId}"
                : $"Owner: {car.Owner.FirstName} {car.Owner.LastName}";
            CarStatusText.Text = $"Car status: {car.Status}";

            if (activeReservation == null)
            {
                ReservationStatusText.Text = "No active reservation.";
                CustomerText.Text = "";
                PickupText.Text = "";
                ReturnText.Text = "";
                PickedUpText.Text = "";
                ReturnedText.Text = "";
                MaintenanceAllowedText.Text =
                    "Maintenance is allowed because this vehicle has no active reservation.";
                return;
            }

            ReservationStatusText.Text = $"Reservation status: {activeReservation.Status}";
            CustomerText.Text =
                $"Customer: {activeReservation.Customer.FirstName} {activeReservation.Customer.LastName} ({activeReservation.Customer.Email})";
            PickupText.Text = $"Pickup: {activeReservation.StartDate:dd MMM yyyy, HH:mm}";
            ReturnText.Text = $"Return: {activeReservation.EndDate:dd MMM yyyy, HH:mm}";
            PickedUpText.Text = activeReservation.PickedUpAt.HasValue
                ? $"Picked up at: {activeReservation.PickedUpAt.Value:dd MMM yyyy, HH:mm}"
                : "Picked up at: Not picked up yet";
            ReturnedText.Text = activeReservation.ReturnedAt.HasValue
                ? $"Returned at: {activeReservation.ReturnedAt.Value:dd MMM yyyy, HH:mm}"
                : "Returned at: Not returned yet";

            if (activeReservation.Status == ReservationStatus.PickedUp)
            {
                MaintenanceAllowedText.Text =
                    "Maintenance is blocked because the vehicle is currently picked up.";
            }
            else if (activeReservation.Status == ReservationStatus.Confirmed)
            {
                var hoursUntilPickup =
                    (activeReservation.StartDate - DateTime.Now).TotalHours;

                if (hoursUntilPickup <= 24)
                {
                    MaintenanceAllowedText.Text =
                        "Maintenance is blocked because pickup is scheduled within 24 hours.";
                }
                else
                {
                    MaintenanceAllowedText.Text =
                        "Maintenance is allowed. Pickup is more than 24 hours away.";
                }
            }
            else if (activeReservation.Status == ReservationStatus.Returned)
            {
                MaintenanceAllowedText.Text =
                    "Vehicle has been returned and is waiting for admin confirmation.";
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}