using CarRental.Backend.Data;
using CarRental.Backend.Models;
using CarRental.Backend.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace Project.Views.Dashboard.Customer
{
    public sealed partial class ReservationDetailsPage : Page
    {
        private Reservation _reservation;

        public ReservationDetailsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            int reservationId = (int)e.Parameter;

            using var db = new AppDbContext();
            var reservationService = new ReservationService(db);
            reservationService.CancelExpiredPickups();

            _reservation = reservationService.GetReservationById(reservationId);

            if (_reservation == null)
                return;

            LoadReservationDetails();
        }

        private void LoadReservationDetails()
        {
            CarImage.Source = new BitmapImage(new Uri(_reservation.Car.ImagePath));

            CarNameText.Text = $"{_reservation.Car.Brand} {_reservation.Car.Model}";
            CarPlateText.Text = $"Plate: {_reservation.Car.PlateNumber}";
            CarPriceText.Text = $"€ {_reservation.Car.PricePerDay:0.00} / day";

            ReservationIdText.Text = $"Reservation #{_reservation.ReservationId}";
            StatusText.Text = _reservation.Status.ToString();

            PickupLocationText.Text = _reservation.PickupLocation;
            PickupDateText.Text = $"Pick-up: {_reservation.StartDate:dd MMM yyyy, HH:mm}";
            ReturnDateText.Text = $"Return: {_reservation.EndDate:dd MMM yyyy, HH:mm}";

            TimeSpan duration = _reservation.EndDate - _reservation.StartDate;

            if (duration.TotalHours < 24)
            {
                DurationText.Text = $"{Math.Ceiling(duration.TotalHours)} hours";
            }
            else
            {
                DurationText.Text = $"{Math.Ceiling(duration.TotalDays)} days";
            }

            TotalPriceText.Text = $"€ {_reservation.TotalCost:0.00}";
            LoadPaymentDetails();
            LoadRentalProgress();

            DateTime cancelDeadline = _reservation.StartDate.AddHours(-2);

            bool canCancel =
                (_reservation.Status == ReservationStatus.Pending ||
                 _reservation.Status == ReservationStatus.Confirmed) &&
                DateTime.Now <= cancelDeadline;

            CancelButton.Visibility = canCancel
                ? Visibility.Visible
                : Visibility.Collapsed;

            UpdateStatusBadge();
        }


        private void LoadRentalProgress()
        {
            ProgressActionButton.Visibility = Visibility.Visible;
            ProgressActionButton.IsEnabled = true;

            if (_reservation.Status == ReservationStatus.Pending)
            {
                RentalProgressText.Text = "Your reservation is waiting for owner approval.";
                ProgressActionButton.Visibility = Visibility.Collapsed;
            }
            else if (_reservation.Status == ReservationStatus.Confirmed)
            {
                DateTime pickupStartTime = _reservation.StartDate.AddMinutes(-15);
                DateTime pickupDeadline = _reservation.StartDate.AddHours(1);
                DateTime now = DateTime.Now;

                RentalProgressText.Text =
                    $"Your reservation is confirmed. Pick-up is available from {pickupStartTime:dd MMM yyyy, HH:mm} until {pickupDeadline:dd MMM yyyy, HH:mm}.";

                ProgressActionButton.Content = "Car picked up";

                ProgressActionButton.IsEnabled =
                    now >= pickupStartTime &&
                    now <= pickupDeadline;
            }
            else if (_reservation.Status == ReservationStatus.PickedUp)
            {
                RentalProgressText.Text = "You currently have the car. Mark it as returned when you bring it back to the DriveEase garage.";
                ProgressActionButton.Content = "Returned to garage";
            }
            else if (_reservation.Status == ReservationStatus.Returned)
            {
                RentalProgressText.Text = "The car was returned to the garage. Waiting for DriveEase garage confirmation.";
                ProgressActionButton.Visibility = Visibility.Collapsed;
            }
            else if (_reservation.Status == ReservationStatus.Completed)
            {
                RentalProgressText.Text = "This rental is completed.";
                ProgressActionButton.Visibility = Visibility.Collapsed;
            }
            else if (_reservation.Status == ReservationStatus.Cancelled)
            {
                RentalProgressText.Text = "This reservation was cancelled.";
                ProgressActionButton.Visibility = Visibility.Collapsed;
            }
        }


        private void ProgressAction_Click(object sender, RoutedEventArgs e)
        {
            if (_reservation == null)
                return;

            using var db = new AppDbContext();
            var reservationService = new ReservationService(db);

            if (_reservation.Status == ReservationStatus.Confirmed)
            {
                reservationService.MarkAsPickedUp(_reservation.ReservationId);
            }
            else if (_reservation.Status == ReservationStatus.PickedUp)
            {
                if (DateTime.Now > _reservation.EndDate)
                {
                    double lateHours = Math.Ceiling((DateTime.Now - _reservation.EndDate).TotalHours);
                    decimal lateFee = (decimal)lateHours * 10m;
                }

                reservationService.MarkAsReturned(_reservation.ReservationId);
            }

            _reservation = reservationService.GetReservationById(_reservation.ReservationId);

            if (_reservation == null)
                return;

            LoadReservationDetails();
        }

        private void LoadPaymentDetails()
        {
            using var db = new AppDbContext();

            var paymentService = new PaymentService(db);

            var payment = paymentService.GetPaymentByReservationId(_reservation.ReservationId);

            if (payment == null)
            {
                PaymentMethodText.Text = "Payment not found";
                PaymentStatusText.Text = "No payment information is available for this reservation.";
                return;
            }

            PaymentMethodText.Text = $"Method: {payment.MethodOfPayment}";

            if (payment.MethodOfPayment == "Cash")
            {
                PaymentStatusText.Text = "Payment will be made in cash at vehicle pick-up.";
            }
            else
            {
                PaymentStatusText.Text = $"Paid by card on {payment.PaymentDate:dd MMM yyyy, HH:mm}.";
            }

            if (_reservation.LateFee > 0)
            {
                LateFeeText.Visibility = Visibility.Visible;
                LateFeeText.Text =
                    $"Late return fee: € {_reservation.LateFee:0.00}";
            }
            else
            {
                LateFeeText.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateStatusBadge()
        {
            if (_reservation.Status == ReservationStatus.Pending)
            {
                StatusBadge.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 139, 92, 246));
            }
            else if (_reservation.Status == ReservationStatus.Confirmed)
            {
                StatusBadge.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 34, 197, 94));
            }
            else if (_reservation.Status == ReservationStatus.PickedUp)
            {
                StatusBadge.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 59, 130, 246));
            }
            else if (_reservation.Status == ReservationStatus.Returned)
            {
                StatusBadge.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 245, 158, 11));
            }
            else if (_reservation.Status == ReservationStatus.Cancelled)
            {
                StatusBadge.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 220, 38, 38));
            }
            else
            {
                StatusBadge.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 100, 116, 139));
            }
        }

        private async void CancelReservation_Click(object sender, RoutedEventArgs e)
        {
            if (_reservation == null)
                return;

            ContentDialog dialog = new ContentDialog
            {
                Title = "Cancel reservation?",
                Content = "Are you sure you want to cancel this reservation? This action cannot be undone.",
                PrimaryButtonText = "Yes, cancel",
                CloseButtonText = "Keep reservation",
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result != ContentDialogResult.Primary)
                return;

            using var db = new AppDbContext();
            var reservationService = new ReservationService(db);

            reservationService.CancelReservation(_reservation.ReservationId);

            Frame.Navigate(typeof(Project.Views.Dashboard.ReservationsPage));
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}