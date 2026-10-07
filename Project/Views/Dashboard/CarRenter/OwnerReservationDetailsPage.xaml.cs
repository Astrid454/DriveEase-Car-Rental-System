using CarRental.Backend.Data;
using CarRental.Backend.Models;
using CarRental.Backend.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace Project.Views.Dashboard.CarRenter
{
    public sealed partial class OwnerReservationDetailsPage : Page
    {
        private Reservation _reservation;

        public OwnerReservationDetailsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            int reservationId = (int)e.Parameter;

            using var db = new AppDbContext();
            var reservationService = new ReservationService(db);

            _reservation = reservationService.GetReservationById(reservationId);

            if (_reservation == null)
                return;

            LoadDetails();
        }

        private void LoadDetails()
        {
            StatusText.Text = _reservation.Status.ToString();
            UpdateStatusBadge();

            CustomerNameText.Text = $"Name: {_reservation.Customer.FirstName} {_reservation.Customer.LastName}";
            CustomerEmailText.Text = $"Email: {_reservation.Customer.Email}";
            CustomerPhoneText.Text = $"Phone: {_reservation.Customer.Phone}";

            VehicleNameText.Text = $"{_reservation.Car.Brand} {_reservation.Car.Model}";
            PlateText.Text = $"Plate: {_reservation.Car.PlateNumber}";
            PriceText.Text = $"Price: € {_reservation.Car.PricePerDay:0.00} / day";

            PickupText.Text = $"Pick-up: {_reservation.StartDate:dd MMM yyyy, HH:mm}";
            ReturnText.Text = $"Return: {_reservation.EndDate:dd MMM yyyy, HH:mm}";
            PickupLocationText.Text = $"Location: {_reservation.PickupLocation}";

            LoadPaymentDetails();
            LoadProgressDetails();
            LoadRevenueDetails();
        }

        private void LoadPaymentDetails()
        {
            using var db = new AppDbContext();
            var paymentService = new PaymentService(db);

            var payment = paymentService.GetPaymentByReservationId(_reservation.ReservationId);

            if (payment == null)
            {
                PaymentMethodText.Text = "Method: Not available";
                PaymentStatusText.Text = "No payment information found.";
            }
            else
            {
                PaymentMethodText.Text = $"Method: {payment.MethodOfPayment}";

                PaymentStatusText.Text = payment.MethodOfPayment == "Cash"
                    ? "Payment will be collected at vehicle pick-up."
                    : $"Paid by card on {payment.PaymentDate:dd MMM yyyy, HH:mm}.";
            }

            LateFeeText.Text = _reservation.LateFee > 0
                ? $"Late fee: € {_reservation.LateFee:0.00}"
                : "Late fee: € 0.00";
        }

        private void LoadProgressDetails()
        {
            if (_reservation.Status == ReservationStatus.Pending)
            {
                ProgressText.Text = "Waiting for owner approval.";
            }
            else if (_reservation.Status == ReservationStatus.Confirmed)
            {
                ProgressText.Text = "Reservation approved. The customer has not picked up the vehicle yet.";
            }
            else if (_reservation.Status == ReservationStatus.PickedUp)
            {
                ProgressText.Text =
                    $"Vehicle picked up by customer on {_reservation.PickedUpAt:dd MMM yyyy, HH:mm}.";
            }
            else if (_reservation.Status == ReservationStatus.Returned)
            {
                ProgressText.Text =
                    $"Vehicle returned to DriveEase garage on {_reservation.ReturnedAt:dd MMM yyyy, HH:mm}. Waiting for DriveEase/admin confirmation.";
            }
            else if (_reservation.Status == ReservationStatus.Completed)
            {
                ProgressText.Text =
                    $"Rental completed. Returned on {_reservation.ReturnedAt:dd MMM yyyy, HH:mm}.";
            }
            else
            {
                ProgressText.Text = "Reservation was cancelled.";
            }
        }

        private void LoadRevenueDetails()
        {
            decimal companyFee = _reservation.TotalCost * 0.15m;
            decimal ownerNet = _reservation.TotalCost - companyFee;

            TotalText.Text = $"€ {_reservation.TotalCost:0.00}";
            FeeText.Text = $"€ {companyFee:0.00}";
            OwnerNetText.Text = $"€ {ownerNet:0.00}";
        }

        private void UpdateStatusBadge()
        {
            if (_reservation.Status == ReservationStatus.Pending)
                StatusBadge.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 139, 92, 246));
            else if (_reservation.Status == ReservationStatus.Confirmed)
                StatusBadge.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 34, 197, 94));
            else if (_reservation.Status == ReservationStatus.PickedUp)
                StatusBadge.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 59, 130, 246));
            else if (_reservation.Status == ReservationStatus.Returned)
                StatusBadge.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 245, 158, 11));
            else if (_reservation.Status == ReservationStatus.Cancelled)
                StatusBadge.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 220, 38, 38));
            else
                StatusBadge.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 100, 116, 139));
        }

        private void Back_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}