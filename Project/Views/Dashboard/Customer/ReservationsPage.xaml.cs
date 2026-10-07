using CarRental.Backend.Data;
using CarRental.Backend.Models;
using CarRental.Backend.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Project.Views.Dashboard
{
    public sealed partial class ReservationsPage : Page
    {
        public ReservationsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            LoadReservations();
        }

        private void LoadReservations()
        {
            if (SessionManager.CurrentUser == null)
                return;

            using var db = new AppDbContext();
            var reservationService = new ReservationService(db);

            ReservationsList.ItemsSource =
                reservationService.GetCustomerReservations(SessionManager.CurrentUser.Email);
        }

        private void ReservationsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Reservation reservation)
            {
                Frame.Navigate(typeof(Customer.ReservationDetailsPage), reservation.ReservationId);
            }
        }
    }
}