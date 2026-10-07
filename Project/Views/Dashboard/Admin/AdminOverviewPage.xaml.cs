using CarRental.Backend.Data;
using CarRental.Backend.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Project.Views.Auth;
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
    public sealed partial class AdminOverviewPage : Page
    {
        private const decimal COMPANY_FEE_PERCENT = 0.15m;
        public AdminOverviewPage()
        {
            InitializeComponent();
            LoadAdminOverview();
        }

        private void LoadAdminOverview()
        {
            using var db = new AppDbContext();

            CustomersCountText.Text = db.Customers.Count().ToString();

            CarRentersCountText.Text = db.Users
                .Count(u => u.Role == "CarRenter")
                .ToString();

            CarsCountText.Text = db.Cars.Count().ToString();

            ReturnedReservationsCountText.Text = db.Reservations
                .Count(r => r.Status == ReservationStatus.Returned)
                .ToString();

            CompletedRentalsCountText.Text = db.Reservations
                .Count(r => r.Status == ReservationStatus.Completed)
                .ToString();

            var now = DateTime.Now;

            decimal monthlyCompanyRevenue = db.Reservations
                .Where(r =>
                    r.Status == ReservationStatus.Completed &&
                    r.StartDate.Month == now.Month &&
                    r.StartDate.Year == now.Year)
                .Sum(r => (r.TotalCost + r.LateFee) * COMPANY_FEE_PERCENT);

            MonthlyRevenueText.Text = $"€ {monthlyCompanyRevenue:0.00}";
        }

        private void SignOut_Click(object sender, RoutedEventArgs e)
        {
            SessionManager.CurrentUser = null;
            MainWindow.Current.Navigate(typeof(SignInPage));
        }
    }
}
