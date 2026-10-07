using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;
using CarRental.Backend.Data;
using CarRental.Backend.Models;
using CarRental.Backend.Services;
using Microsoft.EntityFrameworkCore;

namespace Project.Views.Dashboard
{
    public sealed partial class OverviewPage : Page
    {
        public OverviewPage()
        {
            InitializeComponent();

            var user = SessionManager.CurrentUser;

            if (user != null)
            {
                LoadWelcomeMessage(user);
                LoadDashboardStats(user);
                LoadUpcomingReservation(user);
                LoadRecentActivity(user);
            }
            else
            {
                WelcomeText.Text = "Welcome back!";
                WelcomeSubtitle.Text = "Welcome to DriveEase.";
            }
        }

        private void LoadWelcomeMessage(User user)
        {
            switch (user.Role)
            {
                case "Admin":
                    WelcomeText.Text = $"Welcome back, Admin {user.FirstName}!";
                    WelcomeSubtitle.Text = "Monitor platform activity, manage users, and oversee reservations across DriveEase.";
                    break;

                case "CarRenter":
                    WelcomeText.Text = $"Welcome back, {user.FirstName}! Ready to manage your fleet?";
                    WelcomeSubtitle.Text = "Manage your listed vehicles, track reservations, and grow your rental business with DriveEase.";
                    break;

                default:
                    WelcomeText.Text = $"Welcome back, {user.FirstName} {user.LastName}!";
                    WelcomeSubtitle.Text = "Manage your reservations, explore available cars, save your favorite vehicles, and get support anytime with DriveEase.";
                    break;
            }
        }

        private void LoadDashboardStats(User user)
        {
            using var db = new AppDbContext();

            var reservationService = new ReservationService(db);
            var favoriteService = new FavoriteService(db);
            var supportService = new SupportTicketService(db);

            ReservationsCountText.Text = reservationService
                .GetActiveReservationsCount(user.Email)
                .ToString();

            FavoritesCountText.Text = favoriteService
                .GetFavoriteCount(user.UserId)
                .ToString();

            SupportTicketsText.Text = supportService
                .GetOpenTicketCount(user.UserId)
                .ToString();
        }

        private void LoadUpcomingReservation(User user)
        {
            using var db = new AppDbContext();
            var reservationService = new ReservationService(db);

            var upcomingReservation = reservationService.GetUpcomingReservation(user.Email);

            if (upcomingReservation == null)
            {
                ShowNoUpcomingReservation();
                return;
            }

            UpcomingCarNameText.Text = $"{upcomingReservation.Car.Brand} {upcomingReservation.Car.Model}";
            UpcomingLocationText.Text = upcomingReservation.PickupLocation;
            UpcomingDatesText.Text =
                $"{upcomingReservation.StartDate:dd MMM yyyy, HH:mm} → {upcomingReservation.EndDate:dd MMM yyyy, HH:mm}";
            UpcomingStatusText.Text = $"Status: {upcomingReservation.Status}";

            if (!string.IsNullOrEmpty(upcomingReservation.Car.ImagePath))
            {
                UpcomingCarImage.Source = new BitmapImage(new Uri(upcomingReservation.Car.ImagePath));
            }
        }

        private void ShowNoUpcomingReservation()
        {
            UpcomingCarNameText.Text = "No upcoming reservations";
            UpcomingLocationText.Text = "Explore available cars and reserve your next ride.";
            UpcomingDatesText.Text = "";
            UpcomingStatusText.Text = "";
            UpcomingCarImage.Source = null;
        }

        private void LoadRecentActivity(User user)
        {
            using var db = new AppDbContext();

            var favoriteService = new FavoriteService(db);
            var reservationService = new ReservationService(db);

            var latestFavorite = favoriteService.GetLatestFavorite(user.UserId);
            var latestReservation = reservationService.GetLatestReservation(user.Email);

            string activity = "";

            if (latestFavorite?.Car != null)
            {
                activity += $"• Added {latestFavorite.Car.Brand} {latestFavorite.Car.Model} to favorites\n";
            }

            if (latestReservation?.Car != null)
            {
                activity += $"• Reserved {latestReservation.Car.Brand} {latestReservation.Car.Model}\n";
            }

            RecentActivityText.Text = string.IsNullOrWhiteSpace(activity)
                ? "No recent activity yet."
                : activity.Trim();
        }
    }
}