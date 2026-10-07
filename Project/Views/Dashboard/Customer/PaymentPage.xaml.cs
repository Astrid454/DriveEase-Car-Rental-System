using CarRental.Backend.Data;
using CarRental.Backend.Models;
using CarRental.Backend.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using static Project.Views.Dashboard.Customer.NewReservationPage;

namespace Project.Views.Dashboard.Customer
{
    public sealed partial class PaymentPage : Page
    {
        private PendingReservationData _reservationData;

        public PaymentPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _reservationData = e.Parameter as PendingReservationData;

            if (_reservationData == null)
                return;

            CarNameText.Text = _reservationData.CarName;

            ReservationDatesText.Text =
                $"{_reservationData.StartDate:dd MMM yyyy, HH:mm} → {_reservationData.EndDate:dd MMM yyyy, HH:mm}";

            TotalAmountText.Text = $"€ {_reservationData.TotalCost:0.00}";
        }

        private void PaymentMethodBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CardDetailsPanel == null || PaymentMethodBox == null)
                return;

            if (PaymentMethodBox.SelectedItem is ComboBoxItem item &&
                item.Content.ToString() == "Card")
            {
                CardDetailsPanel.Visibility = Visibility.Visible;
            }
            else
            {
                CardDetailsPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void ConfirmPayment_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = "";

            if (_reservationData == null)
                return;

            if (PaymentMethodBox.SelectedItem is not ComboBoxItem selectedMethod)
            {
                ErrorText.Text = "Please select a payment method.";
                return;
            }

            string method = selectedMethod.Content.ToString();

            if (method == "Card")
            {
                if (string.IsNullOrWhiteSpace(CardHolderBox.Text) ||
                    string.IsNullOrWhiteSpace(CardNumberBox.Text) ||
                    string.IsNullOrWhiteSpace(ExpiryDateBox.Text))
                {
                    ErrorText.Text = "Please complete the card details.";
                    return;
                }
            }

            using var db = new AppDbContext();

            var reservationService = new ReservationService(db);
            var paymentService = new PaymentService(db);

            int reservationId = reservationService.CreateReservationAndReturnId(
                _reservationData.CarId,
                _reservationData.CustomerEmail,
                _reservationData.StartDate,
                _reservationData.EndDate,
                _reservationData.PickupLocation);

            if (reservationId == 0)
            {
                ErrorText.Text = "Reservation could not be completed.";
                return;
            }

            paymentService.CreatePayment(
                _reservationData.TotalCost,
                method,
                reservationId,
                _reservationData.CarName);

            Frame.Navigate(typeof(Project.Views.Dashboard.ReservationsPage));
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}