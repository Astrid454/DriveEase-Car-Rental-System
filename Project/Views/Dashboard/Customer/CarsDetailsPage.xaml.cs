using CarRental.Backend.Data;
using CarRental.Backend.Models;
using CarRental.Backend.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace Project.Views.Dashboard.Customer
{
    public sealed partial class CarDetailsPage : Page
    {
        private Car _car;
        private bool _isFavorite;

        public CarDetailsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            int carId = (int)e.Parameter;

            using var db = new AppDbContext();
            var carService = new CarService(db);
            var favoriteService = new FavoriteService(db);

            _car = carService.GetCarById(carId);

            if (_car == null)
                return;

            CarImage.Source = new BitmapImage(new Uri(_car.ImagePath));
            CarTitle.Text = _car.Brand;
            CarModel.Text = _car.Model;
            CarYear.Text = $"Year: {_car.Year}";
            CarPlate.Text = $"Plate number: {_car.PlateNumber}";
            CarStatus.Text = $"Status: {_car.Status}";
            CarLocation.Text = $"Pick-up location: {_car.GarageLocation}";
            CarPrice.Text = $"€ {_car.PricePerDay:0.00} / day";

            UpdateReservationButtons();

            if (SessionManager.CurrentUser != null)
            {
                _isFavorite = favoriteService.IsFavorite(SessionManager.CurrentUser.UserId, _car.CarId);
                UpdateFavoriteButton();
            }
        }

        private void UpdateReservationButtons()
        {
            if (_car.Status == CarRental.Backend.Models.CarStatus.Maintenance)
            {
                ReserveButton.IsEnabled = false;
                ReserveButton.Content = "Unavailable";
                NotifyMeButton.Visibility = Visibility.Visible;
            }
            else
            {
                ReserveButton.IsEnabled = true;
                ReserveButton.Content = "Reserve";
                NotifyMeButton.Visibility = Visibility.Collapsed;
            }
        }

        private void NotifyMe_Click(object sender, RoutedEventArgs e)
        {
            if (_car == null || SessionManager.CurrentUser == null)
                return;

            using var db = new AppDbContext();
            var carService = new CarService(db);

            carService.CreateAvailabilityAlert(
                SessionManager.CurrentUser.UserId,
                _car.CarId);

            NotifyMeButton.Content = "Notification set";
            NotifyMeButton.IsEnabled = false;
        }

        private void Favorite_Click(object sender, RoutedEventArgs e)
        {
            if (SessionManager.CurrentUser == null || _car == null)
                return;

            using var db = new AppDbContext();
            var favoriteService = new FavoriteService(db);

            _isFavorite = favoriteService.ToggleFavorite(SessionManager.CurrentUser.UserId, _car.CarId);
            UpdateFavoriteButton();
        }

        private void UpdateFavoriteButton()
        {
            BtnFavorite.Content = _isFavorite ? "♥ Favorite" : "♡ Favorite";
        }

        private void Reserve_Click(object sender, RoutedEventArgs e)
        {
            if (_car == null)
                return;

            if (_car.Status == CarRental.Backend.Models.CarStatus.Maintenance)
                return;

            Frame.Navigate(typeof(NewReservationPage), _car.CarId);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}