using CarRental.Backend.Data;
using CarRental.Backend.Models;
using CarRental.Backend.Services;
using Microsoft.UI.Xaml.Controls;
using Project.Views.Dashboard.Customer;

namespace Project.Views.Dashboard
{
    public sealed partial class CarsPage : Page
    {
        public CarsPage()
        {
            InitializeComponent();
            LoadCars();
        }

        private void LoadCars()
        {
            using var db = new AppDbContext();
            var carService = new CarService(db);

            CarsGrid.ItemsSource = carService.GetAllCars();
        }

        private void CarsGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Car selectedCar)
            {
                Frame.Navigate(typeof(CarDetailsPage), selectedCar.CarId);
            }
        }
    }
}